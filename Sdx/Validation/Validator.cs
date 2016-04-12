using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sdx.Html;
using System.Reflection;
using System.IO;

namespace Sdx.Validation
{
  public abstract class Validator
  {
    private const string ErrorAll = "Sdx.Validation.Validator.ErrorAll";

    public Errors Errors { get; internal set; }

    private Dictionary<string, string> messages = new Dictionary<string, string>();

    private Dictionary<string, string> placeholders = new Dictionary<string, string>();

    public IDictionary<string, string> Messages
    {
      get
      {
        return this.messages;
      }
    }

    public string Message
    {
      get
      {
        if (this.messages.ContainsKey(ErrorAll))
        {
          return this.messages[ErrorAll];
        }
        else
        {
          return null;
        }
      }

      set
      {
        this.messages[ErrorAll] = value;
      }
    }

    public Validator(string message = null)
    {
      if (message != null)
      {
        this.messages[ErrorAll] = message;
      }
    }

    protected abstract bool IsValidString(string value);

    protected abstract string GetDefaultMessage(string errorType);


    protected void SetPlaceholder(string key, string value)
    {
      placeholders[key] = value;
    }

    private string DetectMessage(Error error)
    {
      //インスタンスメッセージ（簡易）
      if (this.messages.ContainsKey(ErrorAll))
      {
        return this.messages[ErrorAll];
      }

      //インスタンスメッセージ
      if (this.messages.ContainsKey(error.ErrorType))
      {
        return this.messages[error.ErrorType];
      }

      //クラスメッセージ
      //public static Dictionary<string, Dictionary<string, string>> MessageTemplates { get; private set; }
      //クラスに上記プロパティを作って設定する
      var prop = this.GetType().GetProperty("MessageTemplates");
      if (prop != null)
      {
        var lang = Context.Current.Culture.TwoLetterISOLanguageName;
        var templates = (Dictionary<string, Dictionary<string, string>>)prop.GetValue(null, null);
        if (templates.ContainsKey(lang))
        {
          var msgs = templates[lang];
          if (msgs.ContainsKey(error.ErrorType))
          {
            return msgs[error.ErrorType];
          }
        }
      }

      var message = GetDefaultMessage(error.ErrorType);
      if (message == null)
      {
        throw new NotImplementedException("Missing default message for error " + error.ErrorType);
      }

      return message;
    }

    protected void AddError(string errorType)
    {
      var error = new Error();

      error.ClassName = this.GetType().FullName;
      error.ErrorType = errorType;

      var message = this.DetectMessage(error);
      
      error.Message = this.ReplacePlaceholder(message);

      Errors.Add(error);
    }

    public string ReplacePlaceholder(string message)
    {
      //placeholder
      foreach (var kv in this.placeholders)
      {
        message = message.Replace("%" + kv.Key + "%", kv.Value);
      }

      return message;
    }

    protected Dictionary<string, string> MessagePlaceholder = new Dictionary<string, string>();


    public virtual bool IsValid(IEnumerable<string> values)
    {
      var result = true;
      foreach (var value in values)
      {
        if (!IsValid(value))
        {
          result = false;
          break;
        }
      }

      return result;
    }

    public bool IsValid(string value)
    {
      if (this.Errors == null)
      {
        this.Errors = new Errors();
      }

      this.SetPlaceholder("value", value);
      return IsValidString(value);
    }
  }
}
