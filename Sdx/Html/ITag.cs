using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public interface ITag: IHtml, IEnumerable<IHtml>
  {
    Attr Attr { get; }

    void ForEach(Action<IHtml> action);

    string RenderStartTag(Attr attribute = null);

    string RenderEndTag();
  }
}
