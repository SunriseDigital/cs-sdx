using System;

namespace Sdx.Html
{
  public class CheckBox : Checkable
  {
    public CheckBox():base()
    {

    }

    public CheckBox(string name):base(name)
    {

    }

    internal protected override Html CreateTag()
    {
      var tag = new VoidTag("input");
      tag.Attr["type"] = "checkbox";

      return tag;
    }
  }
}