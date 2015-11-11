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

    public Tag AddHtml(HtmlBase html)
    {
      this.children.Add(html);
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