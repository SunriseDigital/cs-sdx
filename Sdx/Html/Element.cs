using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public abstract class Element
  {
    internal protected Html tag;


    private List<Dictionary<string, object>> validators = new List<Dictionary<string, object>>();

    public Validation.Errors Errors { get; private set; } = new Validation.Errors();

    public Element(string name):this()
    {
      this.Name = name;
    }

    /// <summary>
    /// <see cref="FormValue.HasMany"/>はElement毎に違うのでそれを設定するたの抽象メソッド。
    /// 子クラスで実装してください。
    /// </summary>
    /// <returns></returns>
    internal protected abstract FormValue CreateFormValue();

    internal protected abstract Html CreateTag();

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

    public Html Tag
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
      if (this.Value.HasMany)
      {
        throw new InvalidOperationException("This element must have multiple value.");
      }
      this.BindValue(value);
    }

    public void Bind(string[] value)
    {
      if (!this.Value.HasMany)
      {
        throw new InvalidOperationException("This element must have single value.");
      }
      this.BindValue(value);
    }

    public bool ExecValidators()
    {
      var result = true;
      this.Errors.Clear();

      validators.ForEach(val => {
        var validator = (Validation.Validator)val["validator"];
        var breakChain = (bool)val["breakChain"];

        validator.Errors = this.Errors;
        bool isValid;
        if (this.Value.HasMany)
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
          if(breakChain)
          {
            return;
          }
        }
      });

      return result;
    }

    public void AddValidator(Validation.Validator validator, bool breakChain = false)
    {
      validators.Add(new Dictionary<string, object> {
        {"validator", validator},
        {"breakChain", breakChain}
      });
    }
  }
}
