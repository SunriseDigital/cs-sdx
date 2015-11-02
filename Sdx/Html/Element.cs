using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public abstract class Element : ITag
  {
    internal protected ITag tag;
    public string Name
    {
      get
      {
        return this.tag.Attr["name"];
      }

      set
      {
        this.tag.Attr["name"] = value;
      }
    }

    protected abstract ITag CreateTag();

    public string Render(Attr attribute = null)
    {
      return this.tag.Render(attribute);
    }

    public Element()
    {
      this.tag = this.CreateTag();
    }

    public Attr Attr
    {
      get
      {
        return this.tag.Attr;
      }
    }

    public virtual string Value
    {
      get
      {
        return this.tag.Attr["value"];
      }
      set
      {
        this.tag.Attr["value"] = value;
      }
    }
  }
}
