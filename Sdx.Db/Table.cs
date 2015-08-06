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
          return "{0}." + this.ForeignKey + " = {1}." + this.ReferenceKey;
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
    private Query.Select select;

    abstract protected MetaData CreateTableMeta();

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

    public Table AddColumn(object columnName, string alias = null)
    {
      if (this.ContextName == null)
      {
        throw new Exception("ContextName is null");
      }

      alias = alias != null ? alias + "@" + this.ContextName : columnName + "@" + this.ContextName;
      this.select.Context(this.ContextName).AddColumn(columnName, alias);

      return this;
    }

    public Table SetColumns(params object[] columns)
    {
      this.ClearColumns();
      this.AddColumns(columns);
      return this;
    }

    public Table AddColumns(params object[] columns)
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
