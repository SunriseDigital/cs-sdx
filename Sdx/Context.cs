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
    private const string SDX_CONTEXT_KEY = "SDX.CONTEXT.INSTANCE_KEY";

    private static Context nonWebInstance;

    private Context()
    {
      this.Timer = new Stopwatch();
      this.isDebugMode = false;
      this.Lang = "ja";
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

    private Collection.Holder vars = new Collection.Holder();

    public Collection.Holder Vars
    {
      get
      {
        return this.vars;
      }
    }

    public Sdx.Db.Sql.Profiler DbProfiler { get; set; }

    private bool isDebugMode;

    public bool IsDebugMode
    {
      get
      {
        return this.isDebugMode;
      }

      set
      {
        this.isDebugMode = value;
        if (value == true && this.DbProfiler == null)
        {
          this.DbProfiler = new Sdx.Db.Sql.Profiler();
        }
      }
    }

    private const string DebugContextKey = "Sdx.Context.DebugContextKey";

    public Diagnostics.Debug Debug
    {
      get
      {
        return Sdx.Context.Current.Vars.As<Diagnostics.Debug>(DebugContextKey, () => {
          return new Diagnostics.Debug();
        });
      }
    }

    /// <summary>
    /// ISO 638-1 言語コード
    /// https://ja.wikipedia.org/wiki/ISO_639-1%E3%82%B3%E3%83%BC%E3%83%89%E4%B8%80%E8%A6%A7
    /// </summary>
    public string Lang { get; set; }
  }
}
