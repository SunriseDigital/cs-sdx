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

    public bool StartLineBreak { get; set; }
    public bool EndLineBreak { get; set; }

    public Block(string firstLineCode, params string[] formatValue)
    {
      StartLineBreak = true;
      EndLineBreak = true;
      if (formatValue.Length == 0)
      {
        this.firstLineCode = firstLineCode;
      }
      else
      {
        this.firstLineCode = string.Format(firstLineCode, formatValue);
      }
    }

    internal override void Render(Base rootCode, StringBuilder builder, string currentIndent, string newLineChar)
    {
      var indent = Indent == null ? rootCode.Indent : Indent;
      var blockStart = BlockStart == null ? rootCode.BlockStart : BlockStart;
      var blockEnd = BlockEnd == null ? rootCode.BlockEnd : BlockEnd;

      builder.Append(currentIndent);
      builder.Append(firstLineCode);
      if (StartLineBreak)
      {
        builder.Append(newLineChar);
        builder.Append(currentIndent);
      }

      builder.Append(blockStart);
      builder.Append(newLineChar);
      codeList.ForEach(code => code.Render(rootCode, builder, currentIndent + indent, newLineChar));
      builder.Append(currentIndent);
      builder.Append(blockEnd);
      if(EndLineBreak)
      {
        builder.Append(newLineChar);
      }
    }

    internal override string KeyWord
    {
      get { return firstLineCode; }
    }
  }
}
