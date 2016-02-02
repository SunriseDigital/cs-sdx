using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public abstract class FormElement
  {
    internal protected Tag tag;

    private List<Dictionary<string, object>> validators = new List<Dictionary<string, object>>();

    public Validation.Errors Errors { get; private set; }

    public FormElement(string name):this()
    {
      this.Name = name;
    }

    /// <summary>
    /// <see cref="FormValue.IsMultiple"/>はElement毎に違うのでそれを設定するたの抽象メソッド。
    /// 子クラスで実装してください。
    /// </summary>
    /// <returns></returns>
    internal protected abstract FormValue CreateFormValue();

    internal protected abstract Tag CreateTag();

    public virtual string Name
    {
      get
      {
        return this.tag.Attr["name"];
      }

      set
      {
        this.tag.Attr["name"] = value;
      }
    }

    public Tag Tag
    {
      get
      {
        return this.tag;
      }
    }

    public bool IsAllowEmpty { get; set; }

    public FormElement()
    {
      this.tag = this.CreateTag();
      this.Value = this.CreateFormValue();
      this.Errors = new Validation.Errors();
      this.IsAllowEmpty = false;
    }

    public FormValue Value { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value">string|string[]</param>
    internal protected abstract void BindValueToTag();

    public void Bind(string value)
    {
      if (this.Value.IsMultiple)
      {
        throw new InvalidOperationException("This element must have multiple value.");
      }
      this.Value.Set(value);
      this.BindValueToTag();
    }

    public void Bind(string[] value)
    {
      if (!this.Value.IsMultiple)
      {
        throw new InvalidOperationException("This element must have single value.");
      }
      this.Value.Set(value);
      this.BindValueToTag();
    }

    public bool ExecValidators()
    {
      var result = true;
      this.Errors.Clear();

      if(this.IsAllowEmpty && this.Value.IsEmpty)
      {
        return true;
      }

      foreach (var val in validators)
      {
        var validator = (Validation.Validator)val["validator"];
        var breakChain = (bool)val["breakChain"];

        validator.Errors = this.Errors;
        bool isValid;
        if (this.Value.IsMultiple)
        {
          isValid = validator.IsValid(this.Value.ToArray());
        }
        else
        {
          isValid = validator.IsValid(this.Value.First());
        }

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

    public FormElement AddValidator(Validation.Validator validator, bool breakChain = false)
    {
      validators.Add(new Dictionary<string, object> {
        {"validator", validator},
        {"breakChain", breakChain}
      });

      return this;
    }
  }
}
