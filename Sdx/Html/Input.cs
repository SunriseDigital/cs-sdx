using System;
using System.Linq;

namespace Sdx.Html
{
  public abstract class Input : Element
  {
    protected internal abstract string GetInputType();

    public Input():base()
    {

    }

    public Input(string name):base(name)
    {

    }

    protected internal override FormValue CreateFormValue()
    {
      return new FormValue(false);
    }

    internal protected override Tag CreateTag()
    {
      var tag = new VoidTag("input");
      tag.Attr.Set("type", this.GetInputType());

      return tag;
    }

    internal protected override void BindValueToTag()
    {
      this.Tag.Attr["value"] = this.Value.First();
    }
  }
}