using System;

namespace MGPG
{
    public enum SourceLanguage
    {
        CSharp,
        FSharp,
        VisualBasic
    }

    public static class SourceLanguageEx
    {
        public static string GetFileExtension(this SourceLanguage sl)
        {
            switch (sl)
            {
                case SourceLanguage.CSharp:
                    return "cs";
                case SourceLanguage.FSharp:
                    return "fs";
                case SourceLanguage.VisualBasic:
                    return "vb";
                default:
                    throw new ArgumentOutOfRangeException(nameof(sl), sl, null);
            }
        }
    }
}