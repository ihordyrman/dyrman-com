module SanityTests

open Notes
open Xunit
open Tokenizer
open Renderer
open Transformer

[<Fact>]
let ``Sanity check #1`` () =
    let markdown =
        """```fsharp
let x = 42
let y = x * 2
```"""

    let expectedHtml =
        """<pre><code class="language-fsharp">let x = 42
let y = x * 2
</code></pre>
"""

    let actualHtml, _ = markdown |> tokenize |> transform ||> render

    Assert.Equal(expectedHtml, actualHtml)

[<Fact>]
let ``Sanity check #2`` () =
    let markdown =
        """### Access the exception
Use [IExceptionHandlerPathFeature](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.diagnostics.iexceptionhandlerpathfeature) to access the exception and the original request path in an error handler
```cs
var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

if (exceptionHandlerPathFeature?.Error is FileNotFoundException)
{
	ExceptionMessage = "The file was not found.";
}

if (exceptionHandlerPathFeature?.Path == "/")
{
	ExceptionMessage ??= string.Empty;
    ExceptionMessage += " Page: Home.";
}
```"""

    let expectedHtml =
        """<h3>Access the exception</h3>
Use <a href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.diagnostics.iexceptionhandlerpathfeature">IExceptionHandlerPathFeature</a> to access the exception and the original request path in an error handler
<pre><code class="language-cs">var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

if (exceptionHandlerPathFeature?.Error is FileNotFoundException)
{
	ExceptionMessage = "The file was not found.";
}

if (exceptionHandlerPathFeature?.Path == "/")
{
	ExceptionMessage ??= string.Empty;
    ExceptionMessage += " Page: Home.";
}
</code></pre>
"""

    let actualHtml, _ = markdown |> tokenize |> transform ||> render

    Assert.Equal(expectedHtml, actualHtml)
