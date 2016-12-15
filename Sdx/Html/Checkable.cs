using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public abstract class Checkable : FormElement
  {
    public Checkable():base()
    {

    }

    public Checkable(string name):base(name)
    {

    }

    protected internal override void BindValueToTag()
    {
      if (this.Value.First() == this.tag.Attr["value"])
      {
        this.tag.Attr.Set("checked");
      }
    }
  }
}
