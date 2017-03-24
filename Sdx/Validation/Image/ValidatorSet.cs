﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Validation.Image
{
  public class ValidatorSet
  {
    private List<Dictionary<string, object>> validators = new List<Dictionary<string, object>>();
    public Validation.Errors Errors { get; private set; }
    public bool IsAllowEmpty { get; set; }

    public ValidatorSet()
    {

    }

    public IEnumerable<Validation.Image.Validator> Validators
    {
      get
      {
        foreach(var dic in validators)
        {
          yield return (Validation.Image.Validator)dic["validator"];
        }
      }
    }

    public ValidatorSet AddValidator(Validation.Image.Validator validator, bool breakChain = false)
    {
      validators.Add(new Dictionary<string, object> {
        {"validator", validator},
        {"breakChain", breakChain}
      });

      return this;
    }


    public bool IsValid(Sdx.Image image)
    {
      var result = true;
      this.Errors = new Validation.Errors();

      foreach (var val in validators)
      {
        var validator = (Validation.Image.Validator)val["validator"];
        var breakChain = (bool)val["breakChain"];

        validator.Errors = this.Errors;
        bool isValid = validator.IsValid(image);

        if (!isValid)
        {
          validator.Errors = null;
          result = false;
          if (breakChain)
          {
            break;
          }
        }
      }

      return result;
    }
  }
}
