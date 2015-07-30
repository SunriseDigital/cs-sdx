using System;
using System.Data.Common;

namespace Sdx.Db
{
  public class SqlServerAdapter : Adapter
  {
    override protected DbProviderFactory GetFactory()
    {
      return DbProviderFactories.GetFactory("System.Data.SqlClient");
    }

    internal override string AppendLimitQuery(string selectSql, int limit, int offset)
    {
      selectSql += " OFFSET " + offset + " ROWS FETCH NEXT " + limit + " ROWS ONLY";
      return selectSql;
    }
  }
}
