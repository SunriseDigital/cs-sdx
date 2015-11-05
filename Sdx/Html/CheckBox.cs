using System;

namespace Sdx.Html
{
  public class CheckBox : Checkable
  {
    internal protected override IHtml CreateTag()
    {
      var tag = new VoidTag("input");
      tag.Attr["type"] = "checkbox";

      return tag;
    }
  }
}