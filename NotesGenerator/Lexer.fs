module NotesGenerator.Lexer

open System

type MarkdownToken =
    | Text of string
    | MetaMarker
    | NewLine
    | BoldMarker
    | HeaderMarker of level: int
    | ListMarker
    | CodeBlockMarker of language: string option
    | CodeMarker
    | ImageMarker
    | LocalImageStart
    | LocalImageEnd
    | SquareBracketOpen
    | SquareBracketClose
    | ParenOpen
    | ParenClose

type private State = { Tokens: MarkdownToken list; CurrentText: string }

let private (|Headers|_|) (line: string) =
    if line.StartsWith '#' then
        let count =
            line.Trim() |> fun x -> x.ToCharArray() |> Array.takeWhile ((=) '#') |> Array.length |> min 6

        let rest = line.Substring count
        Some(count, rest)
    else
        None

let private (|Meta|_|) (line: string) =
    if line.StartsWith "---" then
        let rest = line.Substring 3
        Some rest
    else
        None

let private (|List|_|) (line: string) =
    if line.StartsWith "- " then
        let rest = line.Substring 2
        Some rest
    else
        None

let private (|CodeBlock|_|) (line: string) =
    if line.StartsWith "```" then
        let lang = line.Substring(3).Trim()

        if String.IsNullOrEmpty lang then Some(None) else Some(Some lang)
    else
        None

let private (|Bold|_|) (text: string) =
    if text.StartsWith "**" then
        let rest = text.Substring 2
        Some(rest)
    else
        None

let private (|Code|_|) (line: string) =
    if line.StartsWith "`" then
        let rest = line.Substring 1
        Some(rest)
    else
        None

let private (|Image|_|) (text: string) =
    if text.StartsWith "![" then
        let rest = text.Substring 2
        Some(rest)
    else
        None

let private (|LocalImageStart|_|) (text: string) =
    if text.StartsWith "![[" then
        let rest = text.Substring 3
        Some(rest)
    else
        None

let private (|LocalImageEnd|_|) (text: string) =
    if text.StartsWith "]]" then
        let rest = text.Substring 2
        Some(rest)
    else
        None

let private (|SquareBracketOpen|_|) (text: string) =
    if text.StartsWith "[" then
        let rest = text.Substring 1
        Some(rest)
    else
        None

let private (|SquareBracketClose|_|) (text: string) =
    if text.StartsWith "]" then
        let rest = text.Substring 1
        Some(rest)
    else
        None

let private (|ParenOpen|_|) (text: string) =
    if text.StartsWith "(" then
        let rest = text.Substring 1
        Some(rest)
    else
        None

let private (|ParenClose|_|) (text: string) =
    if text.StartsWith ")" then
        let rest = text.Substring 1
        Some(rest)
    else
        None

let tokenize line =

    let cloneState state text marker = { CurrentText = text; Tokens = state.Tokens @ [ marker ] }
    let initialState = { Tokens = []; CurrentText = line }

    // check elements on the beginning
    let state =
        match line with
        | Meta(rest) -> cloneState initialState rest MetaMarker
        | List(rest) -> cloneState initialState rest ListMarker
        | Headers(level, rest) -> cloneState initialState rest (HeaderMarker level)
        | CodeBlock(lang) -> cloneState initialState "" (CodeBlockMarker lang)
        | LocalImageStart(rest) -> cloneState initialState rest LocalImageStart
        | Image(rest) -> cloneState initialState rest ImageMarker
        | _ -> initialState

    let rec getTokens (state: State) =
        match state.CurrentText with
        | "" -> state
        | Bold(rest) -> getTokens (cloneState state rest BoldMarker)
        | Code(rest) -> getTokens (cloneState state rest CodeMarker)
        | LocalImageStart(rest) -> getTokens (cloneState state rest LocalImageStart)
        | Image(rest) -> getTokens (cloneState state rest ImageMarker)
        | LocalImageEnd(rest) -> getTokens (cloneState state rest LocalImageEnd)
        | SquareBracketOpen(rest) -> getTokens (cloneState state rest SquareBracketOpen)
        | SquareBracketClose(rest) -> getTokens (cloneState state rest SquareBracketClose)
        | ParenOpen(rest) -> getTokens (cloneState state rest ParenOpen)
        | ParenClose(rest) -> getTokens (cloneState state rest ParenClose)
        | text ->
            let rest = text.Substring 1
            let character = text.Substring(0, 1)
            getTokens (cloneState state rest (Text character))

    // check the rest of the line
    let finalState = getTokens state
    finalState.Tokens @ [ NewLine ]
