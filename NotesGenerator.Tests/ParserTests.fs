module ParserTests

open NotesGenerator.Lexer
open NotesGenerator.Parser
open Xunit

[<Fact>]
let ``Simple header level 1`` () =
    let tokens = [ HeaderMarker 1; MarkdownToken.Text "Hello"; NewLine ]
    let result = parse tokens
    let expected = [ Header(1, [ HtmlElement.Text "Hello" ]) ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Simple header level 3`` () =
    let tokens = [ HeaderMarker 3; MarkdownToken.Text "Subtitle"; NewLine ]
    let result = parse tokens
    let expected = [ Header(3, [ HtmlElement.Text "Subtitle" ]) ]
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
    let expected = [ Header(2, [ HtmlElement.Text "Hello World" ]) ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Header with bold text`` () =
    let tokens =
        [ HeaderMarker 1
          BoldMarker
          MarkdownToken.Text "Bold"
          BoldMarker
          MarkdownToken.Text " Header"
          NewLine ]

    let result = parse tokens
    let expected = [ Header(1, [ Bold([ HtmlElement.Text "Bold" ]); HtmlElement.Text " Header" ]) ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Header with inline code`` () =
    let tokens =
        [ HeaderMarker 2
          MarkdownToken.Text "Use "
          CodeMarker
          MarkdownToken.Text "print()"
          CodeMarker
          MarkdownToken.Text " function"
          NewLine ]

    let result = parse tokens
    let expected = [ Header(2, [ HtmlElement.Text "Use "; Code("print()"); HtmlElement.Text " function" ]) ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Header with mixed formatting`` () =
    let tokens =
        [ HeaderMarker 3
          MarkdownToken.Text "Complex "
          BoldMarker
          MarkdownToken.Text "bold"
          BoldMarker
          MarkdownToken.Text " and "
          CodeMarker
          MarkdownToken.Text "code"
          CodeMarker
          NewLine ]

    let result = parse tokens

    let expected =
        [ Header(
              3,
              [ HtmlElement.Text "Complex "
                Bold([ HtmlElement.Text "bold" ])
                HtmlElement.Text " and "
                Code("code") ]
          ) ]

    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Empty header`` () =
    let tokens = [ HeaderMarker 1; NewLine ]
    let result = parse tokens
    let expected = [ Header(1, []) ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Header with only spaces`` () =
    let tokens =
        [ HeaderMarker 2; MarkdownToken.Text " "; MarkdownToken.Text " "; MarkdownToken.Text " "; NewLine ]

    let result = parse tokens
    let expected = [ Header(2, [ HtmlElement.Text "   " ]) ]
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
    let expected = [ Header(1, [ HtmlElement.Text "First" ]); Header(2, [ HtmlElement.Text "Second" ]) ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Header level 6 (maximum)`` () =
    let tokens = [ HeaderMarker 6; MarkdownToken.Text "Deep Header"; NewLine ]
    let result = parse tokens
    let expected = [ Header(6, [ HtmlElement.Text "Deep Header" ]) ]
    Assert.Equal<HtmlElement list>(expected, result)
