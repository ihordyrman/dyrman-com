[<RequireQualifiedAccess>]
module Templates

let note title date htmlContent =
    $"""
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="utf-8"/>
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <title>{title} - Ihor's Notes</title>
        <link href="../Notes/Files/styles.css" type="text/css" rel="stylesheet"/>
        <link rel="shortcut icon" href="../favicon.ico"/>
    </head>
    <body>
        <div class="w-full min-h-screen bg-white px-8 py-4 font-mono">
            <div class="max-w-4xl">
                <header class="mb-8 border-b border-gray pb-4">
                    <nav>
                        <a href="../index.html" class="text-indigo">← Back to homepage</a>
                    </nav>
                </header>

                <article class="max-w-4xl">
                    <header class="mb-8">
                        <h1 class="text-2xl text-black mb-2 font-bold">{title}</h1>
                        <time class="text-gray">{date}</time>
                    </header>

                    <div class="note-content text-gray leading-relaxed">
                        {htmlContent}
                    </div>
                </article>

                <footer class="mt-8 pt-4 border-t border-gray">
                    <nav>
                        <a href="../index.html" class="text-indigo">← Back to homepage</a>
                    </nav>
                </footer>
            </div>
        </div>
    </body>
    </html>
    """
