using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public abstract class AbstractTag : IHtml
  {
    protected string tagName;
    private Attr attribute;

    public Attr Attr
    {
      get
      {
        return this.attribute;
      }
    }

    public AbstractTag(string tagName)
    {
      this.tagName = tagName;
      this.attribute = new Attr();
    }

    internal void RenderStartTag(StringBuilder builder, Attr attribute)
    {
      builder
        .Append("<")
        .Append(this.tagName);

      if (this.Attr.Count > 0)
      {
        builder.Append(" ");
        this.Attr.Render(builder, attribute);
      }

      builder.Append(">");
    }

    public abstract void Render(StringBuilder builder, Attr attribute = null);

    public string Render(Attr attribute = null)
    {
      var builder = new StringBuilder();
      this.Render(builder, attribute);
      return builder.ToString();
    }
  }
}
