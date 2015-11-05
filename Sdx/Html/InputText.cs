﻿using System;

namespace Sdx.Html
{
  public class InputText : Element
  {
    public InputText():base()
    {

    }

    public InputText(string name):base(name)
    {

    }

    internal protected override Html CreateTag()
    {
      var tag = new VoidTag("input");
      tag.Attr.Set("type", "text");
      tag.Attr.Set("value", "");

      return tag;
    }

    internal protected override void BindValue(object value)
    {
      base.BindValue(value);
      this.Tag.Attr["value"] = value.ToString();
    }
  }
}