using NGettext;
using NGettext.Loaders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sdx
{
  public class I18n
  {
    private static Dictionary<string, ICatalog> catalogDic = new Dictionary<string, ICatalog>();
    private static ICatalog GetCatalog(CultureInfo info)
    {
      var lang = info.TwoLetterISOLanguageName;
      if(!catalogDic.ContainsKey(lang))
      {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream("Sdx.locale."+lang+".message.mo");

        if (stream == null)
        {
          catalogDic[lang] = new Catalog(info);
        }
        else
        {
          catalogDic[lang] = new Catalog(new MoLoader(stream), info);
        }
      }

      return catalogDic[lang];
    }
    public static string GetString(string key, params object[] args)
    {
      return GetCatalog(Sdx.Context.Current.Culture).GetString(key, args);
    }

    public static string GetPluralString(string text, string pluralText, long n)
    {
      return GetCatalog(Sdx.Context.Current.Culture).GetPluralString(text, pluralText, n);
    }

    public static string GetPluralString(string text, string pluralText, long n, params object[] args)
    {
      return GetCatalog(Sdx.Context.Current.Culture).GetPluralString(text, pluralText, n, args);
    }
  }
}
