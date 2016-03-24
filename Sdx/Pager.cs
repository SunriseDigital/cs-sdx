using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx
{
  public class Pager
  {
    private int? lastPage;
    private int? perPage;
    private int? totalCount;

    private void SetPage(string page)
    {
      Page = page == null ? 1 : Int32.Parse(page);
    }

    public void SetPageData(string page, bool hasNext)
    {
      SetPage(page);
      HasNext = hasNext;
    }

    public void SetPageData(string page, int perPage, int totalCount)
    {
      SetPage(page);

      this.perPage = perPage;
      this.totalCount = totalCount;

      this.lastPage = TotalCount / PerPage;
      if (this.totalCount % this.perPage != 0)
      {
        ++this.lastPage;
      }

      HasNext = Page < this.lastPage;
    }

    public int LastPage
    {
      get
      {
        if (lastPage == null)
        {
          throw new InvalidOperationException("You must call `SetPageData(string int int)` before use this.");
        }
        return (int)lastPage;
      }
    }

    public int PerPage
    {
      get
      {
        if (perPage == null)
        {
          throw new InvalidOperationException("You must call `SetPageData(string int int)` before use this.");
        }
        return (int)perPage;
      }
    }

    public int TotalCount
    {
      get
      {
        if (totalCount == null)
        {
          throw new InvalidOperationException("You must call `SetPageData(string int int)` before use this.");
        }
        return (int)totalCount;
      }
    }

    public int Page { get; private set; }

    public bool HasNext { get; private set; }

    public bool HasPrev
    {
      get
      {
        if (Page == 1)
        {
          return false;
        }

        return true;
      }
    }
  }
}
