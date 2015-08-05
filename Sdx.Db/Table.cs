using System;
using System.Collections.Generic;

namespace Sdx.Db
{
  public abstract class Table
  {
    public class Relation
    {
      public string TableName { get; set; }
      public string ForeignKey { get; set; }
      public string ReferenceKey { get; set; }
      public string JoinCondition
      {
        get
        {
          return "{0}." + this.ReferenceKey + " = {1}." + this.ForeignKey;
        }
      }
    }

    private static Dictionary<string, TableMeta> metaList;
    public Adapter Adapter { get; set; }

    abstract protected TableMeta CreateTableMeta();

    private List<Query.Column> columns;

    public TableMeta Meta
    {
      get
      {
        //cache in static var.
        var className = this.GetType().ToString();
        if(metaList.ContainsKey(className))
        {
          return metaList[className];
        }

        var meta = this.CreateTableMeta();
        metaList[className] = meta;
        return meta;
      }
    }

    static Table()
    {
      Table.metaList = new Dictionary<string, TableMeta>();
    }

    public Table()
    {
      this.columns = new List<Query.Column>();
      this.Meta.Columns.ForEach(columnName => {
        var column = new Query.Column(columnName);
        this.columns.Add(column);
      });
    }

    public Table ClearColumns()
    {
      this.columns.Clear();
      return this;
    }

    public Table AddColumn(string columnName, string alias = null)
    {
      var column = new Query.Column(columnName);
      column.Alias = alias;
      this.columns.Add(column);
      return this;
    }

    public Table SetColumns(params String[] columns)
    {
      this.ClearColumns();
      this.AddColumns(columns);
      return this;
    }

    public Table AddColumns(params String[] columns)
    {
      foreach (var column in columns)
      {
        this.AddColumn(column);
      }
      return this;
    }

    internal List<Query.Column> Columns
    {
      get { return this.columns; }
    }
  }
}
