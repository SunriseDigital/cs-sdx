using Xunit;
using UnitTest.DummyClasses;
using Moq;
using System.Collections.Specialized;
using System.Web;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Reflection;
using System.Collections.Generic;

#if ON_VISUAL_STUDIO
using FactAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestClassAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using ClassInitializeAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
using ClassCleanupAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;
#endif

using System;

namespace UnitTest
{
  [TestClass]
  public class Pager : BaseTest
  {
    [Fact]
    public void TestTotalCount()
    {
      var pager = new Sdx.Pager();
      pager.SetPage("1");
      pager.PerPage = 10;
      pager.TotalCount = 38;

      Assert.Equal(4, pager.LastPage);
      Assert.Equal(10, pager.PerPage);
      Assert.Equal(38, pager.TotalCount);
      Assert.Equal(1, pager.Page);
      Assert.True(pager.HasNext);
      Assert.False(pager.HasPrev);

      pager.SetPage("2");
      Assert.Equal(2, pager.Page);
      Assert.True(pager.HasNext);
      Assert.True(pager.HasPrev);

      pager.SetPage("4");
      Assert.Equal(4, pager.Page);
      Assert.False(pager.HasNext);
      Assert.True(pager.HasPrev);
    }

    [Fact]
    public void TestSimple()
    {
      var pager = new Sdx.Pager();
      pager.SetPage("1");
      pager.HasNext = true;

      Assert.Equal(1, pager.Page);
      Assert.True(pager.HasNext);
      Assert.False(pager.HasPrev);

      Exception ex;
      ex = Record.Exception(() =>
      {
        Assert.Equal(4, pager.LastPage);
      });

      Assert.IsType<InvalidOperationException>(ex);

      ex = Record.Exception(() =>
      {
        Assert.Equal(4, pager.PerPage);
      });

      Assert.IsType<InvalidOperationException>(ex);

      ex = Record.Exception(() =>
      {
        Assert.Equal(4, pager.TotalCount);
      });

      Assert.IsType<InvalidOperationException>(ex);

      pager.SetPage("5");
      pager.HasNext = false; ;

      Assert.Equal(5, pager.Page);
      Assert.False(pager.HasNext);
      Assert.True(pager.HasPrev);
    }

    [Fact]
    public void TestPages()
    {
      var perPage = 3;
      var totalCount = 35;
      var pager = new Sdx.Pager(perPage, totalCount);
      pager.SetPage("5");

      var idList = new List<int>(){};
      var isCurrentList = new List<bool>(){};

      //奇数パターン
      pager.GetPageDataList(5).ForEach(pd => {
        idList.Add(pd.Id);
        isCurrentList.Add(pd.IsCurrent);
      });

      Assert.Equal(3, idList[0]);
      Assert.Equal(4, idList[1]);
      Assert.Equal(5, idList[2]);
      Assert.Equal(6, idList[3]);
      Assert.Equal(7, idList[4]);

      Assert.Equal(false, isCurrentList[0]);
      Assert.Equal(false, isCurrentList[1]);
      Assert.Equal(true, isCurrentList[2]);
      Assert.Equal(false, isCurrentList[3]);
      Assert.Equal(false, isCurrentList[4]);

      //偶数パターン
      idList.Clear();
      isCurrentList.Clear();
      pager.GetPageDataList(6).ForEach(pd =>
      {
        idList.Add(pd.Id);
        isCurrentList.Add(pd.IsCurrent);
      });

      Assert.Equal(2, idList[0]);
      Assert.Equal(3, idList[1]);
      Assert.Equal(4, idList[2]);
      Assert.Equal(5, idList[3]);
      Assert.Equal(6, idList[4]);
      Assert.Equal(7, idList[5]);

      Assert.Equal(false, isCurrentList[0]);
      Assert.Equal(false, isCurrentList[1]);
      Assert.Equal(false, isCurrentList[2]);
      Assert.Equal(true, isCurrentList[3]);
      Assert.Equal(false, isCurrentList[4]);
      Assert.Equal(false, isCurrentList[5]);

      //奇数だが真ん中が現在のページにできないパターン(1)
      idList.Clear();
      isCurrentList.Clear();
      pager.SetPage("2");
      pager.GetPageDataList(5).ForEach(pd =>
      {
        idList.Add(pd.Id);
        isCurrentList.Add(pd.IsCurrent);
      });

      Assert.Equal(1, idList[0]);
      Assert.Equal(2, idList[1]);
      Assert.Equal(3, idList[2]);
      Assert.Equal(4, idList[3]);
      Assert.Equal(5, idList[4]);

      Assert.Equal(false, isCurrentList[0]);
      Assert.Equal(true, isCurrentList[1]);
      Assert.Equal(false, isCurrentList[2]);
      Assert.Equal(false, isCurrentList[3]);
      Assert.Equal(false, isCurrentList[4]);

      //奇数だが真ん中が現在のページにできないパターン(2)
      idList.Clear();
      isCurrentList.Clear();
      pager.SetPage("11");
      pager.GetPageDataList(5).ForEach(pd =>
      {
        idList.Add(pd.Id);
        isCurrentList.Add(pd.IsCurrent);
      });

      Assert.Equal(8, idList[0]);
      Assert.Equal(9, idList[1]);
      Assert.Equal(10, idList[2]);
      Assert.Equal(11, idList[3]);
      Assert.Equal(12, idList[4]);

      Assert.Equal(false, isCurrentList[0]);
      Assert.Equal(false, isCurrentList[1]);
      Assert.Equal(false, isCurrentList[2]);
      Assert.Equal(true, isCurrentList[3]);
      Assert.Equal(false, isCurrentList[4]);

      //現在のページが先頭のページ
      idList.Clear();
      isCurrentList.Clear();
      pager.SetPage("1");
      pager.GetPageDataList(5).ForEach(pd =>
      {
        idList.Add(pd.Id);
        isCurrentList.Add(pd.IsCurrent);
      });

      Assert.Equal(1, idList[0]);
      Assert.Equal(true, isCurrentList[0]);

      //現在のページが最後のページ
      idList.Clear();
      isCurrentList.Clear();
      pager.SetPage("12");
      pager.GetPageDataList(5).ForEach(pd =>
      {
        idList.Add(pd.Id);
        isCurrentList.Add(pd.IsCurrent);
      });

      Assert.Equal(12, idList[4]);
      Assert.Equal(true, isCurrentList[4]);

      //GetPageDataList に総ページ数より多い数を渡した場合
      pager.SetPage("12");
      Assert.Equal(12, pager.GetPageDataList(20).Count);
    }
  }
}
