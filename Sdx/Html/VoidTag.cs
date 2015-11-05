using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sdx.Html
{
  public class VoidTag : Html
  {
    public VoidTag(string tagName):base(tagName)
    {
      this.Attr = new Attr();
    }

    public override string Render(Attr attribute = null)
    {
      var builder = new StringBuilder();
      builder
        .Append("<")
        .Append(this.TagName);

      if (this.Attr.Count > 0)
      {
        builder.Append(" ");
        this.Attr.Render(builder, attribute);
      }

      builder.Append(">");

      return builder.ToString();
    }

    public override string RenderStartTag(Attr attribute = null)
    {
      return this.Render(attribute);
    }

    public override string RenderEndTag()
    {
      throw new NotImplementedException();
    }
  }
}