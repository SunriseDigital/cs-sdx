using System;
using System.Collections.Generic;
using System.Text;

namespace Sdx.Html
{
  public class Attr
  {
    public static Attr Create()
    {
      return new Attr();
    }

    private Collection.OrderedDictionary<string, object> attrDictionary = new Collection.OrderedDictionary<string, object>();

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
    public Attr Add(string value)
    {
      this.attrDictionary[value] = value;
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

    /// <summary>
    /// 属性文字列を組み立てます。
    /// </summary>
    /// <returns></returns>
    public string Render(Attr attribute = null)
    {
      var builder = new StringBuilder();

      if (attribute == null)
      {
        this.BuildAttribute(this.attrDictionary, builder);
      }
      else
      {
        var tmpAttrDic = this.CloneAttrDictionary();
        attribute.attrDictionary.ForEach((key, value) => {
          if(key == "class" && tmpAttrDic.ContainsKey(key))
          {
            var tmpClasses = (List<string>)tmpAttrDic["class"];
            var argClasses = (List<string>)value;
            argClasses.ForEach(val => {
              if(!tmpClasses.Contains(val))
              {
                tmpClasses.Add(val);
              }
            });
          }
          else if (key == "style" && tmpAttrDic.ContainsKey(key))
          {
            var tmpStyles = (Collection.OrderedDictionary<string, string>)tmpAttrDic["style"];
            var argStyles = (Collection.OrderedDictionary<string, string>)value;
            argStyles.ForEach((k, v) => {
              tmpStyles[k] = v;
            });
          }
          else
          {
            tmpAttrDic[key] = value;
          }
        });

        this.BuildAttribute(tmpAttrDic, builder);
      }


      

      return builder.ToString();
    }

    private void BuildAttribute(Collection.OrderedDictionary<string, object> attrDictionary, StringBuilder builder)
    {
      attrDictionary.ForEach((key, value) => {
        if (key == "class")
        {
          builder.Append("class=\"");
          var classes = (List<string>)value;
          classes.ForEach(val => {
            builder.Append(val).Append(' ');
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
          styles.ForEach((styleKey, styleValue) => {
            builder
              .Append(styleKey)
              .Append(": ")
              .Append(styleValue)
              .Append("; ");
          });

          if (styles.Count > 0)
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
  }
}
