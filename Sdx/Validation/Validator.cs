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

    private static Dictionary<string, Data.Tree> messageMemoryCache = new Dictionary<string, Data.Tree>();

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

    public Validator(Dictionary<string, string> messages)
    {
      foreach (var kv in messages)
      {
        this.messages[kv.Key] = kv.Value;
      }
    }

    protected abstract bool ExecIsValue(string value);

    protected virtual bool ExecIsValue(IEnumerable<string> values)
    {
      var result = true;
      foreach (var value in values)
      {
        if (!ExecIsValue(value))
        {
          result = false;
          break;
        }
      }

      return result;
    }

    protected void SetPlaceholder(string key, string value)
    {
      placeholders[key] = value;
    }

    private Stream GetMessagesStream(string lang)
    {
      var assembly = Assembly.GetExecutingAssembly();
      return assembly.GetManifestResourceStream("Sdx._resources.validation.messages." + lang + ".yml");
    }

    private string DetectMessage(Error error)
    {
      if (this.messages.ContainsKey(ErrorAll))
      {
        error.Lang = null;
        return this.messages[ErrorAll];
      }
      else if (this.messages.ContainsKey(error.ErrorType))
      {
        error.Lang = null;
        return this.messages[error.ErrorType];
      }
      else //設定ファイルから読む
      {
        if (!messageMemoryCache.ContainsKey(error.Lang))
        {
          var tree = new Data.TreeYaml();

          var stream = this.GetMessagesStream(error.Lang);
          if (stream == null)
          {
            error.Lang = "ja";
            stream = this.GetMessagesStream(error.Lang);
          }

          using (stream)
          {
            StreamReader sr = new StreamReader(
                stream,
                Encoding.GetEncoding("utf-8")
            );
            tree.Load(sr);
          }

          messageMemoryCache[error.Lang] = tree;
        }

        var messages = messageMemoryCache[error.Lang];
        return messages.Get(error.ClassName).Get(error.ErrorType).Value;
      }
    }

    protected void AddError(string errorType)
    {
      var error = new Error();

      error.ClassName = this.GetType().FullName;
      error.ErrorType = errorType;
      error.Lang = Sdx.Context.Current.Lang;

      var message = this.DetectMessage(error);
      //placeholder
      foreach (var kv in this.placeholders)
      {
        message = message.Replace("%"+kv.Key+"%", kv.Value);
      }
      error.Message = message;

      Errors.Add(error);
    }

    protected Dictionary<string, string> MessagePlaceholder = new Dictionary<string, string>();


    public bool IsValid(IEnumerable<string> values)
    {
      if (this.Errors == null)
      {
        this.Errors = new Errors();
      }
      return ExecIsValue(values);
    }

    public bool IsValid(string value)
    {
      if (this.Errors == null)
      {
        this.Errors = new Errors();
      }
      return ExecIsValue(value);
    }
  }
}
