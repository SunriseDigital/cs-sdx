using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Sdx.Db.Sql
{
  public class Profiler
  {
    public Profiler()
    {
      
    }

    List<Log> logs = new List<Log>();

    public List<Log> Logs
    {
      get
      {
        return this.logs;
      }
    }

    internal Log Begin(DbCommand command)
    {
      var query = new Log(command);
      this.logs.Add(query);
      query.Begin();
      return query;
    }

    internal Log Begin(string commandText)
    {
      var query = new Log(commandText);
      this.logs.Add(query);
      query.Begin();
      return query;
    }
  }
}