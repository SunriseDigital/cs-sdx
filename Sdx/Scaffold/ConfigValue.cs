using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Scaffold
{
  public class ConfigValue
  {
    private object value;

    public ConfigValue(string value)
    {
      var tmp = value.ToLower();
      if(tmp == "on" || tmp == "true")
      {
        this.value = true;
      }
      else if(tmp == "off" || tmp == "false")
      {
        this.value = false;
      }
      else
      {
        this.value = value;
      }
    }

    public string Value
    {
      get
      {
        return this.value.ToString();
      }
    }

    public override string ToString()
    {
      return value.ToString();
    }
  }
}
