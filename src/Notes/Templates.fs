[<RequireQualifiedAccess>]
module Templates

open Notes

let note title date htmlContent =
    $"""
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="utf-8"/>
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <title>{title} - ihor's notes</title>
        <link type="text/css" rel="stylesheet" href="/static/styles.css"/>
        <link rel="shortcut icon" href="/static/favicon.ico"/>
    </head>
    <body>
        <div class="w-full min-h-screen bg-white px-8 py-8 font-mono">
            <div class="max-w-4xl">
                <header class="mb-8 border-b border-gray pb-4">
                    <nav>
                        <a href="/" class="text-indigo">← back to homepage</a>
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
                        <a href="/" class="text-indigo">← back to homepage</a>
                    </nav>
                </footer>
            </div>
        </div>
    </body>
    </html>
    """

let readingList htmlContent =
    $"""
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="utf-8"/>
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <title>reading list - ihor's notes</title>
        <link type="text/css" rel="stylesheet" href="/static/styles.css"/>
        <link rel="shortcut icon" href="/static/favicon.ico"/>
    </head>
    <body>
        <div class="w-full min-h-screen bg-white px-8 py-8 font-mono">
            <div class="max-w-4xl">
                <header class="mb-8 border-b border-gray pb-4">
                    <nav>
                        <a href="/" class="text-indigo">← back to homepage</a>
                    </nav>
                </header>

                <article class="max-w-4xl">
                    <header class="mb-8">
                        <p class="text-gray">books I read recently</p>
                    </header>

                    <div class="note-content text-gray leading-relaxed">
                        {htmlContent}
                    </div>
                </article>

                <footer class="mt-8 pt-4 border-t border-gray">
                    <nav>
                        <a href="/" class="text-indigo">← back to homepage</a>
                    </nav>
                </footer>
            </div>
        </div>
    </body>
    </html>
    """

let articles htmlContent =
    $"""
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="utf-8"/>
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <title>worth reading - ihor's notes</title>
        <link type="text/css" rel="stylesheet" href="/static/styles.css"/>
        <link rel="shortcut icon" href="/static/favicon.ico"/>
    </head>
    <body>
        <div class="w-full min-h-screen bg-white px-8 py-8 font-mono">
            <div class="max-w-4xl">
                <header class="mb-8 border-b border-gray pb-4">
                    <nav>
                        <a href="/" class="text-indigo">← back to homepage</a>
                    </nav>
                </header>

                <article class="max-w-4xl">
                    <header class="mb-8">
                        <p class="text-gray">a curated list of articles that I like and recommend</p>
                    </header>

                    <div class="note-content text-gray leading-relaxed">
                        {htmlContent}
                    </div>
                </article>

                <footer class="mt-8 pt-4 border-t border-gray">
                    <nav>
                        <a href="/" class="text-indigo">← back to homepage</a>
                    </nav>
                </footer>
            </div>
        </div>
    </body>
    </html>
    """

let private streamEntry date htmlContent =
    $"""
    <article class="mb-8 pb-8 border-b border-gray">
        <time class="text-gray text-sm">{date}</time>
        <div class="note-content text-gray leading-relaxed mt-2">
            {htmlContent}
        </div>
    </article>
    """

let stream (entries: (string * string) list) =
    let entriesHtml =
        entries |> List.map (fun (date, content) -> streamEntry date content) |> String.concat ""

    $"""
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="utf-8"/>
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <title>stream - Ihor's notes</title>
        <link type="text/css" rel="stylesheet" href="/static/styles.css"/>
        <link rel="shortcut icon" href="/static/favicon.ico"/>
    </head>
    <body>
        <div class="w-full min-h-screen bg-white px-8 py-8 font-mono">
            <div class="max-w-4xl">
                <header class="mb-8 border-b border-gray pb-4">
                    <nav>
                        <a href="/" class="text-indigo">← back to homepage</a>
                    </nav>
                </header>

                <section class="max-w-4xl">
                    <header class="mb-8">
                        <p class="text-gray">random thoughts and observations</p>
                    </header>

                    <div class="stream-entries">
                        {entriesHtml}
                    </div>
                </section>

                <footer class="mt-8 pt-4 border-t border-gray">
                    <nav>
                        <a href="/" class="text-indigo">← back to homepage</a>
                    </nav>
                </footer>
            </div>
        </div>
    </body>
    </html>
    """

