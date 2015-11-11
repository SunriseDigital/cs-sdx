using System;
using System.Linq;

namespace Sdx.Html
{
  public class InputHidden : Input
  {
    public InputHidden():base()
    {

    }

    public InputHidden(string name):base(name)
    {

    }

    protected internal override string GetInputType()
    {
      return "hidden";
    }
  }
}