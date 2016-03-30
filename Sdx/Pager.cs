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
    private int? page;
    private bool? hasNext;

    public Pager()
    {

    }

    public Pager(int perPage)
    {
      PerPage = perPage;
    }

    public void SetPage(string page)
    {
      this.page = page == null ? 1 : Int32.Parse(page);
      this.hasNext = null;
    }

    public int LastPage
    {
      get
      {
        if(lastPage != null)
        {
          return (int)lastPage;
        }

        lastPage = TotalCount / PerPage;
        if (TotalCount % PerPage != 0)
        {
          ++lastPage;
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
          throw new InvalidOperationException("Missing PerPage data.");
        }

        return (int)perPage;
      }

      set
      {
        perPage = value;
        lastPage = null;
      }
    }

    public int TotalCount
    {
      get
      {
        if (totalCount == null)
        {
          throw new InvalidOperationException("Missing TotalCount data.");
        }

        return (int)totalCount;
      }

      set
      {
        totalCount = value;
        lastPage = null;
      }
    }

    public int Page
    {
      get
      {
        if (page == null)
        {
          throw new InvalidOperationException("Missing Page data.");
        }

        return (int)page;
      }
    }

    public bool HasNext
    {
      get
      {
        if(hasNext != null)
        {
          return (bool)hasNext;
        }

        hasNext = Page < LastPage;
        return (bool)hasNext;
      }

      set
      {
        hasNext = value;
      }
    }

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
