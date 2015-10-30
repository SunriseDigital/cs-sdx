using System;

namespace Sdx.Html
{
  public class InputText : Element
  {
    public InputText()
    {
    }

    protected override AbstractTag CreateTag()
    {
      var tag = new VoidTag("input");
      tag.Attr.Set("type", "text");
      tag.Attr.Set("value", "");

      return tag;
    }
  }
}