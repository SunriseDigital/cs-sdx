using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Gen.Code
{
  public class Block : Base
  {
    private string firstLineCode;
    private string[] formatValue;

    public Block(string firstLineCode, params string[] formatValue)
    {
      this.firstLineCode = firstLineCode;
      this.formatValue = formatValue;
    }

    protected override void Render(StringBuilder builder, string currentIndent, string newLineChar)
    {
      builder.AppendFormat(firstLineCode, formatValue);
      builder.Append(newLineChar);
      builder.Append("{");
      builder.Append(newLineChar);
      base.Render(builder, currentIndent + Indent, newLineChar);
      builder.Append("}");
      builder.Append(newLineChar);
    }
  }
}
