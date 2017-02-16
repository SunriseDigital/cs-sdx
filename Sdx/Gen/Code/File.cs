using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Gen.Code
{
  public class File : Base
  {
    internal override void Render(Base rootCode, StringBuilder builder, string currentIndent, string newLineChar)
    {
      codeList.ForEach(code => code.Render(rootCode, builder, currentIndent, newLineChar));
    }

    internal override string KeyWord
    {
      get { throw new InvalidOperationException("`File` has no KeyWord."); }
    }
  }
}
