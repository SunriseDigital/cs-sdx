using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Sdx.Db.Adapter
{
  public class SqlServer : Base
  {
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
          if (select.OrderList.Count == 0)
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

    public override string RandomOrderKeyword
    {
      get { return "NEWID()"; }
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

    internal override IEnumerable<Table.Column> FetchColumns(string tableName, Connection conn)
    {
      var result = new List<Table.Column>();

      var select = CreateSelect();
      select.AddFrom("sys.columns", cColumn =>
      {
        cColumn.AddColumn("name");
        cColumn.Where.Add("object_id", Db.Sql.Expr.Wrap("OBJECT_ID('" + tableName + "')"));
      });

      Sdx.Diagnostics.Debug.Console(conn.FetchDictionaryList(select));

      //var columns = conn.GetSchema("Columns", new[] { null, null, tableName });
      //Console.WriteLine(tableName);
      //Console.WriteLine("-----------------------");
      //foreach (System.Data.DataRow row in columns.Rows)
      //{
      //  foreach (System.Data.DataColumn column in columns.Columns)
      //  {
      //    Console.WriteLine(string.Format("{0}: {1}", column.ColumnName, row[column]));
      //  }

      //  //Sdx.Diagnostics.Debug.Console(row["COLUMN_NAME"]);
      //  Console.WriteLine("");
      //}

      //Console.WriteLine("");
      //Console.WriteLine("");
      //Console.WriteLine("");

      return result;
    }
  }
}
