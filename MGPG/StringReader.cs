// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Globalization;
using System.Text;

namespace MGPG
{
    public class StringReader
    {
        private readonly StringBuilder _sb;

        private int _position;
        private TextElementEnumerator _srcEnumerator;
        public string Source { get; }
        public int Line { get; private set; }
        public int Column { get; private set; }

        public bool Eof => Position >= Source.Length;

        public int Position
        {
            get { return _position; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Position must be larger than 0.");
                _position = value;
            }
        }

        public StringReader(string source)
        {
            Source = source;
            Position = 0;
            _sb = new StringBuilder();
            _srcEnumerator = StringInfo.GetTextElementEnumerator(source);
        }

        public char Read()
        {
            if (Eof)
                throw new ArgumentOutOfRangeException(nameof(Position), "Position is larger than Source length.");
            var c = Source[Position];
            Position++;

            if (c == '\n' || (c == '\r' && (Eof || Source[Position] != '\n')))
            {
                Column = 0;
                Line++;
            }
            else
            {
                Column++;
            }
            return c;
        }

        public bool ReadTo(char c, out string value)
        {
            var sb = GetStringBuilder();
            var ret = ReadTo(c, sb);
            value = sb.ToString();
            return ret;
        }

        public bool ReadTo(char c, StringBuilder sb)
        {
            while (!Eof)
            {
                var r = Read();
                if (r == c)
                    return true;
                sb.Append(c);
            }
            return false;
        }

        public bool ReadTo(string s, out string value, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            var sb = GetStringBuilder();
            var ret = ReadTo(s, sb, comparisonType);
            value = sb.ToString();
            return ret;
        }

        public bool ReadTo(string c, StringBuilder sb, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            var len = c.Length;
            while (!Eof)
            {
                var ch = Read();
                sb.Append(ch);
                if (Position - len >= 0 && string.Equals(Source.Substring(Position - len, len), c, comparisonType))
                {
                    // remove the matching string
                    sb.Remove(sb.Length - len, len);
                    return true;
                }
            }
            return false;
        }

        private StringBuilder GetStringBuilder()
        {
            _sb.Clear();
            return _sb;
        }
    }
}