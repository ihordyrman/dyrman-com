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
        <title>{title} - Ihor's Notes</title>
        <link type="text/css" rel="stylesheet" href="../static/styles.css"/>
        <link rel="shortcut icon" href="../static/favicon.ico"/>
    </head>
    <body>
        <div class="w-full min-h-screen bg-white px-8 py-8 font-mono">
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

let index (notes: Map<string, string>[]) =

    let notesSection =
        notes
        |> Array.map (fun meta ->
            let title = meta["title"]
            let date = meta["date"]
            let path = "notes/" + (Helpers.titleToUrlPath title) + ".html"

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
                    <h2 class="text-xl mb-4 font-bold">latest notes</h2>
                    <ul class="pl-5">
                        {content}
                    </ul>
                </section>
                """
            | _ -> ""

    $"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8"/>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Ihor's homepage</title>
    <link type="text/css" rel="stylesheet" href="static/styles.css" />
    <link rel="shortcut icon" href="static/favicon.ico"/>
</head>

<body>
<div class="w-full min-h-screen bg-white px-8 py-8 font-mono">
    <div class="max-w-4xl">
        <header class="mb-8 border-b border-gray pb-4">
            <h1 class="text-black text-2xl mb-2">Ihor Dyrman</h1>
        </header>

        <main>
            <nav class="mb-8">
                <a href="https://github.com/ihordyrman/" target="_blank" rel="noopener noreferrer"
                   class="mr-4">GitHub</a>
                <a href="https://www.linkedin.com/in/dyrman/" target="_blank" rel="noopener noreferrer"
                   class="mr-4">LinkedIn</a>
            </nav>

            <section class="mb-8 max-w-2xl">
                <h2 class="text-xl mb-4 font-bold">hey there!</h2>
                <p class="mb-8 text-gray leading-relaxed">
                    I'm a software engineer with a passion for backend development and a strong interest in cloud technologies, distributed
                    systems, and DevOps.
                </p>
            </section>
            
            <section class="mb-8 max-w-2xl">
                <h2 class="text-xl mb-4 font-bold">mostly work with</h2>
                <ul class="pl-5">
                    <li class="mb-3 min-w-0 before:absolute before:-ml-5 before:text-gray relative">
                        <div class="flex flex-wrap items-baseline gap-1 text-gray">
                            <span><b>Languages</b>: C#, F#, PowerShell</span>
                        </div>
                    </li>
                    <li class="mb-3 min-w-0 before:absolute before:-ml-5 before:text-gray relative">
                        <div class="flex flex-wrap items-baseline gap-1 text-gray">
                            <span><b>Frameworks</b>: .NET, ASP.NET Core, Entity Framework Core</span>
                        </div>
                    </li>
                    <li class="mb-3 min-w-0 before:absolute before:-ml-5 before:text-gray relative">
                        <div class="flex flex-wrap items-baseline gap-1 text-gray">
                            <span><b>Infrastructure</b>: Azure, Kubernetes</span>
                        </div>
                    </li>
                    <li class="mb-3 min-w-0 before:absolute before:-ml-5 before:text-gray relative">
                        <div class="flex flex-wrap items-baseline gap-1 text-gray">
                            <span><b>Databases</b>: PostgreSQL, Redis</span>
                        </div>
                    </li>
                    <li class="mb-3 min-w-0 before:absolute before:-ml-5 before:text-gray relative">
                        <div class="flex flex-wrap items-baseline gap-1 text-gray">
                            <span><b>Other</b>: Kafka, Grafana, Prometheus, Open Telemetry</span>
                        </div>
                    </li>
                </ul>
            </section>

            {notesSection}

            <section id="contact" class="mt-8">
                <h2 class="text-xl mb-4 font-bold">contact</h2>
                <p class="text-gray">
                    The easiest way to contact me is via
                    <a href="https://www.linkedin.com/in/dyrman/" target="_blank">LinkedIn</a>.
                </p>
            </section>
        </main>

        <footer class="mt-8 pt-4 border-t border-gray">
            <!-- Footer content epmpy for now -->
        </footer>
    </div>
</div>
</body>
</html>
"""
