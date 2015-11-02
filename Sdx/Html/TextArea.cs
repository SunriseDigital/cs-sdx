using System;

namespace Sdx.Html
{
  public class TextArea: Element
  {
    public TextArea()
    {
    }

    protected override ITag CreateTag()
    {
      return new Tag("textarea");
    }

    public override string Value
    {
      get
      {
        var ta = (Tag)this.tag;
        if(ta.Children.Count == 0)
        {
          return "";
        }
        else
        {
          var rt = (RawText)ta.Children[0];
          return rt.Text;
        }
      }

      set
      {
        var ta = (Tag)this.tag;
        ta.Children.Clear();
        ta.AddHtml(new RawText(value));
      }
    }
  }
}