using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Validation
{
  public class ValidatorSet
  {
    private class ValidatorWrapper
    {
      public Validation.Validator Validator { get; set; }
      public bool BreakChain { get; set; }
    }

    private List<ValidatorWrapper> validators = new List<ValidatorWrapper>();
    public Validation.Errors Errors { get; private set; }
    public bool IsAllowEmpty { get; set; }

    public ValidatorSet()
    {
      this.Errors = new Validation.Errors();
    }

    public IEnumerable<Validation.Validator> Validators
    {
      get
      {
        foreach(var wrapper in validators)
        {
          yield return wrapper.Validator;
        }
      }
    }

    public ValidatorSet AddValidator(Validation.Validator validator, bool breakChain = false)
    {
      validators.Add(new ValidatorWrapper { Validator = validator, BreakChain = breakChain });

      return this;
    }


    public bool IsValid(string value)
    {
      var result = true;
      this.Errors.Clear();

      foreach (var wrapper in validators)
      {
        wrapper.Validator.Errors = this.Errors;
        bool isValid = wrapper.Validator.IsValid(value);

        if (!isValid)
        {
          wrapper.Validator.Errors = null;
          result = false;
          if (wrapper.BreakChain)
          {
            break;
          }
        }
      }

      return result;
    }
  }
}
