using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sdx.Scaffold.Config
{
  public class Value
  {
    private object value;

    public Value(string value)
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

    public Value(MethodInfo methodInfo)
    {
      if(methodInfo == null)
      {
        throw new ArgumentNullException("MethodInfo is null");
      }
      value = methodInfo;
    }

    internal object Invoke(Type type, object target, object[] args)
    {
      if(value is MethodInfo)
      {
        return ((MethodInfo)value).Invoke(target, args);
      }
      else
      {
        return type.GetMethod(value.ToString()).Invoke(target, args);
      }
    }

    public override string ToString()
    {
      return value.ToString();
    }

    internal MethodInfo GetMethodInfo(Type type)
    {
      if (value is MethodInfo)
      {
        return (MethodInfo)value;
      }
      else
      {
        return type.GetMethod(value.ToString());
      }
    }
  }
}
