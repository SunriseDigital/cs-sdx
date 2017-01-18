using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Sdx.Html
{
  public class Attr
  {
    public static Attr Create()
    {
      return new Attr();
    }

    private Collection.OrderedDictionary<string, object> attrDictionary = new Collection.OrderedDictionary<string, object>();

    public int Count
    {
      get
      {
        return this.attrDictionary.Count;
      }
    }

    public string this[string key]
    {
      get
      {
        if(!this.attrDictionary.ContainsKey(key))
        {
          return null;
        }

        if(key == "class")
        {
          return String.Join(" ", ((List<string>)this.attrDictionary[key]));
        }
        else if(key == "style")
        {
          var builder = new StringBuilder();
          var styles = (Collection.OrderedDictionary<string, string>)this.attrDictionary[key];
          this.RenderStyle(builder, styles);
          return builder.ToString();
        }
        else
        {
          return this.attrDictionary[key].ToString();
        }
      }

      set
      {
        this.Set(key, value);
      }
    }

    public bool Exists(string key)
    {
      return this.attrDictionary.ContainsKey(key);
    }

    public Attr AddClass(string value, bool needsAdd)
    {
      if (needsAdd)
      {
        this.AddClass(value);
      }

      return this;
    }

    /// <summary>
    /// クラス属性を追加します。
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public Attr AddClass(params string[] values)
    {
      if (!this.attrDictionary.ContainsKey("class"))
      {
        this.attrDictionary["class"] = new List<string>();
      }

      var classes = (List<string>)this.attrDictionary["class"];

      foreach(var val in values)
      {
        if (!classes.Contains(val))
        {
          classes.Add(val);
        }
      }

      return this;
    }

    /// <summary>
    /// 属性をセットします。同じキーを指定すると上書きます。
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Attr Set(string key, string value)
    {
      this.attrDictionary[key] = value;
      return this;
    }

    /// <summary>
    /// 属性を削除します。classやstyleも削除可能。
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Attr Remove(string key)
    {
      this.attrDictionary.Remove(key);
      return this;
    }

    /// <summary>
    /// クラス属性を削除します。存在しないクラスを指定した場合、何も起きません。
    /// 全てのclass属性が削除されると`class=""`も出力されません。
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public Attr RemoveClass(params string[] values)
    {
      if(this.attrDictionary.ContainsKey("class"))
      {
        var classes = (List<string>)this.attrDictionary["class"];
        foreach (var val in values)
        {
          classes.Remove(val);
        }

        if(classes.Count == 0)
        {
          this.attrDictionary.Remove("class");
        }
      }

      return this;
    }

    /// <summary>
    /// style属性を追加します。同じキーをセットした場合上書きします。
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Attr SetStyle(string key, string value)
    {
      if (!this.attrDictionary.ContainsKey("style"))
      {
        this.attrDictionary["style"] = new Collection.OrderedDictionary<string, string>();
      }

      var styles = (Collection.OrderedDictionary<string, string>)this.attrDictionary["style"];

      styles[key] = value;

      return this;
    }

    /// <summary>
    /// `disabled="disabled"`の様な、属性のキーと値が同じ属性を追加します。
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Attr Set(string value)
    {
      this.attrDictionary[value] = null;
      return this;
    }

    /// <summary>
    /// style属性を削除します。存在しないキーを指定した場合何も起きません。
    /// 全てのstyle属性が削除されると`style=""`も出力されません。
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Attr RemoveStyle(string key)
    {
      if (this.attrDictionary.ContainsKey("style"))
      {
        var values = (Collection.OrderedDictionary<string, string>)this.attrDictionary["style"];
        values.Remove(key);

        if (values.Count == 0)
        {
          this.attrDictionary.Remove("style");
        }
      }

      return this;
    }

    public Attr Merge(Attr attribute)
    {
      var newAttr = new Attr();

      newAttr.attrDictionary = this.CloneAttrDictionary();
      if (attribute != null)
      {
        attribute.attrDictionary.ForEach((key, value) =>
        {
          //merge
          if (key == "class" && newAttr.attrDictionary.ContainsKey(key))
          {
            var tmpClasses = (List<string>)newAttr.attrDictionary["class"];
            var argClasses = (List<string>)value;
            argClasses.ForEach(val =>
            {
              if (!tmpClasses.Contains(val))
              {
                tmpClasses.Add(val);
              }
            });
          }
          //merge
          else if (key == "style" && newAttr.attrDictionary.ContainsKey(key))
          {
            var tmpStyles = (Collection.OrderedDictionary<string, string>)newAttr.attrDictionary["style"];
            var argStyles = (Collection.OrderedDictionary<string, string>)value;
            argStyles.ForEach((k, v) =>
            {
              tmpStyles[k] = v;
            });
          }
          else
          {
            newAttr.attrDictionary[key] = value;
          }
        });
      }

      return newAttr;
    }


    internal void Render(StringBuilder builder)
    {
      this.RenderWithDictionary(builder, this.attrDictionary);
    }

    /// <summary>
    /// 属性文字列を組み立てます。
    /// </summary>
    /// <returns></returns>
    public string Render()
    {
      var builder = new StringBuilder();
      this.Render(builder);
      return builder.ToString();
    }

    private void RenderStyle(StringBuilder builder, Collection.OrderedDictionary<string, string> styles)
    {
      styles.ForEach((styleKey, styleValue) => {
        builder
          .Append(styleKey)
          .Append(": ")
          .Append(HttpUtility.HtmlEncode(styleValue))
          .Append("; ");
      });

      if (styles.Count > 0)
      {
        builder.Remove(builder.Length - 1, 1);
      }
    }

    private void RenderWithDictionary(StringBuilder builder, Collection.OrderedDictionary<string, object> attrDictionary)
    {
      attrDictionary.ForEach((key, value) => {
        if (key == "class")
        {
          builder.Append("class=\"");
          var classes = (List<string>)value;
          classes.ForEach(val => {
            builder.Append(HttpUtility.HtmlEncode(val)).Append(' ');
          });
          if (classes.Count > 0)
          {
            builder.Remove(builder.Length - 1, 1);
          }

          builder.Append("\" ");
        }
        else if (key == "style")
        {
          builder.Append("style=\"");
          var styles = (Collection.OrderedDictionary<string, string>)value;
          this.RenderStyle(builder, styles);
          builder.Append("\" ");
        }
        else
        {
          builder.Append(key);
          if (value != null)
          {
            builder
              .Append("=\"")
              .Append(HttpUtility.HtmlEncode(value))
              .Append("\"");
          }

          builder.Append(" ");
        }

      });

      if (this.attrDictionary.Count > 0)
      {
        builder.Remove(builder.Length - 1, 1);
      }
    }

    private Collection.OrderedDictionary<string, object> CloneAttrDictionary()
    {
      var newDictionary = new Collection.OrderedDictionary<string, object>();
      this.attrDictionary.ForEach((key, value) => {
        if(key == "class")
        {
          newDictionary[key] = new List<string>((List<string>)value);
        }
        else if (key == "style")
        {
          newDictionary[key] = new Collection.OrderedDictionary<string, string>((Collection.OrderedDictionary<string, string>)value);
        }
        else
        {
          newDictionary[key] = value;
        }
      });

      return newDictionary;
    }

    public bool HasClass(string className)
    {
      if (!this.attrDictionary.ContainsKey("class"))
      {
        return false;
      }

      return this["class"].Contains(className);
    }

    public Attr Add(params string[] attributes)
    {
      for (int i = 0; i < attributes.Length; i += 2)
      {
        this[attributes[i]] = attributes[i + 1];
      } 
      return this;
    }
  }
}
