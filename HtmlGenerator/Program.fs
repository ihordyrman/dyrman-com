open System
open System.IO
open Markdig

let file = File.ReadAllText(@"Source\post1.md")

let getProperties (md: string) : (string * string) list =
    let mutable canSkip = false

    md.Split([| '\n' |])
    |> Array.filter (fun x ->
        if x.StartsWith("---") then
            canSkip <- not canSkip
            true
        else
            canSkip)
    |> Array.filter (fun x -> not (x.StartsWith("---")))
    |> Array.map (fun x -> x.Trim())
    |> Array.filter (fun x -> not (String.IsNullOrEmpty x))
    |> Array.map (fun x -> x.Split ": ")
    |> Array.map (fun x -> (x[0], x[1]))
    |> Array.toList

let getPropertyByName properties name = properties |> List.filter (fun (x, _) -> x = name) |> List.head |> snd


let split (separator: char) (str: string) = str.Split([| separator |]) |> Array.toList
let toLower (str: string) = str.ToLowerInvariant()

let convertTextToUrl (text: string) =
    text
    |> toLower
    |> split ' '
    |> List.map (fun x -> x.Trim())
    |> List.filter (fun x -> not (String.IsNullOrEmpty x))
    |> String.concat "-"

let content =
    let mutable printable = true

    file.Split([| '\n' |])
    |> Array.filter (fun x ->
        if x.StartsWith("---") then
            printable <- not printable
            false
        else
            printable)
    |> String.concat ""

let properties = getProperties file

let date = getPropertyByName properties "date"
let title = getPropertyByName properties "page-title"
let url = getPropertyByName properties "url"

let htmlContent = Markdown.ToHtml(content)

let htmlTemplate =
    $"""
<!DOCTYPE html>
<html lang="en">
<head>
    <title>{title}</title>
    <link rel="stylesheet" type="text/css" href="styles.css">
</head>
<body>
    <h1>{title}</h1>
    <p>{date}</p>
    {htmlContent}
</body>
</html>
"""

File.WriteAllText($@"..\..\..\Outputs\{url}.html", htmlTemplate, System.Text.Encoding.UTF8)

printfn $"%s{htmlTemplate}"
