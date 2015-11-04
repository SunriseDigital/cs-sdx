using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public abstract class Element : ITag
  {
    internal protected ITag tag;

    private FormValue value = new FormValue();

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

    public FormValue Value
    {
      get
      {
        return this.value;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value">string|string[]</param>
    internal protected virtual void BindValue(object value)
    {
      this.value.Set(value);
    }

    public void Bind(string value)
    {
      this.BindValue(value);
    }
  }
}
