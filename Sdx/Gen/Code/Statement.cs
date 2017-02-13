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
    private string[] formatValue;

    public Statement(string code, params string[] formatValue)
    {
      this.code = code;
      this.formatValue = formatValue;
    }

    public override void Add(Base code)
    {
      throw new NotSupportedException("You can't Add to Statement.");
    }

    internal override void Render(StringBuilder builder, string currentIndent, string newLineChar)
    {
      builder
        .Append(currentIndent)
        .AppendFormat(code, formatValue)
        .Append(newLineChar)
        ;
    }
  }
}
