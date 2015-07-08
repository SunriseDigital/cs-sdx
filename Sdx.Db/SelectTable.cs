using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Sdx.Db
{
  public class SelectTable
  {
    private Select select;

    private List<String> columns = new List<string>();

    public SelectTable(Select select)
    {
      this.select = select;
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

    public SelectTable AddJoin(string table, JoinType joinType, string condition, string alias = null)
    {
      SelectTable joinTable = new SelectTable(this.select);

      joinTable.ParentTable = this;
      joinTable.TableName = table;
      joinTable.Alias = alias;
      joinTable.JoinCondition = condition;
      joinTable.JoinType = joinType;

      int findIndex = this.select.Joins.FindIndex(jt =>
      {
        return jt.Name == joinTable.Name;
      });

      if(findIndex != -1)
      {
        this.select.Joins.RemoveAt(findIndex);
      }

      this.select.Joins.Add(joinTable);
      return joinTable;
    }

    public SelectTable InnerJoin(string table, string condition, string alias = null)
    {
      return this.AddJoin(table, JoinType.Inner, condition, alias);
    }

    public SelectTable ParentTable { get; set; }

    public string JoinCondition { get; set; }

    internal JoinType JoinType { get; set; }

    public SelectTable ClearColumns()
    {
      this.Columns.Clear();
      return this;
    }

    public SelectTable SetColumns(params String[] columns)
    {
      this.ClearColumns();
      this.AddColumns(columns);
      return this;
    }

    public SelectTable AddColumns(params String[] columns)
    {
      foreach (var column in columns)
      {
        this.Columns.Add(column);
      }
      return this;
    }
  }
}
