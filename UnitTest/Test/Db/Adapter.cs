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
      get { return "Server=localhost;Database=sdxtest;Uid=sdxuser;Pwd=sdx5963;CharSet=utf8;"; }
    }

    public static String SqlServerConnectionString
    {
      get { return "Server=.\\SQLEXPRESS;Database=sdxtest;User Id=sdxuser;Password=sdx5963;"; }
    }

    public static void SetupManager()
    {
      var sqlmg = new Sdx.Db.Adapter.Manager();
      var sqldb = new Sdx.Db.Adapter.SqlServer();
      sqldb.ConnectionString = Test.Db.Adapter.SqlServerConnectionString;
      sqlmg.AddCommonAdapter(sqldb);

      var mymg = new Sdx.Db.Adapter.Manager();
      var mydb = new Sdx.Db.Adapter.MySql();
      mydb.ConnectionString = Test.Db.Adapter.SqlServerConnectionString;
      mymg.AddCommonAdapter(mydb);

      Sdx.Db.Adapter.Manager.Set("main", () =>
      {
        var useMysql = false;
        if (HttpContext.Current != null)
        {
          var cookie = HttpContext.Current.Request.Cookies["sdx_test_use_mysql"];
          if (cookie != null && cookie.Value == "1")
          {
            useMysql = true;
          }
        }

        return useMysql ? mymg : sqlmg;
      });
    }
  }
}