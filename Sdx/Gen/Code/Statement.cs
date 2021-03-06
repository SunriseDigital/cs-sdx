﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Gen.Code
{
  public class Statement : Base
  {
    private string code;

    public Statement(string code, params string[] formatValue)
    {
      if (formatValue.Length == 0)
      {
        this.code = code;
      }
      else
      {
        this.code = string.Format(code, formatValue);
      }
    }

    public override void AddChild(Base code)
    {
      throw new NotSupportedException("You can't Add to Statement.");
    }

    internal override void Render(Base rootCode, StringBuilder builder, string currentIndent)
    {
      var newLine = NewLine == null ? rootCode.NewLine : NewLine;

      if (code.Length > 0)
      {
        builder
          .Append(currentIndent)
          .Append(code);
      }

      builder.Append(newLine);
    }

    internal override string KeyWord
    {
      get { return code; }
    }
  }
}
