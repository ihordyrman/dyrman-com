module Notes.Helpers

let titleToUrlPath (title: string) =
    title.ToLowerInvariant()
    |> Seq.map (fun x ->
        if System.Char.IsLetterOrDigit(x) then string x
        elif x = ' ' || x = '-' then "-"
        else "")
    |> String.concat ""
    |> fun x -> System.Text.RegularExpressions.Regex.Replace(x, "-+", "-")
    |> fun x -> x.Trim('-')
