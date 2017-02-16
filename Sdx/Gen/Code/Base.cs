using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sdx.Gen.Code
{
  abstract public class Base
  {
    protected List<Base> codeList = new List<Base>();

    public Base Parent { get; private set; }

    private string newLineChar = Environment.NewLine;
    public string NewLineChar { get { return newLineChar; } set { newLineChar = value; } }

    public string Indent { get; set; }

    private string startBlockDelimiter = "{";
    public string StartBlockDelimiter { get { return startBlockDelimiter; } set { startBlockDelimiter = value; } }

    private string endBlockDelimiter = "}";
    public string EndBlockDelimiter { get { return endBlockDelimiter; } set { endBlockDelimiter = value; } }

    public IEnumerable<Base> Children
    {
      get
      {
        return codeList;
      }
    }

    public void AddChild(string code, params string[] formatValue)
    {
      AddChild(new Statement(code, formatValue));
    }

    public void AddBlankLine()
    {
      AddChild(new Statement(""));
    }

    public virtual void AddChild(Base code)
    {
      if(code is File)
      {
        throw new ArgumentException("`File` can not be added to other objects. It is the root object.");
      }
      codeList.Add(code);
      code.Parent = this;
    }

    abstract internal void Render(Base rootCode, StringBuilder builder, string currentIndent, string newLineChar, string startBlockDelimiter, string endBlockDelimiter);

    public string Render()
    {
      if (Indent == null)
      {
        Indent = "  ";//defulat value
      }
      
      var builder = new StringBuilder();
      Render(this, builder, "", NewLineChar, StartBlockDelimiter, EndBlockDelimiter);
      return builder.ToString();
    }

    abstract internal string KeyWord { get; }

    public Base GetChild(int lineNumber)
    {
      return codeList[lineNumber];
    }

    /// <summary>
    /// 見つからなかったといはNULLを返します。HASメソッドは検索コストが変わらないのでありません。NULLをチェックしてください。
    /// </summary>
    /// <param name="words"></param>
    /// <returns></returns>
    public Base FindChild(string words)
    {
      var results = codeList.Where(code => code.KeyWord.Contains(words));

      var count = results.Count();
      if(count > 1)
      {
        throw new InvalidOperationException("More than one code with the `" + words + "` words was found.");
      }

      return results.FirstOrDefault();
    }

    /// <summary>
    /// <see cref="FindChild(string words)"/>
    /// </summary>
    /// <param name="words"></param>
    /// <returns></returns>
    public Base FindChild(Regex regex)
    {
      var results = codeList.Where(code => regex.IsMatch(code.KeyWord));

      var count = results.Count();
      if (count > 1)
      {
        throw new InvalidOperationException("More than one code with the `" + regex + "` words was found.");
      }

      return results.FirstOrDefault();
    }
  }
}
