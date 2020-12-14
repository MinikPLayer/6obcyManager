using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


using System.Threading;

public static class MUtil
{

#if DEBUG
    public const bool debug = true;
#else
    public const bool debug = false;
#endif


    public static bool IsLinux
    {
        get
        {
            int p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }
    }

    public static bool IsWindows
    {
        get
        {
            int p = (int)Environment.OSVersion.Platform;
            return (p == 0 || p == 1 || p == 2 || p == 3);
        }
    }

    /// <summary>
    /// Splits string but without splitting when inside a bracket, formatting: entry1{data1},entry2{data2},entry3{data3}
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static List<string> SplitWithBrackets(string str)
    {
        List<string> packets = new List<string>();
        int level = 0;
        string s = "";
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == ',' && level == 0)
            {
                packets.Add(s);
                s = "";
                continue;
            }

            s += str[i];
            if (str[i] == '{')
            {
                level++;
            }
            else if (str[i] == '}')
            {
                level--;
                if (level < 0)
                {
                    Debug.LogError("[MUtil.SplitWithBrackets] Not expected closing bracket");
                    return null;
                }
            }

        }

        if (level != 0)
        {
            Debug.LogError("[MUtil.SplitWithBrackets] Cannot find closing bracket");
            return null;
        }

        if (s.Length > 0)
        {
            packets.Add(s);
        }

        return packets;
    }

    public static string GetStringToSpecialChar(string str, char specialChar)
    {
        string value = "";

        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == specialChar) return value;
            value += str[i];
        }

        return value;
    }

    /// <summary>
    /// Returns string from special character ( without it )
    /// </summary>
    /// <param name="str"></param>
    /// <param name="specialChar"></param>
    /// <param name="strToSpecialChar"></param>
    /// <returns></returns>
    public static string GetStringToSpecialCharAndDelete(string str, char specialChar, out string strToSpecialChar)
    {
        strToSpecialChar = GetStringToSpecialChar(str, specialChar);

        if (strToSpecialChar.Length >= str.Length)
        {
            return "";
        }
        return str.Remove(0, strToSpecialChar.Length + 1);
    }

    public static bool AskUserYesNo(string action = "do this")
    {
        ConsoleColor orColor = Console.ForegroundColor;

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Are you sure you want to " + action + "? Y - yes, N - no");
            Console.ForegroundColor = orColor;

            ConsoleKeyInfo info = Console.ReadKey();
            if (info.Key == ConsoleKey.Y)
            {
                return true;
            }
            if (info.Key == ConsoleKey.N)
            {
                return false;
            }
        }

    }

    public static List<object[]> FlipSQLData(List<object[]> objects)
    {
        if (objects.Count == 0) return new List<object[]>();

        List<object[]> uObjects = new List<object[]>(objects[0].Length);
        for(int i = 0;i<objects[0].Length; i++)
        {
            object[] data = new object[objects.Count];
            for(int j = 0;j<objects.Count;j++)
            {
                data[j] = objects[j][i];
            }
            uObjects.Add(data);
        }

        return uObjects;
    }

    public static List<string> RemoveEmptyLines(List<string> lines, bool removeAlsoNLandCR = true)
    {
            

        //List<string> linesList = new List<string>(lines);
        for(int i = 0;i< lines.Count;i++)
        {
            if(lines[i].Length == 0)
            {
                lines.RemoveAt(i);
                i--;
                continue;
            }
            if(removeAlsoNLandCR)
            {
                if(lines[i] == "\n" || lines[i] == "\r")
                {
                    lines.RemoveAt(i);
                    i--;
                    continue;
                }
            }
        }

            

        return lines;
    }

    public static string[] RemoveEmptyLines(string[] lines)
    {
        string[] lns = RemoveEmptyLines(new List<string>(lines)).ToArray();
        return lns;
    }

    public static string[] StringToStringArray(string input)
    {
        List<string> array = new List<string>();
        string actualLine = "";
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '\n' || input[i] == '\r')
            {
                array.Add(actualLine);


                actualLine = "";
                continue;
            }

            actualLine += input[i];
        }

        if (actualLine.Length != 0)
        {
            array.Add(actualLine);
        }

        return array.ToArray();
    }

    public static string ByteArrayToString(byte[] array)
    {
        return Convert.ToBase64String(array);
    }

    public static byte[] StringToByteArray(string input)
    {
        return Convert.FromBase64String(input);
    }


    public static bool lastParseError = false;
    public static T Parse<T>(string str, bool exception = false, bool fatalError = false)
    {
        lastParseError = false;

        Type tp = typeof(T);


        string sep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        str = str.Replace(",", sep).Replace(".", sep);

        try
        {
            switch (tp.Name)
            {
                // Int
                case "Int32":
                    return (T)Convert.ChangeType(Int32.Parse(str), typeof(T));

                // Unsigned int
                case "UInt32":
                    return (T)Convert.ChangeType(UInt32.Parse(str), typeof(T));

                // Float
                case "Single":
                    return (T)Convert.ChangeType(Single.Parse(str), typeof(T));

                // Double
                case "Double":
                    return (T)Convert.ChangeType(Double.Parse(str), typeof(T));

                // Long
                case "Int64":
                    return (T)Convert.ChangeType(Int64.Parse(str), typeof(T));

                // Unsigned Long
                case "UInt64":
                    return (T)Convert.ChangeType(UInt64.Parse(str), typeof(T));

                // Decimal
                case "Decimal":
                    return (T)Convert.ChangeType(Decimal.Parse(str), typeof(T));

                // Bool
                case "Boolean":
                    return (T)Convert.ChangeType(Boolean.Parse(str), typeof(T));

                // Byte
                case "Byte":
                    return (T)Convert.ChangeType(Byte.Parse(str), typeof(T));

                // Signed Byte
                case "SByte":
                    return (T)Convert.ChangeType(SByte.Parse(str), typeof(T));

                // Short
                case "Int16":
                    return (T)Convert.ChangeType(Int16.Parse(str), typeof(T));

                // Unsigned Short
                case "UInt16":
                    return (T)Convert.ChangeType(UInt16.Parse(str), typeof(T));

                // Char
                case "Char":
                    return (T)Convert.ChangeType(Char.Parse(str), typeof(T));

                default:
                    Debug.LogError("Type " + tp.Name + " not supported");
                    break;
            }
            //return int.Parse(str);
        }
        catch(Exception e)
        {
            lastParseError = true;

            if(exception)
            {
                throw e;
            }

            Debug.Exception(e);
            if(fatalError)
            {
                Debug.FatalError("Fatal exception occured during conversion, exiting", -1999, 5000);
            }
        }

        return default(T);
    }
}

