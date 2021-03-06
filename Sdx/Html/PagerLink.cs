﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Sdx.Html
{
  public class PagerLink
  {
    public const string DEFAULT_VAR_NAME = "page";

    private string varName;

    private string classNameForDisabled = "disabled";

    public PagerLink(Pager pager, Web.Url baseUrl, string varName)
    {
      Pager = pager;
      if(baseUrl == null)
      {
        BaseUrl = new Web.Url(HttpContext.Current.Request.Url.PathAndQuery);
      }
      else
      {
        BaseUrl = baseUrl;
      }

      if(varName == null)
      {
        VarName = PagerLink.DEFAULT_VAR_NAME;
      }
      else
      {
        VarName = varName;
      }
    }

    public PagerLink(Pager pager)
      : this(pager, null, null)
    {

    }

    public PagerLink(Sdx.Pager pager, string varName)
      :this(pager, null, varName)
    {

    }

    public PagerLink(Pager pager, Web.Url baseUrl)
      :this(pager, baseUrl, null)
    {

    }

    public Pager Pager { get; private set; }

    /// <summary>
    /// リンク先URL。ページ番号のクエリはついたままでOK。上書きされます。
    /// </summary>
    public Web.Url BaseUrl { get; set; }

    /// <summary>
    /// URLクエリーのページ番号のキー
    /// </summary>
    public string VarName
    {
      get
      {
        return varName;
      }
      
      set
      {
        varName = value;
        Pager.SetPage(HttpContext.Current.Request.QueryString[varName]);
      }
    }

    private Tag CrateLinkTag(string targetPage)
    {
      var url = (Web.Url)BaseUrl.Clone();
      Html.Tag tag;
      if (targetPage != null)
      {
        url.SetParam(VarName, targetPage);
        tag = new Tag("a");
        tag.Attr.Set("href", url.Build());
      }
      else
      {
        tag = new Tag("span");
        tag.Attr.AddClass(classNameForDisabled);
      }

      return tag;
    }

    public Tag GetPrev()
    {
      return CrateLinkTag(Pager.HasPrev ? (Pager.Page - 1).ToString() : null);
    }


    public Tag GetNext()
    {
      return CrateLinkTag(Pager.HasNext ? (Pager.Page + 1).ToString() : null);
    }

    public Tag GetFisrt()
    {
      return CrateLinkTag(Pager.Page == 1 ? null : "1");
    }

    public Tag GetLast()
    {
      return CrateLinkTag(Pager.Page == Pager.LastPage ? null : Pager.LastPage.ToString());
    }

    public List<Tag> GetLinksTag(int number)
    {
      return GetLinksTag(number, num => num);
    }

    public List<Tag> GetLinksTag(int number, Func<string, string> func)
    {
      classNameForDisabled = "current";
      var links = new List<Tag>() { };
      Pager.GetPageDataList(number).ForEach(pd => {
        var linkTag = CrateLinkTag(pd.IsCurrent ? null : pd.Id.ToString());
        linkTag.AddText(func(pd.Id.ToString()), false);
        links.Add(linkTag);
      });

      return links;
    }
  }
}
