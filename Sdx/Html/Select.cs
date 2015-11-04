using System;
using System.Collections.Generic;

namespace Sdx.Html
{
  public class Select:Element
  {
    private List<Option> options = new List<Option>();

    private Dictionary<string, Tag> optgroups = new Dictionary<string, Html.Tag>();

    public Select()
    {

    }

    protected override ITag CreateTag()
    {
      return new Tag("select");
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

      var values = this.Value.All;
      this.options.ForEach(element => {
        if (Array.IndexOf(values, element.Tag.Attr["value"]) > -1)
        {
          element.Tag.Attr.Add("selected");
        }
        else
        {
          element.Tag.Attr.Remove("selected");
        }
      });
    }
  }
}