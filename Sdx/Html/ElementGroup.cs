using System;
using System.Collections;
using System.Collections.Generic;

namespace Sdx.Html
{
  public class ElementGroup: Element, IEnumerable<Element>
  {
    private List<Element> elements = new List<Element>();

    private string name;

    public ElementGroup()
    {
    }

    protected override ITag CreateTag()
    {
      return new Tag("span");
    }

    public void ForEach(Action<Element> action)
    {
      this.elements.ForEach(elem => action(elem));
    }

    public Element this[int index]
    {
      get
      {
        return this.elements[index];
      }
    }

    public void AddElement(Element element)
    {
      elements.Add(element);

      var tag = (Tag)this.tag;
      tag.AddHtml(element.tag);
      if(this.name != null)
      {
        element.Name = this.name;
      }
    }

    public override string Name
    {
      get
      {
        return this.name;
      }

      set
      {
        this.name = value;
        this.elements.ForEach(element => {
          element.Attr["name"] = value;
        });
      }
    }

    internal protected override void BindValue(object value)
    {
      base.BindValue(value);

      var values = this.Value.All;
      this.elements.ForEach(element => {
        if (element is CheckBox)
        {
          if (Array.IndexOf(values, element.Attr["value"]) > -1)
          {
            element.Attr.Add("checked");
          }
          else
          {
            element.Attr.Remove("checked");
          }
        }
      });
    }

    public IEnumerator<Element> GetEnumerator()
    {
      return ((IEnumerable<Element>)elements).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<Element>)elements).GetEnumerator();
    }
  }
}