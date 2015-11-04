﻿using System;

namespace Sdx.Html
{
  public class InputText : Element
  {
    public InputText()
    {
    }

    protected override ITag CreateTag()
    {
      var tag = new VoidTag("input");
      tag.Attr.Set("type", "text");
      tag.Attr.Set("value", "");

      return tag;
    }

    internal protected override void BindValue(object value)
    {
      base.BindValue(value);
      this.Attr["value"] = value.ToString();
    }
  }
}