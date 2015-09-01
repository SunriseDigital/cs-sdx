using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Util
{
    public class Reflection
    {
      private static Dictionary<string, Type> types;

      static Reflection()
      {
        types = new Dictionary<string, Type>();
      }

      public static Type GetType(string className)
      {
        if (types.ContainsKey(className))
        {
          return types[className];
        }

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
          var type = asm.GetType(className);
          if (type != null)
          {
            types[className] = type;
            return type;
          }
        }
        return null;
      }
    }
}
