open System
open System.IO
open Notes
open Renderer
open Tokenizer
open Transformer

let environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")

let root =
    match environment with
    | "Development" -> "/home/ihor/Projects/dyrman-com/"
    | "Production" -> "."
    | _ -> failwith "Environment variable DOTNET_ENVIRONMENT is not set"

let executableDir = AppContext.BaseDirectory

let homepageFolder = Directory.CreateDirectory(Path.Combine(root, "homepage"))
let notesFolder = Directory.CreateDirectory(Path.Combine(homepageFolder.FullName, "notes"))
let staticFolder = Directory.CreateDirectory(Path.Combine(homepageFolder.FullName, "static"))
let mediaFolder = Directory.CreateDirectory(Path.Combine(notesFolder.FullName, "media"))

let processMarkdown filePath = File.ReadAllText filePath |> tokenize |> transform ||> render

let parseDate (dateStr: string) =
    match DateTime.TryParseExact(dateStr, "dd.MM.yyyy", null, Globalization.DateTimeStyles.None) with
    | true, date -> date
    | false, _ ->
        match DateTime.TryParse(dateStr) with
        | true, date -> date
        | false, _ -> DateTime.MinValue

let notesPath = Path.Combine(root, "notes")

let notes =
    if Directory.Exists(notesPath) then
        Directory.EnumerateFiles(notesPath, "*.md", SearchOption.TopDirectoryOnly)
        |> Seq.filter (fun f -> not (Path.GetFileName(f).StartsWith("_"))) // Skip files starting with _
        |> Seq.toArray
        |> Array.map processMarkdown
    else
        [||]

notes
|> Array.iter (fun (content, meta) ->
    if meta.ContainsKey("title") && meta.ContainsKey("date") then
        let html = Templates.note meta["title"] meta["date"] content
        let path = Helpers.titleToUrlPath meta["title"]
        let noteFolder = Directory.CreateDirectory(Path.Combine(notesFolder.FullName, path))
        File.WriteAllText(Path.Combine(noteFolder.FullName, "index.html"), html))

let readingListPath = Path.Combine(root, "library", "reading-list.md")
let hasReadingList = File.Exists(readingListPath)

if hasReadingList then
    let content, _ = processMarkdown readingListPath
    let html = Templates.readingList content
    let libraryFolder = Directory.CreateDirectory(Path.Combine(homepageFolder.FullName, "library"))
    File.WriteAllText(Path.Combine(libraryFolder.FullName, "index.html"), html)

let streamPath = Path.Combine(root, "stream")

let hasStream =
    Directory.Exists(streamPath) && Directory.EnumerateFiles(streamPath, "*.md") |> Seq.isEmpty |> not

let streamEntries =
    if hasStream then
        Directory.EnumerateFiles(streamPath, "*.md", SearchOption.TopDirectoryOnly)
        |> Seq.toArray
        |> Array.map (fun file ->
            let content, meta = processMarkdown file
            let date = if meta.ContainsKey("date") then meta["date"] else "Unknown"
            (date, content, parseDate date))
        |> Array.sortByDescending (fun (_, _, parsedDate) -> parsedDate)
        |> Array.map (fun (date, content, _) -> (date, content))
        |> Array.toList
    else
        []

if hasStream then
    let html = Templates.stream streamEntries
    let streamFolder = Directory.CreateDirectory(Path.Combine(homepageFolder.FullName, "stream"))
    File.WriteAllText(Path.Combine(streamFolder.FullName, "index.html"), html)

Templates.index (notes |> Array.map snd) hasReadingList hasStream
|> fun x -> File.WriteAllText(Path.Combine(homepageFolder.FullName, "index.html"), x)

let filesPath = Path.Combine(executableDir, "Files")

if Directory.Exists(filesPath) then
    Directory.EnumerateFiles(filesPath, "*", SearchOption.AllDirectories)
    |> Seq.toArray
    |> Array.iter (fun file ->
        let fileName = Path.GetFileName file
        let destination = Path.Combine(staticFolder.FullName, fileName)
        File.Copy(file, destination, true))
else
    printfn $"Warning: Files directory not found at %s{filesPath}"

Directory.EnumerateFiles(root, "*.webp", SearchOption.AllDirectories)
|> Seq.toArray
|> Array.iter (fun file ->
    let fileName = Path.GetFileName file
    let destination = Path.Combine(mediaFolder.FullName, fileName)
    File.Copy(file, destination, true))

printfn "Generated:"
printfn $"  - %d{notes.Length} notes"
printfn $"  - Reading list: %b{hasReadingList}"
printfn $"  - Stream entries: %d{streamEntries.Length}"
