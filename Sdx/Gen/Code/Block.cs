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

    public Block(string firstLineCode, params string[] formatValue)
    {
      if (formatValue.Length == 0)
      {
        this.firstLineCode = firstLineCode;
      }
      else
      {
        this.firstLineCode = string.Format(firstLineCode, formatValue);
      }
    }

    internal override void Render(Base rootCode, StringBuilder builder, string currentIndent)
    {
      var indent = Indent == null ? rootCode.Indent : Indent;
      var blockStart = BlockStart == null ? rootCode.BlockStart : BlockStart;
      var blockEnd = BlockEnd == null ? rootCode.BlockEnd : BlockEnd;
      var newLine = NewLine == null ? rootCode.NewLine : NewLine;
      var startLineBreak = StartLineBreak == null ? rootCode.StartLineBreak : StartLineBreak;
      var endLineBreak = EndLineBreak == null ? rootCode.EndLineBreak : EndLineBreak;

      builder.Append(currentIndent);
      builder.Append(firstLineCode);
      if ((bool)startLineBreak)
      {
        builder.Append(newLine);
        builder.Append(currentIndent);
      }

      builder.Append(blockStart);
      builder.Append(newLine);
      codeList.ForEach(code => code.Render(rootCode, builder, currentIndent + indent));

      if (blockEnd.Length > 0)
      {
        if ((bool)endLineBreak)
        {
          builder.Append(currentIndent);
        }

        builder.Append(blockEnd);
        if ((bool)endLineBreak)
        {
          builder.Append(newLine);
        }
      }
    }

    internal override string KeyWord
    {
      get { return firstLineCode; }
    }
  }
}
