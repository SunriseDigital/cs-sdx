using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Validation.Image
{
  public class ValidatorSet
  {
    private class ValidatorWrapper
    {
      public Validation.Image.Validator Validator { get; set; }
      public bool BreakChain { get; set; }
    }
    private List<ValidatorWrapper> validators = new List<ValidatorWrapper>();
    public Validation.Errors Errors { get; private set; }
    public bool IsAllowEmpty { get; set; }

    public ValidatorSet()
    {

    }

    public IEnumerable<Validation.Image.Validator> Validators
    {
      get
      {
        foreach (var wrapper in validators)
        {
          yield return wrapper.Validator;
        }
      }
    }

    public ValidatorSet AddValidator(Validation.Image.Validator validator, bool breakChain = false)
    {
      validators.Add(new ValidatorWrapper { Validator = validator, BreakChain = breakChain });

      return this;
    }

    /// <summary>
    /// バリデーションをする必要がないシチュエーションで呼んでください。例えば画像は編集時、更新がないと何も飛んできませんが、物と画像があったときはバリデーションそのものをする必要がありません。
    /// </summary>
    public void Ignore()
    {
      this.Errors = new Validation.Errors();
    }

    public bool IsValid(Sdx.Image image)
    {
      var result = true;
      this.Errors = new Validation.Errors();

      foreach (var wrapper in validators)
      {
        wrapper.Validator.Errors = this.Errors;
        bool isValid = wrapper.Validator.IsValid(image);

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
