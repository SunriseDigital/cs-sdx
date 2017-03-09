using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sdx.Html
{
  public abstract class CheckableGroup: FormElement
  {
    private List<Checkable> elements = new List<Checkable>();

    private string name;

    public IEnumerable<Checkable> Checkables
    {
      get
      {
        return this.elements;
      }
    }

    public CheckableGroup():base()
    {

    }

    public CheckableGroup(string name):base(name)
    {

    }

    internal protected override Tag CreateTag()
    {
      return new Tag("span");
    }

    public void ForEach(Action<Checkable> action)
    {
      this.elements.ForEach(elem => action(elem));
    }

    public FormElement this[int index]
    {
      get
      {
        return this.elements[index];
      }
    }

    public Tag AddCheckable(string key, string labelString = null)
    {
      var checkable = CreateCheckable();
      checkable.Tag.Attr["value"] = key;
      return AddCheckable(checkable, labelString);
    }

    public Tag AddCheckable(KeyValuePair<string, string> pair)
    {
      return AddCheckable(pair.Key, pair.Value);
    }

    protected abstract internal Checkable CreateCheckable();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="checkable"></param>
    /// <param name="labelString"></param>
    /// <returns>返り値のTagはlabelがあるときはlabelが、無いときはinputが変えります。checkable.tagは常にinputなので注意してください。</returns>
    public Tag AddCheckable(Checkable checkable, string labelString = null)
    {
      elements.Add(checkable);

      var tag = this.tag;
      
      if(this.name != null)
      {
        checkable.Name = this.name;
      }

      if(labelString != null)
      {
        checkable.Label = labelString;
      }

      if (checkable.Label == null)
      {
        tag.AddHtml(checkable.tag);
        return tag;
      }
      else
      {
        var label = new Tag("label");
        label.AddHtml(checkable.tag);
        if (checkable.tag.Attr["id"] != null)
        {
          label.Attr["for"] = checkable.tag.Attr["id"];
        }
        label.AddHtml(new RawText(checkable.Label));
        tag.AddHtml(label);
        return label;
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

    internal protected override void BindValueToTag()
    {
      this.elements.ForEach(element => {
        var tagValue = element.Tag.Attr["value"];
        if (this.Value.Any(v => v == tagValue))
        {
          element.Bind(tagValue);
        }
        else
        {
          element.Tag.Attr.Remove("checked");
        }
      });
    }
  }
}