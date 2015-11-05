using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public abstract class Element
  {
    internal protected IHtml tag;

    private FormValue value = new FormValue();

    public virtual string Name
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

    internal protected abstract IHtml CreateTag();

    public IHtml Tag
    {
      get
      {
        return this.tag;
      }
    }

    public Element()
    {
      this.tag = this.CreateTag();
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

    public void Bind(string[] value)
    {
      this.BindValue(value);
    }
  }
}