let index (notes: Map<string, string>[]) hasReadingList hasStream hasArticles (recentStream: (string * string) list) =

    let notesSection =
        notes
        |> Array.map (fun meta ->
            let title = meta["title"]
            let date = meta["date"]
            let path = "/notes/" + (Helpers.titleToUrlPath title)

            $"""
             <li class="mb-3 min-w-0 before:absolute before:-ml-5 before:text-gray relative">
                 <div class="flex flex-wrap items-baseline gap-1">
                     <span class="text-gray">{date}</span>
                     <a href="{path}" class="text-indigo">{title}</a>
                 </div>
             </li>
            """)
        |> String.concat ""
        |> fun content ->
            match content with
            | x when x.Length > 0 ->
                $"""
                <section class="mb-8 max-w-2xl">
                    <h2 class="mb-4 font-bold">latest notes</h2>
                    <ul class="pl-5">
                        {content}
                    </ul>
                </section>
                """
            | _ -> ""

    let streamSection =
        recentStream
        |> List.map (fun (date, content) ->
            $"""
            <article class="mb-4">
                <time class="text-gray text-sm">{date}</time>
                <div class="note-content text-gray leading-relaxed mt-2">
                    {content}
                </div>
            </article>
            """)
        |> String.concat ""
        |> fun content ->
            match content with
            | x when x.Length > 0 ->
                $"""
                <section class="mb-8 max-w-2xl">
                    <h2 class="mb-4 font-bold">recent thoughts</h2>
                    {content}
                    <a href="/stream" class="text-indigo">→ all thoughts</a>
                </section>
                """
            | _ -> ""

    let readingListLink = if hasReadingList then """<a href="/library" class="mr-4">library</a>""" else ""

    let streamLink = if hasStream then """<a href="/stream" class="mr-4">stream</a>""" else ""

    let articlesLink = if hasArticles then """<a href="/articles" class="mr-4">worthy</a>""" else ""

    $"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8"/>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>ihor's homepage</title>
    <link type="text/css" rel="stylesheet" href="/static/styles.css" />
    <link rel="shortcut icon" href="/static/favicon.ico"/>
</head>

<body>
<div class="w-full min-h-screen bg-white px-8 py-8 font-mono">
    <div class="max-w-4xl">
        <header class="mb-8 border-b border-gray pb-4">
            <h1 class="text-black">Ihor Dyrman</h1>
        </header>

        <main>
            <nav class="mb-8">
                <a href="https://github.com/ihordyrman/" target="_blank" rel="noopener noreferrer"
                   class="mr-4">code</a>
                {readingListLink}
                {articlesLink}
                {streamLink}
            </nav>

            <section class="mb-8 max-w-2xl">
                <h2 class="mb-4 font-bold">hey there!</h2>
                <p class="mb-8 text-gray leading-relaxed">
                    i'm a software engineer with a passion for backend development and a strong interest in cloud technologies, distributed
                    systems, and DevOps.
                </p>
            </section>
            
            {notesSection}

            {streamSection}

            <section id="contact" class="mt-8">
                <h2 class="mb-4 font-bold">contact</h2>
                <p class="text-gray">
                    the easiest way to contact me is via
                    <a href="https://www.linkedin.com/in/dyrman/" target="_blank">LinkedIn</a>.
                </p>
            </section>
        </main>

        <footer class="mt-8 pt-4 border-t border-gray">
            <!-- Footer content empty for now -->
        </footer>
    </div>
</div>
</body>
</html>
"""
