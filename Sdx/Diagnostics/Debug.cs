using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace Sdx.Diagnostics
{
  public class Debug
  {
    private long prevTimerElapsedTicks = 0;

    public static String DumpIndent
    {
      get
      {
        return "  ";
      }
    }

    public TextWriter Out { get; set; }

    public void Log(Object value, String title = "")
    {
      if(Out == null)
      {
        return;
      }
      var currentTicks = Context.Current.Timer.ElapsedTicks;
      var delta = 0L;
      if(prevTimerElapsedTicks > 0)
      {
        delta = currentTicks - prevTimerElapsedTicks;
      }
      Out.WriteLine(String.Format(
        "[{0}/{1} sec] {2}",
        FormatStopwatchTicks(delta),
        FormatStopwatchTicks(currentTicks),
        title
      ));
      prevTimerElapsedTicks = currentTicks;
      Out.WriteLine(Dump(value));
      Out.WriteLine();
      Out.WriteLine();
    }

    private static string GetDumpTitle(object value, string indent, string appendText = "")
    {
      Type type = value.GetType();
      return indent + type.Namespace + "." + type.Name + appendText + Environment.NewLine;
    }

    public static string Dump(object value, string indent = "")
    {
      if (value == null)
      {
        return indent + "NULL";
      }
      else if (value is string)
      {
        string strVal = value as string;
        //stringは型名を付与して改行するのは冗長なのでString(文字数)を付与
        return indent + "String(" + strVal.Length + ") " + strVal;
      }
      else if (value is IDictionary)
      {
        var dic = value as IDictionary;

        var result =  GetDumpTitle(value, indent, "(" + dic.Count + ")");

        foreach (var key in dic.Keys)
        {
          // ここの`Dump(dic[key], " ")`は`:`の後なので常にスペース一個でOK
          result += indent + DumpIndent + key + " :" + Dump(dic[key], " ") + Environment.NewLine;
        }

        //改行を取り除く
        return result.TrimEnd('\r', '\n');
      }
      else if (value is IList)
      {
        IList list = value as IList;
        var result = GetDumpTitle(value, indent, "(" + list.Count + ")");
        return AppendEnumerableDump(result, value as IEnumerable, indent);
      }
      else if (value is IEnumerable)
      {
        var result = GetDumpTitle(value, indent);
        return AppendEnumerableDump(result, value as IEnumerable, indent);
      }
      else
      {
        return GetDumpTitle(value, indent, " " + value.ToString()).TrimEnd('\r', '\n'); ;
      }
    }

    private static String AppendEnumerableDump(String result, IEnumerable values, String indent)
    { 
      foreach (Object obj in values as IEnumerable)
      {
        result += Dump(obj, indent + DumpIndent) + Environment.NewLine;
      }

      //改行を取り除く
      result = result.TrimEnd('\r', '\n');
      return result;
    }

    public static string FormatStopwatchTicks(Int64 ticks, int precision = 8)
    {
      double result = (double)ticks / Stopwatch.Frequency;
      return result.ToString("N" + precision.ToString());
    }

    public static void Console(object value)
    {
      System.Console.WriteLine(Dump(value));
    }
  }
}
