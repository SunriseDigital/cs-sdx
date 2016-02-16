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
      //staticで名前がmethodForDisplay、引数が一つのStringであるメソッドを探す。
      var method = type.GetMethods().First(m => {
        if(m.Name != methodForDisplay)
        {
          return false;
        }

        if (!m.IsStatic)
        {
          return false;
        }

        var parameters = m.GetParameters();
        if (parameters.Count() != 1)
        {
          return false;
        }

        if (parameters[0].ParameterType != typeof(String))
        {
          return false;
        }

        return true;
      });
      return (string)method.Invoke(null, new object[] { TargetValue });
    }

    protected internal override List<KeyValuePair<string, string>> GetPairListForSelector()
    {
      var method = type.GetMethods().First(m =>
      {
        if(m.Name != methodForList)
        {
          return false;
        }

        if(!m.IsStatic)
        {
          return false;
        }

        if(m.GetParameters().Count() != 0)
        {
          return false;
        }
        
        return true;
      });

      return (List<KeyValuePair<string, string>>)method.Invoke(null, null);
    }
  }
}
