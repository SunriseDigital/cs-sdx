using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public class FormValue: IEnumerable<string>
  {
    private string[] values;

    internal FormValue()
    {
      this.values = new string[0];
    }

    internal void Set(object value)
    {
      if (value is string[])
      {
        this.values = (string[])value;
      }
      else if (value is string)
      {
        var strVal = value.ToString();
        if (strVal != "")
        {
          this.values = new string[] { strVal };
        }
      }
      else
      {
        throw new ArgumentException("Support only string | string[]");
      }
    }

    public string this[int index]
    {
      get
      {
        return this.values[index];
      }
    }

    public IEnumerator<string> GetEnumerator()
    {
      return ((IEnumerable<string>)values).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<string>)values).GetEnumerator();
    }

    public bool IsEmpty
    {
      get
      {
        return this.values.Length == 0;
      } 
    }

    public int Count
    {
      get
      {
        return this.values.Length;
      }
    }
  }
}
