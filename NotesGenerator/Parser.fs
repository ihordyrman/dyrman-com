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

type State = { Elements: HtmlElement list; Tokens: MarkdownToken list}

let parse (tokens: MarkdownToken list) : HtmlElement list =
    
    let initialState = { Elements = []; Tokens = tokens }
    
    // todo: add implementation
    let rec getElement (state: State) =
        match state.Tokens with
        | [] -> state
        | NewLine :: rest -> state
        | _ -> state

    []
