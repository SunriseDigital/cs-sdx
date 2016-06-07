﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sdx.Html
{
  public class CheckableGroup: FormElement
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

    protected internal override FormValue CreateFormValue()
    {
      return new FormValue(true);
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

    public void AddCheckable<T>(string key, string labelString = null) where T : Checkable, new()
    {
      var checkable = new T();
      checkable.Tag.Attr["value"] = key;
      AddCheckable(checkable, labelString);
    }

    public void AddCheckable<T>(KeyValuePair<string, string> pair) where T: Checkable, new()
    {
      AddCheckable<T>(pair.Key, pair.Value);
    }

    public void AddCheckable(Checkable checkable, string labelString = null)
    {
      if(checkable is Radio)
      {
        Value.IsMultiple = false;
      }
      elements.Add(checkable);

      var tag = this.tag;
      
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