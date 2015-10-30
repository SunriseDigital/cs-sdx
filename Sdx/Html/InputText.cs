﻿using System;

namespace Sdx.Html
{
  public class InputText : Element
  {
    public InputText()
    {
    }

    protected override IHtml CreateTag()
    {
      var tag = new VoidTag("input");
      tag.Attr.Set("type", "text");
      tag.Attr.Set("value", "");

      return tag;
    }
  }
}