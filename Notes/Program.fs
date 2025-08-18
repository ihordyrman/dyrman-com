open System.IO
open Notes
open Renderer
open Tokenizer
open Transformer

let environment = System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")

let root =
    match environment with
    | "Development" -> "/home/ihor/Projects/dyrman-com/"
    | "Production" -> "."
    | _ -> failwith "Environment variable DOTNET_ENVIRONMENT is not set to Development"

let homepageFolder = Directory.CreateDirectory(Path.Combine(root, "homepage"))
let notesFolder = Directory.CreateDirectory(Path.Combine(homepageFolder.FullName, "notes"))
let staticFolder = Directory.CreateDirectory(Path.Combine(homepageFolder.FullName, "static"))
let mediaFolder = Directory.CreateDirectory(Path.Combine(notesFolder.FullName, "media"))

let notes =
    Directory.CreateDirectory root
    |> fun x -> Directory.EnumerateFiles(x.FullName, "*.md", SearchOption.AllDirectories)
    |> Seq.toArray
    |> Array.map (fun file -> File.ReadAllText file |> tokenize |> transform ||> render)

notes
|> Array.iter (fun (content, meta) ->
    let html = Templates.note meta["title"] meta["date"] content
    let path = Helpers.titleToUrlPath meta["title"]
    File.WriteAllText($"{notesFolder}/{path}.html", html)
    ())

Templates.index (notes |> Array.map snd)
|> fun x -> File.WriteAllText($"{homepageFolder.FullName}/index.html", x)

Directory.EnumerateFiles("./Files", "*", SearchOption.AllDirectories)
|> Seq.toArray
|> Array.iter (fun file ->
    let fileName = Path.GetFileName file
    let destination = Path.Combine(staticFolder.FullName, fileName)
    File.Copy(file, destination, true))

Directory.EnumerateFiles(root, "*.webp", SearchOption.AllDirectories)
|> Seq.toArray
|> Array.iter (fun file ->
    let fileName = Path.GetFileName file
    let destination = Path.Combine(mediaFolder.FullName, fileName)
    File.Copy(file, destination, true))
