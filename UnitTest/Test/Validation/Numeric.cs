using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Validation
{
  public class Numeric : Sdx.Validation.Numeric
  {
    public static Dictionary<string, Dictionary<string, string>> MessageTemplates { get; private set; }

    static Numeric()
    {
      MessageTemplates = new Dictionary<string, Dictionary<string, string>>
      {
        {"ja", new Dictionary<string, string> {
          {"ErrorNotNumeric", "あへあへうひは"}
        }},
        {"en", new Dictionary<string, string> {
          {"ErrorNotNumeric", "Ahe ahe uhiha"}
        }}
      };
    }
  }
}
