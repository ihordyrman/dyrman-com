module NotesGenerator.Parser

open NotesGenerator.Lexer

type HtmlElement =
    | Header of level: int * content: HtmlElement list
    | Paragraph of content: HtmlElement list
    | Bold of content: HtmlElement list
    | Code of content: string
    | CodeBlock of language: string option * content: string
    | List of items: HtmlElement list list
    | Link of text: HtmlElement list * url: string
    | Image of alt: string * url: string
    | Text of content: string
    | LineBreak
    | ParagraphBreak

type ActiveElement =
    | None
    | InBold of content: HtmlElement list

type State = { Elements: HtmlElement list; Tokens: MarkdownToken list; Active: ActiveElement }

let parse (tokens: MarkdownToken list) : HtmlElement list =

    let initialState = { Elements = []; Tokens = tokens; Active = None }

    let rec getElement (state: State) =
        match state.Tokens, state.Active with
        | BoldMarker :: rest, None -> { state with Tokens = rest; Active = InBold [] }
        | BoldMarker :: rest, InBold content ->
            let boldElement = Bold(List.rev content)
            { state with Tokens = rest; Active = None; Elements = boldElement :: state.Elements }
        | _ -> state

    let finalState = getElement initialState
    finalState.Elements
