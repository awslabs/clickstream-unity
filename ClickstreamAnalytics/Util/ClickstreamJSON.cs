/*
 * MIT License.
 * Copyright (c) 2013 Calvin Rien Based on the JSON parser by Patrick van Bergen
 * http://techblog.procurios.nl/k/618/news/view/14605/14863/How-do-I-write-my-own-parser-for-JSON.html
 * We modified it to meet the data collection of Clickstream Unity SDK
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.IO;

namespace ClickstreamAnalytics.Util
{
    /// <summary>
    /// This class encodes and decodes JSON strings.
    /// Spec. details, see http://www.json.org/
    /// 
    /// JSON uses Arrays and Objects. These correspond here two the datatypes ArrayList and Hashtable.
    /// All numbers are parsed to floats.
    /// </summary>
    internal static class ClickstreamJson
    {
        /// <summary>
        /// Parses the string json into a value
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <returns>A List&lt;object&gt;, a Dictionary&lt;string, object&gt;, a double, an integer,a string, null, true, or false</returns>
        public static Dictionary<string, object> Deserialize(string json)
        {
            // save the string for debug information
            return json == null ? null : Parser.Parse(json);
        }

        private sealed class Parser : IDisposable
        {
            private const string WordBreak = "{}[],:\"";

            private static bool IsWordBreak(char c)
            {
                return char.IsWhiteSpace(c) || WordBreak.IndexOf(c) != -1;
            }

            private enum Token
            {
                None,
                CurlyOpen,
                CurlyClose,
                SquaredOpen,
                SquaredClose,
                Colon,
                Comma,
                String,
                Number,
                True,
                False,
                Null
            };

            private StringReader _json;

            private Parser(string jsonString)
            {
                _json = new StringReader(jsonString);
            }

            public static Dictionary<string, object> Parse(string jsonString)
            {
                using var instance = new Parser(jsonString);
                return instance.ParseObject();
            }

            public void Dispose()
            {
                _json.Dispose();
                _json = null;
            }

            private Dictionary<string, object> ParseObject()
            {
                var table = new Dictionary<string, object>();

                // ditch opening brace
                _json.Read();

                // {
                while (true)
                {
                    switch (NextToken)
                    {
                        case Token.None:
                            return null;
                        case Token.Comma:
                            continue;
                        case Token.CurlyClose:
                            return table;
                        case Token.CurlyOpen:
                        case Token.SquaredOpen:
                        case Token.SquaredClose:
                        case Token.Colon:
                        case Token.String:
                        case Token.Number:
                        case Token.True:
                        case Token.False:
                        case Token.Null:
                        default:
                            // name
                            var name = ParseString();
                            if (name == null)
                            {
                                return null;
                            }

                            // :
                            if (NextToken != Token.Colon)
                            {
                                return null;
                            }

                            // ditch the colon
                            _json.Read();

                            // value
                            table[name] = ParseValue();
                            break;
                    }
                }
            }

            private List<object> ParseArray()
            {
                var array = new List<object>();

                // ditch opening bracket
                _json.Read();

                // [
                var parsing = true;
                while (parsing)
                {
                    var nextToken = NextToken;

                    switch (nextToken)
                    {
                        case Token.None:
                            return null;
                        case Token.Comma:
                            continue;
                        case Token.SquaredClose:
                            parsing = false;
                            break;
                        case Token.CurlyOpen:
                        case Token.CurlyClose:
                        case Token.SquaredOpen:
                        case Token.Colon:
                        case Token.String:
                        case Token.Number:
                        case Token.True:
                        case Token.False:
                        case Token.Null:
                        default:
                            var value = ParseByToken(nextToken);
                            array.Add(value);
                            break;
                    }
                }

                return array;
            }

            private object ParseValue()
            {
                var nextToken = NextToken;
                return ParseByToken(nextToken);
            }

            private object ParseByToken(Token token)
            {
                switch (token)
                {
                    case Token.String:
                        var str = ParseString();
                        if (DateTime.TryParseExact(str, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture,
                                DateTimeStyles.None, out var dateTime))
                        {
                            return dateTime;
                        }

                        return str;
                    case Token.Number:
                        return ParseNumber();
                    case Token.CurlyOpen:
                        return ParseObject();
                    case Token.SquaredOpen:
                        return ParseArray();
                    case Token.True:
                        return true;
                    case Token.False:
                        return false;
                    case Token.Null:
                        return null;
                    case Token.None:
                    case Token.CurlyClose:
                    case Token.SquaredClose:
                    case Token.Colon:
                    case Token.Comma:
                    default:
                        return null;
                }
            }

            private string ParseString()
            {
                var s = new StringBuilder();

                // ditch opening quote
                _json.Read();

                var parsing = true;
                while (parsing)
                {
                    if (_json.Peek() == -1)
                    {
                        break;
                    }

                    var c = NextChar;
                    switch (c)
                    {
                        case '"':
                            parsing = false;
                            break;
                        case '\\':
                            if (_json.Peek() == -1)
                            {
                                parsing = false;
                                break;
                            }

                            c = NextChar;
                            switch (c)
                            {
                                case '"':
                                case '\\':
                                case '/':
                                    s.Append(c);
                                    break;
                                case 'b':
                                    s.Append('\b');
                                    break;
                                case 'f':
                                    s.Append('\f');
                                    break;
                                case 'n':
                                    s.Append('\n');
                                    break;
                                case 'r':
                                    s.Append('\r');
                                    break;
                                case 't':
                                    s.Append('\t');
                                    break;
                                case 'u':
                                    var hex = new char[4];

                                    for (var i = 0; i < 4; i++)
                                    {
                                        hex[i] = NextChar;
                                    }

                                    s.Append((char)Convert.ToInt32(new string(hex), 16));
                                    break;
                            }

                            break;
                        default:
                            s.Append(c);
                            break;
                    }
                }

                return s.ToString();
            }

            private object ParseNumber()
            {
                var number = NextWord;

                if (number.IndexOf('.') == -1)
                {
                    long.TryParse(number, out var parsedInt);
                    return parsedInt;
                }

                if (!double.TryParse(number, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                        CultureInfo.InvariantCulture, out var parsedDouble))
                {
                    double.TryParse(number, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                        CultureInfo.CreateSpecificCulture("es-ES"), out parsedDouble);
                }

                return parsedDouble;
            }

            private void EatWhitespace()
            {
                while (char.IsWhiteSpace(PeekChar))
                {
                    _json.Read();

                    if (_json.Peek() == -1)
                    {
                        break;
                    }
                }
            }

            private char PeekChar => Convert.ToChar(_json.Peek());

            private char NextChar => Convert.ToChar(_json.Read());

            private string NextWord
            {
                get
                {
                    var word = new StringBuilder();

                    while (!IsWordBreak(PeekChar))
                    {
                        word.Append(NextChar);

                        if (_json.Peek() == -1)
                        {
                            break;
                        }
                    }

                    return word.ToString();
                }
            }

            private Token NextToken
            {
                get
                {
                    EatWhitespace();

                    if (_json.Peek() == -1)
                    {
                        return Token.None;
                    }

                    switch (PeekChar)
                    {
                        case '{':
                            return Token.CurlyOpen;
                        case '}':
                            _json.Read();
                            return Token.CurlyClose;
                        case '[':
                            return Token.SquaredOpen;
                        case ']':
                            _json.Read();
                            return Token.SquaredClose;
                        case ',':
                            _json.Read();
                            return Token.Comma;
                        case '"':
                            return Token.String;
                        case ':':
                            return Token.Colon;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                        case '-':
                            return Token.Number;
                    }

                    return NextWord switch
                    {
                        "false" => Token.False,
                        "true" => Token.True,
                        "null" => Token.Null,
                        _ => Token.None
                    };
                }
            }
        }


        /// <summary>
        /// Converts a IDictionary / IList object or a simple type (string, int, etc.) into a JSON string
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
        public static string Serialize(object obj)
        {
            return Serializer.SerializeObj(obj);
        }

        private sealed class Serializer
        {
            private readonly StringBuilder _builder;

            private Serializer()
            {
                _builder = new StringBuilder();
            }

            public static string SerializeObj(object obj)
            {
                var instance = new Serializer();
                instance.SerializeValue(obj);
                return instance._builder.ToString();
            }

            private void SerializeValue(object value)
            {
                IList asList;
                IDictionary asDict;
                string asStr;

                if (value == null)
                {
                    _builder.Append("null");
                }
                else if ((asStr = value as string) != null)
                {
                    SerializeString(asStr);
                }
                else if (value is bool b)
                {
                    _builder.Append(b ? "true" : "false");
                }
                else if ((asList = value as IList) != null)
                {
                    SerializeArray(asList);
                }
                else if ((asDict = value as IDictionary) != null)
                {
                    SerializeObject(asDict);
                }
                else if (value is char c)
                {
                    SerializeString(new string(c, 1));
                }
                else
                {
                    SerializeOther(value);
                }
            }

            private void SerializeObject(IDictionary obj)
            {
                var first = true;

                _builder.Append('{');

                foreach (var e in obj.Keys)
                {
                    if (!first)
                    {
                        _builder.Append(',');
                    }

                    SerializeString(e.ToString());
                    _builder.Append(':');

                    SerializeValue(obj[e]);

                    first = false;
                }

                _builder.Append('}');
            }

            private void SerializeArray(IList anArray)
            {
                _builder.Append('[');

                var first = true;

                foreach (var obj in anArray)
                {
                    if (!first)
                    {
                        _builder.Append(',');
                    }

                    SerializeValue(obj);

                    first = false;
                }

                _builder.Append(']');
            }

            private void SerializeString(string str)
            {
                _builder.Append('\"');

                var charArray = str.ToCharArray();
                foreach (var c in charArray)
                {
                    switch (c)
                    {
                        case '"':
                            _builder.Append("\\\"");
                            break;
                        case '\\':
                            _builder.Append("\\\\");
                            break;
                        case '\b':
                            _builder.Append("\\b");
                            break;
                        case '\f':
                            _builder.Append("\\f");
                            break;
                        case '\n':
                            _builder.Append("\\n");
                            break;
                        case '\r':
                            _builder.Append("\\r");
                            break;
                        case '\t':
                            _builder.Append("\\t");
                            break;
                        default:
                            if (char.IsControl(c))
                            {
                                var codepoint = Convert.ToInt32(c);
                                _builder.Append("\\u");
                                _builder.Append(codepoint.ToString("x4"));
                            }
                            else
                            {
                                _builder.Append(c);
                            }

                            break;
                    }
                }

                _builder.Append('\"');
            }

            private void SerializeOther(object value)
            {
                switch (value)
                {
                    case float f:
                        _builder.Append(f.ToString("R", CultureInfo.InvariantCulture));
                        break;
                    case int:
                    case uint:
                    case long:
                    case sbyte:
                    case byte:
                    case short:
                    case ushort:
                    case ulong:
                        _builder.Append(value);
                        break;
                    case double:
                        _builder.Append(Convert.ToDouble(value)
                            .ToString("R", CultureInfo.InvariantCulture));
                        break;
                    case decimal:
                        _builder.Append(Convert.ToDecimal(value)
                            .ToString("G", CultureInfo.InvariantCulture));
                        break;
                    default:
                        SerializeString(value.ToString());
                        break;
                }
            }
        }
    }
}