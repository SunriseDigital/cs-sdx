using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Sdx.Db
{
  public class Profiler
  {
    public Profiler()
    {
      
    }

    List<QueryLog> queries = new List<QueryLog>();

    public List<QueryLog> Queries
    {
      get
      {
        return this.queries;
      }
    }

    internal QueryLog Begin(DbCommand command)
    {
      var query = new QueryLog(command);
      this.queries.Add(query);
      query.Begin();
      return query;
    }
  }
}