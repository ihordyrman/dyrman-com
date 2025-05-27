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

let parse (tokens: MarkdownToken list) : HtmlElement list =

    []
