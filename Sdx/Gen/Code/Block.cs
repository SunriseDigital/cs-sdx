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

    internal override void Render(StringBuilder builder, string currentIndent, string newLineChar)
    {
      if (firstLineCode != null)
      {
        builder.Append(currentIndent);
        builder.AppendFormat(firstLineCode, formatValue);
        builder.Append(newLineChar);
      }

      builder.Append(currentIndent);
      builder.Append("{");
      builder.Append(newLineChar);
      codeList.ForEach(code => code.Render(builder, currentIndent + Indent, newLineChar));
      builder.Append(currentIndent);
      builder.Append("}");
      builder.Append(newLineChar);
    }
  }
}
