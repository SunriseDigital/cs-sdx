using System;
using System.Collections.Generic;
using System.Text;

namespace Sdx.Html
{
  public class Tag : AbstractTag
  {
    private List<IHtml> children;

    public Tag(string tagName) : base(tagName)
    {
      this.children = new List<IHtml>();
    }

    public Tag AddHtml(IHtml html)
    {
      this.children.Add(html);
      return this;
    }

    public string RenderStartTag(Attr attribute = null)
    {
      var builder = new StringBuilder();
      this.RenderStartTag(builder, attribute);
      return builder.ToString();
    }

    public string RenderEndTag()
    {
      return "</" + this.tagName + ">";
    }

    public override void Render(StringBuilder builder, Attr attribute = null)
    {
      this.RenderStartTag(builder, attribute);

      foreach (IHtml elem in children)
      {
        builder.Append(elem.Render());
      }

      builder.Append(RenderEndTag());
    }
  }
}