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
    private string methodForDisplay;
    private string methodForList;

    public StaticClass(string columnName, Type type, string methodForDisplay = null, string methodForList = null)
      : base(columnName, methodForList != null)
    {
      this.columnName = columnName;
      this.type = type;
      this.methodForDisplay = methodForDisplay;
      this.methodForList = methodForList;
    }

    protected override string FetchName()
    {
      var method = type.GetMethods().First(m => m.Name == methodForDisplay && m.IsStatic && m.GetParameters()[0].ParameterType == typeof(String));
      return (string)method.Invoke(null, new object[] { TargetValue });
    }

    protected internal override List<KeyValuePair<string, string>> BuildPairListForSelector()
    {
      if (!HasSelector)
      {
        return null;
      }
      var method = type.GetMethods().First(m => m.Name == methodForList && m.IsStatic);
      return (List<KeyValuePair<string, string>>)method.Invoke(null, null);
    }
  }
}
