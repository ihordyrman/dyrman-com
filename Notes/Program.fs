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
|> Array.iter (fun file ->
    let content, meta = File.ReadAllText file |> tokenize |> transform ||> render

    let html = Templates.note meta["title"] meta["date"] content
    File.WriteAllText($"{solutionFolder}test.html", html)
    ())
