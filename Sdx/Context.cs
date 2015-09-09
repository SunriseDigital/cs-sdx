using System.Web;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;
using System.Collections;

namespace Sdx
{
  public class Context
  {
    public class ContextVars : IEnumerable<KeyValuePair<string, object>>
    {
      private Dictionary<string, object> items = new Dictionary<string, object>();

      public object this[string key]
      {
        set
        {
          this.items[key] = value;
        }

        get
        {
          return this.items[key];
        }
      }

      public ContextVars Add(string key, object value)
      {
        this.items.Add(key, value);
        return this;
      }


      public bool ContainsKey(string key)
      {
        return this.items.ContainsKey(key);
      }

      public T As<T>(string key)
      {
        return (T)this[key];
      }

      public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
      {
        return ((IEnumerable<KeyValuePair<string, object>>)items).GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return ((IEnumerable<KeyValuePair<string, object>>)items).GetEnumerator();
      }
    }
    private const string SDX_CONTEXT_KEY = "SDX.CONTEXT.INSTANCE_KEY";

    private static Context nonWebInstance;

    

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

    private ContextVars vars = new ContextVars();



    public ContextVars Vars
    {
      get
      {
        return this.vars;
      }
    }

    //public Context SetVar(string key, object value)
    //{
    //  this.items[key] = value;
    //  return this;
    //}

    //public T GetVar<T>(string key)
    //{
    //  return (T)this.items[key];
    //}

    //public object GetVar(string key)
    //{
    //  return this.items[key];
    //}

    //public bool HasVar(string key)
    //{
    //  return this.items.ContainsKey(key);
    //}
  }
}
