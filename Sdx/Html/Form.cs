﻿using System;
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

    public void Bind(NameValueCollection values)
    {
      foreach(var name in values.AllKeys)
      {
        if(this.elements.ContainsKey(name))
        {
          if (this.elements[name].Value.IsMultiple)
          {
            this.elements[name].Bind(values.GetValues(name));
          }
          else
          {
            var vals = values.GetValues(name);
            if (vals.Length > 1)
            {
              throw new InvalidOperationException(name + "element must have single value.");
            }
            this.elements[name].Bind(vals[0]);
          }
        }
      }
    }

    public bool ExecValidators()
    {
      var result = true;
      foreach(var kv in elements)
      {
        if(!kv.Value.ExecValidators())
        {
          result = false;
        }
      }

      return result;
    }

    public T As<T>(string name) where T : FormElement
    {
      return (T)this[name];
    }

  }
}