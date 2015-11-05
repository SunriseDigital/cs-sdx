using System;

namespace Sdx.Html
{
  public class CheckBox : Checkable
  {
    internal protected override Html CreateTag()
    {
      var tag = new VoidTag("input");
      tag.Attr["type"] = "checkbox";

      return tag;
    }
  }
}