using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;


namespace Sdx.Util
{
  public static class Json
  {
    public static string Encoder(object obj)
    {
      return JsonConvert.SerializeObject(obj);
    }

    public static T Decode<T>(string json)
    {
      return JsonConvert.DeserializeObject<T>(json);
    }
  }
}
