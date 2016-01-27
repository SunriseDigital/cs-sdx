using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
  public class Db
  {
    public static String MySqlConnectionString
    {
      get { return "Server=localhost;Database=sdxtest;Uid=sdxuser;Pwd=sdx5963;"; }
    }

    public static String SqlServerConnectionString
    {
      get { return "Server=.\\SQLEXPRESS;Database=sdxtest;User Id=sdxuser;Password=sdx5963;"; }
    }

    public static Sdx.Db.Adapter CreateMySql()
    {
      var db = new Sdx.Db.MySqlAdapter();
      db.ConnectionString = MySqlConnectionString;
      return db;
    }

    public static Sdx.Db.Adapter CreateSqlServer()
    {
      var db = new Sdx.Db.SqlServerAdapter();
      db.ConnectionString = SqlServerConnectionString;
      return db;
    }
  }
}
