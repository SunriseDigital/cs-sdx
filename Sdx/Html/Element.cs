using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public abstract class Element : IHtml
  {
    private IHtml tag;
    public string Name { get; set; }

    protected abstract IHtml CreateTag();

    public string Render(Attr attribute = null)
    {
      return this.tag.Render(attribute);
    }

    public Element()
    {
      this.tag = this.CreateTag();
    }
  }
}
