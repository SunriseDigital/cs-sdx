using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Sdx.Db.Query
{
  public class Insert
  {
    private List<string> columns = new List<string>();

    private List<object> values = new List<object>();

    public Adapter Adapter { get; private set; }

    public string Into { get; set; }

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
      this.columns.Add(column);
      this.values.Add(value);
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

      builder.Append(") VALUES (");

      count = 1;
      var placeHolderNumber = 0;
      this.values.ForEach(value => {
        if(value is Select)
        {

        }
        else
        {
          var parameterName = "@" + placeHolderNumber;
          builder.Append(parameterName);

          var param = command.CreateParameter();
          param.ParameterName = parameterName;
          param.Value = value;
          command.Parameters.Add(param);
          ++placeHolderNumber;
        }

        ++count;
        if (count <= this.columns.Count)
        {
          builder.Append(", ");
        }
      });

      builder.Append(")");

      //var valuesBuilder = new StringBuilder();
      //valuesBuilder.Append(" VALUES (");

      //var count = values.Count;
      //foreach (var kv in values)
      //{
      //  var parameterName = "@" + (values.Count - count);
      //  builder.Append(this.Adapter.QuoteIdentifier(kv.Key));
      //  valuesBuilder.Append(parameterName);

      //  var param = command.CreateParameter();
      //  param.ParameterName = parameterName;
      //  param.Value = kv.Value;
      //  command.Parameters.Add(param);

      //  --count;
      //  if (count > 0)
      //  {
      //    builder.Append(", ");
      //    valuesBuilder.Append(", ");
      //  }
      //}

      //builder.Append(")");
      //valuesBuilder.Append(")");

      command.CommandText = builder.ToString();

      return command;
    }
  }
}
