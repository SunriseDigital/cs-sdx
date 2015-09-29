using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Sdx.Db.Sql
{
  public class Insert
  {
    private List<string> columns = new List<string>();

    private List<object> values = new List<object>();

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

    public Insert AddPair(string column, object value)
    {
      this.AddColumn(column);
      this.AddValue(value);
      return this;
    }

    public DbCommand Build()
    {
      var command = this.Adapter.CreateCommand();

      var builder = new StringBuilder();
      builder.AppendFormat("INSERT INTO " + this.Adapter.QuoteIdentifier(this.Into) + " (");

      var count = 1;
      this.columns.ForEach(column => {
        builder.Append(this.Adapter.QuoteIdentifier(column));
        ++count;
        if (count <= this.columns.Count)
        {
          builder.Append(", ");
        }
      });

      

      
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
        count = 1;
        this.values.ForEach(value =>
        {
          var parameterName = "@" + counter.Value;
          builder.Append(parameterName);

          var param = command.CreateParameter();
          param.ParameterName = parameterName;
          param.Value = value;
          command.Parameters.Add(param);
          counter.Incr();

          ++count;
          if (count <= this.values.Count)
          {
            builder.Append(", ");
          }
        });
      }

      builder.Append(")");

      command.CommandText = builder.ToString();

      return command;
    }

    public Insert SetSubquery(Select select)
    {
      if(this.values.Count > 0)
      {
        throw new InvalidOperationException("Already has values.");
      }
      this.Subquery = select;
      return this;
    }

    public Insert AddColumn(string column)
    {
      this.columns.Add(column);
      return this;
    }

    public Insert AddValue(object value)
    {
      if(this.Subquery != null)
      {
        throw new InvalidOperationException("Already has subquery.");
      }
      this.values.Add(value);
      return this;
    }
  }
}
