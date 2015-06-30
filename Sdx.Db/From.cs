using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Sdx.Db
{
  public class From
  {
    private DbCommandBuilder builder;

    private List<String> columns;

    public From(DbCommandBuilder builder)
    {
      this.builder = builder;
      this.columns = new List<String>();
    }

    public string TableName { get; set; }

    public string Alias { get; set; }

    public List<String> Columns
    {
      get { return columns; }
    }

    public string Name 
    {
      get { return this.Alias == null ? this.TableName : this.Alias; }
    }

    internal string BuildColumsString()
    {
      if (this.columns.Count == 0)
      {
        return this.builder.QuoteIdentifier(this.Name) + ".*";
      }

      var result = "";
      this.columns.ForEach((column) => {
        result += this.builder.QuoteIdentifier(this.Name) 
          + "."
          + this.builder.QuoteIdentifier(column)  
          + ", ";
      });

      //最後のカンマとスペースを取り除く
      return result.Substring(0, result.Length - 2);
    }

    internal string BuildTableString()
    {
      var result = this.builder.QuoteIdentifier(this.TableName);
      if(this.Alias != null)
      {
        result += " AS " + this.builder.QuoteIdentifier(this.Alias);
      }
      return result;
    }

    public From SetColumns(params string[] columns)
    {
      this.columns.Clear();
      this.columns.AddRange(columns);
      return this;
    }

    public From AddColumn(string column)
    {
      this.columns.Add(column);
      return this;
    }
  }
}
