using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Web
{
  public class Holder
  {
    private Dictionary<string, object> vars = new Dictionary<string, object>();

    public bool ContainsKey(string key)
    {
      return this.vars.ContainsKey(key);
    }

    public Holder Set(string key, object value)
    {
      this.vars[key] = value;
      return this;
    }

    public object Get(string key)
    {
      if(this.ContainsKey(key))
      {
        return this.vars[key];
      }

      return null;
    }

    public T As<T>(string key)
    {
      if(this.ContainsKey(key))
      {
        object value = this.Get(key);
        if(value is T)
        {
          return (T)value;
        }
      }

      return (T)Activator.CreateInstance(typeof(T));
    }
  }
}
