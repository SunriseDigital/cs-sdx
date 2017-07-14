using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Linq;

namespace Sdx.Db.Adapter
{
  public class SqlServer : Base
  {
    private static Dictionary<string, Table.ColumnType?> columnMapping;

    public SqlServer()
    {
      columnMapping = new Dictionary<string, Table.ColumnType?> 
      { 
        {"text", Table.ColumnType.String},
        {"date", Table.ColumnType.DateTime},
        {"datetime2", Table.ColumnType.DateTime},
        {"tinyint", Table.ColumnType.Integer},
        {"smallint", Table.ColumnType.Integer},
        {"int", Table.ColumnType.Integer},
        {"smalldatetime", Table.ColumnType.DateTime},
        {"real", Table.ColumnType.Float},
        {"datetime", Table.ColumnType.DateTime},
        {"float", Table.ColumnType.Float},
        {"ntext", Table.ColumnType.String},
        {"decimal", Table.ColumnType.Float},
        {"bigint", Table.ColumnType.Integer},
        {"varchar", Table.ColumnType.String},
        {"timestamp", Table.ColumnType.DateTime},
        {"nvarchar", Table.ColumnType.String},
        {"bit", Table.ColumnType.Boolean}
      };
    }

    override protected DbProviderFactory GetFactory()
    {
      return DbProviderFactories.GetFactory("System.Data.SqlClient");
    }

    protected override string SecureConnectionString
    {
      get
      {
        return Regex.Replace(this.ConnectionString, "(P|p)assword=[^;]+", "${1}assword=" + PasswordForSecureConnectionString);
      }
    }

    internal override void InitSelectEvent(Sql.Select select)
    {
      //AfterFromFunc(ForUpdate)
      select.AfterFromFunc = (sel) => {
        if (sel.ForUpdate)
        {
          return " WITH (UPDLOCK,ROWLOCK)";
        }

        return "";
      };

      //AfterOrderFunc(Limit/Offset)
      select.AfterOrderFunc = (sel) => {
        if (sel.Limit >= 0)
        {
          if (sel.OrderList.Count == 0)
          {
            throw new InvalidOperationException("Needs ORDER BY statement to use OFFSET/LIMIT on SQLServer.");
          }
          return " OFFSET " + sel.Offset + " ROWS FETCH NEXT " + sel.Limit + " ROWS ONLY";
        }

        return "";
      };
    }

    internal override object FetchLastInsertId(Connection connection)
    {
      var command = connection.CreateCommand();
      command.CommandText = "SELECT @@IDENTITY";
      return connection.ExecuteScalar(command);
    }

    public override string RandomOrderKeyword(int? seed = null, Sql.Column column = null)
    {
      if (seed == null)
      {
        return "NEWID()";
      }
      else
      {
        if (column == null || column.Target == null)
        {
          throw new InvalidOperationException("Require base column to random order with seed in SQL SERVER.");
        }
        return "HASHBYTES('md5',cast(" + seed.ToString() + " + " + column.Build(this, null, null)+ " as varchar))";
      }
    }

    internal override IEnumerable<string> FetchTableNames(Connection conn)
    {
      var result = new List<string>();
      var tables = conn.GetSchema("Tables");
      foreach (DataRow row in tables.Rows)
      {
        result.Add((string)row["TABLE_NAME"]);
      }

      return result;
    }

    internal override List<Table.Column> FetchColumns(string tableName, Connection conn)
    {
      var result = new List<Table.Column>();

      var select = CreateSelect();
      select.AddFrom("sys.columns", cColumns =>
      {
        cColumns.AddColumn("name");
        cColumns.AddColumn("max_length");
        cColumns.AddColumn("is_nullable");
        cColumns.AddColumn("is_identity");
        cColumns.InnerJoin(
          "sys.types",
          CreateCondition().Add(
            new Sdx.Db.Sql.Column("user_type_id", "sys.columns"),
            new Sdx.Db.Sql.Column("user_type_id", "sys.types")
          ),
          cTypes => 
          {
            cTypes.AddColumn("name", "type");
          }
        );
        cColumns.InnerJoin(
          "sys.tables",
          CreateCondition().Add(
            new Sdx.Db.Sql.Column("object_id", "sys.columns"),
            new Sdx.Db.Sql.Column("object_id", "sys.tables")
          ),
          cTables =>
          {
            //SQLサーバーのsys.系は接続したDBの情報のみ取得できるのでこれだけ見ればOK
            cTables.Where.Add("name", tableName);
          }
        );
        cColumns.LeftJoin(
          "sys.index_columns",
          CreateCondition().Add(
            new Sdx.Db.Sql.Column("object_id", "sys.columns"),
            new Sdx.Db.Sql.Column("object_id", "sys.index_columns")
          ).Add(
            new Sdx.Db.Sql.Column("column_id", "sys.columns"),
            new Sdx.Db.Sql.Column("column_id", "sys.index_columns")
          ),
          cIndexeColumns =>
          {
            cIndexeColumns.LeftJoin(
              "sys.indexes",
              CreateCondition().Add(
                new Sdx.Db.Sql.Column("object_id", "sys.indexes"),
                new Sdx.Db.Sql.Column("object_id", "sys.index_columns")
              ).Add(
                new Sdx.Db.Sql.Column("index_id", "sys.indexes"),
                new Sdx.Db.Sql.Column("index_id", "sys.index_columns")
              )
            );
          }
        );
      });

      select.AddColumn(Sdx.Db.Sql.Expr.Wrap("CAST(ISNULL(is_primary_key, 0) as BIT)"), "is_primary_key");

      conn.FetchDictionaryList(select).ForEach(dic =>
      {
        int maxLength;
        result.Add(new Table.Column(
          name: dic["name"].ToString(),
          type: columnMapping.ContainsKey(dic["type"].ToString()) ? columnMapping[dic["type"].ToString()] : null,
          isNotNull: !(bool)dic["is_nullable"],
          isAutoIncrement: (bool)dic["is_identity"],
          maxLength: Int32.TryParse(dic["max_length"].ToString(), out maxLength) ? (int?)maxLength : null,
          isPkey: (bool)dic["is_primary_key"]
        ));
      });

      return result;
    }
  }
}
