using System;
using System.Collections;
using System.Collections.Generic;

namespace Sdx.Html
{
  public class RawText : Html
  {
    public string Text { get; private set; }

    public RawText(string text):base(null)
    {
      this.Text = text;
    }

    public override string Render(Attr attribute = null)
    {
      return this.Text;
    }

    public override string RenderStartTag(Attr attribute = null)
    {
      return null;
    }

    public override string RenderEndTag()
    {
      return null;
    }
  }
}