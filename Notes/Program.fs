open System.IO
open Notes
open Renderer
open Tokenizer
open Transformer

let environment = System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")

let solutionFolder =
    match environment with
    | "Development" -> "/home/ihor/Projects/dyrman-com/notes/"
    | "Production" -> "./notes/"
    | _ -> failwith "Environment variable DOTNET_ENVIRONMENT is not set to Development"
    
Directory.CreateDirectory solutionFolder
|> fun x -> Directory.EnumerateFiles(x.FullName, "*.md", SearchOption.AllDirectories)
|> Seq.toArray
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
        
    File.WriteAllText($"{solutionFolder}test.html", content)
    
    // check list to fix
    // - new line handling in the code block
    // - remove redundant empty code blocks
    // - code block is being closed to quickly
    // - add tests for full pipeline check/e2e (questionable)
    ())
