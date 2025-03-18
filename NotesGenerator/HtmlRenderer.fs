module HtmlRenderer

open System

type Token =
    | Text of string
    | Bold of string
    | Code of string
    | Link of text: string * url: string

type ProcessingState =
    { Tokens: Token list
      CurrentText: string
      IsBold: bool
      IsCode: bool
      IsLink: bool
      LinkText: string option }

let private appendToken token state = { state with Tokens = token :: state.Tokens; CurrentText = "" }
let private appendChar ch state = { state with CurrentText = state.CurrentText + string ch }

let private finalizeBold state =
    if state.CurrentText.Length > 0 then
        appendToken (Bold state.CurrentText) { state with IsBold = false }
    else
        { state with IsBold = false }

let private finalizeCode (state: ProcessingState) =
    if state.CurrentText.Length > 0 then
        appendToken (Code state.CurrentText) { state with IsCode = false }
    else
        { state with IsCode = false }

let private finalizeLink url state =
    match state.LinkText with
    | Some text -> appendToken (Link(text, url)) { state with IsLink = false; LinkText = None }
    | None -> state

let private tokensToHtml tokens =
    let rec convertToken =
        function
        | Text text -> System.Web.HttpUtility.HtmlEncode text
        | Bold text -> $"<b>{text}</b>" |> System.Web.HttpUtility.HtmlEncode
        | Code text -> $"<code>{text}</code>" |> System.Web.HttpUtility.HtmlEncode
        | Link(text, url) -> $"<a href=\"{url}\">{text}</a>" |> System.Web.HttpUtility.HtmlEncode

    tokens |> List.rev |> List.map convertToken |> String.concat ""

let state =
    { Tokens = []; CurrentText = ""; IsBold = false; IsCode = false; IsLink = false; LinkText = None }

let rec private parseText (text: char list) (state: ProcessingState) =
    match text with
    | [] ->
        match state with
        | s when s.CurrentText.Length > 0 -> appendToken (Text state.CurrentText) state
        | _ -> state

    | '*' :: '*' :: rest when not state.IsCode ->
        let newState =
            match state with
            | s when s.CurrentText.Length > 0 -> appendToken (Text state.CurrentText) state
            | _ -> state

        parseText rest { newState with IsBold = not state.IsBold }

    | '*' :: '*' :: rest when state.IsBold ->
        let newState = finalizeBold state
        parseText rest { newState with IsBold = not state.IsBold }

    | '`' :: '`' :: '`' :: rest when not state.IsCode ->
        let newState =
            match state with
            | s when s.CurrentText.Length > 0 -> appendToken (Text state.CurrentText) state
            | _ -> state

        parseText rest { newState with IsCode = true }

    | '`' :: '`' :: '`' :: rest when state.IsCode ->
        let newState = finalizeCode state
        parseText rest { newState with IsCode = false }

    | '[' :: rest when not (state.IsCode || state.IsBold) ->
        let newState =
            match state with
            | s when s.CurrentText.Length > 0 -> appendToken (Text state.CurrentText) state
            | _ -> state

        parseText rest newState

    | ']' :: '(' :: rest when not (state.IsCode || state.IsBold) ->
        parseText rest { state with LinkText = Some state.CurrentText; CurrentText = ""; IsLink = true }

    | ')' :: rest when state.IsLink ->
        let newState = finalizeLink state.CurrentText state
        parseText rest newState

    | c :: rest -> parseText rest (appendChar c state)

let processText (markdown: string) =
    markdown.ToCharArray()
    |> Array.toList
    |> fun chars -> parseText chars state
    |> fun state -> state.Tokens
    |> tokensToHtml

type Meta = { Title: string; Date: string; Path: string; Tags: string list; Url: string }

type MarkdownElement =
    | MetaMarker
    | MetaContent of key: string * value: string
    | CodeBlockMarker
    | CodeContent of string
    | Image of alt: string * path: string
    | ListItem of string
    | Header of level: int * content: string
    | PlainText of string

type HtmlPage = { Meta: Meta; HtmlContent: string }

type ConversionState = { Meta: Map<string, string>; HtmlContent: string list; IsInMeta: bool; IsInCode: bool }
type ParsingState = { IsInMeta: bool; IsInCode: bool; MarkdownContent: MarkdownElement list }

let private parseMetaLine (line: string) =
    match line.Split(':') with
    | [| key; value |] -> MetaContent(key.Trim(), value.Trim())
    | _ -> PlainText line

let private parseHeader (line: string) =
    let headerLevel = line |> Seq.takeWhile ((=) '#') |> Seq.length |> min 6

    let content = line.TrimStart('#').Trim()
    Header(headerLevel, content)

let private parseImage (line: string) =
    let content = line.Trim() |> fun s -> s.TrimStart('!', '[').TrimEnd(']')

    Image(content, $"./Images/{content}")

