using System;

namespace Sdx.Html
{
  public class Radio : Checkable
  {
    internal protected override Html CreateTag()
    {
      var tag = new VoidTag("input");
      tag.Attr["type"] = "radio";

      return tag;
    }
  }
}