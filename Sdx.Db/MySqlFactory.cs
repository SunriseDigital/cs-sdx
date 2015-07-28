using System;
using System.Data.Common;

namespace Sdx.Db
{
  public class MySqlFactory : Factory
  {
    override protected DbProviderFactory GetFactory()
    {
      return DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
    }

    internal override string AppendLimitQuery(string selectSql, int limit, int offset)
    {
      selectSql += " LIMIT " + limit;
      if (offset > 0)
      {
        selectSql += " OFFSET " + offset;
      }
      return selectSql;
    }
  }
}
