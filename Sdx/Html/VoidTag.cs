using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sdx.Html
{
  public class VoidTag : Tag
  {
    public VoidTag(string tagName):base(tagName)
    {
      this.Attr = new Attr();
    }

    public VoidTag(string tagName, params string[] classes)
      : this(tagName)
    {
      this.Attr = new Attr();
      this.Attr.AddClass(classes);
    }

    public VoidTag(string tagName, Action<Attr> callback)
      : this(tagName)
    {
      this.Attr = new Attr();
      this.AttrCall(callback);
    }

    public override string Render(Attr attribute = null)
    {
      var builder = new StringBuilder();
      builder
        .Append("<")
        .Append(this.TagName);

      var newAttr = this.Attr.Merge(attribute);
      if (newAttr.Count > 0)
      {
        builder.Append(" ");
        newAttr.Render(builder);
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