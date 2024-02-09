open System.IO

let fileContent (file: string) =
    let mutable printable = true

    file.Split([| '\n' |])
    |> Array.filter (fun x ->
        if x.StartsWith("---") then
            printable <- not printable
            false
        else
            printable)
    |> String.concat ""

Directory.GetFiles(@"..\..\..\..\notes\", "*.md", SearchOption.AllDirectories)
    |> Array.iter (fun file ->
        let content = fileContent file
        let getProperties = MarkdownProcessor.getProperties file |> MarkdownProcessor.getPropertyByName

        let date = getProperties "date"
        let title = getProperties "page-title"
        let url = getProperties "url"

        let htmlContent = MarkdownProcessor.processMarkdown content
        let htmlTemplate = HtmlProcessor.getHtmlFromTemplate title date htmlContent

        File.WriteAllText($@"..\..\..\Outputs\{url}.html", htmlTemplate, System.Text.Encoding.UTF8)

        printfn $"%s{htmlTemplate}")

File.Copy("StaticFiles\styles.css", @"..\..\..\Outputs\styles.css", true)
File.Copy("StaticFiles\Bahnschrift.ttf", @"..\..\..\Outputs\Bahnschrift.ttf", true)
File.Copy("StaticFiles\favicon.ico", @"..\..\..\Outputs\favicon.ico", true)
