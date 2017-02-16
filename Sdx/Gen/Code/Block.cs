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
    public string StartDelimiter { get; set; }
    public string EndDelimiter { get; set; }

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

    internal override void Render(Base rootCode, StringBuilder builder, string currentIndent, string newLineChar, string startBlockDelimiter, string endBlockDelimiter)
    {
      Sdx.Context.Current.Debug.Log(string.Format("{0}: {1} {2}", KeyWord, rootCode.Indent, Indent == null ? "null" : Indent));
      var indent = Indent == null ? rootCode.Indent : Indent;
      builder.Append(currentIndent);
      builder.Append(firstLineCode);
      if (StartLineBreak)
      {
        builder.Append(newLineChar);
        builder.Append(currentIndent);
      }

      builder.Append(StartDelimiter == null ? StartBlockDelimiter : StartDelimiter);
      builder.Append(newLineChar);
      codeList.ForEach(code => code.Render(rootCode, builder, currentIndent + indent, newLineChar, startBlockDelimiter, endBlockDelimiter));
      builder.Append(currentIndent);
      builder.Append(EndDelimiter == null ? EndBlockDelimiter : EndDelimiter);
      if(EndLineBreak)
      {
        builder.Append(newLineChar);
      }
    }

    internal override string KeyWord
    {
      get { return firstLineCode; }
    }

    public void ChangeDilimiter(string start, string end)
    {
      StartDelimiter = start;
      EndDelimiter = end;
    }
  }
}
