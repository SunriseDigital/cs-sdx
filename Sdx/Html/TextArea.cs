using System;

namespace Sdx.Html
{
  public class TextArea: Element
  {
    public TextArea():base()
    {

    }

    public TextArea(string name):base(name)
    {

    }

    internal protected override Html CreateTag()
    {
      return new Tag("textarea");
    }

    internal protected override void BindValue(object value)
    {
      base.BindValue(value);
      var ta = (Tag)this.tag;
      ta.children.Clear();
      ta.AddHtml(new RawText(value.ToString()));
    }
  }
}