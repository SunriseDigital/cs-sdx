using System;

namespace Sdx.Html
{
  public class CheckBox : Element
  {
    private string value;

    public CheckBox()
    {
    }

    protected override ITag CreateTag()
    {
      var tag = new VoidTag("input");
      tag.Attr["type"] = "checkbox";
      tag.Attr["value"] = "";

      return tag;
    }

    public override string Value
    {
      get
      {
        if (this.TagValue == this.value)
        {
          return this.value;
        }

        return "";
      }

      set
      {
        this.value = value;
      }
    }
  }
}