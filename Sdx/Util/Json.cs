using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;


namespace Sdx.Util
{
  public static class Json
  {
    public static string Encoder(object obj)
    {
      var serializer = new JavaScriptSerializer();
      return serializer.Serialize(obj);
    }

    public static T Decode<T>(string json)
    {
      var serializer = new JavaScriptSerializer();
      return serializer.Deserialize<T>(json);
    }
  }
}
