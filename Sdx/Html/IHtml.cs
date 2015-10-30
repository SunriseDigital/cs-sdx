using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public interface IHtml
  {
    /// <summary>
    /// HTMLのタグとして正当な文字列を返す
    /// </summary>
    /// <param name="attribute">タグの属性</param>
    /// <returns></returns>
    string Render(Attr attribute = null);
  }
}
