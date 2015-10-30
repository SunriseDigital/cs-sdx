using System;
using System.Text;

namespace Sdx.Html
{
  public class VoidTag : IHtml
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
  }
}