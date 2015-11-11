using System;

namespace Sdx.Html
{
  public class Radio : Checkable
  {
    public Radio():base()
    {

    }

    public Radio(string name):base(name)
    {

    }

    protected internal override FormValue CreateFormValue()
    {
      return new FormValue(false);
    }

    internal protected override Html CreateTag()
    {
      var tag = new VoidTag("input");
      tag.Attr["type"] = "radio";

      return tag;
    }
  }
}