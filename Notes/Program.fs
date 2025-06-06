open System.IO

let environment = System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")

let solutionFolder =
    match environment with
    | "Development" -> "C:/Projects/homepage/"
    | "Production" -> "."
    | _ -> failwith "Environment variable DOTNET_ENVIRONMENT is not set to Development"

Directory.GetFiles(@$"{solutionFolder}/notes/", "*.md", SearchOption.AllDirectories)
|> fun file ->
    printfn $"{file.Length}"
    file
|> Array.iter (fun file ->
    let content = File.ReadAllLines file |> Renderer.convertMarkdownToHtml

    let htmlTemplate = Templates.note content.Meta.Title content.Meta.Date content.HtmlContent

    File.WriteAllText(@$"{solutionFolder}/Notes/Outputs/{content.Meta.Path}.html", htmlTemplate, System.Text.Encoding.UTF8)

    printfn $"{content}"
    ())

Directory.GetFiles(@$"{solutionFolder}/Notes/Outputs/Images/", "*", SearchOption.AllDirectories)
|> Array.iter (fun file -> File.Delete(file))

Directory.GetFiles(@$"{solutionFolder}/notes/Images/", "*", SearchOption.AllDirectories)
|> Array.iter (fun file ->
    let fileName = Path.GetFileName file
    File.Copy(file, $@"{solutionFolder}/Notes/Outputs/Images/{fileName}", true))

// Tokenize -> Transform -> Render
// "# **Bold** title"
//     ↓ Lexer
// [HeaderMarker 1; BoldMarker; Text "B"; Text "o"...; BoldMarker; ...]
//     ↓ Transformer
// Header(1, [Bold([Text "Bold"]); Text " title"])
//     ↓ Renderer
// "<h1><strong>Bold</strong> title</h1>"
