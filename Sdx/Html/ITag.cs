using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public interface ITag: IHtml
  {
    Attr Attr { get; }
  }
}
