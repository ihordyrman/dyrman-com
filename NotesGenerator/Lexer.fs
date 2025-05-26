module NotesGenerator.Lexer

open System

type MarkdownTokens =
    | Text of string
    | MetaMarker // ---

    // formatting
    | NewLine
    | BoldMarker // **
    | HeaderMarker of int // #, ##, ### ...
    | ListMarker // -

    // code
    | CodeBlockMarker of string option // ``` or ```yaml
    | CodeMarker // `

    // images
    | ImageMarker // ![
    | LocalImageStart // ![[
    | LocalImageEnd // ]]

    // links
    | SquareBracketOpen // [
    | SquareBracketClose // ]
    | ParenOpen // (
    | ParenClose // )

type State = { Tokens: MarkdownTokens list; CurrentText: string }

let (|Headers|_|) (line: string) =
    if line.StartsWith '#' then
        let count =
            line.Trim() |> fun x -> x.ToCharArray() |> Array.takeWhile ((=) '#') |> Array.length |> min 6

        let rest = line.Substring count
        Some(count, rest)
    else
        None

let (|Meta|_|) (line: string) =
    if line.StartsWith "---" then
        let rest = line.Substring 3
        Some rest
    else
        None

let (|List|_|) (line: string) =
    if line.StartsWith "- " then
        let rest = line.Substring 2
        Some rest
    else
        None

let (|CodeBlock|_|) (line: string) =
    if line.StartsWith "```" then
        let lang = line.Substring(3).Trim()

        if String.IsNullOrEmpty lang then Some(None) else Some(Some lang)
    else
        None

let (|Bold|_|) (text: string) =
    if text.StartsWith "**" then
        let rest = text.Substring(2)
        Some(rest)
    else
        None

let (|Code|_|) (line: string) =
    if line.StartsWith "`" then
        let rest = line.Substring(1)
        Some(rest)
    else
        None

let tokenize (line: string) : MarkdownTokens list =

    let cloneState state text marker = { CurrentText = text; Tokens = state.Tokens @ [ marker ] }
    let initialState = { Tokens = []; CurrentText = line }

    // check elements on the beginning
    let state =
        match line with
        | Meta(rest) -> (cloneState initialState rest MetaMarker)
        | List(rest) -> (cloneState initialState rest ListMarker)
        | Headers(level, rest) -> (cloneState initialState rest (HeaderMarker level))
        | CodeBlock(lang) -> (cloneState initialState "" (CodeBlockMarker lang))
        | _ -> initialState

    // check the rest of the line
    let rec getTokens (state: State) =
        match state.CurrentText with
        | "" -> state
        | Bold(rest) -> getTokens (cloneState state rest BoldMarker)
        | Code(rest) -> getTokens (cloneState state rest CodeMarker)
        | text ->
            let rest = text.Substring 1
            let character = text[..0]
            getTokens (cloneState state rest (Text character))

    let finalState = getTokens state
    finalState.Tokens @ [NewLine]
    