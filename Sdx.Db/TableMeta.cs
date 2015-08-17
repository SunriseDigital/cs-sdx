using System;
using System.Collections.Generic;

namespace Sdx.Db
{
  public class TableMeta
  {
    public string Name { get; set; }
    public List<string> Pkeys { get; set; }
    public List<string> Columns { get; set; }
    public Dictionary<string, Sdx.Db.Table.Relation> Relations { get; set; }
  }
}
