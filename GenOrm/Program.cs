using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
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
          var className = Sdx.Util.String.ToCamelCase(table);
          var tableClass = CreateTableClass(options.Namespace, className, table, db);
          SaveClassFile(
            options.BaseDir,
            options.Namespace,
            className,
            tableClass.Render(),
            options.ForceOverWrite,
            "Table"
          );
        }
      });
    }

    private static void SaveClassFile(string baseDir, string ns, string className, string body, bool forceOverWrite, string additionalns = null)
    {
      var bPath = new StringBuilder();
      bPath.Append(baseDir);
      bPath.Append(Path.DirectorySeparatorChar);

      var acutualNs = ns;
      if (additionalns != null)
      {
        acutualNs = acutualNs + "." + additionalns;
      }

      bPath.Append(acutualNs.Replace('.', Path.DirectorySeparatorChar));
      bPath.Append(Path.DirectorySeparatorChar);
      bPath.Append(className);
      bPath.Append(".cs");

      var path = bPath.ToString();
      var fileExsits = File.Exists(path);
      if (!forceOverWrite && fileExsits)
      {
        Console.WriteLine("Already exists {0}", path);
        return;
      }

      var action = fileExsits ? "Overwrite" : "Create";
      Console.WriteLine("{0} {1}.{2} at {3}", action, acutualNs, className, path);

      var dir = Path.GetDirectoryName(path);
      if (!Directory.Exists(dir))
      {
        Directory.CreateDirectory(dir);
      }
      File.WriteAllText(path, body);
    }

    private static Sdx.Gen.Code.File CreateTableClass(string ns, string className, string tableName, Sdx.Db.Adapter.Base db)
    {
      var file = new Sdx.Gen.Code.File();

      file.AddChild("using System;");
      file.AddChild("using System.Collections.Generic;");
      file.AddChild("using System.Linq;");
      file.AddBlankLine();

      var bNamespace = new Sdx.Gen.Code.Block("namespace {0}.Table", ns);
      bNamespace.AppendTo(file);

      var bClass = new Sdx.Gen.Code.Block("public class {0} : Sdx.Db.Table", className);
      bClass.AppendTo(bNamespace);
      bClass.AddChild("public static Sdx.Db.TableMeta Meta { get; private set; }");

      var bClassCtor = new Sdx.Gen.Code.Block("static {0}()", className);
      bClassCtor.AppendTo(bClass);

      var bCreateTableMeta = new Sdx.Gen.Code.Block("Meta =  new Sdx.Db.TableMeta");
      bCreateTableMeta.AppendTo(bClassCtor);
      bCreateTableMeta.ChangeBlockStrings("(", ");");
      bCreateTableMeta.StartLineBreak = false;
      bCreateTableMeta.AddChild(@"""{0}"",", tableName);

      var bColumnList = new Sdx.Gen.Code.Block("new List<Column>()");
      bColumnList.AppendTo(bCreateTableMeta);
      bColumnList.BlockEnd = "},";
      foreach (var column in GetColumns(tableName, db))
      {
        bColumnList.AddChild(BuildCreateColumnString(column));
      }

      var bRelation = new Sdx.Gen.Code.Block("new Dictionary<string, Relation>()");
      bRelation.AppendTo(bCreateTableMeta);
      bRelation.BlockEnd = "},";

      bCreateTableMeta.AddChild("typeof(Test.Orm.{0}),", className);
      bCreateTableMeta.AddChild("typeof(Test.Orm.Table.{0})", className);

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
