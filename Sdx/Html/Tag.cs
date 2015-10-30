using System;
using System.Collections.Generic;

namespace Sdx.Html
{
  public class Tag : ITag
  {
    private string tagName;
    private List<ITag> children;
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
      this.children = new List<ITag>();
    }

    public Tag(string tagName) : this()
    {
      this.tagName = tagName;
    }

    public Tag AddTag(ITag element)
    {
      this.children.Add(element);
      return this;
    }

    public virtual string RenderStartTag(Attr attribute = null)
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
      string tags = RenderStartTag(attribute);

      if (this.children != null)
      {
        foreach (ITag elem in children)
        {
          tags += elem.Render();
        }
      }

      tags += RenderEndTag();
      return tags;
    }
  }
}