using System;
using System.Runtime.InteropServices;

namespace SecondaryLanguageSubtitle;

public static class Utils
{
    public static string SplitLangStr(string str, EngineMain.Language lang)
    {
        if (str == null) return string.Empty;
        var start = 0;
        var end = str.IndexOf('\u2402');
        if (end == -1) return str;
        while ((int)lang != 0)
        {
            start = end + 1;
            end = str.IndexOf('\u2402', start);
            if (end == -1) return str.Substring(start);
            lang--;
        }
        
        return str.Substring(start, end - start);
    }
}