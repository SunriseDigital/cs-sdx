using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Gen.Code
{
  public class File : Base
  {
    internal override void Render(StringBuilder builder, string currentIndent, string newLineChar, string startBlockDelimiter, string endBlockDelimiter)
    {
      codeList.ForEach(code => code.Render(builder, currentIndent, newLineChar, startBlockDelimiter, endBlockDelimiter));
    }

    internal override string KeyWord
    {
      get { throw new InvalidOperationException("`File` has no KeyWord."); }
    }
  }
}
