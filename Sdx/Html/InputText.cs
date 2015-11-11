using System;
using System.Linq;

namespace Sdx.Html
{
  public class InputText : Element
  {
    public InputText():base()
    {

    }

    public InputText(string name):base(name)
    {

    }

    protected internal override FormValue CreateFormValue()
    {
      return new FormValue(false);
    }

    internal protected override Tag CreateTag()
    {
      var tag = new VoidTag("input");
      tag.Attr.Set("type", "text");

      return tag;
    }

    internal protected override void BindValue(object value)
    {
      base.BindValue(value);
      this.Tag.Attr["value"] = this.Value.First();
    }
  }
}