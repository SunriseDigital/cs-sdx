using System;
using System.Collections;
using System.Collections.Generic;

namespace Sdx.Html
{
  public class RawText : IHtml
  {
    public string Text { get; private set; }

    public Attr Attr
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public RawText(string text)
    {
      this.Text = text;
    }

    public string Render(Attr attribute = null)
    {
      return this.Text;
    }

    public void ForEach(Action<IHtml> action)
    {
      throw new NotImplementedException();
    }

    public string RenderStartTag(Attr attribute = null)
    {
      throw new NotImplementedException();
    }

    public string RenderEndTag()
    {
      throw new NotImplementedException();
    }

    public IEnumerator<IHtml> GetEnumerator()
    {
      throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      throw new NotImplementedException();
    }
  }
}