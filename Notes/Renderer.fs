module Notes.Renderer

open Notes.Transformer

let private renderElement element =
    match element with
    | Header(level, content) -> $"<h{level}>{content}</h{level}>"
    | Bold content -> $"<strong>{content}</strong>"
    | Code content -> $"<code>{content}</code>"
    | CodeBlock(language, content) ->
        match language with
        | Some lang -> $"<pre><code class=\"language-{lang}\">{content}</code></pre>"
        | None -> $"<pre><code>{content}</code></pre>"
    | List items ->
        let listItems = items |> List.map (fun item -> $"<li>{item}</li>") |> String.concat ""
        $"<ul>{listItems}</ul>"
    | Link(text, url) -> $"<a href=\"{url}\">{text}</a>"
    | Image(alt, url) -> $"<img src=\"{url}\" alt=\"{alt}\" />"
    | Text content -> content
    | LineBreak -> "\n"

let render (elements: HtmlElement list) (meta: Map<string, string>) : string = elements |> List.map renderElement |> String.concat ""
