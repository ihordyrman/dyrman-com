module ParserTests

open NotesGenerator.Lexer
open NotesGenerator.Parser
open Xunit

[<Fact>]
let ``Simple header level 1`` () =
    let tokens = [ HeaderMarker 1; MarkdownToken.Text "Hello"; NewLine ]
    let result = parse tokens
    let expected = [ Header(1, "Hello") ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Simple header level 3`` () =
    let tokens = [ HeaderMarker 3; MarkdownToken.Text "Subtitle"; NewLine ]
    let result = parse tokens
    let expected = [ Header(3, "Subtitle") ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Header with multiple text tokens`` () =
    let tokens =
        [ HeaderMarker 2
          MarkdownToken.Text "H"
          MarkdownToken.Text "e"
          MarkdownToken.Text "l"
          MarkdownToken.Text "l"
          MarkdownToken.Text "o"
          MarkdownToken.Text " "
          MarkdownToken.Text "W"
          MarkdownToken.Text "o"
          MarkdownToken.Text "r"
          MarkdownToken.Text "l"
          MarkdownToken.Text "d"
          NewLine ]

    let result = parse tokens
    let expected = [ Header(2, "Hello World") ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Empty header`` () =
    let tokens = [ HeaderMarker 1; NewLine ]
    let result = parse tokens
    let expected = [ Header(1, "") ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Header with only spaces`` () =
    let tokens =
        [ HeaderMarker 2; MarkdownToken.Text " "; MarkdownToken.Text " "; MarkdownToken.Text " "; NewLine ]

    let result = parse tokens
    let expected = [ Header(2, "   ") ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Multiple headers in sequence`` () =
    let tokens =
        [ HeaderMarker 1
          MarkdownToken.Text "First"
          NewLine
          HeaderMarker 2
          MarkdownToken.Text "Second"
          NewLine ]

    let result = parse tokens
    let expected = [ Header(1, "First"); Header(2, "Second") ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Header level 6 (maximum)`` () =
    let tokens = [ HeaderMarker 6; MarkdownToken.Text "Deep Header"; NewLine ]
    let result = parse tokens
    let expected = [ Header(6, "Deep Header") ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Simple bold text`` () =
    let tokens =
        [ BoldMarker
          MarkdownToken.Text "b"
          MarkdownToken.Text "o"
          MarkdownToken.Text "l"
          MarkdownToken.Text "d"
          BoldMarker ]

    let result = parse tokens
    let expected = [ Bold "bold" ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Bold with multiple text tokens`` () =
    let tokens =
        [ BoldMarker
          MarkdownToken.Text "b"
          MarkdownToken.Text "o"
          MarkdownToken.Text "l"
          MarkdownToken.Text "d"
          MarkdownToken.Text " "
          MarkdownToken.Text "t"
          MarkdownToken.Text "e"
          MarkdownToken.Text "x"
          MarkdownToken.Text "t"
          BoldMarker ]

    let result = parse tokens
    let expected = [ Bold "bold text" ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Text before and after bold`` () =
    let tokens =
        [ MarkdownToken.Text "start "
          BoldMarker
          MarkdownToken.Text "bold"
          BoldMarker
          MarkdownToken.Text " end" ]

    let result = parse tokens
    let expected = [ HtmlElement.Text "start "; Bold "bold"; HtmlElement.Text " end" ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Multiple bold sections`` () =
    let tokens =
        [ BoldMarker
          MarkdownToken.Text "first"
          BoldMarker
          MarkdownToken.Text " and "
          BoldMarker
          MarkdownToken.Text "second"
          BoldMarker ]

    let result = parse tokens

    let expected = [ Bold "first"; HtmlElement.Text " and "; Bold "second" ]

    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Empty bold section`` () =
    let tokens = [ BoldMarker; BoldMarker ]
    let result = parse tokens
    let expected = [ Bold "" ]
    Assert.Equal<HtmlElement list>(expected, result)
