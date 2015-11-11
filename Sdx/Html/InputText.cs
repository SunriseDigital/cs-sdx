using System;
using System.Linq;

namespace Sdx.Html
{
  public class InputText : Input
  {
    public InputText():base()
    {

    }

    public InputText(string name):base(name)
    {

    }

    protected internal override string GetInputType()
    {
      return "text";
    }
  }
}