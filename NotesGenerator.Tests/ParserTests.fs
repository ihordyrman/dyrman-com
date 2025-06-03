module ParserTests

open NotesGenerator.Lexer
open NotesGenerator.Parser
open Xunit

[<Fact>]
let ``Simple header level 1`` () =
    let tokens = [ HeaderMarker 1; Symbol 'H'; Symbol 'e'; Symbol 'l'; Symbol 'l'; Symbol 'o'; NewLine ]
    let result = parse tokens
    let expected = [ Header(1, "Hello"); LineBreak ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Simple header level 3`` () =
    let tokens = [ HeaderMarker 3; Symbol 'H'; Symbol 'e'; Symbol 'l'; Symbol 'l'; Symbol 'o'; NewLine ]
    let result = parse tokens
    let expected = [ Header(3, "Hello"); LineBreak ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Header with multiple text tokens`` () =
    let tokens =
        [ HeaderMarker 2
          Symbol 'H'
          Symbol 'e'
          Symbol 'l'
          Symbol 'l'
          Symbol 'o'
          Symbol ' '
          Symbol 'W'
          Symbol 'o'
          Symbol 'r'
          Symbol 'l'
          Symbol 'd'
          NewLine ]

    let result = parse tokens
    let expected = [ Header(2, "Hello World"); LineBreak ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Empty header`` () =
    let tokens = [ HeaderMarker 1; NewLine ]
    let result = parse tokens
    let expected = [ Header(1, ""); LineBreak ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Header with only spaces`` () =
    let tokens = [ HeaderMarker 2; Symbol ' '; Symbol ' '; Symbol ' '; NewLine ]

    let result = parse tokens
    let expected = [ Header(2, "   "); LineBreak ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Multiple headers in sequence`` () =
    let tokens =
        [ HeaderMarker 1
          Symbol 'F'
          Symbol 'i'
          Symbol 'r'
          Symbol 's'
          Symbol 't'
          NewLine
          HeaderMarker 2
          Symbol 'S'
          Symbol 'e'
          Symbol 'c'
          Symbol 'o'
          Symbol 'n'
          Symbol 'd'
          NewLine ]

    let result = parse tokens
    let expected = [ Header(1, "First"); Header(2, "Second") ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Header level 6 (maximum)`` () =
    let tokens =
        [ HeaderMarker 6; Symbol 'H'; Symbol 'e'; Symbol 'a'; Symbol 'd'; Symbol 'e'; Symbol 'r'; NewLine ]

    let result = parse tokens
    let expected = [ Header(6, "Header"); LineBreak ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Simple bold text`` () =
    let tokens = [ BoldMarker; Symbol 'b'; Symbol 'o'; Symbol 'l'; Symbol 'd'; BoldMarker ]

    let result = parse tokens
    let expected = [ Bold "bold" ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Bold with multiple text tokens`` () =
    let tokens =
        [ BoldMarker
          Symbol 'b'
          Symbol 'o'
          Symbol 'l'
          Symbol 'd'
          Symbol ' '
          Symbol 't'
          Symbol 'e'
          Symbol 'x'
          Symbol 't'
          BoldMarker ]

    let result = parse tokens
    let expected = [ Bold "bold text" ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Text before and after bold`` () =
    let tokens =
        [ Symbol 's'
          Symbol 't'
          Symbol 'a'
          Symbol 'r'
          Symbol 't'
          Symbol ' '
          BoldMarker
          Symbol 'b'
          Symbol 'o'
          Symbol 'l'
          Symbol 'd'
          BoldMarker
          Symbol ' '
          Symbol 'e'
          Symbol 'n'
          Symbol 'd' ]

    let result = parse tokens
    let expected = [ Text "start "; Bold "bold"; Text " end" ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Multiple bold sections`` () =
    let tokens =
        [ BoldMarker
          Symbol 'f'
          Symbol 'i'
          Symbol 'r'
          Symbol 's'
          Symbol 't'
          BoldMarker
          Symbol ' '
          Symbol 'a'
          Symbol 'n'
          Symbol 'd'
          Symbol ' '
          BoldMarker
          Symbol 's'
          Symbol 'e'
          Symbol 'c'
          Symbol 'o'
          Symbol 'n'
          Symbol 'd'
          BoldMarker ]

    let result = parse tokens
    let expected = [ Bold "first"; Text " and "; Bold "second" ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Empty bold section`` () =
    let tokens = [ BoldMarker; BoldMarker ]
    let result = parse tokens
    let expected = []
    Assert.Equal<HtmlElement list>(expected, result)
