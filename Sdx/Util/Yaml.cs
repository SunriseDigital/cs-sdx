using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Util
{
  public static class Yaml
  {
    public static T Decode<T>(string yaml)
    {
      object targetObj;
      using (TextReader sr = new StringReader(yaml))
      {
        var deserializer = new YamlDotNet.Serialization.Deserializer();
        targetObj = deserializer.Deserialize(sr);
      }

      return (T)targetObj;
    }
  }
}
