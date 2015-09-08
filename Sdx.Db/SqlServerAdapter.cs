using System;
using System.Data.Common;
using System.Text.RegularExpressions;

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

    protected override string SecureConnectionString
    {
      get
      {
        return Regex.Replace(this.ConnectionString, "(P|p)assword=[^;]+", "${1}assword=" + PWD_FOR_SECURE_CONNECTION_STRING);
      }
    }
  }
}
