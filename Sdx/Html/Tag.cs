using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sdx.Html
{
  public class Tag : HtmlBase
  {
    public Tag(string tagName):base(tagName)
    {
      this.Attr = new Attr();
    }

    public Tag(string tagName, params string[] classes)
      : this(tagName)
    {
      this.Attr = new Attr();
      this.Attr.AddClass(classes);
    }

    public Tag(string tagName, Action<Attr> callback)
      : this(tagName)
    {
      this.Attr = new Attr();
      this.AttrCall(callback);
    }

    public Tag AddHtml(HtmlBase html)
    {
      this.children.Add(html);
      return this;
    }

    /// <summary>
    /// 子要素にテキストを追加します。
    /// </summary>
    /// <param name="text"></param>
    /// <param name="htmlEncode">HTMLエンティティにエンコードするかどうか。デフォルトtrue</param>
    /// <returns></returns>
    public Tag AddText(string text, bool htmlEncode = true)
    {
      this.AddHtml(new RawText(text, htmlEncode));
      return this;
    }

    public Tag AddChild(object target)
    {
      if(target is HtmlBase)
      {
        AddHtml((HtmlBase)target);
      }
      else if(target is string)
      {
        AddText((string)target);
      }
      else
      {
        throw new ArgumentException("Illegal type argument");
      }

      return this;
    }

    public override string RenderStartTag(Attr attribute = null)
    {
      var builder = new StringBuilder();
      this.RenderStartTag(builder, attribute);
      return builder.ToString();
    }

    private void RenderStartTag(StringBuilder builder, Attr attribute)
    {
      builder
        .Append("<")
        .Append(this.TagName);

      var newAttr = this.Attr.Merge(attribute);
      if(newAttr.Count > 0)
      {
        builder.Append(" ");
        newAttr.Render(builder);
      }

      builder.Append(">");
    }

    public override string RenderEndTag()
    {
      return "</" + this.TagName + ">";
    }

    public override string Render(Attr attribute = null)
    {
      var builder = new StringBuilder();
      this.RenderStartTag(builder, attribute);

      foreach (HtmlBase child in children)
      {
        builder.Append(child.Render());
      }

      builder.Append(RenderEndTag());
      return builder.ToString();
    }
  }
}