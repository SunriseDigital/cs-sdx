using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Sdx.Html
{
  public class Form : IEnumerable<FormElement>
  {
    private Dictionary<string, FormElement> elements = new Dictionary<string, FormElement>();
    public Dictionary<string, Validation.Group.Base> GroupValidators { get; private set; }

    public Form()
    {
      GroupValidators = new Dictionary<string, Validation.Group.Base>();
    }

    public FormElement this[string name]
    {
      get
      {
        return this.elements[name];
      }
    }

    public void SetElement(FormElement element)
    {
      if(element.Name == null)
      {
        throw new InvalidOperationException("Element name is null.");
      }

      this.elements[element.Name] = element;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.elements.Values.GetEnumerator();
    }

    public IEnumerator<FormElement> GetEnumerator()
    {
      return this.elements.Values.GetEnumerator();
    }

    public void Bind(params NameValueCollection[] collections)
    {
      foreach (var values in collections)
      {
        foreach (var kv in elements)
        {
          var elem = kv.Value;
          var name = kv.Key;
          var vals = values.GetValues(name);
          if (elem.Value.IsMultiple)
          {
            if (vals == null)
            {
              elem.Bind(new string[] { });
            }
            else
            {
              elem.Bind(vals);
            }
          }
          else
          {
            if (vals == null)
            {
              elem.Bind("");
            }
            else
            {
              if (vals.Length > 1)
              {
                throw new InvalidOperationException(name + "element must have single value.");
              }
              elem.Bind(vals[0]);
            }
          }
        }
      }
    }

    private bool? isValidCache = null;

    public bool ExecValidators()
    {
      isValidCache = true;

      foreach (var kv in GroupValidators)
      {
        if (!kv.Value.Exec(this))
        {
          isValidCache = false;
        }
      }

      foreach(var kv in elements)
      {
        if(!kv.Value.ExecValidators())
        {
          isValidCache = false;
        }
      }

      return (bool)isValidCache;
    }

    public bool IsValid
    {
      get
      {
        if(isValidCache == null)
        {
          throw new InvalidOperationException("Call ExecValidators before this.");
        }

        return (bool)isValidCache;
      }
    }

    public T As<T>(string name) where T : FormElement
    {
      return (T)this[name];
    }

    /// <summary>
    /// Recordにセットする用のNameValueCollectionを生成する。
    /// IsSecretの処理はここでやってます。
    /// </summary>
    /// <returns></returns>
    public NameValueCollection ToNameValueCollection()
    {
      if(isValidCache != true)
      {
        throw new InvalidOperationException("Not pass validation yet.");
      }

      var result = new NameValueCollection();

      foreach (var kv in elements)
      {
        var key = kv.Key;
        var elem = kv.Value;

        if (!(elem.IsSecret && elem.Value.IsEmpty))
        {
          foreach (var value in elem.Value)
          {
            result.Add(key, value);
          }
        }
      }

      return result;
    }

    public Collection.OrderedDictionary<string, Validation.Errors> Errors()
    {
      var result = new Collection.OrderedDictionary<string, Validation.Errors>();
      foreach(var kv in elements)
      {
        result[kv.Key] = kv.Value.Errors;
      }

      return result;
    }

    public void AddGroupValidator(string key, Validation.Group.Base groupValidator)
    {
      GroupValidators[key] = groupValidator;
    }
  }
}