module Notes.Transformer

open System
open Notes.Tokenizer

type HtmlElement =
    | Header of level: int * content: string
    | Bold of content: string
    | Code of content: string
    | CodeBlock of language: string option * content: string
    | List of items: string list
    | Link of text: string * url: string
    | Image of alt: string * url: string
    | Text of content: string
    | LineBreak

type private State = { Elements: HtmlElement list; Tokens: MarkdownToken list; Meta: Map<string, string> }

let private extractText tokens =
    let rec extract acc tokens =
        match tokens with
        | Symbol ch :: rest -> extract (acc + ch.ToString()) rest
        | _ -> acc, tokens

    extract "" tokens

let private extractCodeBlock tokens language =
    let rec extract content tokens =
        match tokens with
        | Symbol ch :: rest -> extract (content + ch.ToString()) rest
        | ParenOpen :: rest -> extract (content + "(") rest
        | ParenClose :: rest -> extract (content + ")") rest
        | SquareBracketOpen :: rest -> extract (content + "[") rest
        | SquareBracketClose :: rest -> extract (content + "]") rest
        | NewLine :: rest -> extract (content + Environment.NewLine) rest
        | CodeBlockMarker _ :: rest -> content, rest
        | _ -> content, tokens

    extract "" tokens

let private extractList tokens =
    let rec extract content tokens =
        match tokens with
        | Symbol ch :: rest ->
            let result =
                match List.rev content with
                | [] -> [ ch.ToString() ]
                | last :: others -> List.rev others @ [ last + ch.ToString() ]

            extract result rest
        | NewLine :: ListMarker :: rest -> extract (content @ [ "" ]) rest
        | _ -> content, tokens

    extract [] tokens

let private extractLink tokens =
    let rec extractText acc tokens =
        match tokens with
        | Symbol ch :: rest -> extractText (acc + ch.ToString()) rest
        | SquareBracketClose :: ParenOpen :: rest -> acc, rest
        | _ -> acc, tokens

    let rec extractUrl acc tokens =
        match tokens with
        | Symbol ch :: rest -> extractUrl (acc + ch.ToString()) rest
        | ParenClose :: rest -> acc, rest
        | _ -> acc, tokens

    let text, urlTokens = extractText "" tokens
    let url, remaining = extractUrl "" urlTokens
    (text, url), remaining

let private extractImage tokens =
    let rec extractAlt acc tokens =
        match tokens with
        | Symbol ch :: rest -> extractAlt (acc + ch.ToString()) rest
        | SquareBracketClose :: ParenOpen :: rest -> acc, rest
        | _ -> acc, tokens

    let rec extractUrl acc tokens =
        match tokens with
        | Symbol ch :: rest -> extractUrl (acc + ch.ToString()) rest
        | ParenClose :: rest -> acc, rest
        | _ -> acc, tokens

    let alt, urlTokens = extractAlt "" tokens
    let url, remaining = extractUrl "" urlTokens
    (alt, url), remaining

let private extractLocalImage tokens =
    let rec extract acc tokens =
        match tokens with
        | Symbol ch :: rest -> extract (acc + ch.ToString()) rest
        | LocalImageEnd :: rest -> acc, rest
        | _ -> acc, tokens

    extract "" tokens

let private extractMeta tokens =
    let rec extract acc meta tokens =
        match tokens with
        | Symbol ch :: rest -> extract (acc + ch.ToString()) meta rest
        | NewLine :: rest ->
            let colonIndex = acc.IndexOf ':'

            if colonIndex > 0 then
                let key = acc.Substring(0, colonIndex).Trim()
                let value = acc.Substring(colonIndex + 1).Trim()
                let updatedMeta = Map.add key value meta
                extract "" updatedMeta rest
            else
                extract "" meta rest
        | MetaMarker :: rest -> meta, rest
        | _ -> meta, tokens

    extract "" Map.empty tokens

let transform tokens =
    let initialState = { Elements = []; Tokens = tokens; Meta = Map.empty }

    let rec processTokens state =
        match state.Tokens with
        | [] -> state
        | MetaMarker :: rest ->
            let meta, remaining = extractMeta rest
            processTokens { state with Tokens = remaining; Meta = meta }
        | BoldMarker :: rest ->
            let text, remaining = extractText rest

            match remaining with
            | BoldMarker :: finalRemaining ->
                processTokens { state with Tokens = finalRemaining; Elements = state.Elements @ [ Bold text ] }
            | _ -> processTokens { state with Tokens = state.Tokens.Tail }
        | HeaderMarker level :: rest ->
            let text, remaining = extractText rest
            processTokens { state with Tokens = remaining; Elements = state.Elements @ [ Header(level, text) ] }
        | CodeBlockMarker language :: NewLine :: rest ->
            let content, remaining = extractCodeBlock rest language
            processTokens { state with Tokens = remaining; Elements = state.Elements @ [ CodeBlock(language, content) ] }
        | CodeMarker :: rest ->
            let text, remaining = extractText rest

            match remaining with
            | CodeMarker :: finalRemaining ->
                processTokens { state with Tokens = finalRemaining; Elements = state.Elements @ [ Code text ] }
            | _ -> processTokens { state with Tokens = state.Tokens.Tail }
        | ListMarker :: rest ->
            let items, remaining = extractList rest
            processTokens { state with Tokens = remaining; Elements = state.Elements @ [ List items ] }
        | ImageMarker :: rest ->
            let (alt, url), remaining = extractImage rest
            processTokens { state with Tokens = remaining; Elements = state.Elements @ [ Image(alt, url) ] }
        | LocalImageStart :: rest ->
            let url, remaining = extractLocalImage rest
            processTokens { state with Tokens = remaining; Elements = state.Elements @ [ Image("", url) ] }
        | SquareBracketOpen :: rest ->
            let (text, url), remaining = extractLink rest
            processTokens { state with Tokens = remaining; Elements = state.Elements @ [ Link(text, url) ] }
        | Symbol _ :: _ ->
            let text, remaining = extractText state.Tokens
            processTokens { state with Tokens = remaining; Elements = state.Elements @ [ Text text ] }
        | NewLine :: rest -> processTokens { state with Tokens = rest; Elements = state.Elements @ [ LineBreak ] }
        | _ :: rest -> processTokens { state with Tokens = rest }

    let finalState = processTokens initialState
    finalState.Elements, finalState.Meta
