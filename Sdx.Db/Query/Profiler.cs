using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Sdx.Db.Query
{
  public class Profiler
  {
    public Profiler()
    {
      
    }

    List<Log> queries = new List<Log>();

    public List<Log> Queries
    {
      get
      {
        return this.queries;
      }
    }

    internal Log Begin(DbCommand command)
    {
      var query = new Log(command);
      this.queries.Add(query);
      query.Begin();
      return query;
    }
  }
}