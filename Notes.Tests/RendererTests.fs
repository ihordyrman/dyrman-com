module RendererTests

open Notes.Transformer
open Notes.Renderer
open Xunit

[<Theory>]
[<InlineData(1)>]
[<InlineData(2)>]
[<InlineData(3)>]
[<InlineData(4)>]
[<InlineData(5)>]
[<InlineData(6)>]
let ``Should render header level correctly`` level =
    let assertLevel (header: string) (level: int) =
        let actualLevel = header.Substring((header.IndexOf '<') + 2, (header.IndexOf '>') - 2) |> int
        Assert.Equal<int>(actualLevel, level)

    let elements = [ Header(level, "Hello World") ]
    let result = render elements Map.empty
    assertLevel result level

[<Fact>]
let ``Should render multiple headers`` () =
    let elements = [ Header(1, "Main Title"); Header(2, "Subtitle"); Header(3, "Section") ]
    let result = render elements Map.empty
    let expected = "<h1>Main Title</h1><h2>Subtitle</h2><h3>Section</h3>"
    Assert.Equal<string>(expected, result)

[<Fact>]
let ``Should render headers mixed with other elements`` () =
    let elements = [ Header(1, "Title"); Text("Some text"); Header(2, "Subtitle") ]
    let result = render elements Map.empty
    let expected = "<h1>Title</h1>Some text<h2>Subtitle</h2>"
    Assert.Equal<string>(expected, result)

[<Fact>]
let ``Should render header with line breaks`` () =
    let elements = [ Header(1, "First Header"); LineBreak; Header(2, "Second Header") ]
    let result = render elements Map.empty
    let expected = "<h1>First Header</h1>\n<h2>Second Header</h2>"
    Assert.Equal<string>(expected, result)

[<Fact>]
let ``Should render simple bold text`` () =
    let elements = [ Bold("Hello World") ]
    let result = render elements Map.empty
    let expected = "<strong>Hello World</strong>"
    Assert.Equal<string>(expected, result)

[<Fact>]
let ``Should render multiple bold elements`` () =
    let elements = [ Bold("First"); Bold("Second"); Bold("Third") ]
    let result = render elements Map.empty
    let expected = "<strong>First</strong><strong>Second</strong><strong>Third</strong>"
    Assert.Equal<string>(expected, result)

[<Fact>]
let ``Should render bold mixed with text`` () =
    let elements = [ Text("This is "); Bold("bold"); Text(" text") ]
    let result = render elements Map.empty
    let expected = "This is <strong>bold</strong> text"
    Assert.Equal<string>(expected, result)

[<Fact>]
let ``Should render bold with line breaks`` () =
    let elements = [ Bold("First bold"); LineBreak; Bold("Second bold") ]
    let result = render elements Map.empty
    let expected = "<strong>First bold</strong>\n<strong>Second bold</strong>"
    Assert.Equal<string>(expected, result)

[<Fact>]
let ``Should render simple image`` () =
    let elements = [ Image("image", "image.jpg") ]
    let result = render elements Map.empty
    let expected = "<img src=\"image.jpg\" alt=\"image\" />"
    Assert.Equal<string>(expected, result)

[<Fact>]
let ``Should render image with empty alt text`` () =
    let elements = [ Image("", "image.png") ]
    let result = render elements Map.empty
    let expected = "<img src=\"image.png\" alt=\"\" />"
    Assert.Equal<string>(expected, result)

[<Fact>]
let ``Should render multiple images`` () =
    let elements = [ Image("First", "img1.jpg"); Image("Second", "img2.png"); Image("Third", "img3.gif") ]
    let result = render elements Map.empty

    let expected =
        "<img src=\"img1.jpg\" alt=\"First\" /><img src=\"img2.png\" alt=\"Second\" /><img src=\"img3.gif\" alt=\"Third\" />"

    Assert.Equal<string>(expected, result)

[<Fact>]
let ``Should render image mixed with text`` () =
    let elements =
        [ Text("Text above the image "); Image("image", "image.jpg"); Text(" Text below the image") ]

    let result = render elements Map.empty
    let expected = "Text above the image <img src=\"image.jpg\" alt=\"image\" /> Text below the image"
    Assert.Equal<string>(expected, result)

[<Fact>]
let ``Should render image with line breaks`` () =
    let elements = [ Image("First image", "img1.jpg"); LineBreak; Image("Second image", "img2.jpg") ]
    let result = render elements Map.empty

    let expected =
        "<img src=\"img1.jpg\" alt=\"First image\" />\n<img src=\"img2.jpg\" alt=\"Second image\" />"

    Assert.Equal<string>(expected, result)

[<Fact>]
let ``Should render single item list`` () =
    let elements = [ List([ "Item 1" ]) ]
    let result = render elements Map.empty
    let expected = "<ul><li>Item 1</li></ul>"
    Assert.Equal<string>(expected, result)

[<Theory>]
[<InlineData(1)>]
[<InlineData(5)>]
[<InlineData(10)>]
[<InlineData(100)>]
let ``Should render multiple items correctly`` count =
    let items = List.init count (fun i -> $"item {i}")
    let result = render [ List(items) ] Map.empty
    let actualCount = result.Split("<li>").Length - 1
    Assert.Equal<int>(actualCount, count)
