using System;
using System.Collections.Generic;
using System.Linq;

public static class Tokenizers
{
    public static Bimachine CreateForArithmeticExpr()
    {
        const string tokenBoundary = "\n";
        var whitespaces = new[] { ' ', '\t' };
        var digits = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        var operators = new[] { '+', '-', '/', '*' };
        var alphabet = operators.Concat(digits).Concat(whitespaces).ToHashSet();

        var clearWS = FsaBuilder.FromSymbolSet(whitespaces)
            .Plus()
            .Product(FsaBuilder.FromEpsilon())
            .ToLmlRewriter(alphabet);

        var integerFsa = FsaBuilder.FromSymbolSet(digits).Plus();
        var operatorFsa = FsaBuilder.FromSymbolSet(operators);
        var insertIntBoundary = FstBuilder.FromWordPair(string.Empty, $"<INT>{tokenBoundary}");
        var insertOperatorBoundary = FstBuilder.FromWordPair(string.Empty, $"<OP>{tokenBoundary}");
        
        var markTokens = integerFsa
            .Identity()
            .Concat(insertIntBoundary)
            .Union(operatorFsa.Identity().Concat(insertOperatorBoundary))
            .ToLmlRewriter(alphabet);

        return clearWS.Compose(markTokens).ToBimachine(alphabet);
    }
    public static Bimachine CreateForEnglish()
    {
        var alphabet = Enumerable.Range(32, 95).Select(x => (char)x)
            .Concat(new[] { '\t', '\n', '\v', '\f', '\r' })
            .ToHashSet();
        var whitespaces = new[] { ' ', '\t', '\n' };
        var upperCaseLetters = Enumerable.Range(65, 27).Select(x => (char)x);
        var lowerCaseLetters = Enumerable.Range(97, 27).Select(x => (char)x);
        var digits = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        var letters = upperCaseLetters.Concat(lowerCaseLetters);

        Console.WriteLine("Constructing the \"rise case\" transducer.");
        
        var riseCase = alphabet
            .Select(symbol =>
                FstBuilder.FromWordPair(
                    symbol.ToString(),
                    char.IsLower(symbol)
                        ? symbol.ToString().ToUpper()
                        : symbol.ToString()))
            .Aggregate((aggr, fst) => aggr.UnionWith(fst))
            .Star();

        Console.WriteLine("Constructing the \"multi word expression list\" transducer.");
        
        var multiWordExprList = new[] { "AT LEAST", "IN SPITE OF" };
        var multiWordExpr = 
            multiWordExprList
                .Select(exp => FsaBuilder.FromWord(exp))
                .Aggregate((aggr, fsa) => aggr.Union(fsa));

        Console.WriteLine("Constructing the \"token\" transducer.");
        
        var token = 
            FsaBuilder.FromSymbolSet(letters)
            .Plus()
            .Union(
                FsaBuilder.FromSymbolSet(digits).Plus(),
                riseCase.Compose(multiWordExpr.Identity()).Domain(),
                FsaBuilder.FromSymbolSet(alphabet.Except(whitespaces)));

        Console.WriteLine("Constructing the \"insert leading newline\" transducer.");

        var insertLeadingNewLine = 
            FstBuilder.FromWordPair(string.Empty, "\n")
                .Concat(FsaBuilder.FromSymbolSet(alphabet).Star().Identity());

        Console.WriteLine("Constructing the \"clear spaces\" transducer.");
        
        var clearSpaces = 
                FsaBuilder.FromSymbolSet(whitespaces)
                .Plus()
                .Product(FsaBuilder.FromWord(" "))
                .ToLmlRewriter(alphabet);

        Console.WriteLine("Constructing the \"mark tokens\" transducer.");
        
        var markTokens = 
            token.Identity()
                .Concat(FstBuilder.FromWordPair(string.Empty, "\n"))
                .ToLmlRewriter(alphabet);

        Console.WriteLine("Constructing the \"clear leading whitespace\" transducer.");
        
        var clearLeadingSpace = 
            insertLeadingNewLine.Compose(
                FstBuilder.FromWordPair("\n ", "\n").ToRewriter(alphabet),
                insertLeadingNewLine.Inverse());

        Console.WriteLine("Creating the composed transducer.");
        
        var fst = clearSpaces.Compose(markTokens, clearLeadingSpace);
        
        Console.WriteLine("Converting to a bimachine.");
        
        return fst.ToBimachine(alphabet);
    }
}