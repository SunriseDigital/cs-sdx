using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public abstract class HtmlBase
  {
    internal protected List<HtmlBase> children = new List<HtmlBase>();
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

    public string Render(Action<Attr> callback)
    {
      var attr = new Attr();
      callback.Invoke(attr);

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

    public string RenderStartTag(Action<Attr> callback)
    {
      var attr = new Attr();
      callback.Invoke(attr);

      return this.RenderStartTag(attr);
    }

    public HtmlBase AttrCall(Action<Attr> callback)
    {
      callback.Invoke(Attr);
      return this;
    }

    public Attr Attr { get; internal protected set; }

    public string TagName { get; private set; }

    public HtmlBase(string tagName)
    {
      this.TagName = tagName;
    }

    public void ForEach(Action<HtmlBase> action)
    {
      this.children.ForEach(action);
    }

    public IEnumerable<HtmlBase> Children
    {
      get
      {
        return this.children;
      }
    }

    public HtmlBase If(Predicate<HtmlBase> condition, Action<HtmlBase> trueCallback, Action<HtmlBase> falseCallback = null)
    {
      if (condition.Invoke(this))
      {
        trueCallback.Invoke(this);
      }
      else if (falseCallback != null)
      {
        falseCallback.Invoke(this);
      }

      return this;
    }
  }
}
