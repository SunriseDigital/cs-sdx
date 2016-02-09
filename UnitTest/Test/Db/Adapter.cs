using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Test.Db
{
  public class Adapter
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

    public static Sdx.Db.Adapter CreateDb()
    {
      var useMysql = false;
      if(HttpContext.Current != null)
      {
        var cookie = HttpContext.Current.Request.Cookies["sdx_test_use_mysql"];
        if(cookie != null && cookie.Value == "1")
        {
          useMysql = true;
        }
      }

      return useMysql ? CreateMySql() : CreateSqlServer();
    }
  }
}
