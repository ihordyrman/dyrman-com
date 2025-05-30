module NotesGenerator.Parser

open NotesGenerator.Lexer

type HtmlElement =
    | Header of level: int * content: string
    | Paragraph of content: HtmlElement list
    | Bold of content: string
    | Code of content: string
    | CodeBlock of language: string option * content: string
    | List of items: HtmlElement list list
    | Link of text: string * url: string
    | Image of alt: string * url: string
    | Text of content: string
    | LineBreak
    | ParagraphBreak

type private State = { Elements: HtmlElement list; Tokens: MarkdownToken list; Active: HtmlElement option }

let parse (tokens: MarkdownToken list) : HtmlElement list =

    let initialState = { Elements = []; Tokens = tokens; Active = None }

    let rec extractText acc tokens =
        match tokens with
        | MarkdownToken.Text text :: rest -> extractText (acc + text) rest
        | _ -> acc, tokens

    // looks like we need active patterns here
    let rec getElement (state: State) =
        match state.Tokens, state.Active with
        | [], _ -> state
        | BoldMarker :: rest, None -> getElement { state with Tokens = rest; Active = Some(Bold "") }
        | BoldMarker :: rest, Some(Bold _) -> getElement { state with Tokens = rest; Active = None }
        | MarkdownToken.Text _ :: _, Some(Bold _) ->
            let text, leftover = extractText "" state.Tokens
            getElement { state with Tokens = leftover; Elements = state.Elements @ [ Bold(text) ] }
        | _ -> failwithf "wrong tokens sequence"

    let finalState = getElement initialState
    finalState.Elements
