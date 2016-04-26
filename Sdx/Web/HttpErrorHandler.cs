using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Sdx.Web
{
  public class HttpErrorHandler
  {
    private Dictionary<int, Action> handlers = new Dictionary<int, Action>();

    public void SetHandler(int statusCode, Action handler)
    {
      handlers[statusCode] = handler;
    }

    public bool HasHandler(int statusCode)
    {
      return handlers.ContainsKey(statusCode);
    }

    public void Invoke(int statusCode)
    {
      if(handlers.ContainsKey(statusCode))
      {
        handlers[statusCode].Invoke();
      }
    }
  }
}
