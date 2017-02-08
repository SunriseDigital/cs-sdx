using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Sdx.Db.Adapter
{
  public class MySql : Base
  {
    override protected DbProviderFactory GetFactory()
    {
      return DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
    }

    internal override void InitSelectEvent(Sql.Select select)
    {
      //AfterOrderFunc(Limit/Offset+ForUpdate)
      select.AfterOrderFunc = (sel) => {
        var result = "";

        if (sel.Limit >= 0)
        {
          result += " LIMIT " + sel.Limit;
          if (sel.Offset > 0)
          {
            result += " OFFSET " + sel.Offset;
          }
        }

        if (sel.ForUpdate)
        {
          result += " FOR UPDATE";
        }

        return result;
      };
    }

    protected override string SecureConnectionString
    {
      get
      {
        return Regex.Replace(this.ConnectionString, "(P|p)wd=[^;]+", "${1}wd=" + PasswordForSecureConnectionString);
      }
    }

    internal override object FetchLastInsertId(Connection connection)
    {
      var command = connection.CreateCommand();
      command.CommandText = "SELECT LAST_INSERT_ID()";
      return connection.ExecuteScalar(command);
    }

    public override string RandomOrderKeyword
    {
      get { return "RAND()"; }
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

      var columns = conn.GetSchema("Columns", new[] { null, null, tableName });
      Console.WriteLine(tableName);
      Console.WriteLine("-----------------------");
      foreach (System.Data.DataRow row in columns.Rows)
      {
        foreach (System.Data.DataColumn column in columns.Columns)
        {
          Console.WriteLine(string.Format("{0}: {1}", column.ColumnName, row[column]));
        }

        //Sdx.Diagnostics.Debug.Console(row["COLUMN_NAME"]);
        Console.WriteLine("");
      }

      Console.WriteLine("");
      Console.WriteLine("");
      Console.WriteLine("");

      return result;
    }
  }
}
