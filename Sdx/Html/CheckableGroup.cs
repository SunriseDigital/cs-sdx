using System;
using System.Collections;
using System.Collections.Generic;

namespace Sdx.Html
{
  public class CheckableGroup: Element, IEnumerable<Checkable>
  {
    private List<Checkable> elements = new List<Checkable>();

    private string name;

    public CheckableGroup()
    {
    }

    internal protected override ITag CreateTag()
    {
      return new Tag("span");
    }

    public void ForEach(Action<Checkable> action)
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

    public void AddCheckable(Checkable checkable, string labelString = null)
    {
      elements.Add(checkable);

      var tag = (Tag)this.tag;
      
      if(this.name != null)
      {
        checkable.Name = this.name;
      }

      if (labelString == null)
      {
        tag.AddHtml(checkable.tag);
      }
      else
      {
        var label = new Tag("label");
        label.AddHtml(checkable.tag);
        if (checkable.tag.Attr["id"] != null)
        {
          label.Attr["for"] = checkable.tag.Attr["id"];
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
        if (Array.IndexOf(values, element.Tag.Attr["value"]) > -1)
        {
          element.Tag.Attr.Add("checked");
        }
        else
        {
          element.Tag.Attr.Remove("checked");
        }
      });
    }

    public IEnumerator<Checkable> GetEnumerator()
    {
      return ((IEnumerable<Checkable>)elements).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<Checkable>)elements).GetEnumerator();
    }
  }
}