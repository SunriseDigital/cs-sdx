using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public abstract class Element
  {
    internal protected Html tag;

    private FormValue value = new FormValue();

    private List<Dictionary<string, object>> validators = new List<Dictionary<string, object>>();

    public Errors Errors { get; private set; } = new Errors();

    public Element(string name):this()
    {
      this.Name = name;
    }

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

    internal protected abstract Html CreateTag();

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
    }

    public FormValue Value
    {
      get
      {
        return this.value;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value">string|string[]</param>
    internal protected virtual void BindValue(object value)
    {
      this.value.Set(value);
    }

    public void Bind(string value)
    {
      this.BindValue(value);
    }

    public void Bind(string[] value)
    {
      this.BindValue(value);
    }

    internal protected virtual object GetValueForValidator()
    {
      return Value.First();
    }

    public bool ExecValidators()
    {
      var result = true;
      validators.ForEach(val => {
        var validator = (Validation.Validator)val["validator"];
        var breakChain = (bool)val["breakChain"];

        validator.Errors = Errors;
        if (!validator.IsValid(GetValueForValidator()))
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
