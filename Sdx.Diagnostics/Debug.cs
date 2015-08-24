using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Diagnostics;

namespace Sdx.Diagnostics
{
  public class Debug
  {
    private static String logKey = "SDX.DEBUG_TOOL.DEBUG.LOGS_KEY";
    private static String requestTimerKey = "SDX.DEBUG_TOOL.DEBUG.REQUEST_TIMER_KEY";

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
        if (!HttpContext.Current.Items.Contains(logKey))
        {
          values = new List<Dictionary<String, Object>>();
          HttpContext.Current.Items.Add(logKey, values);
        }
        else
        {
          values = HttpContext.Current.Items[logKey] as List<Dictionary<String, Object>>;
        }
        return values;
      }
    }

    private static Stopwatch RequestTimer
    {
      get
      {
        return HttpContext.Current.Items[requestTimerKey] as Stopwatch; ;
      }
    }

    public static void Log(Object value, String title = "")
    {
      Int64 ticks = Debug.RequestTimer.ElapsedTicks;
      Dictionary<String, Object> dic = new Dictionary<String, Object>();
      dic.Add("title", title);
      dic.Add("value", value);
      dic.Add("elapsedTicks", ticks);
      Logs.Add(dic);
    }

    public static String Dump(Object value)
    {
      return Dump(value, "");
    }

    private static String Dump(Object value, String indent)
    {
      //文字列だったら即返す
      if (value is String)
      {
        String strVal = value as String;
        return indent + "String(" + strVal.Length + ") " + strVal;
      }

      if (value == null)
      {
        return indent + "NULL";
      }

      //それ以外のクラスはTypeNameを付与
      Type type = value.GetType();
      String result = indent + type.Namespace + "." + type.Name;

      if (value is IDictionary)
      {
        var dic = value as IDictionary;

        //タイトル部分
        result += "("+dic.Count+")" + Environment.NewLine;

        foreach (var key in dic.Keys)
        {
          // ここの`Dump(dic[key], " ")`は`:`の後なので常にスペース一個でOK
          result += indent + DumpIndent + key + " :" + Dump(dic[key], " ") + Environment.NewLine;
        }

        //改行を取り除く
        result = result.TrimEnd('\r', '\n');
      }
      else if (value is IList)
      {
        IList list = value as IList;
        //タイトル部分
        result += "("+list.Count+")" + Environment.NewLine;
        result = appendEnumerableDump(result, value as IEnumerable, indent);
      }
      else if (value is IEnumerable)
      {
        //タイトル部分
        result += Environment.NewLine;
        result = appendEnumerableDump(result, value as IEnumerable, indent);
      }
      else
      {
        //タイトル部分
        result += " " + value.ToString();
      }

      return result;
    }

    private static String appendEnumerableDump(String result, IEnumerable values, String indent)
    { 
      foreach (Object obj in values as IEnumerable)
      {
        result += Dump(obj, indent + DumpIndent) + Environment.NewLine;
      }

      //改行を取り除く
      result = result.TrimEnd('\r', '\n');
      return result;
    }

    internal static void initRequestTimer()
    {
      Stopwatch sw = new Stopwatch();
      sw.Start();
      HttpContext.Current.Items.Add(requestTimerKey, sw);
    }

    public static string FormatStopwatchTicks(Int64 ticks, int precision)
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
