using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common.Communication
{

    internal enum TokenType
    {
        Separator,
        SingleQuote,
        DoubleQoute,
        Value
    }

    internal class Token
    {
        public Token(TokenType type, string value)
        {
            Value = value;
            Type = type;
        }

        public String Value { get; private set; }
        public TokenType Type { get; private set; }
    }

    internal class StreamTokenizer : IEnumerable<Token>
    {
        private TextReader reader;

        public StreamTokenizer(TextReader reader)
        {
            this.reader = reader;
        }

        public IEnumerator<Token> GetEnumerator()
        {
            string line;
            StringBuilder value = new StringBuilder();

            while ((line = reader.ReadLine()) != null)
            {
                foreach (char c in line)
                {
                    switch (c)
                    {
                        case '\'':
                        case '"':
                            if (value.Length > 0)
                            {
                                yield return new Token(TokenType.Value, value.ToString());
                                value.Length = 0;
                            }
                            yield return new Token(c =='"' ?TokenType.DoubleQoute : TokenType.SingleQuote, c.ToString());
                            break;
                        case ',':
                        case ':':
                        case ' ':
                            if (value.Length > 0)
                            {
                                yield return new Token(TokenType.Value, value.ToString());
                                value.Length = 0;
                            }
                            yield return new Token(TokenType.Separator, c.ToString());
                            break;
                        default:
                            value.Append(c);
                            break;
                    }
                }
                if (value.Length > 0)
                {
                    yield return new Token(TokenType.Value, value.ToString());
                    value.Length = 0;
                }
                yield return new Token(TokenType.Separator, string.Empty);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static class StreamReaderExtensions
    {
        public static IEnumerable<string> GetTokens(this StreamReader reader)
        {
            StreamTokenizer tokenizer = new StreamTokenizer(reader);
            return new StringTextParser(tokenizer);
        }
    }

    public static class StringExtensions
    {
        public static IEnumerable<string> GetTokens(this string value)
        {
            StreamTokenizer tokenizer = new StreamTokenizer(new StringReader(value));
            return new StringTextParser(tokenizer);
        }
    }

    internal class StringTextParser : IEnumerable<String>
    {
        private StreamTokenizer tokenizer;

        public StringTextParser(StreamTokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
        }

        public IEnumerator<string> GetEnumerator()
        {
            bool insideQuote = false;
            TokenType quoteType = TokenType.Separator;
            StringBuilder result = new StringBuilder();

            foreach (Token token in tokenizer)
            {
                switch (token.Type)
                {
                    case TokenType.Separator:
                        if (insideQuote)
                        {
                            result.Append(token.Value);
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(token.Value) || result.Length > 0)
                                yield return result.ToString();
                            result.Length = 0;
                        }
                        break;
                    case TokenType.SingleQuote:
                    case TokenType.DoubleQoute:
                        if (!insideQuote)
                        {
                            quoteType = token.Type;
                            insideQuote = true;
                            if (result.Length > 0)
                                yield return result.ToString();
                            result.Length = 0;

                        }
                        else if (token.Type == quoteType)
                        {
                            insideQuote = false;
                            quoteType = TokenType.Separator;
                        }
                        else
                        {
                            result.Append(token.Value);
                        }
                        break;
                    case TokenType.Value:
                        result.Append(token.Value);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown token type: " + token.Type);
                }
            }
            if (result.Length > 0)
            {
                yield return result.ToString();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}