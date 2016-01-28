using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Scaffold
{
  public class Param : IEnumerable<KeyValuePair<string, string>>
  {
    public static Param Create()
    {
      return new Param();
    }

    private Dictionary<string, string> vars = new Dictionary<string, string>();

    public bool StrictCheck = true;

    public string this[string key]
    {
      set
      {
        this.vars[key] = value;
      }

      get
      {
        return this.vars[key];
      }
    }

    public bool ContainsKey(string key)
    {
      return this.vars.ContainsKey(key);
    }

    public Param Set(string key, string value)
    {
      this.vars[key] = value;
      return this;
    }

    public object Get(string key)
    {
      return this.vars[key];
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
      return ((IEnumerable<KeyValuePair<string, string>>)vars).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<KeyValuePair<string, string>>)vars).GetEnumerator();
    }

  }
}
