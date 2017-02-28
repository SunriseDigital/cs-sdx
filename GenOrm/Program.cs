using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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

        var firstNs = options.Namespace.Split('.').First();
        var csproj = options.BaseDir + Path.DirectorySeparatorChar + firstNs + Path.DirectorySeparatorChar + firstNs + ".csproj";
        XmlDocument doc = new XmlDocument();
        doc.Load(csproj);

        foreach (var table in GetTargetTableNames(options, db))
        {
          var className = Sdx.Util.String.ToCamelCase(table);
          
          var tableClass = CreateTableClass(options.Namespace, className, table, db);
          SaveClassFile(
            doc,
            options.BaseDir,
            options.Namespace,
            className,
            tableClass.Render(),
            options.ForceOverWrite,
            "Table"
          );

          var recordClass = CreateRecordClass(options.Namespace, className, table, db);
          SaveClassFile(
            doc,
            options.BaseDir,
            options.Namespace,
            className,
            recordClass.Render(),
            options.ForceOverWrite
          );
        }

        doc.Save(csproj);
      });
    }

    private static Sdx.Gen.Code.File CreateRecordClass(string ns, string className, string tableName, Sdx.Db.Adapter.Base db)
    {
      var file = new Sdx.Gen.Code.File();

      file.AddChild("using System;");
      file.AddChild("using System.Collections.Generic;");
      file.AddChild("using System.Linq;");
      file.AddBlankLine();

      var bNamespace = new Sdx.Gen.Code.Block("namespace {0}", ns);
      bNamespace.AppendTo(file);

      var bClass = new Sdx.Gen.Code.Block("public class {0} : Sdx.Db.Record", className);
      bClass.AppendTo(bNamespace);
      bClass.AddChild("public static Sdx.Db.TableMeta Meta { get; private set; }");
      bClass.AddBlankLine();

      var bStaticCtor = new Sdx.Gen.Code.Block("static {0}()", className);
      bStaticCtor.AppendTo(bClass);
      bStaticCtor.AddChild("Meta = {0}.Table.{1}.Meta;", ns, className);

      foreach (var column in GetColumns(tableName, db))
      {
        bClass.AddBlankLine();

        string type = "string";
        string getter = "GetString";
        if(column.Type == Sdx.Db.Table.ColumnType.Integer)
        {
          if (column.MaxLength != null && column.MaxLength > 4)
          {
            type = "long";
            getter = "GetInt64";
          }
          else
          {
            type = "int";
            getter = "GetInt32";
          }
        }
        else if (column.Type == Sdx.Db.Table.ColumnType.UnsignedInteger)
        {
          if (column.MaxLength != null && column.MaxLength > 4)
          {
            type = "ulong";
            getter = "GetUInt64";
          }
          else
          {
            type = "uint";
            getter = "GetUInt32";
          }
        }
        else if(column.Type == Sdx.Db.Table.ColumnType.DateTime)
        {
          type = "DateTime";
          getter = "GetDateTime";
        }
        else if ( column.Type == Sdx.Db.Table.ColumnType.Float)
        {
          if (column.MaxLength != null && column.MaxLength > 4)
          {
            type = "double";
            getter = "GetDouble";
          }
          else
          {
            type = "float";
            getter = "GetFloat";
          }
        }

        var bProps = new Sdx.Gen.Code.Block("public {0} {1}", type, Sdx.Util.String.ToCamelCase(column.Name));
        bProps.AppendTo(bClass);

        var bPropsGetter = new Sdx.Gen.Code.Block("get");
        bPropsGetter.AppendTo(bProps);
        bPropsGetter.AddChild(@"return {0}(""{1}"");", getter, column.Name);
      }

      return file;
    }

    private static void SaveClassFile(XmlDocument doc, string baseDir, string ns, string className, string body, bool forceOverWrite, string additionalns = null)
    {
      var pathChunk = ns.Split('.').ToList();
      if (additionalns != null)
      {
        pathChunk.Add(additionalns);
      }

      pathChunk.Add(className + ".cs");

      var path = baseDir + Path.DirectorySeparatorChar + string.Join(Path.DirectorySeparatorChar.ToString(), pathChunk);
      var fileExsits = File.Exists(path);
      if (!forceOverWrite && fileExsits)
      {
        Console.WriteLine("Already exists {0}", path);
        return;
      }

      UpdateCsproj(doc, pathChunk);

      var action = fileExsits ? "Overwrite" : "Create";
      Console.WriteLine("{0} {1}.{2} at {3}", action, string.Join(".", pathChunk.Take(pathChunk.Count - 1)), className, path);

      var dir = Path.GetDirectoryName(path);
      if (!Directory.Exists(dir))
      {
        Directory.CreateDirectory(dir);
      }
      File.WriteAllText(path, body);
    }

    private static void UpdateCsproj(XmlDocument doc, List<string> pathChunk)
    {

      //XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
      //ns.AddNamespace("msbld", "http://schemas.microsoft.com/developer/msbuild/2003");
      //var compilesNode = doc.DocumentElement.SelectNodes(@"/msbld:Project/msbld:ItemGroup/msbld:Compile", ns);


      var projNode = doc.ChildNodes.Cast<XmlNode>().Where(node => node.Name == "Project").First();
      var compilesNode = projNode.ChildNodes.Cast<XmlNode>()
          .Where(node => node.Name == "ItemGroup")
          .Where(node => node.ChildNodes.Cast<XmlNode>().Any(childNode => childNode.Name == "Compile"))
          .First();

      var path = string.Join("\\", pathChunk.Skip(1));
      var elem = doc.CreateElement("Compile", doc.DocumentElement.NamespaceURI);
      elem.SetAttribute("Include", path);
      compilesNode.AppendChild(elem);
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

      bCreateTableMeta.AddChild("typeof({0}.{1}),", ns, className);
      bCreateTableMeta.AddChild("typeof({0}.Table.{1})", ns, className);

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
