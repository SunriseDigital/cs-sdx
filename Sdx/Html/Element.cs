using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public abstract class Element
  {
    internal protected Tag tag;


    private List<Dictionary<string, object>> validators = new List<Dictionary<string, object>>();

    public Validation.Errors Errors { get; private set; } = new Validation.Errors();

    public Element(string name):this()
    {
      this.Name = name;
    }

    /// <summary>
    /// <see cref="FormValue.Multiple"/>はElement毎に違うのでそれを設定するたの抽象メソッド。
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

    public Element()
    {
      this.tag = this.CreateTag();
      this.Value = this.CreateFormValue();
    }

    public FormValue Value { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value">string|string[]</param>
    internal protected virtual void BindValue(object value)
    {
      this.Value.Set(value);
    }

    public void Bind(string value)
    {
      if (this.Value.Multiple)
      {
        throw new InvalidOperationException("This element must have multiple value.");
      }
      this.BindValue(value);
    }

    public void Bind(string[] value)
    {
      if (!this.Value.Multiple)
      {
        throw new InvalidOperationException("This element must have single value.");
      }
      this.BindValue(value);
    }

    public bool ExecValidators()
    {
      var result = true;
      this.Errors.Clear();

      foreach (var val in validators)
      {
        var validator = (Validation.Validator)val["validator"];
        var breakChain = (bool)val["breakChain"];

        validator.Errors = this.Errors;
        bool isValid;
        if (this.Value.Multiple)
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

    public Element AddValidator(Validation.Validator validator, bool breakChain = false)
    {
      validators.Add(new Dictionary<string, object> {
        {"validator", validator},
        {"breakChain", breakChain}
      });

      return this;
    }
  }
}
