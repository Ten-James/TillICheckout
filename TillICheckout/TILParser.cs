using TillICheckout.Nodes;

namespace TillICheckout;

public class TILParser
{
    private class Lexer()
    {
        internal static bool isTypeContext = false;

        internal enum TokenType
        {
            LParen, RParen,
            LBracket, RBracket,
            TOmicron, TIndiv, TTau, TOmega,
            At, Trivialization, Lambda, Comma,
            Indent
        }

        internal record Token(TokenType type, string str)
        {
            public override string ToString()
            {
                return type switch
                {
                    TokenType.LParen => "(",
                    TokenType.RParen => ")",
                    TokenType.LBracket => "[",
                    TokenType.RBracket => "]",
                    TokenType.TOmicron => "o",
                    TokenType.TIndiv => "i",
                    TokenType.TTau => "t",
                    TokenType.TOmega => "w",
                    TokenType.At => "@",
                    TokenType.Trivialization => "'",
                    TokenType.Lambda => "\\",
                    TokenType.Comma => ",",
                    TokenType.Indent => str,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        };

        private static bool CanBeIndent(char c) =>
            c switch
            {
                 ' ' => false,
         '\n' => false,
         '\t' => false,
         '[' => false,
         ']' => false,
         '(' => false,
         ')' => false,
         '@' => false,
         ',' => false,
         '\\' => false,
         '\'' => false,
                _ => true
            };


        private static (Token, string) Next(string rest) {

            switch (rest[0])
            {
                case ' ':
                case '\n':
                case '\t':
                    return Next(rest[1..]);
                case '[':
                    return (new Token(TokenType.LBracket, "["), rest[1..]);
                case ']':
                    return (new Token(TokenType.RBracket, "]"), rest[1..]);
                case '(':
                    return (new Token(TokenType.LParen, "("), rest[1..]);
                case ')':
                    return (new Token(TokenType.RParen, ")"), rest[1..]);
                case '@':
                    return (new Token(TokenType.At, "@"), rest[1..]);
                case ',':
                    return (new Token(TokenType.Comma, ","), rest[1..]);
                case '\\':
                    return (new Token(TokenType.Lambda, "\\"), rest[1..]);
                case '\'':
                    return (new Token(TokenType.Trivialization, "'"), rest[1..]);

            }

            if (!CanBeIndent(rest[0])) throw new ArgumentException();

            if (rest.Length <= 2 || isTypeContext || !CanBeIndent(rest[1]))
            {
                switch (rest[0])
                {
                    case 't':
                        return (new Token(TokenType.TTau, "t"), rest[1..]);
                    case 'w':
                        return (new Token(TokenType.TOmega, "w"), rest[1..]);
                    case 'i':
                        return (new Token(TokenType.TIndiv, "i"), rest[1..]);
                    case 'o':
                        return (new Token(TokenType.TOmicron, "o"), rest[1..]);
                }
            }

            var length = 1;
            while (length < rest.Length)
            {
                if (!CanBeIndent(rest[length])) break;
                length++;
            }

            return (new Token(TokenType.Indent, rest[..length]), rest[length..]);

        }

        internal static IEnumerator<Token> Lex(string sentence)
        {
            while (sentence.Length > 0)
            {
                var (token, rest) = Next(sentence);
                yield return token;
                sentence = rest;
            }
        }
    }

    public static IExpr Parse(string sentence)
    {
        Lexer.isTypeContext = false;
        var iter = Lexer.Lex(sentence);
        iter.MoveNext();
        return ParseExprComposition(iter);

    }

    private static IExpr ParseExprComposition(IEnumerator<Lexer.Token> iter)
    {
        var expr = ParseExpr(iter);
        if (iter.Current.type == Lexer.TokenType.At)
        {
            iter.MoveNext();
            expr = new IntensionalDescent(expr, iter.Current.str);
            iter.MoveNext();
            while (iter.Current.type == Lexer.TokenType.Comma)
            {
                iter.MoveNext();
                expr = new IntensionalDescent(expr, iter.Current.str);
                iter.MoveNext();
            }
        }

        return expr;
    }
    private static IExpr ParseExpr(IEnumerator<Lexer.Token> iter)
    {
        IExpr? expr;
        switch (iter.Current.type)
        {
            case Lexer.TokenType.Indent:
                expr = new Variable(iter.Current.str);
                iter.MoveNext();
                return expr;
            case Lexer.TokenType.Trivialization:
                iter.MoveNext();
                expr = ParseExpr(iter);
                return new Trivialization(expr);
            case Lexer.TokenType.LBracket:
                iter.MoveNext();
                expr = ParseExprComposition(iter);
                List<IExpr> args = [];
                while (iter.Current.type != Lexer.TokenType.RBracket)
                {
                    args.Add(ParseExprComposition(iter));
                }

                iter.MoveNext();
                return new Composition(expr, [.. args]);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    public static TILType ParseType(string sentence)
    {
        Lexer.isTypeContext = true;
        var iter = Lexer.Lex(sentence);
        iter.MoveNext();
        return ParseType(iter);
    }

    private static TILType ParseType(IEnumerator<Lexer.Token> token)
    {
        switch (token.Current.type)
        {
            case Lexer.TokenType.TTau:
                token.MoveNext();
                return TILType.Time();
            case Lexer.TokenType.TOmicron:
                token.MoveNext();
                return TILType.Bool();
            case Lexer.TokenType.TOmega:
                token.MoveNext();
                return TILType.World();
            case Lexer.TokenType.TIndiv:
                token.MoveNext();
                return TILType.Individual();
            case Lexer.TokenType.LParen:
                token.MoveNext();
                var restype = ParseType(token);
                List<TILType> args = [];
                while (token.Current.type != Lexer.TokenType.RParen)
                {
                    args.Add(ParseType(token));
                }

                token.MoveNext();
                return TILType.Function(restype, [.. args]);

            default:
                throw new ArgumentOutOfRangeException();
        }

    }

}