module TokenizerTests

open Notes.Tokenizer
open Xunit

[<Fact>]
let ``Empty line produces only NewLine`` () =
    let result = tokenize ""
    Assert.Equal<MarkdownToken list>([ NewLine ], result)

[<Fact>]
let ``Plain text produces Text tokens and NewLine`` () =
    let result = tokenize "hello world"

    let expected =
        [ Symbol 'h'
          Symbol 'e'
          Symbol 'l'
          Symbol 'l'
          Symbol 'o'
          Symbol ' '
          Symbol 'w'
          Symbol 'o'
          Symbol 'r'
          Symbol 'l'
          Symbol 'd'
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Single hash creates HeaderMarker level 1`` () =
    let result = tokenize "# Title"

    let expected =
        [ HeaderMarker 1; Symbol ' '; Symbol 'T'; Symbol 'i'; Symbol 't'; Symbol 'l'; Symbol 'e'; NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Triple hash creates HeaderMarker level 3`` () =
    let result = tokenize "### Subtitle"

    let expected =
        [ HeaderMarker 3
          Symbol ' '
          Symbol 'S'
          Symbol 'u'
          Symbol 'b'
          Symbol 't'
          Symbol 'i'
          Symbol 't'
          Symbol 'l'
          Symbol 'e'
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Six hashes creates HeaderMarker level 6`` () =
    let result = tokenize "###### Deep Header"

    let expected =
        [ HeaderMarker 6
          Symbol ' '
          Symbol 'D'
          Symbol 'e'
          Symbol 'e'
          Symbol 'p'
          Symbol ' '
          Symbol 'H'
          Symbol 'e'
          Symbol 'a'
          Symbol 'd'
          Symbol 'e'
          Symbol 'r'
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``More than six hashes capped at level 6`` () =
    let result = tokenize "######### Too Many"

    let expected =
        [ HeaderMarker 6
          Symbol '#'
          Symbol '#'
          Symbol '#'
          Symbol ' '
          Symbol 'T'
          Symbol 'o'
          Symbol 'o'
          Symbol ' '
          Symbol 'M'
          Symbol 'a'
          Symbol 'n'
          Symbol 'y'
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Triple dash creates MetaMarker`` () =
    let result = tokenize "---"
    let expected = [ MetaMarker; NewLine ]
    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Meta with content`` () =
    let result = tokenize "---content"

    let expected =
        [ MetaMarker
          Symbol 'c'
          Symbol 'o'
          Symbol 'n'
          Symbol 't'
          Symbol 'e'
          Symbol 'n'
          Symbol 't'
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Dash space creates ListMarker`` () =
    let result = tokenize "- Item"
    let expected = [ ListMarker; Symbol 'I'; Symbol 't'; Symbol 'e'; Symbol 'm'; NewLine ]
    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Dash without space is plain text`` () =
    let result = tokenize "-Item"
    let expected = [ Symbol '-'; Symbol 'I'; Symbol 't'; Symbol 'e'; Symbol 'm'; NewLine ]
    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Triple backticks without language`` () =
    let result = tokenize "```"
    let expected = [ CodeBlockMarker None; NewLine ]
    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Triple backticks with language`` () =
    let result = tokenize "```fsharp"
    let expected = [ CodeBlockMarker(Some "fsharp"); NewLine ]
    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Double asterisk creates BoldMarker`` () =
    let result = tokenize "**bold**"
    let expected = [ BoldMarker; Symbol 'b'; Symbol 'o'; Symbol 'l'; Symbol 'd'; BoldMarker; NewLine ]
    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Text with bold in middle`` () =
    let result = tokenize "start**bold**end"

    let expected =
        [ Symbol 's'
          Symbol 't'
          Symbol 'a'
          Symbol 'r'
          Symbol 't'
          BoldMarker
          Symbol 'b'
          Symbol 'o'
          Symbol 'l'
          Symbol 'd'
          BoldMarker
          Symbol 'e'
          Symbol 'n'
          Symbol 'd'
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Single backtick creates CodeMarker`` () =
    let result = tokenize "`code`"
    let expected = [ CodeMarker; Symbol 'c'; Symbol 'o'; Symbol 'd'; Symbol 'e'; CodeMarker; NewLine ]
    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Text with inline code`` () =
    let result = tokenize "Use `print()` function"

    let expected =
        [ Symbol 'U'
          Symbol 's'
          Symbol 'e'
          Symbol ' '
          CodeMarker
          Symbol 'p'
          Symbol 'r'
          Symbol 'i'
          Symbol 'n'
          Symbol 't'
          ParenOpen
          ParenClose
          CodeMarker
          Symbol ' '
          Symbol 'f'
          Symbol 'u'
          Symbol 'n'
          Symbol 'c'
          Symbol 't'
          Symbol 'i'
          Symbol 'o'
          Symbol 'n'
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``List item with code`` () =
    let result = tokenize "- Use `git status`"

    let expected =
        [ ListMarker
          Symbol 'U'
          Symbol 's'
          Symbol 'e'
          Symbol ' '
          CodeMarker
          Symbol 'g'
          Symbol 'i'
          Symbol 't'
          Symbol ' '
          Symbol 's'
          Symbol 't'
          Symbol 'a'
          Symbol 't'
          Symbol 'u'
          Symbol 's'
          CodeMarker
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Single asterisk is plain text`` () =
    let result = tokenize "*italic*"

    let expected =
        [ Symbol '*'
          Symbol 'i'
          Symbol 't'
          Symbol 'a'
          Symbol 'l'
          Symbol 'i'
          Symbol 'c'
          Symbol '*'
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Line starting with space and hash`` () =
    let result = tokenize " # Not Header"

    let expected =
        [ Symbol ' '
          Symbol '#'
          Symbol ' '
          Symbol 'N'
          Symbol 'o'
          Symbol 't'
          Symbol ' '
          Symbol 'H'
          Symbol 'e'
          Symbol 'a'
          Symbol 'd'
          Symbol 'e'
          Symbol 'r'
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Only spaces`` () =
    let result = tokenize "   "
    let expected = [ Symbol ' '; Symbol ' '; Symbol ' '; NewLine ]
    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Long text line`` () =
    let longText = String.replicate 100 "a"
    let result = tokenize longText
    let expectedTexts = List.replicate 100 (Symbol 'a')
    let expected = expectedTexts @ [ NewLine ]
    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Multiple bold markers`` () =
    let result = tokenize "**one****two**"

    let expected =
        [ BoldMarker
          Symbol 'o'
          Symbol 'n'
          Symbol 'e'
          BoldMarker
          BoldMarker
          Symbol 't'
          Symbol 'w'
          Symbol 'o'
          BoldMarker
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Regular image at line start`` () =
    let result = tokenize "![alt text](image.png)"

    let expected =
        [ ImageMarker
          Symbol 'a'
          Symbol 'l'
          Symbol 't'
          Symbol ' '
          Symbol 't'
          Symbol 'e'
          Symbol 'x'
          Symbol 't'
          SquareBracketClose
          ParenOpen
          Symbol 'i'
          Symbol 'm'
          Symbol 'a'
          Symbol 'g'
          Symbol 'e'
          Symbol '.'
          Symbol 'p'
          Symbol 'n'
          Symbol 'g'
          ParenClose
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Local image at line start`` () =
    let result = tokenize "![[local-image]]"

    let expected =
        [ LocalImageStart
          Symbol 'l'
          Symbol 'o'
          Symbol 'c'
          Symbol 'a'
          Symbol 'l'
          Symbol '-'
          Symbol 'i'
          Symbol 'm'
          Symbol 'a'
          Symbol 'g'
          Symbol 'e'
          LocalImageEnd
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Regular image in middle of line`` () =
    let result = tokenize "Check ![alt](img.jpg) here"

    let expected =
        [ Symbol 'C'
          Symbol 'h'
          Symbol 'e'
          Symbol 'c'
          Symbol 'k'
          Symbol ' '
          ImageMarker
          Symbol 'a'
          Symbol 'l'
          Symbol 't'
          SquareBracketClose
          ParenOpen
          Symbol 'i'
          Symbol 'm'
          Symbol 'g'
          Symbol '.'
          Symbol 'j'
          Symbol 'p'
          Symbol 'g'
          ParenClose
          Symbol ' '
          Symbol 'h'
          Symbol 'e'
          Symbol 'r'
          Symbol 'e'
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Local image in middle of line`` () =
    let result = tokenize "See ![[diagram]] above"

    let expected =
        [ Symbol 'S'
          Symbol 'e'
          Symbol 'e'
          Symbol ' '
          LocalImageStart
          Symbol 'd'
          Symbol 'i'
          Symbol 'a'
          Symbol 'g'
          Symbol 'r'
          Symbol 'a'
          Symbol 'm'
          LocalImageEnd
          Symbol ' '
          Symbol 'a'
          Symbol 'b'
          Symbol 'o'
          Symbol 'v'
          Symbol 'e'
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Square brackets tokenized correctly`` () =
    let result = tokenize "[link text]"

    let expected =
        [ SquareBracketOpen
          Symbol 'l'
          Symbol 'i'
          Symbol 'n'
          Symbol 'k'
          Symbol ' '
          Symbol 't'
          Symbol 'e'
          Symbol 'x'
          Symbol 't'
          SquareBracketClose
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Parentheses tokenized correctly`` () =
    let result = tokenize "(some content)"

    let expected =
        [ ParenOpen
          Symbol 's'
          Symbol 'o'
          Symbol 'm'
          Symbol 'e'
          Symbol ' '
          Symbol 'c'
          Symbol 'o'
          Symbol 'n'
          Symbol 't'
          Symbol 'e'
          Symbol 'n'
          Symbol 't'
          ParenClose
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Complete link structure`` () =
    let result = tokenize "[text](url)"

    let expected =
        [ SquareBracketOpen
          Symbol 't'
          Symbol 'e'
          Symbol 'x'
          Symbol 't'
          SquareBracketClose
          ParenOpen
          Symbol 'u'
          Symbol 'r'
          Symbol 'l'
          ParenClose
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Link with bold text`` () =
    let result = tokenize "[**bold link**](url)"

    let expected =
        [ SquareBracketOpen
          BoldMarker
          Symbol 'b'
          Symbol 'o'
          Symbol 'l'
          Symbol 'd'
          Symbol ' '
          Symbol 'l'
          Symbol 'i'
          Symbol 'n'
          Symbol 'k'
          BoldMarker
          SquareBracketClose
          ParenOpen
          Symbol 'u'
          Symbol 'r'
          Symbol 'l'
          ParenClose
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Image with code in alt text`` () =
    let result = tokenize "![`code` alt](image.png)"

    let expected =
        [ ImageMarker
          CodeMarker
          Symbol 'c'
          Symbol 'o'
          Symbol 'd'
          Symbol 'e'
          CodeMarker
          Symbol ' '
          Symbol 'a'
          Symbol 'l'
          Symbol 't'
          SquareBracketClose
          ParenOpen
          Symbol 'i'
          Symbol 'm'
          Symbol 'a'
          Symbol 'g'
          Symbol 'e'
          Symbol '.'
          Symbol 'p'
          Symbol 'n'
          Symbol 'g'
          ParenClose
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Incomplete image syntax`` () =
    let result = tokenize "![incomplete"

    let expected =
        [ ImageMarker
          Symbol 'i'
          Symbol 'n'
          Symbol 'c'
          Symbol 'o'
          Symbol 'm'
          Symbol 'p'
          Symbol 'l'
          Symbol 'e'
          Symbol 't'
          Symbol 'e'
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Incomplete local image`` () =
    let result = tokenize "![[incomplete"

    let expected =
        [ LocalImageStart
          Symbol 'i'
          Symbol 'n'
          Symbol 'c'
          Symbol 'o'
          Symbol 'm'
          Symbol 'p'
          Symbol 'l'
          Symbol 'e'
          Symbol 't'
          Symbol 'e'
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Nested brackets`` () =
    let result = tokenize "[[nested]]"

    let expected =
        [ SquareBracketOpen
          SquareBracketOpen
          Symbol 'n'
          Symbol 'e'
          Symbol 's'
          Symbol 't'
          Symbol 'e'
          Symbol 'd'
          LocalImageEnd
          NewLine ]

    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Multiple closing brackets`` () =
    let result = tokenize "]]]"
    let expected = [ LocalImageEnd; SquareBracketClose; NewLine ]
    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Empty brackets`` () =
    let result = tokenize "[]"
    let expected = [ SquareBracketOpen; SquareBracketClose; NewLine ]
    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Empty parentheses`` () =
    let result = tokenize "()"
    let expected = [ ParenOpen; ParenClose; NewLine ]
    Assert.Equal<MarkdownToken list>(expected, result)

[<Fact>]
let ``Image at start vs image in middle have different tokenization`` () =
    let startResult = tokenize "![alt](url)"
    let middleResult = tokenize "x![alt](url)"

    let startExpected =
        [ ImageMarker
          Symbol 'a'
          Symbol 'l'
          Symbol 't'
          SquareBracketClose
          ParenOpen
          Symbol 'u'
          Symbol 'r'
          Symbol 'l'
          ParenClose
          NewLine ]

    let middleExpected =
        [ Symbol 'x'
          ImageMarker
          Symbol 'a'
          Symbol 'l'
          Symbol 't'
          SquareBracketClose
          ParenOpen
          Symbol 'u'
          Symbol 'r'
          Symbol 'l'
          ParenClose
          NewLine ]

    Assert.Equal<MarkdownToken list>(startExpected, startResult)
    Assert.Equal<MarkdownToken list>(middleExpected, middleResult)
