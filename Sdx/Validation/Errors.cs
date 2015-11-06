using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sdx.Validation;

namespace Sdx.Validation
{
  public class Errors : IEnumerable<Error>
  {
    private List<Error> errors = new List<Error>();

    public int Count
    {
      get
      {
        return errors.Count;
      }
    }

    public Error this[int index]
    {
      get
      {
        return this.errors[index];
      }
    }

    public Html.Html Html
    {
      get
      {
        if(this.errors.Count == 0)
        {
          return new Html.RawText("");
        }
        else
        {
          var ul = new Html.Tag("ul");
          this.errors.ForEach(error => {
            var li = new Html.Tag("li");
            li.AddHtml(new Html.RawText(error.Message));
            ul.AddHtml(li);
          });

          return ul;
        }
      }
    }

    internal void Add(Error error)
    {
      this.errors.Add(error);
    }

    public IEnumerator<Error> GetEnumerator()
    {
      return ((IEnumerable<Error>)errors).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<Error>)errors).GetEnumerator();
    }
  }
}
