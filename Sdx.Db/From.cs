using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Sdx.Db
{
  public class From
  {
    private Select select;

    private List<String> columns;

    public From(Select select)
    {
      this.select = select;
      this.columns = new List<String>();

      this.columns.Add("*");
    }

    public string TableName { get; set; }

    internal string QuotedTableName
    {
      get { return this.select.Builder.QuoteIdentifier(this.TableName); }
    }

    public string Alias { get; set; }

    public List<String> Columns
    {
      get { return columns; }
    }

    public string Name 
    {
      get { return this.Alias == null ? this.TableName : this.Alias; }
    }

    internal string QuotedName
    {
      get { return this.select.Builder.QuoteIdentifier(this.Name); }
    }


    internal string BuildColumsString()
    {
      if(this.columns.Count == 0 && this.ParentTable == null)
      {
        throw new Exception("Column is empty.");
      }

      var result = "";
      this.columns.ForEach((column) => {
        if(result.Length > 0)
        {
          result += ", ";
        }

        result += this.select.Builder.QuoteIdentifier(this.Name) + ".";

        result += (column == "*") ? column : this.select.Builder.QuoteIdentifier(column);
      });

      return result;
    }

    internal string BuildTableString()
    {
      var result = this.select.Builder.QuoteIdentifier(this.TableName);
      if(this.Alias != null)
      {
        result += " AS " + this.select.Builder.QuoteIdentifier(this.Alias);
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

    public From InnerJoin(string table, string condition = null, string alias = null)
    {
      From joinTable = new From(this.select);

      joinTable.ParentTable = this;
      joinTable.TableName = table;
      joinTable.Alias = alias;
      joinTable.JoinCondition = condition;
      joinTable.JoinType = JoinType.Inner;
      this.select.Joins.Add(joinTable);
      return this;
    }

    public From ParentTable { get; set; }

    public string JoinCondition { get; set; }

    internal JoinType JoinType { get; set; }
  }
}
