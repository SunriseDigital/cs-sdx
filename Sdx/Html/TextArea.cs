using System;
using System.Linq;

namespace Sdx.Html
{
  public class TextArea: FormElement
  {
    public TextArea():base()
    {

    }

    public TextArea(string name):base(name)
    {

    }

    protected internal override FormValue CreateFormValue()
    {
      return new FormValue(false);
    }

    internal protected override Tag CreateTag()
    {
      return new Tag("textarea");
    }

    internal protected override void BindValueToTag()
    {
      var ta = this.tag;
      ta.children.Clear();
      ta.AddHtml(new RawText(this.Value.First()));
    }
  }
}