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
