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
    let htmlTemplate = HtmlTemplates.getNoteFromTemplate title date htmlContent

    File.WriteAllText($@"..\..\..\Outputs\{url}.html", htmlTemplate, System.Text.Encoding.UTF8)

    printfn $"%s{htmlTemplate}")


Directory.GetFiles("StaticFiles", "*", SearchOption.AllDirectories)
|> Array.iter (fun file -> File.Copy(file, $@"..\..\..\Outputs\{Path.GetFileName file}", true))

// todo:
// 1. better formatting for notes
// 2. add notes button to the main page
// 3. add content page for notes with notes preview
// 4. add tags to notes (based on the properties)
// 5. change styles
