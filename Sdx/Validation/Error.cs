using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Sdx.Validation
{
  public class Error
  {
    private static Dictionary<string, Data.Tree> memoryCache = new Dictionary<string, Data.Tree>();

    public Error(string className, string errorType, string lang)
    {
      this.ClassName = className;
      this.ErrorType = errorType;
      this.Lang = lang;
    }

    public string ErrorType { get; private set; }
    public string ClassName { get; private set; }
    public string Lang { get; private set; }

    private Stream GetMessagesStream()
    {
      var assembly = Assembly.GetExecutingAssembly();
      return assembly.GetManifestResourceStream("Sdx._resources.validation.messages."+this.Lang+".yml");
    }

    public string Message
    {
      get
      {
        if(!memoryCache.ContainsKey(this.Lang))
        {
          var tree = new Data.TreeYaml();

          var stream = this.GetMessagesStream();
          if(stream == null)
          {
            this.Lang = "ja";
            stream = this.GetMessagesStream();
          }

          using (stream)
          {
            StreamReader sr = new StreamReader(
                stream,
                Encoding.GetEncoding("utf-8")
            );
            tree.Load(sr);
          }

          memoryCache[this.Lang] = tree;
        }

        var messages = memoryCache[this.Lang];
        return messages.Get(this.ClassName).Get(this.ErrorType).Value;
      }
    }
  }
}