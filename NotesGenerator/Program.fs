open System.IO

let environment = System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")

let solutionFolder =
    match environment with
    | "Development" -> "/home/broken/projects/homepage/"
    | "Production" -> "."
    | _ -> failwith "Environment variable DOTNET_ENVIRONMENT is not set to Development"

Directory.GetFiles(@$"{solutionFolder}/notes/", "*.md", SearchOption.AllDirectories)
|> fun file ->
    printfn $"{file.Length}"
    file
|> Array.iter (fun file ->
    let content = File.ReadAllLines file |> HtmlRenderer.convertMarkdownToHtml

    let htmlTemplate =
        HtmlTemplates.getHtmlFromTemplate content.Meta.Title content.Meta.Date content.HtmlContent

    File.WriteAllText(
        @$"{solutionFolder}/NotesGenerator/Outputs/{content.Meta.Path}.html",
        htmlTemplate,
        System.Text.Encoding.UTF8
    )

    printfn $"{content}"
    ())

Directory.GetFiles(@$"{solutionFolder}/NotesGenerator/Outputs/Images/", "*", SearchOption.AllDirectories)
|> Array.iter (fun file -> File.Delete(file))

Directory.GetFiles(@$"{solutionFolder}/notes/Images/", "*", SearchOption.AllDirectories)
|> Array.iter (fun file ->
    let fileName = Path.GetFileName file
    File.Copy(file, $@"{solutionFolder}/NotesGenerator/Outputs/Images/{fileName}", true))