let private parseLine (state: ParsingState) (line: string) : ParsingState =
    match (line.Trim(), state.IsInCode, state.IsInMeta) with
    | "---", _, false -> { IsInMeta = true; IsInCode = false; MarkdownContent = state.MarkdownContent @ [ MetaMarker ] }
    | "---", _, true -> { IsInMeta = false; IsInCode = false; MarkdownContent = state.MarkdownContent @ [ MetaMarker ] }
    | _, _, true ->
        let key = line.Split(':').[0].Trim()
        let value = line.Split(':').[1].Trim()
        { state with MarkdownContent = state.MarkdownContent @ [ MetaContent(key, value) ] }
    | "```", _, _ ->
        { IsInCode = not state.IsInCode
          IsInMeta = false
          MarkdownContent = state.MarkdownContent @ [ CodeBlockMarker ] }
    | line, _, _ when
        line.StartsWith("# ")
        || line.StartsWith("## ")
        || line.StartsWith("### ")
        || line.StartsWith("#### ")
        || line.StartsWith("##### ")
        || line.StartsWith("###### ")
        ->
        let header = parseHeader line
        { state with MarkdownContent = state.MarkdownContent @ [ header ] }
    | line, _, _ when line.StartsWith("![") ->
        let image = parseImage line
        { state with MarkdownContent = state.MarkdownContent @ [ image ] }
    | line, _, _ when line.StartsWith("- ") ->
        { state with MarkdownContent = state.MarkdownContent @ [ ListItem(line.TrimStart('-').Trim()) ] }
    | line, _, _ -> { state with MarkdownContent = state.MarkdownContent @ [ PlainText line ] }

let private elementToHtml =
    function
    | MetaMarker -> ""
    | MetaContent _ -> ""
    | CodeBlockMarker -> ""
    | CodeContent content -> System.Web.HttpUtility.HtmlEncode(content) + System.Environment.NewLine
    | Image(alt, path) -> $"""<img src="{path}" alt="{alt}"/><br />"""
    | ListItem content -> $"<li>{processText content}</li>"
    | Header(level, content) -> $"<h{level}>{content}</h{level}>"
    | PlainText content -> $"{processText content}<br />"

let convertMarkdownToHtml (markdown: string[]) : HtmlPage =

    let state = { Meta = Map.empty; HtmlContent = []; IsInMeta = false; IsInCode = false }
    let parsingState = { IsInMeta = false; IsInCode = false; MarkdownContent = [] }

    let processMetaContent (state: ConversionState) (key: string) (value: string) =
        let newMeta =
            match key.ToLower() with
            | "tags" ->
                let tags = value.Split(',') |> Array.map (fun t -> t.Trim()) |> String.concat ", "
                state.Meta.Add(key, tags)
            | _ -> state.Meta.Add(key, value)

        { state with Meta = newMeta }

    let folder (state: ConversionState) (element: MarkdownElement) =
        match element, state.IsInMeta, state.IsInCode with
        | MetaMarker, _, _ -> { state with IsInMeta = not state.IsInMeta }
        | MetaContent(key, value), true, _ -> processMetaContent state key value
        | CodeBlockMarker, _, _ ->
            let html =
                if state.IsInCode then
                    $"{Environment.NewLine}</code></pre>"
                else
                    $"{Environment.NewLine}<pre><code>"

            { state with
                IsInCode = not state.IsInCode
                HtmlContent = Environment.NewLine :: html :: state.HtmlContent }
        | CodeContent(content), _, true ->
            { state with HtmlContent = (elementToHtml (CodeContent content)) :: state.HtmlContent }
        | _, false, false -> { state with HtmlContent = (elementToHtml element) :: state.HtmlContent }
        | _ -> state

    let finalState =
        markdown
        |> Array.fold parseLine parsingState
        |> fun x -> x.MarkdownContent |> List.toArray |> Array.fold folder state

    let meta =
        { Title = Map.tryFind "title" finalState.Meta |> Option.defaultValue ""
          Date = Map.tryFind "date" finalState.Meta |> Option.defaultValue ""
          Path = Map.tryFind "path" finalState.Meta |> Option.defaultValue ""
          Tags =
            Map.tryFind "tags" finalState.Meta
            |> Option.map (fun t -> t.Split(',') |> Array.map (fun s -> s.Trim()) |> Array.toList)
            |> Option.defaultValue []
          Url = Map.tryFind "url" finalState.Meta |> Option.defaultValue "" }

    { Meta = meta; HtmlContent = finalState.HtmlContent |> List.rev |> String.concat "" }


// todo: code block doesnt work correctly
// todo: bold text doesnt work correctly
// todo: each sentence starts with a new line (wrong behavior)
// todo: minor. code starts from space symbol
// todo: a link doesn't work, despite the fact they look correct
