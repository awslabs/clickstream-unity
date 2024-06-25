/*
 * MIT License.  Forked from GA_MiniJSON.
 * I modified it so that it could be used for TD limitations.
 */
// using UnityEngine;

using System;
using System.Collections;
using System.Text;
using System.Globalization;

namespace ClickstreamAnalytics.Util
{
    /* Based on the JSON parser from
     *
     * I simplified it so that it doesn't throw exceptions
     * and can be used in Unity iPhone with maximum code stripping.
     */
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

            void SerializeObject(IDictionary obj)
            {
                bool first = true;

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

            void SerializeArray(IList anArray)
            {
                _builder.Append('[');

                var first = true;

                foreach (object obj in anArray)
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
                    // NOTE: decimals lose precision during serialization.
                    // They always have, I'm just letting you know.
                    // Previously floats and doubles lost precision too.
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