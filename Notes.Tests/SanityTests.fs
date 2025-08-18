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

// [<Fact>]
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


// [<Fact>]
let ``Sanity check #3`` () =
    let markdown =
        """## Exception handler page
To configure a custom error handling page for the [Production environment](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-9.0), call [UseExceptionHandler](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.exceptionhandlerextensions.useexceptionhandler). This exception handling middleware:
- Catches and logs unhandled exceptions.
- Re-executes the request in an alternate pipeline using the path indicated. The request isn't re-executed if the response has started. The template-generated code re-executes the request using the path."""

    let expectedHtml =
        """<h2>Exception handler page</h2>
To configure a custom error handling page for the <a href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-9.0">Production environment</a>, call <a href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.exceptionhandlerextensions.useexceptionhandler">UseExceptionHandler</a>. This exception handling middleware:
<ul><li>Catches and logs unhandled exceptions.</li><li>Re-executes the request in an alternate pipeline using the path indicated. The request isn't re-executed if the response has started. The template-generated code re-executes the request using the path.</li></ul>
"""

    let actualHtml, _ = markdown |> tokenize |> transform ||> render
    Assert.Equal(expectedHtml, actualHtml)
