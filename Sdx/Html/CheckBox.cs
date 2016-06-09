using System;
using System.Collections.Generic;
using System.Linq;

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

    protected internal override FormValue CreateFormValue()
    {
      return new FormValue(false);
    }

    internal protected override Tag CreateTag()
    {
      var tag = new VoidTag("input");
      tag.Attr["type"] = "checkbox";

      return tag;
    }
  }
}