using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Sdx.Db.Sql
{
  public class Insert
  {
    private List<Column> columns = new List<Column>();

    public Adapter Adapter { get; private set; }

    public string Into { get; set; }

    public Select Subquery { get; set; }

    public Insert(Adapter adapter)
    {
      this.Adapter = adapter;
    }

    public Insert SetInto(string tableName)
    {
      this.Into = tableName;
      return this;
    }

    public Insert AddColumnValue(string columnName, object value)
    {
      var column = new Column(columnName);
      column.Value = value;
      this.columns.Add(column);
      return this;
    }

    public DbCommand Build()
    {
      var command = this.Adapter.CreateCommand();

      var builder = new StringBuilder();
      builder.AppendFormat("INSERT INTO " + this.Adapter.QuoteIdentifier(this.Into) + " (");

      this.columns.ForEach(column => {
        builder.Append(column.Build(this.Adapter));
        builder.Append(", ");
      });

      builder.Remove(builder.Length - 2, 2);

      var counter = new Counter();
      if(this.Subquery != null)
      {
        builder.Append(") (");
        var strSelect = this.Subquery.BuildSelectString(command.Parameters, counter);
        builder.Append(strSelect);
      }
      else
      {
        builder.Append(") VALUES (");
        this.columns.ForEach(column =>
        {
          if (column.Value is Select)
          {
            var select = column.Value as Select;
            builder
              .Append('(')
              .Append(select.BuildSelectString(command.Parameters, counter))
              .Append(')');
          }
          else if(column.Value is Expr)
          {
            builder.Append(column.Value.ToString());
          }
          else
          {
            var parameterName = "@" + counter.Value;
            builder.Append(parameterName);

            var param = command.CreateParameter();
            param.ParameterName = parameterName;
            param.Value = column.Value;
            command.Parameters.Add(param);
            counter.Incr();
          }

          builder.Append(", ");
        });

        builder.Remove(builder.Length - 2, 2);
      }

      builder.Append(")");

      command.CommandText = builder.ToString();

      return command;
    }

    public Insert SetSubquery(Select select)
    {
      this.Subquery = select;
      return this;
    }

    public Insert AddColumn(string columnName)
    {
      var column = new Column(columnName);
      this.columns.Add(column);
      return this;
    }
  }
}
