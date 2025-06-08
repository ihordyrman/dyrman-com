module Notes.Transformer

open System
open Notes.Tokenizer

type HtmlElement =
    | Header of level: int * content: string
    | Bold of content: string
    | Code of content: string
    | CodeBlock of language: string option * content: string
    | List of items: string list
    | Link of text: string * url: string
    | Image of alt: string * url: string
    | Text of content: string
    | LineBreak

type private State = { Elements: HtmlElement list; Tokens: MarkdownToken list; Meta: Map<string, string> }

let private extractText tokens =
    let rec extract text tokens =
        match tokens with
        | Symbol symbol :: rest -> extract (text + symbol.ToString()) rest
        | _ -> text, tokens

    extract "" tokens

let private extractCodeBlock tokens lang =
    let rec extract code tokens =
        match tokens with
        | Symbol text :: rest -> extract (code + text.ToString()) rest
        | NewLine :: rest -> extract (code + Environment.NewLine) rest
        | CodeBlockMarker _ :: rest -> code, rest
        | _ -> code, tokens

    let content, tokens = extract "" tokens
    CodeBlock(lang, content), tokens

let private extractList tokens =
    let rec extract content tokens =
        match tokens with
        | Symbol text :: rest ->
            let result =
                match List.rev content with
                | [] -> [ text.ToString() ]
                | last :: rest -> List.rev rest @ [ last + text.ToString() ]

            extract result rest
        | NewLine :: ListMarker :: rets -> extract (content @ [ "" ]) rets
        | _ -> content, tokens

    let content, tokens = extract [] tokens
    List content, tokens

let private extractLink tokens =
    let rec extract acc tokens =
        match tokens with
        | Symbol text :: rest -> extract (acc + text.ToString()) rest
        | SquareBracketClose :: ParenOpen :: rest -> acc, rest
        | ParenClose :: rest -> acc, rest
        | _ -> acc, tokens

    let text, rest = extract "" tokens
    let url, rest = extract "" rest
    Link(text, url), rest

let private extractImage tokens =
    let rec extract acc tokens =
        match tokens with
        | Symbol text :: rest -> extract (acc + text.ToString()) rest
        | SquareBracketClose :: ParenOpen :: rest -> acc, rest
        | ParenClose :: rest -> acc, rest
        | _ -> acc, tokens

    let alt, rest = extract "" tokens
    let url, rest = extract "" rest
    Image(alt, url), rest

let private extractLocalImage tokens =
    let rec extract acc tokens =
        match tokens with
        | Symbol text :: rest -> extract (acc + text.ToString()) rest
        | LocalImageEnd :: rest -> acc, rest
        | _ -> acc, tokens

    let url, rest = extract "" tokens
    Image("", url), rest

let private extractMeta tokens =
    let rec extract acc meta tokens =
        match tokens with
        | Symbol text :: rest -> extract (acc + text.ToString()) meta rest
        | NewLine :: rest ->
            let index = acc.IndexOf ':'
            let key = acc.Substring(0, index).Trim()
            let value = acc.Substring(index + 1).Trim()
            let meta = Map.add key value meta
            "", meta, rest
        | MetaMarker :: rest -> acc, meta, rest
        | _ -> acc, meta, tokens

    let _, meta, rest = extract "", Map.empty, tokens
    meta, rest

let transform tokens =
    let initialState = { Elements = []; Tokens = tokens; Meta = Map.empty }

    let rec getElement state =
        match state.Tokens with
        | [] -> state
        | MetaMarker :: rest ->
            let meta, rest = extractMeta rest
            getElement { state with Tokens = rest; Meta = meta }
        | BoldMarker :: rest ->
            let text, rest = extractText rest
            let bold = Bold text
            getElement { state with Tokens = rest.Tail; Elements = state.Elements @ [ bold ] }
        | HeaderMarker level :: rest ->
            let text, rest = extractText rest
            getElement { state with Tokens = rest; Elements = state.Elements @ [ Header(level, text) ] }
        | CodeBlockMarker lang :: NewLine :: rest ->
            let codeBlock, rest = extractCodeBlock rest lang
            getElement { state with Tokens = rest; Elements = state.Elements @ [ codeBlock ] }
        | CodeMarker :: rest ->
            let text, rest = extractText rest
            let code = Code text
            getElement { state with Tokens = rest.Tail; Elements = state.Elements @ [ code ] }
        | ListMarker :: rest ->
            let list, rest = extractList rest
            getElement { state with Tokens = rest; Elements = state.Elements @ [ list ] }
        | ImageMarker :: rest ->
            let image, rest = extractImage rest
            getElement { state with Tokens = rest; Elements = state.Elements @ [ image ] }
        | LocalImageStart :: rest ->
            let image, rest = extractLocalImage rest
            getElement { state with Tokens = rest; Elements = state.Elements @ [ image ] }
        | SquareBracketOpen :: rest ->
            let link, rest = extractLink rest
            getElement { state with Tokens = rest; Elements = state.Elements @ [ link ] }
        | Symbol _ :: _ ->
            let text, rest = extractText state.Tokens
            getElement { state with Tokens = rest; Elements = state.Elements @ [ Text text ] }
        | NewLine :: rest -> getElement { state with Tokens = rest; Elements = state.Elements @ [ LineBreak ] }
        | _ -> failwithf "wrong tokens sequence"

    let finalState = getElement initialState
    finalState.Elements