public class ParsingExcetpion : Exception
{

    public string src = "";
    public object dst;

    public ParsingExcetpion(string _src, object _dst)
        : base("Cannot convert \"" + _src + "\" to " + _dst.GetType().Name)
    {
        src = _src;
        dst = _dst;
    }
}


public static class Debug
{
    static string logPath;

    public static string GetLogPath(bool withFile = true)
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "Minik");
        if(!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        if (withFile)
        {
            path = Path.Combine(path, "log.txt");
        }

        return path;
    }

    public static void Log(object data, ConsoleColor color = ConsoleColor.White, bool newLine = true)
    {
        if(logPath == null)
        {
            logPath = GetLogPath();
        }

        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        try
        {
            string line = "[" + DateTime.Now.ToString() + "] " + data.ToString();
            if (newLine)
            {
                Console.WriteLine(line);
                File.AppendAllText(logPath, line + "\n");
            }
            else
            {
                Console.Write(line);
                File.AppendAllText(logPath, line);
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("Exception when saving data to file: " + e.ToString());
        }
        Console.ForegroundColor = originalColor;
    }

    public static void LogWarning(object data, bool newLine = true)
    {
        /*ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        if (newLine)
        {
            Console.WriteLine("[DEBUG] " + data);
        }
        else
        {
            Console.Write("[DEBUG] " + data);
        }
        Console.ForegroundColor = originalColor;*/
        Log(data, ConsoleColor.DarkYellow, newLine);
    }

    public static void LogError(object data, bool newLine = true)
    {
        /*ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        if (newLine)
        {
            Console.WriteLine("[DEBUG] " + data);
        }
        else
        {
            Console.Write("[DEBUG] " + data);
        }
        Console.ForegroundColor = originalColor;*/
        Log(data, ConsoleColor.Red, newLine);
    }

    /// <summary>
    /// Displays fatal error
    /// </summary>
    /// <param name="data">Message to display</param>
    /// <param name="exitCode">If different from 0 exit application with this code</param>
    public static void FatalError(object data, int exitCode = 0, int sleepTime = 0)
    {
        

        LogError("\n\n\n\n" + "FATAL ERROR:\n" + data);

        if (sleepTime != 0)
        {
            Thread.Sleep(sleepTime);
        }

        if (exitCode != 0)
        {
            Environment.Exit(exitCode);
        }


    }

    public static void ConversionError(string src, string dstName = "", object dst = null)
    {
        if (dst == null)
        {
            LogError("Cannot convert \"" + src + "\"");
            return;
        }

        LogError("Cannot convert \"" + src + "\" to " + dstName + " ( " + dst.GetType().Name + " )");
    }

    public static void Exception(Exception e, string prefix = "")
    {
        if (prefix.Length == 0)
        {
            prefix = "Exception";
        }
        LogError(prefix + ": " + e.Message);
        LogError(e.StackTrace);

    }
}
