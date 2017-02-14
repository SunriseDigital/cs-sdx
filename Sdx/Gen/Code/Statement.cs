using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Gen.Code
{
  public class Statement : Base
  {
    private string code;

    public Statement(string code, params object[] formatValue)
    {
      this.code = string.Format(code, formatValue);
    }

    public override void AddChild(Base code)
    {
      throw new NotSupportedException("You can't Add to Statement.");
    }

    internal override void Render(StringBuilder builder, string currentIndent, string newLineChar)
    {
      builder
        .Append(currentIndent)
        .Append(code)
        .Append(newLineChar)
        ;
    }

    internal override string KeyWord
    {
      get { return code; }
    }
  }
}
