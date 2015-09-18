using System;
using System.Data.Common;
using System.Diagnostics;
using System.Collections.Generic;

namespace Sdx.Db.Query
{
  public class Log
  {
    private string commandText;
    private Dictionary<string, object> parameters = new Dictionary<string, object>();
    private Stopwatch stopwatch;

    /// <summary>
    /// 桁数をそろえるため一番長いキーを保持しておく
    /// </summary>
    private int maxParameterKeyLength = 0;

    public Log()
    {
      this.stopwatch = new Stopwatch();
    }

    /// <summary>
    /// コマンドが書き換えられてもQueryLogは影響を受けないようにコピーします。
    /// </summary>
    /// <param name="command"></param>
    public Log(DbCommand command) : this()
    {
      this.commandText = command.CommandText;

      //キーの桁数をそろえるため最大文字数を記録します。
      foreach (DbParameter param in command.Parameters)
      {
        if(param.ParameterName.Length > this.maxParameterKeyLength)
        {
          this.maxParameterKeyLength = param.ParameterName.Length;
        }
        parameters[param.ParameterName] = param.Value;
      }
    }

    public Adapter Adapter { get; internal set; }

    internal void Begin()
    {
      this.stopwatch.Start();
    }

    internal void End()
    {
      this.stopwatch.Stop();
    }

    public string CommandText
    {
      get
      {
        return this.commandText;
      }
    }

    public Int64 ElapsedTime
    {
      get
      {
        return this.stopwatch.ElapsedTicks;
      }
    }

    /// <summary>
    /// 経過時間を秒単位で返す。doubleだと桁あふれを起こして指数表記になるのでStringで取得します。
    /// </summary>
    public string FormatedElapsedTime
    {
      get
      {
        return Sdx.Diagnostics.Debug.FormatStopwatchTicks(this.ElapsedTime);
      }
    }

    public string FormatedParameters
    {
      get
      {
        var result = "";
        foreach(KeyValuePair<string, object> param in this.parameters)
        {
          if(result.Length > 0)
          {
            result += System.Environment.NewLine;
          }

          result += string.Format(
            "{0, "+this.maxParameterKeyLength.ToString()+"} : {1}",
            param.Key,
            param.Value
          );
        }

        return result;
      }
    }

    public string Comment { get; internal set; }
  }
}