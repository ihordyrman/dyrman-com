[<RequireQualifiedAccess>]
module Templates

let note title date htmlContent =
    $"""
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>{title}</title>
        <link rel="stylesheet" type="text/css" href="../Notes/Files/styles.css">
    </head>
    <body>
        <div class="content-wrapper">
            <article class="blog-post">
                <h1>{title}</h1>
                <p class="post-date">{date}</p>
                {htmlContent}
            </article>
        </div>
    </body>
    </html>
    """
