using System.Web;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;
using System.Collections;
using System.Globalization;

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
      this.Culture = new CultureInfo("ja");
      this.HttpErrorHandler = new Web.HttpErrorHandler();
      if (HttpContext.Current != null)
      {
        UserAgent = new Web.UserAgent(HttpContext.Current.Request.UserAgent);
        Request = HttpContext.Current.Request;
      }
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

    /// <summary>
    /// <seealso cref="Sdx.Diagnostics.DebugHttpModule.Application_BeginRequest"/>
    /// </summary>
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


    public CultureInfo Culture { get; set; }

    public Web.HttpErrorHandler HttpErrorHandler { get; private set; }

    public bool PreventDebugDisplay { get; set; }

    private bool isTestServer;

    // TODO とりあえずでreturn Trueのみです。実装してください
    public bool IsTestServer
    {
      get
      {
        return false;
      }
    }

    public Web.UserAgent UserAgent { get; set; }

    public HttpRequest Request { get; set; }

    /// <summary>
    /// <see cref="Sdx.Db.Adapter.Base.SharedConnection"/>で生成された共有接続をすべて開放する。
    /// <see cref="Sdx.Web.HttpModule"/>を登録すると呼ばれます。
    /// </summary>
    internal void DisposeSharedConnections()
    {
      if (Sdx.Context.Current.Vars.ContainsKey(Db.Adapter.Base.SharedConnectionKey))
      {
        foreach (var kv in Sdx.Context.Current.Vars.As<Dictionary<string, Db.Connection>>(Db.Adapter.Base.SharedConnectionKey))
        {
          kv.Value.Dispose();
        }
      }
    }

    public static bool HasSdxHttpModule { get; internal set; }
  }
}
