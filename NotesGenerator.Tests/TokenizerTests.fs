module TokenizerTests

open NotesGenerator.Lexer
open Xunit

[<Fact>]
let ``Empty line produces only NewLine`` () =
    let result = tokenize ""
    Assert.Equal<MarkdownTokens list>([ NewLine ], result)

[<Fact>]
let ``Plain text produces Text tokens and NewLine`` () =
    let result = tokenize "hello world"

    let expected =
        [ Text "h"
          Text "e"
          Text "l"
          Text "l"
          Text "o"
          Text " "
          Text "w"
          Text "o"
          Text "r"
          Text "l"
          Text "d"
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Single hash creates HeaderMarker level 1`` () =
    let result = tokenize "# Title"
    let expected = [ HeaderMarker 1; Text " "; Text "T"; Text "i"; Text "t"; Text "l"; Text "e"; NewLine ]
    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Triple hash creates HeaderMarker level 3`` () =
    let result = tokenize "### Subtitle"

    let expected =
        [ HeaderMarker 3
          Text " "
          Text "S"
          Text "u"
          Text "b"
          Text "t"
          Text "i"
          Text "t"
          Text "l"
          Text "e"
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Six hashes creates HeaderMarker level 6`` () =
    let result = tokenize "###### Deep Header"

    let expected =
        [ HeaderMarker 6
          Text " "
          Text "D"
          Text "e"
          Text "e"
          Text "p"
          Text " "
          Text "H"
          Text "e"
          Text "a"
          Text "d"
          Text "e"
          Text "r"
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``More than six hashes capped at level 6`` () =
    let result = tokenize "######### Too Many"

    let expected =
        [ HeaderMarker 6
          Text "#"
          Text "#"
          Text "#"
          Text " "
          Text "T"
          Text "o"
          Text "o"
          Text " "
          Text "M"
          Text "a"
          Text "n"
          Text "y"
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Triple dash creates MetaMarker`` () =
    let result = tokenize "---"
    let expected = [ MetaMarker; NewLine ]
    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Meta with content`` () =
    let result = tokenize "---content"

    let expected =
        [ MetaMarker; Text "c"; Text "o"; Text "n"; Text "t"; Text "e"; Text "n"; Text "t"; NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Dash space creates ListMarker`` () =
    let result = tokenize "- Item"
    let expected = [ ListMarker; Text "I"; Text "t"; Text "e"; Text "m"; NewLine ]
    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Dash without space is plain text`` () =
    let result = tokenize "-Item"
    let expected = [ Text "-"; Text "I"; Text "t"; Text "e"; Text "m"; NewLine ]
    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Triple backticks without language`` () =
    let result = tokenize "```"
    let expected = [ CodeBlockMarker None; NewLine ]
    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Triple backticks with language`` () =
    let result = tokenize "```fsharp"
    let expected = [ CodeBlockMarker(Some "fsharp"); NewLine ]
    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Triple backticks with language and spaces`` () =
    let result = tokenize "```  python  "
    let expected = [ CodeBlockMarker(Some "python"); NewLine ]
    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Double asterisk creates BoldMarker`` () =
    let result = tokenize "**bold**"
    let expected = [ BoldMarker; Text "b"; Text "o"; Text "l"; Text "d"; BoldMarker; NewLine ]
    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Text with bold in middle`` () =
    let result = tokenize "start**bold**end"

    let expected =
        [ Text "s"
          Text "t"
          Text "a"
          Text "r"
          Text "t"
          BoldMarker
          Text "b"
          Text "o"
          Text "l"
          Text "d"
          BoldMarker
          Text "e"
          Text "n"
          Text "d"
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Single backtick creates CodeMarker`` () =
    let result = tokenize "`code`"
    let expected = [ CodeMarker; Text "c"; Text "o"; Text "d"; Text "e"; CodeMarker; NewLine ]
    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Text with inline code`` () =
    let result = tokenize "Use `print()` function"

    let expected =
        [ Text "U"
          Text "s"
          Text "e"
          Text " "
          CodeMarker
          Text "p"
          Text "r"
          Text "i"
          Text "n"
          Text "t"
          ParenOpen
          ParenClose
          CodeMarker
          Text " "
          Text "f"
          Text "u"
          Text "n"
          Text "c"
          Text "t"
          Text "i"
          Text "o"
          Text "n"
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Header with bold text`` () =
    let result = tokenize "## **Bold Header**"

    let expected =
        [ HeaderMarker 2
          Text " "
          BoldMarker
          Text "B"
          Text "o"
          Text "l"
          Text "d"
          Text " "
          Text "H"
          Text "e"
          Text "a"
          Text "d"
          Text "e"
          Text "r"
          BoldMarker
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``List item with code`` () =
    let result = tokenize "- Use `git status`"

    let expected =
        [ ListMarker
          Text "U"
          Text "s"
          Text "e"
          Text " "
          CodeMarker
          Text "g"
          Text "i"
          Text "t"
          Text " "
          Text "s"
          Text "t"
          Text "a"
          Text "t"
          Text "u"
          Text "s"
          CodeMarker
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Single asterisk is plain text`` () =
    let result = tokenize "*italic*"

    let expected =
        [ Text "*"; Text "i"; Text "t"; Text "a"; Text "l"; Text "i"; Text "c"; Text "*"; NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Line starting with space and hash`` () =
    let result = tokenize " # Not Header"

    let expected =
        [ Text " "
          Text "#"
          Text " "
          Text "N"
          Text "o"
          Text "t"
          Text " "
          Text "H"
          Text "e"
          Text "a"
          Text "d"
          Text "e"
          Text "r"
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Only spaces`` () =
    let result = tokenize "   "
    let expected = [ Text " "; Text " "; Text " "; NewLine ]
    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Long text line`` () =
    let longText = String.replicate 100 "a"
    let result = tokenize longText
    let expectedTexts = List.replicate 100 (Text "a")
    let expected = expectedTexts @ [ NewLine ]
    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Multiple bold markers`` () =
    let result = tokenize "**one****two**"

    let expected =
        [ BoldMarker
          Text "o"
          Text "n"
          Text "e"
          BoldMarker
          BoldMarker
          Text "t"
          Text "w"
          Text "o"
          BoldMarker
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Regular image at line start`` () =
    let result = tokenize "![alt text](image.png)"

    let expected =
        [ ImageMarker
          Text "a"
          Text "l"
          Text "t"
          Text " "
          Text "t"
          Text "e"
          Text "x"
          Text "t"
          SquareBracketClose
          ParenOpen
          Text "i"
          Text "m"
          Text "a"
          Text "g"
          Text "e"
          Text "."
          Text "p"
          Text "n"
          Text "g"
          ParenClose
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Local image at line start`` () =
    let result = tokenize "![[local-image]]"

    let expected =
        [ LocalImageStart
          Text "l"
          Text "o"
          Text "c"
          Text "a"
          Text "l"
          Text "-"
          Text "i"
          Text "m"
          Text "a"
          Text "g"
          Text "e"
          LocalImageEnd
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Local image takes precedence over regular image`` () =
    let result = tokenize "![[not ![regular"

    let expected =
        [ LocalImageStart
          Text "n"
          Text "o"
          Text "t"
          Text " "
          ImageMarker
          Text "r"
          Text "e"
          Text "g"
          Text "u"
          Text "l"
          Text "a"
          Text "r"
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Regular image in middle of line`` () =
    let result = tokenize "Check ![alt](img.jpg) here"

    let expected =
        [ Text "C"
          Text "h"
          Text "e"
          Text "c"
          Text "k"
          Text " "
          ImageMarker
          Text "a"
          Text "l"
          Text "t"
          SquareBracketClose
          ParenOpen
          Text "i"
          Text "m"
          Text "g"
          Text "."
          Text "j"
          Text "p"
          Text "g"
          ParenClose
          Text " "
          Text "h"
          Text "e"
          Text "r"
          Text "e"
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Local image in middle of line`` () =
    let result = tokenize "See ![[diagram]] above"

    let expected =
        [ Text "S"
          Text "e"
          Text "e"
          Text " "
          LocalImageStart
          Text "d"
          Text "i"
          Text "a"
          Text "g"
          Text "r"
          Text "a"
          Text "m"
          LocalImageEnd
          Text " "
          Text "a"
          Text "b"
          Text "o"
          Text "v"
          Text "e"
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Square brackets tokenized correctly`` () =
    let result = tokenize "[link text]"

    let expected =
        [ SquareBracketOpen
          Text "l"
          Text "i"
          Text "n"
          Text "k"
          Text " "
          Text "t"
          Text "e"
          Text "x"
          Text "t"
          SquareBracketClose
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Parentheses tokenized correctly`` () =
    let result = tokenize "(some content)"

    let expected =
        [ ParenOpen
          Text "s"
          Text "o"
          Text "m"
          Text "e"
          Text " "
          Text "c"
          Text "o"
          Text "n"
          Text "t"
          Text "e"
          Text "n"
          Text "t"
          ParenClose
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Complete link structure`` () =
    let result = tokenize "[text](url)"

    let expected =
        [ SquareBracketOpen
          Text "t"
          Text "e"
          Text "x"
          Text "t"
          SquareBracketClose
          ParenOpen
          Text "u"
          Text "r"
          Text "l"
          ParenClose
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Link with bold text`` () =
    let result = tokenize "[**bold link**](url)"

    let expected =
        [ SquareBracketOpen
          BoldMarker
          Text "b"
          Text "o"
          Text "l"
          Text "d"
          Text " "
          Text "l"
          Text "i"
          Text "n"
          Text "k"
          BoldMarker
          SquareBracketClose
          ParenOpen
          Text "u"
          Text "r"
          Text "l"
          ParenClose
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Image with code in alt text`` () =
    let result = tokenize "![`code` alt](image.png)"

    let expected =
        [ ImageMarker
          CodeMarker
          Text "c"
          Text "o"
          Text "d"
          Text "e"
          CodeMarker
          Text " "
          Text "a"
          Text "l"
          Text "t"
          SquareBracketClose
          ParenOpen
          Text "i"
          Text "m"
          Text "a"
          Text "g"
          Text "e"
          Text "."
          Text "p"
          Text "n"
          Text "g"
          ParenClose
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Incomplete image syntax`` () =
    let result = tokenize "![incomplete"

    let expected =
        [ ImageMarker
          Text "i"
          Text "n"
          Text "c"
          Text "o"
          Text "m"
          Text "p"
          Text "l"
          Text "e"
          Text "t"
          Text "e"
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Incomplete local image`` () =
    let result = tokenize "![[incomplete"

    let expected =
        [ LocalImageStart
          Text "i"
          Text "n"
          Text "c"
          Text "o"
          Text "m"
          Text "p"
          Text "l"
          Text "e"
          Text "t"
          Text "e"
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Nested brackets`` () =
    let result = tokenize "[[nested]]"

    let expected =
        [ SquareBracketOpen
          SquareBracketOpen
          Text "n"
          Text "e"
          Text "s"
          Text "t"
          Text "e"
          Text "d"
          LocalImageEnd
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Multiple closing brackets`` () =
    let result = tokenize "]]]"
    let expected = [ LocalImageEnd; SquareBracketClose; NewLine ]
    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Empty brackets`` () =
    let result = tokenize "[]"
    let expected = [ SquareBracketOpen; SquareBracketClose; NewLine ]
    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Empty parentheses`` () =
    let result = tokenize "()"
    let expected = [ ParenOpen; ParenClose; NewLine ]
    Assert.Equal<MarkdownTokens list>(expected, result)

[<Fact>]
let ``Image at start vs image in middle have different tokenization`` () =
    let startResult = tokenize "![alt](url)"
    let middleResult = tokenize "x![alt](url)"

    let startExpected =
        [ ImageMarker
          Text "a"
          Text "l"
          Text "t"
          SquareBracketClose
          ParenOpen
          Text "u"
          Text "r"
          Text "l"
          ParenClose
          NewLine ]

    let middleExpected =
        [ Text "x"
          ImageMarker
          Text "a"
          Text "l"
          Text "t"
          SquareBracketClose
          ParenOpen
          Text "u"
          Text "r"
          Text "l"
          ParenClose
          NewLine ]

    Assert.Equal<MarkdownTokens list>(startExpected, startResult)
    Assert.Equal<MarkdownTokens list>(middleExpected, middleResult)

[<Fact>]
let ``Complex markdown line with multiple elements`` () =
    let result = tokenize "Check [**bold link**](url) and ![image](pic.jpg)"

    let expected =
        [ Text "C"
          Text "h"
          Text "e"
          Text "c"
          Text "k"
          Text " "
          SquareBracketOpen
          BoldMarker
          Text "b"
          Text "o"
          Text "l"
          Text "d"
          Text " "
          Text "l"
          Text "i"
          Text "n"
          Text "k"
          BoldMarker
          SquareBracketClose
          ParenOpen
          Text "u"
          Text "r"
          Text "l"
          ParenClose
          Text " "
          Text "a"
          Text "n"
          Text "d"
          Text " "
          ImageMarker
          Text "i"
          Text "m"
          Text "a"
          Text "g"
          Text "e"
          SquareBracketClose
          ParenOpen
          Text "p"
          Text "i"
          Text "c"
          Text "."
          Text "j"
          Text "p"
          Text "g"
          ParenClose
          NewLine ]

    Assert.Equal<MarkdownTokens list>(expected, result)
