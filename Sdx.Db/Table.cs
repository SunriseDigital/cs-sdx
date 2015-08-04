using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Db
{
  public abstract class Table
  {
    private static Dictionary<string, TableMeta> metaList;
    public Adapter Adapter { get; set; }

    abstract protected TableMeta CreateMeta();

    public TableMeta Meta
    {
      get
      {
        var className = this.GetType().ToString();
        if(metaList.ContainsKey(className))
        {
          return metaList[className];
        }

        var meta = this.CreateMeta();
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
