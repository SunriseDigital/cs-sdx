using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public class FormValue : IEnumerable<string>, ICloneable
  {
    private string[] values;

    internal FormValue(bool isMultiple)
    {
      this.values = new string[0];
      this.IsMultiple = isMultiple;
    }

    public bool IsMultiple { get; internal set; }

    internal void Set(object value)
    {
      if (value is string[])
      {
        this.values = (string[])value;
      }
      else if (value is string)
      {
        var strVal = value.ToString();
        this.values = new string[] { strVal };
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

    public override string ToString()
    {
      return String.Join(",", this.values);
    }

    public bool IsEmpty
    {
      get
      {
        if(this.IsMultiple)
        {
          return this.Count == 0;
        }
        else if (this.Count == 0)
        {
          return true;
        }
        else
        {
          return this.First() == "";
        }
      } 
    }

    public int Count
    {
      get
      {
        return this.values.Length;
      }
    }

    public object Clone()
    {
      return this.MemberwiseClone();
    }

    public int ToInt()
    {
      if(IsMultiple)
      {
        throw new InvalidOperationException("This value has multilple value: " + Sdx.Diagnostics.Debug.Dump(values));
      }
      return Int32.Parse(values.First());
    }
  }
}
