﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Sdx.Html
{
  public class Form : IEnumerable<Element>
  {
    private Dictionary<string, Element> elements = new Dictionary<string, Element>();

    public Element this[string name]
    {
      get
      {
        return this.elements[name];
      }
    }

    public void SetElement(Element element)
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

    public IEnumerator<Element> GetEnumerator()
    {
      return this.elements.Values.GetEnumerator();
    }
  }
}