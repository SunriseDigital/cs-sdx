using System;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Sdx.Db
{
  public class MySqlAdapter : Adapter
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

    protected override string SecureConnectionString
    {
      get
      {
        return Regex.Replace(this.ConnectionString, "(P|p)wd=[^;]+", "${1}wd=" + PWD_FOR_SECURE_CONNECTION_STRING);
      }
    }
  }
}
