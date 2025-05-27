module RendererTests

open System
open Xunit
open NotesGenerator.Types

let concat elements = elements |> String.concat Environment.NewLine |> (fun x -> x + Environment.NewLine)

[<Fact>]
let ``If the string starts with #, it should be a header`` () =
    let input = "# Header"

    Renderer.convertMarkdownToHtml [| input |]
    |> fun x -> Assert.Equal("<h1>Header</h1>", x.HtmlContent)

[<Fact>]
let ``If the string starts with - , it should be a list item`` () =
    let input = "- List item"
    let expected = [| "<ul>"; "<li>List item</li>"; "</ul>" |] |> concat

    Renderer.convertMarkdownToHtml [| input |] |> fun x -> Assert.Equal(expected, x.HtmlContent)

[<Fact>]
let ``If the string starts with ---, it should be a meta`` () =
    let assertValue (a: HtmlPage) (b: string) (c: string) =
        Assert.Equal(b, c)
        a

    Renderer.convertMarkdownToHtml
        [| "---"
           "title: Title"
           "date: 2024-01-01"
           "url: url"
           "tags: test"
           "---" |]
    |> fun x -> assertValue x "2024-01-01" x.Meta.Date
    |> fun x -> assertValue x "Title" x.Meta.Title
    |> fun x -> assertValue x "url" x.Meta.Url
    |> fun x -> assertValue x "" x.HtmlContent
    |> fun x -> Assert.Equal<string list>([ "test" ], x.Meta.Tags)

[<Fact>]
let ``If the string starts with ![ , it should be an image`` () =
    let input = "![[image.png]]"

    Renderer.convertMarkdownToHtml [| input |]
    |> fun x -> Assert.Equal("<img src=\"./Images/image.png\" alt=\"image.png\"/><br />", x.HtmlContent)

[<Fact>]
let ``If the string starts with three ` , it should be a code block`` () =
    let input =
        [| "```"
           "let x = 1"
           "let y = 2"
           "let z = x + y"
           "```" |]

    let expected =
        [| "<pre><code>"
           "let x = 1"
           "let y = 2"
           "let z = x + y"
           "</code></pre><br />" |]
        |> concat

    Renderer.convertMarkdownToHtml input |> fun x -> Assert.Equal(expected, x.HtmlContent)

[<Fact>]
let ``If the string starts with anything else, it should be a regular text`` () =
    let input = "Regular text"

    Renderer.convertMarkdownToHtml [| input |]
    |> fun x -> Assert.Equal($"Regular text<br />{Environment.NewLine}", x.HtmlContent)

[<Fact>]
let ``If the string starts with *, it should be a bold text`` () =
    let input = "**bold text**"

    Renderer.convertMarkdownToHtml [| input |]
    |> fun x -> Assert.Equal($"<b>bold text</b><br />{Environment.NewLine}", x.HtmlContent)

[<Fact>]
let ``If the string contains *, it should be a bold text`` () =
    let input = "not bold **bold**"

    Renderer.convertMarkdownToHtml [| input |]
    |> fun x -> Assert.Equal($"not bold <b>bold</b><br />{Environment.NewLine}", x.HtmlContent)


[<Fact>]
let ``If the string start with `  , it should be a code content`` () =
    let input = "`code content`"

    Renderer.convertMarkdownToHtml [| input |]
    |> fun x -> Assert.Equal($"<code>code content</code><br />{Environment.NewLine}", x.HtmlContent)

[<Fact>]
let ``Regular text with code block should be generated correctly`` () =
    let input =
        """Just a regular text

```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder();
var currentAssembly = Assembly.GetExecutingAssembly();

builder.Host.AddSerilog()
    .AddMasterDbContext<AdminMasterDbContext, MasterDbContext>()
    .AddKafka<Program>();
```

Another regular text"""

    let expected =
        """Just a regular text<br />
<br />
<pre><code>
WebApplicationBuilder builder = WebApplication.CreateBuilder();
var currentAssembly = Assembly.GetExecutingAssembly();

builder.Host.AddSerilog()
    .AddMasterDbContext<AdminMasterDbContext, MasterDbContext>()
    .AddKafka<Program>();
</code></pre><br />
<br />
Another regular text<br />"""
        + Environment.NewLine

    input
    |> fun x -> x.Split Environment.NewLine
    |> Renderer.convertMarkdownToHtml
    |> fun x -> Assert.Equal(expected, (x.HtmlContent |> System.Web.HttpUtility.HtmlDecode))


[<Fact>]
let ``Link should be displayed correctly`` () =
    let input = "text with [url](https://google.com)"
    let expected = [| """text with <a href="https://google.com">url</a><br />""" |] |> concat

    Renderer.convertMarkdownToHtml [| input |] |> fun x -> Assert.Equal(expected, x.HtmlContent)
