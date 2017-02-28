using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Linq;

namespace Sdx.Db.Adapter
{
  public class MySql : Base
  {
    private static Dictionary<string, Table.ColumnType?> columnMapping;

    public MySql()
    {
      columnMapping = new Dictionary<string, Table.ColumnType?> 
      { 
        {"char", Table.ColumnType.String},
        {"varchar", Table.ColumnType.String},
        {"binary", Table.ColumnType.String},
        {"varbinary", Table.ColumnType.String},
        {"blob", Table.ColumnType.String},
        {"text", Table.ColumnType.String},
        {"enum", null},
        {"set", null},

        {"integer", Table.ColumnType.Integer},
        {"int", Table.ColumnType.Integer},
        {"smallint", Table.ColumnType.Integer},
        {"tinyint", Table.ColumnType.Integer},
        {"mediumint", Table.ColumnType.Integer},
        {"bigint", Table.ColumnType.Integer},

        {"uinteger", Table.ColumnType.UnsignedInteger},
        {"uint", Table.ColumnType.UnsignedInteger},
        {"usmallint", Table.ColumnType.UnsignedInteger},
        {"utinyint", Table.ColumnType.UnsignedInteger},
        {"umediumint", Table.ColumnType.UnsignedInteger},
        {"ubigint", Table.ColumnType.UnsignedInteger},

        {"decimal", null},
        {"numeric", null},

        {"float", Table.ColumnType.Float},
        {"double", Table.ColumnType.Float},

        {"ufloat", Table.ColumnType.Float},
        {"udouble", Table.ColumnType.Float},

        {"date", Table.ColumnType.Date},
        {"datetime", Table.ColumnType.DateTime},
        {"timestamp", Table.ColumnType.DateTime},
        {"time", null},
        {"year", null},
      };
    }

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

    public override string RandomOrderKeyword(int? seed = null, Sql.Column column = null)
    {
      if(seed == null)
      {
        return "RAND()";
      }
      else
      {
        return "RAND("+seed.ToString()+")";
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
      select.AddFrom("information_schema.columns", cColumns =>
      {
        cColumns.AddColumn("column_name");
        cColumns.AddColumn("column_type");
        cColumns.AddColumn("is_nullable");
        cColumns.AddColumn("character_maximum_length");
        cColumns.AddColumn("column_key");
        cColumns.AddColumn("extra");
        cColumns.Where.Add("table_name", tableName);
        cColumns.Where.Add("table_schema", conn.Database);
      });

      conn.FetchDictionaryList(select).ForEach(dic =>
      {
        var typeKey = DetectColumnType(dic["column_type"].ToString());
        result.Add(new Table.Column(
          name: dic["column_name"].ToString(),
          type: columnMapping.ContainsKey(typeKey) ? columnMapping[typeKey] : null,
          isNotNull: dic["is_nullable"].ToString() == "NO",
          isAutoIncrement: dic["extra"].ToString() == "auto_increment",
          maxLength: DelectMaxLength(typeKey, dic["character_maximum_length"].ToString()),
          isPkey: dic["column_key"].ToString() == "PRI"
        ));
      });

      return result;
    }

    private long? DelectMaxLength(string typeKey, string maxLength)
    {
      if(columnMapping.ContainsKey(typeKey))
      {
        var type = columnMapping[typeKey];
        if (type == Table.ColumnType.String)
        {
          return (long?)Int64.Parse(maxLength);
        }
        else if(new Table.ColumnType?[]{
          Table.ColumnType.Integer,
          Table.ColumnType.UnsignedInteger
        }.Any(tp => tp == type))
        {
          if(typeKey.IndexOf("big") >= 0)
          { 
            return 8;
          }
          else if (typeKey.IndexOf("menium") >= 0)
          {
            return 3;
          }
          else if (typeKey.IndexOf("small") >= 0)
          {
            return 2;
          }
          else if (typeKey.IndexOf("tiny") >= 0)
          {
            return 1;
          }
          else
          {
            return 4;
          }
        }
        else if(new Table.ColumnType?[]{
          Table.ColumnType.Float
        }.Any(tp => tp == type))
        {
          if (typeKey.IndexOf("double") >= 0)
          {
            return 8;
          }
          else if(typeKey.IndexOf("float") >= 0)
          {
            return 4;
          }
          else
          {
            return null;
          }
        }
      }

      return null;
    }

    private string DetectColumnType(string columnType)
    {
      var type = columnType.Split(' ')[0];
      var charIndex = type.IndexOf('(');
      if (charIndex >= 0)
      {
        type = type.Substring(0, charIndex);
      }

      if (columnType.IndexOf("unsigned") >= 0)
      {
        type = "u" + type;
      }

      return type;
    }
  }
}
