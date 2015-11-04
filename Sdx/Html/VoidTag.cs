using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sdx.Html
{
  public class VoidTag : ITag
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

    public VoidTag(string tagName)
    {
      this.tagName = tagName;
      this.attribute = new Attr();
    }

    public string Render(Attr attribute = null)
    {
      var builder = new StringBuilder();
      builder
        .Append("<")
        .Append(this.tagName);

      if (this.attribute.Count > 0)
      {
        builder.Append(" ");
        this.attribute.Render(builder, attribute);
      }

      builder.Append(">");

      return builder.ToString();
    }

    public void ForEach(Action<IHtml> action)
    {
      throw new NotImplementedException();
    }

    public IEnumerator<IHtml> GetEnumerator()
    {
      throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      throw new NotImplementedException();
    }

    public string RenderStartTag(Attr attribute = null)
    {
      return this.Render(attribute);
    }

    public string RenderEndTag()
    {
      throw new NotImplementedException();
    }
  }
}