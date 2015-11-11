using System;
using System.Collections.Generic;
using System.Linq;

namespace Sdx.Html
{
  public class Select:Element
  {
    private List<Option> options = new List<Option>();

    private Dictionary<string, Tag> optgroups = new Dictionary<string, Tag>();

    internal protected override HtmlBase CreateTag()
    {
      return new Tag("select");
    }

    public IEnumerable<Option> Options
    {
      get
      {
        return this.options;
      }
    }

    public bool Multiple
    {
      get
      {
        return this.Tag.Attr.Exists("multiple");
      }

      set
      {
        if (value)
        {
          this.Value.Multiple = true;
          this.Tag.Attr.Set("multiple");
        }
        else
        {
          this.Value.Multiple = false;
          this.Tag.Attr.Remove("multiple");
        }
      }
    }

    public Select():base()
    {

    }

    public Select(string name):base(name)
    {

    }

    protected internal override FormValue CreateFormValue()
    {
      return new FormValue(false);
    }

    public void AddOption(Option option, string optgroupLabel = null)
    {
      this.options.Add(option);

      var tag = (Tag)this.tag;

      if (optgroupLabel == null)
      {
        tag.AddHtml(option.tag);
      }
      else
      {
        Tag optgroup;
        if(optgroups.ContainsKey(optgroupLabel))
        {
          optgroup = optgroups[optgroupLabel];
        }
        else
        {
          optgroup = new Tag("optgroup");
          optgroup.Attr["label"] = optgroupLabel;
          optgroups[optgroupLabel] = optgroup;
          tag.AddHtml(optgroup);
        }

        optgroup.AddHtml(option.tag);
      }
    }

    internal protected override void BindValue(object value)
    {
      base.BindValue(value);

      this.options.ForEach(element => {
        if (this.Value.Contains(element.Tag.Attr["value"]))
        {
          element.Tag.Attr.Set("selected");
        }
        else
        {
          element.Tag.Attr.Remove("selected");
        }
      });
    }
  }
}