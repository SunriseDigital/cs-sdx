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

    public class MetaData
    {
      public string Name { get; set; }
      public List<string> Columns { get; set; }
      public Dictionary<string, Sdx.Db.Table.Relation> Relations { get; set; }
    }

    private static Dictionary<string, MetaData> metaList;
    public Adapter Adapter { get; set; }

    abstract protected MetaData CreateTableMeta();

    private Query.Select select;

    public MetaData Meta
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
      Table.metaList = new Dictionary<string, MetaData>();
    }


    public Table ClearColumns()
    {
      this.select.ClearColumns(this.ContextName);
      return this;
    }

    private void _AddColumn(object columnName, string alias)
    {
      if (this.ContextName == null)
      {
        throw new Exception("ContextName is null");
      }

      alias = alias != null ? alias + "@" + this.ContextName : columnName + "@" + this.ContextName;

      this.select.AddColumn(columnName, alias, this.ContextName);
    }

    public Table AddColumn(Query.Expr columnName, string alias = null)
    {
      this._AddColumn(columnName, alias);
      return this;
    }

    public Table AddColumn(string columnName, string alias = null)
    {
      this._AddColumn(columnName, alias);
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
      foreach (var columnName in columns)
      {
        this.AddColumn(columnName);
      }
      return this;
    }

    internal string ContextName { get; set; }

    internal Query.Select Select
    {
      get
      {
        return this.select;
      }
      set
      {
        this.select = value;
        this.Meta.Columns.ForEach(columnName =>
        {
          this.AddColumn(columnName);
        });
      }

    }
  }
}
