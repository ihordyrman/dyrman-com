module MdConverter

open System.Collections.Generic
open System.Text

// Convert a markdown string to HTML
// map the markdown string to a string of HTML
// 1. # -> <h1>
// 2. ## -> <h2>
// 3. * -> <li>
// 4. ** -> <strong>
// 5. _ -> <em>
// 6. [text](url) -> <a href="url">text</a>
// 7. ![alt](url) -> <img src="url" alt="alt">
// 8. > -> <blockquote>
// 9. ``` -> <pre>
// 10. ` -> <code>
// 11. --- -> <hr>

// active pattern
let private (|Header|_|) (str: string) =
    match str with
    | s when s.StartsWith("#") -> Some s
    | _ -> None

let private (|ListItem|_|) (str: string) =
    match str with
    | text when text.StartsWith("- ") -> Some text
    | _ -> None

let private (|Meta|_|) (str: string) =
    match str with
    | text when text.StartsWith("---") -> Some text
    | _ -> None

let private (|Image|_|) (str: string) =
    match str with
    | text when text.StartsWith("![") -> Some text
    | _ -> None

let private appendHeader (sb: StringBuilder) (markdown: string) =
        let count =
            markdown |> Seq.takeWhile (fun c -> c = '#') |> Seq.length |> (fun x -> if x > 6 then 6 else x)

        sb.Append $"<h{count}>%s{markdown.TrimStart('#').Trim()}</h{count}>" |> ignore

let private appendListItem (sb: StringBuilder) (markdown: string) =
    sb.Append $"<li>%s{markdown.TrimStart('*').Trim()}</li>" |> ignore

let private appendRegularText (sb: StringBuilder) (markdown: string) =
    sb.Append(markdown) |> ignore

let private appendImage (sb: StringBuilder) (markdown: string) =
        sb.Append $"<img src=\"%s{markdown.TrimStart('!').Trim()}\" alt=\"%s{markdown.TrimStart('!').Trim()}\" />"
        |> ignore

let private extractMeta (markdown: string) (dic: Dictionary<string, string>) =
    let meta = markdown.Split(":")
    let key = meta.[0].Trim()
    let value = meta.[1].Trim()
    dic.Add(key, value)

let convertMarkdownToHtml (markdown: string[]) : string =
    let sb = StringBuilder()
    let dic = Dictionary<string, string>()
    let mutable metaArea = false

    markdown
    |> Array.iter (fun markdown ->
        match markdown with
        | Meta _ -> metaArea <- not metaArea
        | meta when metaArea -> extractMeta meta dic
        | Image image -> appendImage sb image
        | ListItem li -> appendListItem sb li
        | Header header -> appendHeader sb header
        | _ -> appendRegularText sb markdown)

    sb.ToString()
