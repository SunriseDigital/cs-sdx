using System;
using System.Web.Script.Serialization;

namespace Sdx.Util
{
  public static class Json
  {
    public static string Encoder(object obj)
    {
      return new JavaScriptSerializer().Serialize(obj);
    }

    public static dynamic Decode(string json)
    {
      return new JavaScriptSerializer().Deserialize<dynamic>(json);
    }
  }
}
