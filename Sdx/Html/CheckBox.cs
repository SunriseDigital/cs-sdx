using System;

namespace Sdx.Html
{
  public class CheckBox : Element
  {
    public CheckBox()
    {
    }

    protected override ITag CreateTag()
    {
      var tag = new VoidTag("input");
      tag.Attr["type"] = "checkbox";
      tag.Attr["value"] = "";

      return tag;
    }
  }
}