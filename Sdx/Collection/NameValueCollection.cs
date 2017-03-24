using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Collection
{
  public class NameValueCollection : System.Collections.Specialized.NameValueCollection
  {
    public int GetInt32Value(string key)
    {
      return Int32.Parse(this[key]);
    }

    public bool IsInt32(string key)
    {
      return GetValues(key).All(val => {
        int i;
        return Int32.TryParse(val, out i);
      });
    }

    public IEnumerable<int> GetInt32Values(string key)
    {
      return GetValues(key).Select(val => Int32.Parse(val));
    }

    public bool IsDateTime(string key)
    {
      return GetValues(key).All(val =>
      {
        DateTime i;
        return DateTime.TryParse(val, out i);
      });
    }

    public DateTime GetDateTimeValue(string key)
    {
      return DateTime.Parse(this[key]);
    }

    public IEnumerable<DateTime> GetDateTimeValues(string key)
    {
      return GetValues(key).Select(val => DateTime.Parse(val));
    }
  }
}
