using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenOrm
{
  class Program
  {
    static void Main(string[] args)
    {
      var result = Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
      {
        var db = Sdx.Cli.Options.Db.CreateAdapter(options);
        var tableNames = GetTargetTableNames(options, db);

        foreach(var tableName in tableNames)
        {
          var columns = GetColumns(tableName, db);
        }
      });
    }

    private static IEnumerable<Sdx.Db.Table.Column> GetColumns(string tableName, Sdx.Db.Adapter.Base db)
    {
      using (var conn = db.CreateConnection())
      {
        conn.Open();
        return conn.FetchColumns(tableName);
      }
    }

    private static IEnumerable<string> GetTargetTableNames(Options options, Sdx.Db.Adapter.Base db)
    {
      if(options.TableNames.Count() > 0)
      {
        return options.TableNames;
      }
      else
      {
        using (var conn = db.CreateConnection())
        {
          conn.Open();
          return conn.FetchTableNames();
        }
      }
    }
  }
}
