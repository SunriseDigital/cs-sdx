using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sdx.Validation;

namespace Sdx.Html
{
  public class Errors
  {
    private List<string> messages = new List<string>();

    public int Count
    {
      get
      {
        return messages.Count;
      }
    }

    public string this[int index]
    {
      get
      {
        return this.messages[index];
      }
    }
  }
}
