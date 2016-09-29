using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

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

    public void Log(Object value, String title = "", bool dumpPublicProperties = false)
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

      var fileName = "";
      var lineNumber = 0;
#if DEBUG
      var stack = new System.Diagnostics.StackFrame(1, true);
      fileName = stack.GetFileName();
      lineNumber = stack.GetFileLineNumber();
#endif
      
      Out.WriteLine(String.Format(
        "[{0}/{1} sec] {2} {3} - {4}",
        FormatStopwatchTicks(delta),
        FormatStopwatchTicks(currentTicks),
        title,
        fileName,
        lineNumber
      ));
      prevTimerElapsedTicks = currentTicks;
      Out.WriteLine(Dump(value, dumpPublicProperties));
      Out.WriteLine();
      Out.WriteLine();
    }

    private static string GetDumpTitle(object value, string indent, bool needType, string appendText = "")
    {
      if(needType)
      {
        Type type = value.GetType();
        return indent + type.Namespace + "." + type.Name + appendText + Environment.NewLine;
      }
      else
      {
        return indent;
      }
    }

    public static void Response()
    {
      var response = HttpContext.Current.Response;
      response.Write(Environment.NewLine);
    }

    public static void Response(object value)
    {
      var response = HttpContext.Current.Response;
      response.Write(Dump(value) + Environment.NewLine);
    }

    public static string Export(object value, bool dumpPublicProperties = false)
    {
      return Dump(value, "", false, dumpPublicProperties);
    }

    public static string Dump(object value, bool dumpPublicProperties = false)
    {
      return Dump(value, "", true, dumpPublicProperties);
    }

    private static string Dump(object value, string indent, bool needType, bool dumpPublicProperties)
    {
      if (value == null)
      {
        return indent + "NULL";
      }
      else if (value is string)
      {
        string strVal = value as string;
        //stringは型名を付与して改行するのは冗長なのでString(文字数)を付与
        if(needType)
        {
          return indent + "String(" + strVal.Length + ") " + strVal;
        }
        else
        {
          return indent + strVal;
        }
      }
      else if (value is IDictionary)
      {
        var dic = value as IDictionary;

        var result =  GetDumpTitle(value, indent, needType, "(" + dic.Count + ")");

        foreach (var key in dic.Keys)
        {
          // ここの`Dump(dic[key], " ")`は`:`の後なので常にスペース一個でOK
          result += indent + DumpIndent + key + " :" + Dump(dic[key], " ", needType, dumpPublicProperties) + Environment.NewLine;
        }

        //改行を取り除く
        return result.TrimEnd('\r', '\n');
      }
      //Request.Form/QueryStringなどのコレクション
      else if(value is NameValueCollection)
      {
        var nvcol = value as NameValueCollection;

        var result = GetDumpTitle(value, indent, needType, "(" + nvcol.Count + ")");
        foreach (var key in nvcol.Keys)
        {
          result += indent + DumpIndent + key + " :" + Dump(nvcol.GetValues(key.ToString()), " ", needType, dumpPublicProperties) + Environment.NewLine;
        }

        //改行を取り除く
        return result.TrimEnd('\r', '\n');
      }
      else if (value is IList)
      {
        IList list = value as IList;
        var result = GetDumpTitle(value, indent, needType, "(" + list.Count + ")");
        return AppendEnumerableDump(result, value as IEnumerable, indent, needType, dumpPublicProperties);
      }
      else if (value is IEnumerable)
      {
        var enume = (IEnumerable)value;
        var count = 0;
        foreach(var elem in enume)
        {
          ++count;
        }
        var result = GetDumpTitle(value, indent, needType, "(" + count + ")");
        return AppendEnumerableDump(result, value as IEnumerable, indent, needType, dumpPublicProperties);
      }
      else
      {
        var type = value.GetType();
        if (type.Namespace + "." + type.Name == "System.Collections.Generic.KeyValuePair`2")
        {
          var dynamicValue = (dynamic)value;
          return indent + dynamicValue.Key.ToString() + " " + Dump(dynamicValue.Value, indent, needType, dumpPublicProperties);
        }
        else if (dumpPublicProperties)
        {
          var result = GetDumpTitle(value, indent, needType, "( properties )");
          indent = indent + indent;
          foreach(var prop in type.GetProperties().Where(prop => prop.CanRead && prop.GetIndexParameters().Length == 0))
          {
            var val = prop.GetValue(value);
            result += indent + prop.Name + " " + Dump(val, indent, needType, needsDumpPublicMember(val)) + Environment.NewLine;
          }

          foreach (var fd in type.GetFields().Where(fd => fd.IsPublic))
          {
            var val = fd.GetValue(value);
            result += indent + fd.Name + " " + Dump(val, indent, needType, needsDumpPublicMember(val)) + Environment.NewLine;
          }

          return result;
        }
        else
        {
          return GetDumpTitle(value, indent, needType, " " + value.ToString()).TrimEnd('\r', '\n');
        }
      }
    }

    private static bool needsDumpPublicMember(object value)
    {
      return false;
    }

    private static String AppendEnumerableDump(String result, IEnumerable values, String indent, bool needType, bool dumpPublicProperties)
    { 
      foreach (Object obj in values as IEnumerable)
      {
        result += Dump(obj, indent + DumpIndent, needType, dumpPublicProperties) + Environment.NewLine;
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

    public static void DumpToFile(object value, string path)
    {
      var now = DateTime.Now;
      System.IO.File.AppendAllText(path, String.Format("[{0}] {1}{2}", now.ToString("yyyy-MM-dd HH:mm:ss"), Dump(value), Environment.NewLine));
    }
  }
}
