module Tests

open Xunit

[<Fact>]
let ``If the string starts with #, it should be a header`` () =
    let input = "# Header"

    HtmlRenderer.convertMarkdownToHtml [| input |]
    |> fun x -> Assert.Equal("<h1>Header</h1>", x.HtmlContent)

[<Fact>]
let ``If the string starts with - , it should be a list item`` () =
    let input = "- List item"

    HtmlRenderer.convertMarkdownToHtml [| input |]
    |> fun x -> Assert.Equal("<li>List item</li>", x.HtmlContent)

[<Fact>]
let ``If the string starts with ---, it should be a meta`` () =
    let assertValue (a: HtmlRenderer.HtmlPage) (b: string) (c: string) =
        Assert.Equal(b, c)
        a

    HtmlRenderer.convertMarkdownToHtml [| "---"; "title: Title"; "date: 2024-01-01"; "url: url"; "tags: test"; "---" |]
    |> fun x -> assertValue x "2024-01-01" x.Meta.Date
    |> fun x -> assertValue x "Title" x.Meta.Title
    |> fun x -> assertValue x "url" x.Meta.Url
    |> fun x -> assertValue x "" x.HtmlContent
    |> fun x -> Assert.Equal<string list>(["test"], x.Meta.Tags)

[<Fact>]
let ``If the string starts with ![ , it should be an image`` () =
    let input = "![[image.png]]"

    HtmlRenderer.convertMarkdownToHtml [| input |]
    |> fun x -> Assert.Equal("<img src=\"./Images/image.png\" alt=\"image.png\"/><br />", x.HtmlContent)

[<Fact>]
let ``If the string starts with three ` , it should be a code block`` () =
    let input = [| "```"; "let x = 1"; "let y = 2"; "let z = x + y"; "```" |]

    let expected =
        """<pre><code>let x = 1
let y = 2
let z = x + y
</code></pre>"""

    HtmlRenderer.convertMarkdownToHtml input |> fun x -> Assert.Equal(expected, x.HtmlContent)

[<Fact>]
let ``If the string starts with anything else, it should be a regular text`` () =
    let input = "Regular text"

    HtmlRenderer.convertMarkdownToHtml [| input |]
    |> fun x -> Assert.Equal("Regular text<br />", x.HtmlContent)

[<Fact>]
let ``If the string starts with *, it should be a bold text`` () =
    let input = "**bold text**"

    HtmlRenderer.convertMarkdownToHtml [| input |]
    |> fun x -> Assert.Equal("<b>bold text</b><br />", x.HtmlContent)


[<Fact>]
let ``If the string start with `  , it should be a code content`` () =
    let input = "`code content`"

    HtmlRenderer.convertMarkdownToHtml [| input |]
    |> fun x -> Assert.Equal("<code>code content</code>", x.HtmlContent)
