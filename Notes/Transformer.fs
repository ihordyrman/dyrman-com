module Notes.Transformer

open System
open Notes.Tokenizer

type HtmlElement =
    | Header of level: int * content: string // +
    | Paragraph of content: HtmlElement list // -
    | Bold of content: string // +
    | Code of content: string // +
    | CodeBlock of language: string option * content: string // +
    | List of items: string list // +
    | Link of text: string * url: string // -
    | Image of alt: string * url: string // -
    | Text of content: string // +
    | LineBreak // ?
    | ParagraphBreak // ?

type private State = { Elements: HtmlElement list; Tokens: MarkdownToken list; Active: HtmlElement option }

let private newLine = Environment.NewLine

let transform (tokens: MarkdownToken list) : HtmlElement list =

    let initialState = { Elements = []; Tokens = tokens; Active = None }

    let extractText tokens =
        let rec extract acc tokens =
            match tokens with
            | Symbol text :: rest -> extract (acc + text.ToString()) rest
            | _ -> acc, tokens

        extract "" tokens

    let extractCodeBlock tokens lang =
        let rec extract acc tokens =
            match tokens with
            | Symbol text :: rest -> extract (acc + text.ToString()) rest
            | NewLine :: rest -> extract (acc + newLine) rest
            | _ -> acc, tokens

        let content, tokens = extract "" tokens
        CodeBlock(lang, content), tokens

    let extractList tokens =
        let rec extract (acc: string list) tokens =
            match tokens with
            | Symbol text :: rest ->
                let result =
                    match List.rev acc with
                    | [] -> [ text.ToString() ]
                    | last :: rest -> List.rev rest @ [ last + text.ToString() ]

                result, rest
            | NewLine :: ListMarker :: rets -> acc @ [ "" ], rets
            | _ -> acc, tokens

        let content, tokens = extract [] tokens
        List content, tokens

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
        | ListMarker :: _, None ->
            let list, leftover = extractList state.Tokens
            getElement { state with Tokens = leftover; Elements = state.Elements @ [ list ] }
        | Symbol _ :: _, None ->
            let text, leftover = extractText state.Tokens
            getElement { state with Tokens = leftover; Elements = state.Elements @ [ Text text ] }
        | NewLine :: rest, None -> getElement { Tokens = rest; Elements = state.Elements @ [ LineBreak ]; Active = None }
        | _ -> failwithf "wrong tokens sequence"

    let finalState = getElement initialState
    finalState.Elements
