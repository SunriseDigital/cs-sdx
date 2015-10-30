using System;
using System.Text;

namespace Sdx.Html
{
  public class VoidTag : AbstractTag
  {
    public VoidTag(string tagName):base(tagName)
    {

    }

    public override void Render(StringBuilder builder, Attr attribute = null)
    {
      this.RenderStartTag(builder, attribute);
    }
  }
}