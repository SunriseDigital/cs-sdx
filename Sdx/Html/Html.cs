using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public abstract class Html
  {
    private string tagName;

    internal protected List<Html> children = new List<Html>();
    /// <summary>
    /// HTMLのタグとして正当な文字列を返す
    /// </summary>
    /// <param name="attribute">タグの属性</param>
    /// <returns></returns>
    public abstract string Render(Attr attribute = null);

    public abstract string RenderStartTag(Attr attribute = null);

    public abstract string RenderEndTag();

    public string Render(params string[] classes)
    {
      var attr = new Attr();
      foreach(var val in classes)
      {
        attr.AddClass(val);
      }

      return this.Render(attr);
    }

    public string RenderStartTag(params string[] classes)
    {
      var attr = new Attr();
      foreach (var val in classes)
      {
        attr.AddClass(val);
      }

      return this.RenderStartTag(attr);
    }


    public Attr Attr { get; internal protected set; }

    public string TagName { get; private set; }

    public Html(string tagName)
    {
      this.TagName = tagName;
    }

    public void ForEach(Action<Html> action)
    {
      this.children.ForEach(action);
    }

    public IEnumerable<Html> Children
    {
      get
      {
        return this.children;
      }
    }
  }
}
