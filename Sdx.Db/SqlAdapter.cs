using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace Sdx.Db
{
  public class SqlAdapter : Adapter
  {
    override protected DbConnection createDbConection()
    {
      return new SqlConnection();
    }

    protected override DbParameter CreateDbParameter(string key, string value)
    {
      return new SqlParameter(key, value);
    }
  }
}
