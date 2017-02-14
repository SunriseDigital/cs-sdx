using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Gen.Code
{
  public class String
  {
    public string Value{get; private set;}
    public String(string value)
    {
      Value = value;
    }

    public override string ToString()
    {
      return string.Format(@"""{0}""", Value);
    }
  }
}
