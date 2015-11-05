using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sdx.Html
{
  public class Tag : IHtml
  {
    private string tagName;
    private Attr attribute;

    public Attr Attr
    {
      get
      {
        return this.attribute;
      }
    }

    internal List<IHtml> Children { get; private set; }

    public Tag(string tagName)
    {
      this.tagName = tagName;
      this.attribute = new Attr();
      this.Children = new List<IHtml>();
    }

    public Tag AddHtml(IHtml html)
    {
      this.Children.Add(html);
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

      foreach (IHtml elem in Children)
      {
        builder.Append(elem.Render());
      }

      builder.Append(RenderEndTag());
      return builder.ToString();
    }

    public void ForEach(Action<IHtml> action)
    {
      this.Children.ForEach(html => action(html));
    }

    public IEnumerator<IHtml> GetEnumerator()
    {
      return ((IEnumerable<IHtml>)this.Children).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<IHtml>)this.Children).GetEnumerator();
    }
  }
}