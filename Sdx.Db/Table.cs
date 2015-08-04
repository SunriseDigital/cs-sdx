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

    public TableMeta Meta
    {
      get
      {
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
  }
}
