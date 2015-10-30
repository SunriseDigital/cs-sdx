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

    public Tag()
    {
      this.attribute = new Attr();
      init();
    }

    protected virtual void init()
    {
      this.children = new List<IHtml>();
    }

    public Tag(string tagName) : this()
    {
      this.tagName = tagName;
    }

    public Tag AddHtml(IHtml html)
    {
      Sdx.Context.Current.Debug.Log(this.children);
      this.children.Add(html);
      return this;
    }

    public string RenderStartTag(Attr attribute = null)
    {
      if(this.attribute.Count > 0)
      {
        return "<" + this.tagName + " " + this.attribute.Render(attribute) + ">";
      }
      else
      {
        return "<" + this.tagName + ">";
      }
    }

    public virtual string RenderEndTag()
    {
      return "</" + this.tagName + ">";
    }

    public virtual string Render(Attr attribute = null)
    {
      var builder = new StringBuilder();
      builder.Append(RenderStartTag(attribute));

      if (this.children != null)
      {
        foreach (IHtml elem in children)
        {
          builder.Append(elem.Render());
        }
      }

      builder.Append(RenderEndTag());
      return builder.ToString();
    }
  }
}