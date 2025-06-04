module Notes.Parser

open Notes.Tokenizer

type HtmlElement =
    | Header of level: int * content: string // +
    | Paragraph of content: HtmlElement list // -
    | Bold of content: string // +
    | Code of content: string // +
    | CodeBlock of language: string option * content: string // -
    | List of items: HtmlElement list list // -
    | Link of text: string * url: string // -
    | Image of alt: string * url: string // -
    | Text of content: string // +
    | LineBreak // ?
    | ParagraphBreak // ?

type private State = { Elements: HtmlElement list; Tokens: MarkdownToken list; Active: HtmlElement option }

let parse (tokens: MarkdownToken list) : HtmlElement list =

    let initialState = { Elements = []; Tokens = tokens; Active = None }

    let extractText tokens =
        let rec extract acc tokens =
            match tokens with
            | MarkdownToken.Symbol text :: rest -> extract (acc + text.ToString()) rest
            | _ -> acc, tokens

        extract "" tokens

    // we might need active patterns here to hide a mess
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

            getElement { state with Tokens = leftover; Elements = state.Elements @ [ Header(level, text) ]; Active = None }
        | CodeMarker :: rest, None -> getElement { state with Tokens = rest; Active = Some(Code "") }
        | Symbol _ :: _, Some(Code _) ->
            let text, leftover = extractText state.Tokens
            getElement { state with Tokens = leftover; Elements = state.Elements @ [ Code text ]; Active = None }
        | Symbol _ :: _, None ->
            let text, leftover = extractText state.Tokens
            getElement { state with Tokens = leftover; Elements = state.Elements @ [ Text text ] }
        | NewLine :: rest, None -> getElement { state with Tokens = rest; Elements = state.Elements @ [ LineBreak ]; Active = None }
        | _ -> failwithf "wrong tokens sequence"

    let finalState = getElement initialState
    finalState.Elements
