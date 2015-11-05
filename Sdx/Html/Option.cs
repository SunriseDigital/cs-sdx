using System;

namespace Sdx.Html
{
  public class Option : Element
  {
    internal protected override Html CreateTag()
    {
      return new Tag("option");
    }

    public string Text
    {
      get
      {
        var op = (Tag)this.tag;
        var rt = (RawText)op.children[0];
        return rt.Text;
      }
      set
      {
        var op = (Tag)this.tag;
        op.children.Clear();
        op.AddHtml(new RawText(value));
      }
    }
  }
}