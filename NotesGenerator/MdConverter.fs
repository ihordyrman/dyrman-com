module MdConverter

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
let (|Header|) (str: string) =
    match str with
    | s when s.StartsWith("#") -> Some s
    | _ -> None

let (|ListItem|) (str: string) =
    match str with
    | s when s.StartsWith("- ") -> Some s
    | _ -> None

let (|Meta|) (str: string) =
    match str with
    | s when s.StartsWith("---") -> Some s
    | _ -> None

let (|Image|) (str: string) =
    match str with
    | s when s.StartsWith("![") -> Some s
    | _ -> None

let appendHeader (sb: StringBuilder) (markdown: string option) =
    match markdown with
    | Some markdown ->
        let count =
            markdown |> Seq.takeWhile (fun c -> c = '#') |> Seq.length |> (fun x -> if x > 6 then 6 else x)

        sb.Append $"<h{count}>%s{markdown.TrimStart('#').Trim()}</h{count}>"
    | None -> sb

let appendListItem (sb: StringBuilder) (markdown: string option) =
    match markdown with
    | Some markdown -> sb.Append $"<li>%s{markdown.TrimStart('*').Trim()}</li>"
    | None -> sb

let appendRegularText (sb: StringBuilder) (markdown: string option) =
    match markdown with
    | Some markdown -> sb.Append(markdown)
    | None -> sb

let appendImage (sb: StringBuilder) (markdown: string option) =
    match markdown with
    | Some markdown ->
        sb.Append $"<img src=\"%s{markdown.TrimStart('!').Trim()}\" alt=\"%s{markdown.TrimStart('!').Trim()}\" />"
    | None -> sb

let extractMeta (markdown: string option) =


    ()

let convertMarkdownToHtml (markdown: string[]) string =
    let sb = StringBuilder()

    let rec convert (markdown: string) =
        match markdown with
        | "" -> ()
        | _ ->
            let builder =
                match markdown with
                | Header header -> appendHeader sb header
                | ListItem li -> appendListItem sb li
                | Meta meta ->
                    extractMeta meta
                    sb
                | Image image -> appendImage sb image
                | _ -> appendRegularText sb (Some markdown)

            convert (markdown.Substring(1))

    markdown |> Array.iter convert

    sb.ToString()
