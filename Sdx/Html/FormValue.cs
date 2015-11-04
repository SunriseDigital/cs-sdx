using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public class FormValue
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

    public int Count
    {
      get
      {
        return this.values.Length;
      }
    }

    public string First
    {
      get
      {
        if(this.values.Length == 0)
        {
          return "";
        }

        return this.values[0];
      }
    }

    public string[] All
    {
      get
      {
        return (string[])this.values.Clone();
      }
    }
  }
}
