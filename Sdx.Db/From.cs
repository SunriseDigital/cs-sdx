using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Sdx.Db
{
  public class From
  {
    private DbCommandBuilder builder;

    public From(DbCommandBuilder builder)
    {
      this.builder = builder;
    }

    public string TableName { get; set; }

    public string Alias { get; set; }

    internal string BuildColumsString()
    {
      return this.BuildTableString() + ".*";
    }

    internal string BuildTableString()
    {
      String table = this.Alias == null ? this.TableName : this.Alias;
      return this.builder.QuoteIdentifier(table);
    }
  }
}
