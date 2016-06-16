using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Validation
{
  public class Function : Validator
  {
    private Func<string, Error> func;

    /// <summary>
    /// コールバックが<see cref="Sdx.Validation.Error"/>を返せばfalse。nullを返せばtrueのValidatorです。
    /// </summary>
    /// <param name="func"></param>
    public Function(Func<string, Error> func)
    {
      this.func = func;
    }

    protected override bool IsValidString(string value)
    {
      var err = func(value);
      if(err != null)
      {
        this.AddError(err);
        return false;
      }

      return true;
    }

    protected override string GetDefaultMessage(string errorType)
    {
      throw new NotImplementedException();
    }
  }
}
