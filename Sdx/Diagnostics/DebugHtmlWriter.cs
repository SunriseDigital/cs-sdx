using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sdx.Diagnostics
{
  public class DebugHtmlWriter : TextWriter
  {
    private StringBuilder builder = new StringBuilder();
    public override Encoding Encoding
    {
      get
      {
        //TODOどっかから共通の値をとれるようにする。
        return Encoding.UTF8;
      }
    }

    internal StringBuilder Builder
    {
      get
      {
        return this.builder;
      }
    }

    public override void Write(char value)
    {
      builder.Append(value);
    }

    public override string ToString()
    {
      return builder.ToString();
    }
  }
}
