open System
open System.IO

// todo: create a css style file
// todo: apply styles to different elements
// todo: add more elements, like images, links, etc
// todo: convert the whole website to plain html with css (no javascript)

let file =
    File.ReadAllText(@"Source\post1.txt").Split([| '\n' |])
    |> Array.map (fun x -> $"<p>{x}</p>")
    |> String.concat ""

printfn $"%i{file.Length}"

let date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
let title = "Post 1"

let post =
    $"<html>
      <head>
      <title>{title}</title>
      </head>
      <body>
          <h1>{title}</h1>
          <p>{date}</p>
          {file}
      </body>
      </html>"

File.WriteAllText("post1.html", post)
