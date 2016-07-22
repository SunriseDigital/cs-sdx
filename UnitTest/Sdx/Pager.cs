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

      var numberList = new List<int>(){};
      var isCurrentList = new List<bool>(){};

      //奇数パターン
      pager.GetPageHolderList(5).ForEach(ph => {
        numberList.Add(ph.number);
        isCurrentList.Add(ph.is_current);
      });

      Assert.Equal(3, numberList[0]);
      Assert.Equal(4, numberList[1]);
      Assert.Equal(5, numberList[2]);
      Assert.Equal(6, numberList[3]);
      Assert.Equal(7, numberList[4]);

      Assert.Equal(false, isCurrentList[0]);
      Assert.Equal(false, isCurrentList[1]);
      Assert.Equal(true, isCurrentList[2]);
      Assert.Equal(false, isCurrentList[3]);
      Assert.Equal(false, isCurrentList[4]);

      //偶数パターン
      numberList.Clear();
      isCurrentList.Clear();
      pager.GetPageHolderList(6).ForEach(ph =>
      {
        numberList.Add(ph.number);
        isCurrentList.Add(ph.is_current);
      });

      Assert.Equal(2, numberList[0]);
      Assert.Equal(3, numberList[1]);
      Assert.Equal(4, numberList[2]);
      Assert.Equal(5, numberList[3]);
      Assert.Equal(6, numberList[4]);
      Assert.Equal(7, numberList[5]);

      Assert.Equal(false, isCurrentList[0]);
      Assert.Equal(false, isCurrentList[1]);
      Assert.Equal(false, isCurrentList[2]);
      Assert.Equal(true, isCurrentList[3]);
      Assert.Equal(false, isCurrentList[4]);
      Assert.Equal(false, isCurrentList[5]);

      //奇数だが真ん中が現在のページにできないパターン(1)
      numberList.Clear();
      isCurrentList.Clear();
      pager.SetPage("2");
      pager.GetPageHolderList(5).ForEach(ph =>
      {
        numberList.Add(ph.number);
        isCurrentList.Add(ph.is_current);
      });

      Assert.Equal(1, numberList[0]);
      Assert.Equal(2, numberList[1]);
      Assert.Equal(3, numberList[2]);
      Assert.Equal(4, numberList[3]);
      Assert.Equal(5, numberList[4]);

      Assert.Equal(false, isCurrentList[0]);
      Assert.Equal(true, isCurrentList[1]);
      Assert.Equal(false, isCurrentList[2]);
      Assert.Equal(false, isCurrentList[3]);
      Assert.Equal(false, isCurrentList[4]);

      //奇数だが真ん中が現在のページにできないパターン(2)
      numberList.Clear();
      isCurrentList.Clear();
      pager.SetPage("11");
      pager.GetPageHolderList(5).ForEach(ph =>
      {
        numberList.Add(ph.number);
        isCurrentList.Add(ph.is_current);
      });

      Assert.Equal(8, numberList[0]);
      Assert.Equal(9, numberList[1]);
      Assert.Equal(10, numberList[2]);
      Assert.Equal(11, numberList[3]);
      Assert.Equal(12, numberList[4]);

      Assert.Equal(false, isCurrentList[0]);
      Assert.Equal(false, isCurrentList[1]);
      Assert.Equal(false, isCurrentList[2]);
      Assert.Equal(true, isCurrentList[3]);
      Assert.Equal(false, isCurrentList[4]);

      //現在のページが先頭のページ
      numberList.Clear();
      isCurrentList.Clear();
      pager.SetPage("1");
      pager.GetPageHolderList(5).ForEach(ph =>
      {
        numberList.Add(ph.number);
        isCurrentList.Add(ph.is_current);
      });

      Assert.Equal(1, numberList[0]);
      Assert.Equal(true, isCurrentList[0]);

      //現在のページが最後のページ
      numberList.Clear();
      isCurrentList.Clear();
      pager.SetPage("12");
      pager.GetPageHolderList(5).ForEach(ph =>
      {
        numberList.Add(ph.number);
        isCurrentList.Add(ph.is_current);
      });

      Assert.Equal(12, numberList[4]);
      Assert.Equal(true, isCurrentList[4]);

      //GetPageHolderList に総ページ数より多い数を渡した場合
      pager.SetPage("12");
      Assert.Equal(12, pager.GetPageHolderList(20).Count);
    }
  }
}
