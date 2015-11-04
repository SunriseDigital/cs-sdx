using System;
using System.Collections.Generic;

namespace Sdx.Html
{
  public class Select:Element
  {
    private List<Option> options = new List<Option>();

    public Select()
    {

    }

    protected override ITag CreateTag()
    {
      return new Tag("select");
    }

    public void AddOption(Option option)
    {
      this.options.Add(option);
      var tag = (Tag)this.tag;
      tag.AddHtml(option.tag);
    }

    internal protected override void BindValue(object value)
    {
      base.BindValue(value);

      var values = this.Value.All;
      this.options.ForEach(element => {
        if (Array.IndexOf(values, element.Attr["value"]) > -1)
        {
          element.Attr.Add("selected");
        }
        else
        {
          element.Attr.Remove("selected");
        }
      });
    }
  }
}