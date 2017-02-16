﻿using CommandLine;
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

        foreach (var table in GetTargetTableNames(options, db))
        {
          var tableClass = CreateTableClass(table, options, db);
          Sdx.Context.Current.Debug.Log(tableClass.Render());
        }
      });
    }

    private static Sdx.Gen.Code.File CreateTableClass(string tableName, Options options, Sdx.Db.Adapter.Base db)
    {
      var className = Sdx.Util.String.ToCamelCase(tableName);
      var file = new Sdx.Gen.Code.File();

      file.AddChild("using System;");
      file.AddChild("using System.Collections.Generic;");
      file.AddChild("using System.Linq");
      file.AddBlankLine();

      var bNamespace = new Sdx.Gen.Code.Block("namespace {0}.Table", options.Namespace);
      file.AddChild(bNamespace);

      var bClass = new Sdx.Gen.Code.Block("public class {0} : Sdx.Db.Table", className);
      bNamespace.AddChild(bClass);
      bClass.AddChild("public static Sdx.Db.TableMeta Meta { get; private set; }");

      var bClassCtor = new Sdx.Gen.Code.Block("static {0}()", className);
      bClass.AddChild(bClassCtor);

      var bCreateTableMeta = new Sdx.Gen.Code.Block("Meta =  new Sdx.Db.TableMeta");
      bClassCtor.AddChild(bCreateTableMeta);
      bCreateTableMeta.ChangeBlockStrings("(", ");");
      bCreateTableMeta.StartLineBreak = false;
      bCreateTableMeta.AddChild(@"""{0}"",", tableName);

      var bColumnList = new Sdx.Gen.Code.Block("new List<Column>()");
      bCreateTableMeta.AddChild(bColumnList);
      bColumnList.BlockEnd = "},";
      foreach (var column in GetColumns(tableName, db))
      {
        bColumnList.AddChild(BuildCreateColumnString(column));
      }

      var bRelation = new Sdx.Gen.Code.Block("new Dictionary<string, Relation>()");
      bCreateTableMeta.AddChild(bRelation);
      bRelation.BlockEnd = "},";

      bCreateTableMeta.AddChild("typeof(Test.Orm.{0}),", className);
      bCreateTableMeta.AddChild("typeof(Test.Orm.Table.{0}),", className);

      return file;
    }

    private static string BuildCreateColumnString(Sdx.Db.Table.Column column)
    {
      var builder = new StringBuilder();
      builder.Append(@"new Column(");
      builder.Append('"');
      builder.Append(column.Name);
      builder.Append('"');

      if(column.Type != null)
      {
        builder.Append(", type: ColumnType.");
        builder.Append(column.Type.ToString());
      }

      if(column.IsAutoIncrement)
      {
        builder.Append(", isAutoIncrement: true");
      }

      if (column.IsPkey)
      {
        builder.Append(", isPkey: true");
      }

      if (column.IsNotNull)
      {
        builder.Append(", isNotNull: true");
      }

      if(column.MaxLength != null)
      {
        builder.Append(", maxLength: ");
        builder.Append(column.MaxLength);
      }

      return builder.Append("),").ToString();
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
