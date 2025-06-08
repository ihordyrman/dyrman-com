module TransformerTests

open Notes.Tokenizer
open Notes.Transformer
open Xunit

[<Fact>]
let ``Simple header level 1`` () =
    let tokens = [ HeaderMarker 1; Symbol 'H'; Symbol 'e'; Symbol 'l'; Symbol 'l'; Symbol 'o'; NewLine ]
    let result = transform tokens
    let expected = [ Header(1, "Hello"); LineBreak ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Simple header level 3`` () =
    let tokens = [ HeaderMarker 3; Symbol 'H'; Symbol 'e'; Symbol 'l'; Symbol 'l'; Symbol 'o'; NewLine ]
    let result = transform tokens
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

    let result = transform tokens
    let expected = [ Header(2, "Hello World"); LineBreak ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Header with only spaces`` () =
    let tokens = [ HeaderMarker 2; Symbol ' '; Symbol ' '; Symbol ' '; NewLine ]
    let result = transform tokens
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

    let result = transform tokens
    let expected = [ Header(1, "First"); LineBreak; Header(2, "Second"); LineBreak ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Header level 6 (maximum)`` () =
    let tokens =
        [ HeaderMarker 6; Symbol 'H'; Symbol 'e'; Symbol 'a'; Symbol 'd'; Symbol 'e'; Symbol 'r'; NewLine ]

    let result = transform tokens
    let expected = [ Header(6, "Header"); LineBreak ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Simple bold text`` () =
    let tokens = [ BoldMarker; Symbol 'b'; Symbol 'o'; Symbol 'l'; Symbol 'd'; BoldMarker ]
    let result = transform tokens
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

    let result = transform tokens
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

    let result = transform tokens
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

    let result = transform tokens
    let expected = [ Bold "first"; Text " and "; Bold "second" ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Empty bold section`` () =
    let tokens = [ BoldMarker; BoldMarker ]
    let result = transform tokens
    let expected = []
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Should transform simple inline code`` () =
    let tokens =
        [ CodeMarker
          Symbol 'p'
          Symbol 'r'
          Symbol 'i'
          Symbol 'n'
          Symbol 't'
          Symbol '('
          Symbol '\''
          Symbol 'h'
          Symbol 'e'
          Symbol 'l'
          Symbol 'l'
          Symbol 'o'
          Symbol '\''
          Symbol ')'
          CodeMarker ]

    let result = transform tokens
    let expected = [ Code "print('hello')" ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Should transform inline code with multiple text tokens`` () =
    let tokens =
        [ CodeMarker
          Symbol 'l'
          Symbol 'e'
          Symbol 't'
          Symbol ' '
          Symbol 'x'
          Symbol ' '
          Symbol '='
          Symbol ' '
          Symbol '4'
          Symbol '2'
          CodeMarker ]

    let result = transform tokens
    let expected = [ Code "let x = 42" ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Should transform multiple inline code blocks`` () =
    let tokens =
        [ CodeMarker
          Symbol 'f'
          Symbol 'o'
          Symbol 'o'
          CodeMarker
          Symbol ' '
          Symbol 'a'
          Symbol 'n'
          Symbol 'd'
          Symbol ' '
          CodeMarker
          Symbol 'b'
          Symbol 'a'
          Symbol 'r'
          CodeMarker ]

    let result = transform tokens
    let expected = [ Code "foo"; Text " and "; Code "bar" ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Should transform code block with language`` () =
    let tokens =
        [ CodeBlockMarker(Some "fsharp")
          NewLine
          Symbol 'l'
          Symbol 'e'
          Symbol 't'
          Symbol ' '
          Symbol 'x'
          Symbol ' '
          Symbol '='
          Symbol ' '
          Symbol '4'
          Symbol '2'
          CodeBlockMarker None ]

    let result = transform tokens
    let expected = [ CodeBlock(Some "fsharp", "let x = 42") ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Should transform code block without language`` () =
    let tokens =
        [ CodeBlockMarker None
          NewLine
          Symbol 'p'
          Symbol 'r'
          Symbol 'i'
          Symbol 'n'
          Symbol 't'
          Symbol '('
          Symbol '"'
          Symbol 'h'
          Symbol 'e'
          Symbol 'l'
          Symbol 'l'
          Symbol 'o'
          Symbol '"'
          Symbol ')'
          CodeBlockMarker None ]

    let result = transform tokens
    let expected = [ CodeBlock(None, "print(\"hello\")") ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Should transform multiple code blocks`` () =
    let tokens =
        [ CodeBlockMarker(Some "python")
          NewLine
          Symbol 'x'
          Symbol ' '
          Symbol '='
          Symbol ' '
          Symbol '1'
          CodeBlockMarker None
          NewLine
          CodeBlockMarker(Some "js")
          NewLine
          Symbol 'y'
          Symbol ' '
          Symbol '='
          Symbol ' '
          Symbol '2'
          CodeBlockMarker None ]

    let result = transform tokens
    let expected = [ CodeBlock(Some "python", "x = 1"); LineBreak; CodeBlock(Some "js", "y = 2") ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Should transform single list item`` () =
    let tokens =
        [ ListMarker
          Symbol 'F'
          Symbol 'i'
          Symbol 'r'
          Symbol 's'
          Symbol 't'
          Symbol ' '
          Symbol 'i'
          Symbol 't'
          Symbol 'e'
          Symbol 'm' ]

    let result = transform tokens
    let expected = [ List [ "First item" ] ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Should transform multiple list items`` () =
    let tokens =
        [ ListMarker
          Symbol 'I'
          Symbol 't'
          Symbol 'e'
          Symbol 'm'
          Symbol ' '
          Symbol '1'
          NewLine
          ListMarker
          Symbol 'I'
          Symbol 't'
          Symbol 'e'
          Symbol 'm'
          Symbol ' '
          Symbol '2'
          NewLine
          ListMarker
          Symbol 'I'
          Symbol 't'
          Symbol 'e'
          Symbol 'm'
          Symbol ' '
          Symbol '3' ]

    let result = transform tokens
    let expected = [ List [ "Item 1"; "Item 2"; "Item 3" ] ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Should transform list with spaces and punctuation`` () =
    let tokens =
        [ ListMarker
          Symbol 'B'
          Symbol 'u'
          Symbol 'y'
          Symbol ' '
          Symbol 'm'
          Symbol 'i'
          Symbol 'l'
          Symbol 'k'
          Symbol ','
          Symbol ' '
          Symbol 'b'
          Symbol 'r'
          Symbol 'e'
          Symbol 'a'
          Symbol 'd'
          NewLine
          ListMarker
          Symbol 'C'
          Symbol 'a'
          Symbol 'l'
          Symbol 'l'
          Symbol ' '
          Symbol 'J'
          Symbol 'o'
          Symbol 'h'
          Symbol 'n'
          Symbol '!' ]

    let result = transform tokens
    let expected = [ List [ "Buy milk, bread"; "Call John!" ] ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Should transform list mixed with other elements`` () =
    let tokens =
        [ Symbol 'H'
          Symbol 'e'
          Symbol 'r'
          Symbol 'e'
          Symbol ':'
          NewLine
          ListMarker
          Symbol 'F'
          Symbol 'i'
          Symbol 'r'
          Symbol 's'
          Symbol 't'
          NewLine
          ListMarker
          Symbol 'S'
          Symbol 'e'
          Symbol 'c'
          Symbol 'o'
          Symbol 'n'
          Symbol 'd'
          NewLine
          Symbol 'D'
          Symbol 'o'
          Symbol 'n'
          Symbol 'e'
          Symbol '.' ]

    let result = transform tokens
    let expected = [ Text "Here:"; LineBreak; List [ "First"; "Second" ]; LineBreak; Text "Done." ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Should parse simple link`` () =
    let tokens =
        [ ImageMarker
          Symbol 'c'
          Symbol 'l'
          Symbol 'i'
          Symbol 'c'
          Symbol 'k'
          Symbol ' '
          Symbol 'h'
          Symbol 'e'
          Symbol 'r'
          Symbol 'e'
          SquareBracketClose
          ParenOpen
          Symbol 'h'
          Symbol 't'
          Symbol 't'
          Symbol 'p'
          Symbol ':'
          Symbol '/'
          Symbol '/'
          Symbol 'e'
          Symbol 'x'
          Symbol 'a'
          Symbol 'm'
          Symbol 'p'
          Symbol 'l'
          Symbol 'e'
          Symbol '.'
          Symbol 'c'
          Symbol 'o'
          Symbol 'm'
          ParenClose ]

    let result = transform tokens
    let expected = [ Link("click here", "http://example.com") ]
    Assert.Equal<HtmlElement list>(expected, result)


[<Fact>]
let ``Should parse link with special characters`` () =
    let tokens =
        [ ImageMarker
          Symbol 'C'
          Symbol 'h'
          Symbol 'e'
          Symbol 'c'
          Symbol 'k'
          Symbol ' '
          Symbol 'R'
          Symbol 'E'
          Symbol 'A'
          Symbol 'D'
          Symbol 'M'
          Symbol 'E'
          Symbol '!'
          SquareBracketClose
          ParenOpen
          Symbol '/'
          Symbol 'd'
          Symbol 'o'
          Symbol 'c'
          Symbol 's'
          Symbol '?'
          Symbol 'v'
          Symbol '='
          Symbol '1'
          ParenClose ]

    let result = transform tokens
    let expected = [ Link("Check README!", "/docs?v=1") ]
    Assert.Equal<HtmlElement list>(expected, result)

[<Fact>]
let ``Should parse multiple links`` () =
    let tokens =
        [ ImageMarker
          Symbol 'F'
          Symbol 'i'
          Symbol 'r'
          Symbol 's'
          Symbol 't'
          SquareBracketClose
          ParenOpen
          Symbol '/'
          Symbol '1'
          ParenClose
          Symbol ' '
          Symbol 'a'
          Symbol 'n'
          Symbol 'd'
          Symbol ' '
          ImageMarker
          Symbol 'S'
          Symbol 'e'
          Symbol 'c'
          Symbol 'o'
          Symbol 'n'
          Symbol 'd'
          SquareBracketClose
          ParenOpen
          Symbol '/'
          Symbol '2'
          ParenClose ]

    let result = transform tokens
    let expected = [ Link("First", "/1"); Text " and "; Link("Second", "/2") ]
    Assert.Equal<HtmlElement list>(expected, result)
