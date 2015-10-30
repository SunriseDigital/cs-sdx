using System;
using System.Collections.Generic;
using System.Text;

namespace Sdx.Html
{
  public class Tag : IHtml
  {
    private string tagName;
    private List<IHtml> children;
    private Attr attribute;

    public Attr Attr
    {
      get
      {
        return this.attribute;
      }
    }

    public Tag(string tagName)
    {
      this.tagName = tagName;
      this.attribute = new Attr();
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

    /// <summary>
    /// Form.Render()から使っているのでinternal
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="attribute"></param>
    internal void RenderStartTag(StringBuilder builder, Attr attribute)
    {
      builder
        .Append("<")
        .Append(this.tagName);

      if (this.attribute.Count > 0)
      {
        builder.Append(" ");
        this.attribute.Render(builder, attribute);
      }

      builder.Append(">");
    }

    public string RenderEndTag()
    {
      return "</" + this.tagName + ">";
    }

    public string Render(Attr attribute = null)
    {
      var builder = new StringBuilder();
      this.RenderStartTag(builder, attribute);

      foreach (IHtml elem in children)
      {
        builder.Append(elem.Render());
      }

      builder.Append(RenderEndTag());
      return builder.ToString();
    }
  }
}