using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Gen.Code
{
  abstract public class Base
  {
    protected List<Base> codeList = new List<Base>();

    private string newLineChar = Environment.NewLine;
    public string NewLineChar { get { return newLineChar; } set { newLineChar = value; } }

    private string indent = "  ";
    public string Indent { get { return indent; } set { newLineChar = value; } }

    public void Add(string code, params string[] formatValue)
    {
      Add(new Statement(code, formatValue));
    }

    public void AddBlankLine()
    {
      Add(new Statement(""));
    }

    public virtual void Add(Base code)
    {
      codeList.Add(code);
    }

    abstract internal void Render(StringBuilder builder, string currentIndent, string newLineChar);

    public string Render()
    {
      var builder = new StringBuilder();
      Render(builder, "", NewLineChar);
      return builder.ToString();
    }
  }
}
