using System;

namespace Sdx.Html
{
  public class TextArea: Element
  {
    internal protected override ITag CreateTag()
    {
      return new Tag("textarea");
    }

    internal protected override void BindValue(object value)
    {
      base.BindValue(value);
      var ta = (Tag)this.tag;
      ta.Children.Clear();
      ta.AddHtml(new RawText(value.ToString()));
    }
  }
}