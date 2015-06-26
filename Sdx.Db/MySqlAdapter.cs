using System;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace Sdx.Db
{
  public class MySqlAdapter : Adapter
  {
    override protected DbConnection CreateDbConection()
    {
      return new MySqlConnection();
    }

    protected override DbParameter CreateDbParameter(string key, string value)
    {
      return new MySqlParameter(key, value);
    }
  }
}
