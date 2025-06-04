module Notes.Tokenizer

open System

type MarkdownToken =
    | Symbol of ch: char
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

let private (|CodeBlock|_|) (line: string) =
    if line.StartsWith "```" then
        let lang = line.Substring(3).Trim()

        if String.IsNullOrEmpty lang then Some(None) else Some(Some lang)
    else
        None

let private (|Meta|_|) (line: string) = if line.StartsWith "---" then Some(line.Substring 3) else None

let private (|List|_|) (line: string) = if line.StartsWith "- " then Some(line.Substring 2) else None

let private (|Bold|_|) (text: string) = if text.StartsWith "**" then Some(text.Substring 2) else None

let private (|Code|_|) (line: string) = if line.StartsWith "`" then Some(line.Substring 1) else None

let private (|Image|_|) (text: string) = if text.StartsWith "![" then Some(text.Substring 2) else None

let private (|LocalImageStart|_|) (text: string) = if text.StartsWith "![[" then Some(text.Substring 3) else None

let private (|LocalImageEnd|_|) (text: string) = if text.StartsWith "]]" then Some(text.Substring 2) else None

let private (|SquareBracketOpen|_|) (text: string) = if text.StartsWith "[" then Some(text.Substring 1) else None

let private (|SquareBracketClose|_|) (text: string) = if text.StartsWith "]" then Some(text.Substring 1) else None

let private (|ParenOpen|_|) (text: string) = if text.StartsWith "(" then Some(text.Substring 1) else None

let private (|ParenClose|_|) (text: string) = if text.StartsWith ")" then Some(text.Substring 1) else None

let tokenize line =

    let cloneState state text marker = { CurrentText = text; Tokens = state.Tokens @ [ marker ] }
    let emptyState = { Tokens = []; CurrentText = line }

    let initialState =
        match line with
        | Meta rest -> cloneState emptyState rest MetaMarker
        | List rest -> cloneState emptyState rest ListMarker
        | Headers(level, rest) -> cloneState emptyState rest (HeaderMarker level)
        | CodeBlock lang -> cloneState emptyState "" (CodeBlockMarker lang)
        | LocalImageStart rest -> cloneState emptyState rest LocalImageStart
        | Image rest -> cloneState emptyState rest ImageMarker
        | _ -> emptyState

    let rec getTokens state =
        match state.CurrentText with
        | "" -> state
        | Bold rest -> getTokens (cloneState state rest BoldMarker)
        | Code rest -> getTokens (cloneState state rest CodeMarker)
        | LocalImageStart rest -> getTokens (cloneState state rest LocalImageStart)
        | Image rest -> getTokens (cloneState state rest ImageMarker)
        | LocalImageEnd rest -> getTokens (cloneState state rest LocalImageEnd)
        | SquareBracketOpen rest -> getTokens (cloneState state rest SquareBracketOpen)
        | SquareBracketClose rest -> getTokens (cloneState state rest SquareBracketClose)
        | ParenOpen rest -> getTokens (cloneState state rest ParenOpen)
        | ParenClose rest -> getTokens (cloneState state rest ParenClose)
        | text ->
            let rest = text.Substring 1
            let character = text.Substring(0, 1)
            getTokens (cloneState state rest (Symbol character[0]))

    let finalState = getTokens initialState
    finalState.Tokens @ [ NewLine ]
