using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public class Attr : ICloneable
  {
    public static Attr Create()
    {
      return new Attr();
    }

    private Collection.OrderedDictionary<string, object> attributes = new Collection.OrderedDictionary<string, object>();

    /// <summary>
    /// クラス属性を追加します。
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Attr AddClass(string value)
    {
      if (!this.attributes.ContainsKey("class"))
      {
        this.attributes["class"] = new List<string>();
      }

      var classes = (List<string>)this.attributes["class"];

      classes.Add(value);

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
      this.attributes[key] = value;
      return this;
    }

    /// <summary>
    /// 属性を削除します。classやstyleも削除可能。
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Attr Remove(string key)
    {
      this.attributes.Remove(key);
      return this;
    }

    /// <summary>
    /// クラス属性を削除します。存在しないクラスを指定した場合、何も起きません。
    /// 全てのclass属性が削除されると`class=""`も出力されません。
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Attr RemoveClass(string value)
    {
      if(this.attributes.ContainsKey("class"))
      {
        var values = (List<string>)this.attributes["class"];
        values.Remove(value);

        if(values.Count == 0)
        {
          this.attributes.Remove("class");
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
      if (!this.attributes.ContainsKey("style"))
      {
        this.attributes["style"] = new Collection.OrderedDictionary<string, string>();
      }

      var styles = (Collection.OrderedDictionary<string, string>)this.attributes["style"];

      styles[key] = value;

      return this;
    }

    /// <summary>
    /// `disabled="disabled"`の様な、属性のキーと値が同じ属性を追加します。
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Attr Add(string value)
    {
      this.attributes[value] = value;
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
      if (this.attributes.ContainsKey("style"))
      {
        var values = (Collection.OrderedDictionary<string, string>)this.attributes["style"];
        values.Remove(key);

        if (values.Count == 0)
        {
          this.attributes.Remove("style");
        }
      }

      return this;
    }

    /// <summary>
    /// 属性文字列を組み立てます。
    /// </summary>
    /// <returns></returns>
    public string Render()
    {
      var builder = new StringBuilder();
      this.attributes.ForEach((key, value) => {
        if (key == "class")
        {
          builder.Append("class=\"");
          var classes = (List<string>) value;
          classes.ForEach(val => {
            builder.Append(val).Append(' ');
          });
          if(classes.Count > 0)
          {
            builder.Remove(builder.Length - 1, 1);
          }

          builder.Append("\" ");
        }
        else if(key == "style")
        {
          builder.Append("style=\"");
          var styles = (Collection.OrderedDictionary<string, string>)value;
          styles.ForEach((styleKey, styleValue) => {
            builder
              .Append(styleKey)
              .Append(": ")
              .Append(styleValue)
              .Append("; ");
          });

          if(styles.Count > 0)
          {
            builder.Remove(builder.Length - 1, 1);
          }

          builder.Append("\" ");
        }
        else
        {
          builder
            .Append(key)
            .Append("=\"")
            .Append(value)
            .Append("\" ");
        }

      });

      if (this.attributes.Count > 0)
      {
        builder.Remove(builder.Length - 1, 1);
      }

      return builder.ToString();
    }

    public object Clone()
    {
      var cloned = (Attr)this.MemberwiseClone();

      cloned.attributes = new Collection.OrderedDictionary<string, object>();
      this.attributes.ForEach((key, value) => {
        if(key == "class")
        {
          cloned.attributes[key] = new List<string>((List<string>)value);
        }
        else if (key == "style")
        {
          cloned.attributes[key] = new Collection.OrderedDictionary<string, string>((Collection.OrderedDictionary<string, string>)value);
        }
        else
        {
          cloned.attributes[key] = value;
        }
      });

      return cloned;
    }
  }
}
