using System;
using System.Linq;

namespace Sdx.Html
{
  public class InputHidden : Element
  {
    public InputHidden():base()
    {

    }

    public InputHidden(string name):base(name)
    {

    }

    protected internal override FormValue CreateFormValue()
    {
      return new FormValue(false);
    }

    protected internal override Tag CreateTag()
    {
      var tag =  new VoidTag("input");
      tag.Attr["type"] = "hidden";
      return tag;
    }

    internal protected override void BindValueToTag()
    {
      this.Tag.Attr["value"] = this.Value.First();
    }
  }
}