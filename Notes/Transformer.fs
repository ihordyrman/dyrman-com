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
                let bold = Bold text
                processTokens { state with Tokens = finalRemaining; Elements = state.Elements @ [ bold ] }
            | _ -> processTokens { state with Tokens = state.Tokens.Tail }
        | HeaderMarker level :: rest ->
            let text, remaining = extractText rest
            let header = Header(level, text)
            processTokens { state with Tokens = remaining; Elements = state.Elements @ [ header ] }
        | CodeBlockMarker language :: NewLine :: rest ->
            let content, remaining = extractCodeBlock rest language
            let codeBlock = CodeBlock(language, content)
            processTokens { state with Tokens = remaining; Elements = state.Elements @ [ codeBlock ] }
        | CodeMarker :: rest ->
            let text, remaining = extractText rest

            match remaining with
            | CodeMarker :: finalRemaining ->
                let code = Code text
                processTokens { state with Tokens = finalRemaining; Elements = state.Elements @ [ code ] }
            | _ -> processTokens { state with Tokens = state.Tokens.Tail }
        | ListMarker :: rest ->
            let items, remaining = extractList rest
            let list = List items
            processTokens { state with Tokens = remaining; Elements = state.Elements @ [ list ] }
        | ImageMarker :: rest ->
            let (alt, url), remaining = extractImage rest
            let image = Image(alt, url)
            processTokens { state with Tokens = remaining; Elements = state.Elements @ [ image ] }
        | LocalImageStart :: rest ->
            let url, remaining = extractLocalImage rest
            let image = Image("", url)
            processTokens { state with Tokens = remaining; Elements = state.Elements @ [ image ] }
        | SquareBracketOpen :: rest ->
            let (text, url), remaining = extractLink rest
            let link = Link(text, url)
            processTokens { state with Tokens = remaining; Elements = state.Elements @ [ link ] }
        | Symbol _ :: _ ->
            let text, remaining = extractText state.Tokens
            let textElement = Text text
            processTokens { state with Tokens = remaining; Elements = state.Elements @ [ textElement ] }
        | NewLine :: rest ->
            let lineBreak = LineBreak
            processTokens { state with Tokens = rest; Elements = state.Elements @ [ lineBreak ] }
        | _ :: rest -> processTokens { state with Tokens = rest }

    let finalState = processTokens initialState
    finalState.Elements
