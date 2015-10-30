using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public abstract class Element : IHtml
  {
    private AbstractTag tag;
    public string Name { get; set; }

    protected abstract AbstractTag CreateTag();

    public void Render(StringBuilder builder, Attr attribute = null)
    {
      this.tag.Render(builder, attribute);
    }

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
