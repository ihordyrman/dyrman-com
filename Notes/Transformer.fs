module Notes.Transformer

open System
open Notes.Tokenizer

type HtmlElement =
    | Header of level: int * content: string // +
    | Bold of content: string // +
    | Code of content: string // +
    | CodeBlock of language: string option * content: string // +
    | List of items: string list // +
    | Link of text: string * url: string // +
    | Image of alt: string * url: string // -
    | Text of content: string // +
    | LineBreak // ?
    | ParagraphBreak // ?

type private State = { Elements: HtmlElement list; Tokens: MarkdownToken list; Active: HtmlElement option }

let private newLine = Environment.NewLine

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
        | NewLine :: rest -> extract (code + newLine) rest
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

    let alt, leftover = extract "" tokens
    let url, leftover = extract "" leftover

    Link(alt, url), leftover

let transform (tokens: MarkdownToken list) : HtmlElement list =

    let initialState = { Elements = []; Tokens = tokens; Active = None }

    // might need active patterns here to hide a mess
    let rec getElement (state: State) =
        match state.Tokens, state.Active with
        | [], _ -> state
        | BoldMarker :: rest, None -> getElement { state with Tokens = rest; Active = Some(Bold "") }
        | BoldMarker :: rest, Some(Bold _) -> getElement { state with Tokens = rest; Active = None }
        | Symbol _ :: _, Some(Bold _) ->
            let text, leftover = extractText state.Tokens
            getElement { state with Tokens = leftover; Elements = state.Elements @ [ Bold text ] }
        | HeaderMarker level :: rest, None -> getElement { state with Tokens = rest; Active = Some(Header(level, "")) }
        | Symbol _ :: _, Some(Header(level, _)) ->
            let text, leftover = extractText state.Tokens

            getElement { Tokens = leftover; Elements = state.Elements @ [ Header(level, text) ]; Active = None }
        | CodeBlockMarker lang :: NewLine :: rest, None -> getElement { state with Tokens = rest; Active = Some(CodeBlock(lang, "")) }
        | CodeBlockMarker _ :: rest, Some(CodeBlock(lang, content)) ->
            getElement { Tokens = rest; Elements = state.Elements @ [ CodeBlock(lang, content) ]; Active = None }
        | Symbol _ :: _, Some(CodeBlock(lang, _)) ->
            let codeBlock, leftover = extractCodeBlock state.Tokens lang
            getElement { state with Tokens = leftover; Active = Some(codeBlock) }
        | CodeMarker :: rest, None -> getElement { state with Tokens = rest; Active = Some(Code "") }
        | CodeMarker :: rest, Some(Code _) -> getElement { state with Tokens = rest; Active = None }
        | Symbol _ :: _, Some(Code _) ->
            let text, leftover = extractText state.Tokens
            getElement { state with Tokens = leftover; Elements = state.Elements @ [ Code text ] }
        | ListMarker :: rest, None ->
            let list, leftover = extractList rest
            getElement { state with Tokens = leftover; Elements = state.Elements @ [ list ] }
        | ImageMarker :: rest, None ->
            let link, leftover = extractLink rest
            getElement { state with Tokens = leftover; Elements = state.Elements @ [ link ] }
        | Symbol _ :: _, None ->
            let text, leftover = extractText state.Tokens
            getElement { state with Tokens = leftover; Elements = state.Elements @ [ Text text ] }
        | NewLine :: rest, None -> getElement { Tokens = rest; Elements = state.Elements @ [ LineBreak ]; Active = None }
        | _ -> failwithf "wrong tokens sequence"

    let finalState = getElement initialState
    finalState.Elements
