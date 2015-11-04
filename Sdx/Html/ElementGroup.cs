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

    public void AddElement(Element element, string labelString = null)
    {
      elements.Add(element);

      var tag = (Tag)this.tag;
      
      if(this.name != null)
      {
        element.Name = this.name;
      }

      if (labelString == null)
      {
        tag.AddHtml(element.tag);
      }
      else
      {
        var label = new Tag("label");
        label.AddHtml(element.tag);
        if (element.tag.Attr["id"] != null)
        {
          label.Attr["for"] = element.tag.Attr["id"];
        }
        label.AddHtml(new RawText(labelString));
        tag.AddHtml(label);
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
          element.Tag.Attr["name"] = value;
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
          if (Array.IndexOf(values, element.Tag.Attr["value"]) > -1)
          {
            element.Tag.Attr.Add("checked");
          }
          else
          {
            element.Tag.Attr.Remove("checked");
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