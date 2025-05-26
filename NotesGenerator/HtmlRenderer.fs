module HtmlRenderer

open System
open NotesGenerator.Types

let empty = String.Empty

let private appendToken token state = { state with Tokens = token :: state.Tokens; CurrentText = empty }
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
        | Text text -> text
        | Bold text -> $"<b>{text}</b>"
        | Code text -> text |> System.Web.HttpUtility.HtmlEncode |> (fun code -> $"<code>{code}</code>")
        | Link(text, url) -> $"<a href=\"{url}\">{text}</a>"

    tokens |> List.rev |> List.map convertToken |> String.concat ""

let state =
    { Tokens = []
      CurrentText = empty
      IsBold = false
      IsCode = false
      IsLink = false
      LinkText = None }

let rec private parseText (text: char list) (state: ProcessingState) =
    match text with
    | [] ->
        match state with
        | s when s.CurrentText.Length > 0 -> appendToken (Text state.CurrentText) state
        | _ -> state

    | '*' :: '*' :: rest when not state.IsBold && not state.IsCode ->
        let newState =
            match state with
            // save current text before processing bold
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

    | '`' :: rest when not state.IsCode ->
        let newState =
            match state with
            | s when s.CurrentText.Length > 0 -> appendToken (Text state.CurrentText) state
            | _ -> state

        parseText rest { newState with IsCode = true }

    | '`' :: rest when state.IsCode ->
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
    |> _.Tokens
    |> tokensToHtml

let private parseMetaLine (line: string) =
    let index = line.IndexOf ':'
    let key = line.Substring(0, index).Trim()
    let value = line.Substring(index + 1).Trim()
    MetaContent(key, value)

let private parseHeader (line: string) =
    let headerLevel = line |> Seq.takeWhile ((=) '#') |> Seq.length |> min 6

    let content = line.TrimStart('#').Trim()
    Header(headerLevel, content)

let private parseImage (line: string) =
    let content = line.Trim() |> _.TrimStart('!', '[').TrimEnd(']')

    Image(content, $"./Images/{content}")

let private parse (state: ParsingState) (line: string) : ParsingState =
    match (line.TrimEnd(), state.IsInCode, state.IsInMeta) with
    | "---", _, false -> { IsInMeta = true; IsInCode = false; MarkdownContent = state.MarkdownContent @ [ MetaMarker ] }
    | "---", _, true -> { IsInMeta = false; IsInCode = false; MarkdownContent = state.MarkdownContent @ [ MetaMarker ] }
    | _, _, true -> { state with MarkdownContent = state.MarkdownContent @ [ parseMetaLine line ] }
    | line, _, _ when line.StartsWith "```" ->
        { IsInCode = not state.IsInCode
          IsInMeta = false
          MarkdownContent = state.MarkdownContent @ [ CodeBlockMarker ] }
    | line, true, _ ->
        let encodedLine = System.Web.HttpUtility.HtmlEncode line
        { state with MarkdownContent = state.MarkdownContent @ [ CodeContent encodedLine ] }
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
    | MetaMarker -> empty
    | MetaContent _ -> empty
    | CodeBlockMarker -> empty
    | CodeContent content -> content + Environment.NewLine
    | Image(alt, path) -> $"""<img src="{path}" alt="{alt}"/><br />"""
    | ListItem content -> $"<li>{processText content}</li>"
    | Header(level, content) -> $"<h{level}>{content}</h{level}>"
    | PlainText content -> $"{processText content}<br />{Environment.NewLine}"

let convertMarkdownToHtml (markdown: string[]) : HtmlPage =

    let convertingState = { Meta = Map.empty; HtmlContent = []; IsInMeta = false; IsInCode = false }
    let parsingState = { IsInMeta = false; IsInCode = false; MarkdownContent = [] }

    let processMetaContent (state: ConversionState) (key: string) (value: string) =
        let newMeta =
            match key.ToLower() with
            | "tags" ->
                let tags = value.Split(',') |> Array.map _.Trim() |> String.concat ", "
                state.Meta.Add(key, tags)
            | _ -> state.Meta.Add(key, value)

        { state with Meta = newMeta }

    let convert (state: ConversionState) (element: MarkdownElement) =
        match element, state.IsInMeta, state.IsInCode with
        | MetaMarker, _, _ -> { state with IsInMeta = not state.IsInMeta }
        | MetaContent(key, value), true, _ -> processMetaContent state key value
        | CodeBlockMarker, _, _ ->
            let html = if state.IsInCode then "</code></pre>" else "<pre><code>"
            let newLine = if state.IsInCode then $"<br />{Environment.NewLine}" else empty

            { state with IsInCode = not state.IsInCode; HtmlContent = newLine :: html :: state.HtmlContent }
        | CodeContent(content), _, true -> { state with HtmlContent = (elementToHtml (CodeContent content)) :: state.HtmlContent }
        | _, false, false -> { state with HtmlContent = (elementToHtml element) :: state.HtmlContent }
        | _ -> state

    let finalState =
        markdown
        |> Array.fold parse parsingState
        |> fun x -> x.MarkdownContent |> List.fold convert convertingState

    let meta =
        { Title = Map.tryFind "title" finalState.Meta |> Option.defaultValue empty
          Date = Map.tryFind "date" finalState.Meta |> Option.defaultValue empty
          Path = Map.tryFind "path" finalState.Meta |> Option.defaultValue empty
          Tags =
            Map.tryFind "tags" finalState.Meta
            |> Option.map (fun t -> t.Split(',') |> Array.map _.Trim() |> Array.toList)
            |> Option.defaultValue []
          Url = Map.tryFind "url" finalState.Meta |> Option.defaultValue empty }

    { Meta = meta; HtmlContent = finalState.HtmlContent |> List.rev |> String.concat empty }

// Tokenize -> Parse -> Transform