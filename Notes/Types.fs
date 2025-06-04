namespace Notes.Types

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
