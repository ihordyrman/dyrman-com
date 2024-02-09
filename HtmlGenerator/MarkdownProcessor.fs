[<RequireQualifiedAccess>]
module MarkdownProcessor

open System

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
    |> Array.map _.Trim()
    |> Array.filter (fun x -> not (String.IsNullOrEmpty x))
    |> Array.map (fun x -> x.Split ": ")
    |> Array.map (fun x -> (x[0], x[1]))
    |> Array.toList

let convertTextToUrl (text: string) =
    text
    |> _.ToLowerInvariant()
    |> _.Split(' ')
    |> Array.toList
    |> List.map _.Trim()
    |> List.filter (fun x -> not (String.IsNullOrEmpty x))
    |> String.concat "-"

let getPropertyByName properties name = properties |> List.filter (fun (x, _) -> x = name) |> List.head |> snd

let processMarkdown (md: string) = Markdig.Markdown.ToHtml(md)
