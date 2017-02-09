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

      var select = CreateSelect();
      select.AddFrom("information_schema.columns", cColumns =>
      {
        cColumns.AddColumn("column_name");
        cColumns.AddColumn("data_type");
        cColumns.AddColumn("is_nullable");
        cColumns.AddColumn("character_maximum_length");
        cColumns.AddColumn("column_key");
        cColumns.AddColumn("extra");
        cColumns.Where.Add("table_name", tableName);
        cColumns.Where.Add("table_schema", conn.Database);
      });

      conn.FetchDictionaryList(select).ForEach(dic =>
      {
        Sdx.Diagnostics.Debug.Console(dic);
      });

      return result;
    }
  }
}
