using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Scaffold.Group
{
  public class StaticClass : Base
  {
    private string columnName;
    private Type type;
    private Config.Value methodForDisplay;
    private Config.Value methodForList;

    public StaticClass(string columnName, Type type, Config.Value methodForDisplay = null, Config.Value methodForList = null)
      : base(columnName, methodForList != null)
    {
      this.columnName = columnName;
      this.type = type;
      this.methodForDisplay = methodForDisplay;
      this.methodForList = methodForList;
    }

    protected override string FetchName()
    {
      return (string)methodForDisplay.Invoke(type, null, new object[] { TargetValue });
    }

    protected internal override List<KeyValuePair<string, string>> BuildPairListForSelector(Sdx.Db.Connection conn)
    {
      if (!HasSelector)
      {
        return null;
      }
      return (List<KeyValuePair<string, string>>)methodForList.Invoke(type, null, null);
    }
  }
}
