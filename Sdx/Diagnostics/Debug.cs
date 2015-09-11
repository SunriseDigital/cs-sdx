using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

namespace Sdx.Diagnostics
{
  public class Debug
  {
    private static String logKey = "SDX.DEBUG_TOOL.DEBUG.LOGS_KEY";

    public static String DumpIndent
    {
      get
      {
        return "  ";
      }
    }

    internal static List<Dictionary<String, Object>> Logs
    {
      get
      {
        List<Dictionary<String, Object>> values;
        if (!Sdx.Context.Current.Vars.ContainsKey(logKey))
        {
          values = new List<Dictionary<String, Object>>();
          Sdx.Context.Current.Vars[logKey] = values;
        }
        else
        {
          values = Sdx.Context.Current.Vars.As<List<Dictionary<String, Object>>>(logKey);
        }
        return values;
      }
    }


    public static void Log(Object value, String title = "")
    {
      Int64 ticks = Context.Current.Timer.ElapsedTicks;
      Dictionary<String, Object> dic = new Dictionary<String, Object>();
      dic.Add("title", title);
      dic.Add("value", value);
      dic.Add("elapsedTicks", ticks);
      Logs.Add(dic);
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
        return GetDumpTitle(value, indent) + "NULL";
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
      if (result == 0.0)
      {
        return "0";
      }
      else
      {
        return result.ToString("N" + precision.ToString());
      }
    }
  }
}
