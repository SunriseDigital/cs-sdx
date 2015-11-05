using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sdx.Html
{
  public class Tag : Html
  {
    public Tag(string tagName):base(tagName)
    {
      this.Attr = new Attr();
    }

    public Tag AddHtml(Html html)
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

      if (this.Attr.Count > 0)
      {
        builder.Append(" ");
        this.Attr.Render(builder, attribute);
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

      foreach (Html child in children)
      {
        builder.Append(child.Render());
      }

      builder.Append(RenderEndTag());
      return builder.ToString();
    }
  }
}