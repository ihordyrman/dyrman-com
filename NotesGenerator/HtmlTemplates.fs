[<RequireQualifiedAccess>]
module HtmlTemplates

let getNoteFromTemplate title date htmlContent =
    $"""
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <title>{title}</title>
        <link rel="stylesheet" type="text/css" href="styles.css">
    </head>
    <body>
        <h1>{title}</h1>
        <p>{date}</p>
        {htmlContent}
    </body>
    </html>
    """
