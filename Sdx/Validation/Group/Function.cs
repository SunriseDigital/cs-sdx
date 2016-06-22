using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Validation.Group
{
  public class Function : Base
  {
    private Func<Html.Form, Error> func;

    /// <summary>
    /// コールバックが<see cref="Sdx.Validation.Error"/>を返せばfalse。nullを返せばtrue。
    /// </summary>
    /// <param name="func"></param>
    public Function(Func<Html.Form, Error> func)
    {
      this.func = func;
    }

    protected override bool IsValidForm(Html.Form form)
    {
      var err = func(form);
      if (err != null)
      {
        this.AddError(err);
        return false;
      }

      return true;
    }
  }
}
