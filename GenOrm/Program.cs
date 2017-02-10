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
      Sdx.Context.Current.Debug.Out = Console.Out;
      var result = Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
      {
        Sdx.Cli.Options.Db.SetUpAdapters(options);
        var db = Sdx.Db.Adapter.Manager.Get(options.DbAdapterName).Read;
        Sdx.Context.Current.Debug.Log(db.ConnectionString);
        //var tableNames = GetTargetTableNames(options, db);

        //foreach(var tableName in tableNames)
        //{
        //  var columns = GetColumns(tableName, db);
        //  //Sdx.Diagnostics.Debug.Console(tableName);
        //  //Sdx.Diagnostics.Debug.Console(columns);
        //  //Console.WriteLine("\n\n");
        //}
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
