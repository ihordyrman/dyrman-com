module Notes.Renderer

open System
open System.Net
open Notes.Transformer

let private encode = WebUtility.HtmlEncode

let private renderElement element =
    match element with
    | Header(level, content) -> $"<h{level}>{encode content}</h{level}>"
    | Bold content -> $"<strong>{encode content}</strong>"
    | Code content -> $"<code>{encode content}</code>"
    | CodeBlock(language, content) ->
        match language with
        | Some lang -> $"<pre><code class=\"language-{lang}\">{encode content}</code></pre>"
        | None -> $"<pre><code>{encode content}</code></pre>"
    | List items ->
        let listItems = items |> List.map (fun item -> $"<li>{encode item}</li>") |> String.concat ""
        $"<ul>{listItems}</ul>"
    | Link(text, url) -> $"<a href=\"{url}\">{encode text}</a>"
    | Image(alt, url) -> $"<img src=\"{url}\" alt=\"{encode alt}\" />"
    | Text content -> encode content
    | LineBreak -> Environment.NewLine

let render elements meta =
    let content = elements |> List.map renderElement |> String.concat ""
    content, meta
