---
title: Global exception handling
date: 22.05.2025
---

## Developer exception page
displays detailed information about unhandled request exceptions. It uses [DeveloperExceptionPageMiddleware](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.diagnostics.developerexceptionpagemiddleware) to capture synchronous and asynchronous exceptions from the HTTP pipeline and to generate error responses. The developer exception page runs early in the middleware pipeline, so that it can catch unhandled exceptions thrown in middleware that follows.

## Exception handler page
To configure a custom error handling page for the [Production environment](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-9.0), call [UseExceptionHandler](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.exceptionhandlerextensions.useexceptionhandler). This exception handling middleware:
- Catches and logs unhandled exceptions.
- Re-executes the request in an alternate pipeline using the path indicated. The request isn't re-executed if the response has started. The template-generated code re-executes the request using the /Error path.

### Access the exception
Use [IExceptionHandlerPathFeature](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.diagnostics.iexceptionhandlerpathfeature) to access the exception and the original request path in an error handler
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
```

## Exception handler lambda
An alternative to a [custom exception handler page](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-9.0#exception-handler-page) is to provide a lambda to [UseExceptionHandler](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.exceptionhandlerextensions.useexceptionhandler). Using a lambda allows access to the error before returning the response.
```cs
app.UseExceptionHandler(exceptionHandlerApp =>
{
	exceptionHandlerApp.Run(async context =>
	{
		context.Response.StatusCode = StatusCodes.Status500InternalServerError;
		context.Response.ContentType = Text.Plain;

		await context.Response.WriteAsync("An exception was thrown.");

		var exceptionHandlerPathFeature =
			context.Features.Get<IExceptionHandlerPathFeature>();

		if (exceptionHandlerPathFeature?.Error is FileNotFoundException)
		{
			await context.Response.WriteAsync(" The file was not found.");
		}

		if (exceptionHandlerPathFeature?.Path == "/")
		{
			await context.Response.WriteAsync(" Page: Home.");
		}
	});
});
```

## IExceptionHandler
[IExceptionHandler](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.diagnostics.iexceptionhandler) is an interface that gives the developer a callback for handling known exceptions in a central location.
The lifetime of an `IExceptionHandler` instance is singleton. Multiple implementations can be added, and they're called in the order registered.
```cs
public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
{
	public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		var exceptionMessage = exception.Message;
		logger.LogError(
			"Error Message: {exceptionMessage}, Time of occurrence {time}",
			exceptionMessage, DateTime.UtcNow);
		// Return false to continue with the default behavior
		// - or - return true to signal that this exception is handled
		return ValueTask.FromResult(false);
	}
}
```