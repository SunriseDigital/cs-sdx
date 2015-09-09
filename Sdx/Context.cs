using System.Web;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;

namespace Sdx
{
  public class Context
  {
    private const string SDX_CONTEXT_KEY = "SDX.CONTEXT.INSTANCE_KEY";

    private static Context nonWebInstance;

    private Dictionary<string, object> items = new Dictionary<string, object>();

    private Context()
    {
      this.Timer = new Stopwatch();
    }

    public static Context Current
    {
      get
      {
        if(HttpContext.Current != null)
        {
          if(HttpContext.Current.Items[SDX_CONTEXT_KEY] == null)
          {
            HttpContext.Current.Items[SDX_CONTEXT_KEY] = new Context();
          }

          return (Context)HttpContext.Current.Items[SDX_CONTEXT_KEY];
        }
        else
        {
          if(nonWebInstance == null)
          {
            nonWebInstance = new Context();
          }

          return nonWebInstance;
        }
      }
    }

    public Stopwatch Timer { get; private set; }

    public Context SetVar(string key, object value)
    {
      this.items[key] = value;
      return this;
    }

    public T GetVar<T>(string key)
    {
      return (T)this.items[key];
    }

    public object GetVar(string key)
    {
      return this.items[key];
    }

    public bool HasVar(string key)
    {
      return this.items.ContainsKey(key);
    }
  }
}
