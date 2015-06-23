using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Diagnostics;

namespace Sdx.DebugTool
{
  public class Debug
  {
    private static String logKey = "SDX.DEBUG_TOOL.DEBUG.LOGS_KEY";
    private static String requestTimerKey = "SDX.DEBUG_TOOL.DEBUG.REQUEST_TIMER_KEY";
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
        return indent + value as String;
      }

      //それ以外のクラスはTypeNameを付与
      Type type = value.GetType();
      String result = indent + type.Namespace + "." + type.Name + Environment.NewLine;

      if (value is IDictionary)
      {
        var dic = value as IDictionary;
        foreach (var key in dic.Keys)
        {
          result += indent + " " + key + " :" + Dump(dic[key], " ") + Environment.NewLine;
        }

        //改行を取り除く
        result = result.TrimEnd('\r', '\n');
      }
      else if (value is IEnumerable)
      {
        foreach (Object obj in value as IEnumerable)
        {
          result += Dump(obj, indent + " ") + Environment.NewLine;
        }

        //改行を取り除く
        result = result.TrimEnd('\r', '\n');
      }
      else
      {
        result += value.ToString();
      }

      return result;
    }

    internal static void initRequestTimer()
    {
      Stopwatch sw = new Stopwatch();
      sw.Start();
      HttpContext.Current.Items.Add(requestTimerKey, sw);
    }
  }
}
