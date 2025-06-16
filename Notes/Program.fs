open System.IO
open Notes
open Renderer
open Tokenizer
open Transformer

let environment = System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")

let solutionFolder =
    match environment with
    | "Development" -> "~/Projects/dyrman-com/"
    | "Production" -> "."
    | _ -> failwith "Environment variable DOTNET_ENVIRONMENT is not set to Development"

Directory.GetFiles(@$"{solutionFolder}/notes/", "*.md", SearchOption.AllDirectories)
|> fun file ->
    printfn $"{file.Length}"
    file
|> Array.iter (fun file ->
    let content =
        File.ReadAllLines file
        |> Array.map tokenize
        |> List.concat
        |> transform
        ||> render

    // todo: pack content inside template
    // todo: clean up a mess
    ())

Directory.GetFiles(@$"{solutionFolder}/Notes/Outputs/Images/", "*", SearchOption.AllDirectories)
|> Array.iter File.Delete

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
