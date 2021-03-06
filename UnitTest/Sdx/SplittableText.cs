﻿using Xunit;
using UnitTest.DummyClasses;

#if ON_VISUAL_STUDIO
using FactAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestClassAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using ClassInitializeAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
using ClassCleanupAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;
#endif

using System;

using System.IO;
using System.Collections.Generic;
using System.Text;

namespace UnitTest
{
  [TestClass]
  public class SplittableText : BaseTest
  {
    [Fact]
    public void TestBasic()
    {
      var target = "aaaaaaaaaa@Sdx.SplittableText.Boundary@bbbbbbbbbb@Sdx.SplittableText.Boundary@cccccccccc";
      var splittable = new Sdx.SplittableText(target);

      Assert.Equal(3, splittable.PartCount);
      Assert.Equal("aaaaaaaaaa", splittable.First);
      Assert.Equal("cccccccccc", splittable.Last);
      Assert.Equal("bbbbbbbbbb", splittable.PartAt(2));
      Assert.Equal(target, splittable.RawText);
      Assert.Equal(true, splittable.HasMultipleParts);
    }

    [Fact]
    public void TestFreeBoundarySetting()
    {
      var target = "aaaaaaaaaa---myBoundary---bbbbbbbbbb";
      var splittable = new Sdx.SplittableText(target, "---myBoundary---");

      Assert.Equal("aaaaaaaaaa", splittable.First);
      Assert.Equal("bbbbbbbbbb", splittable.Last);
    }

    [Fact]
    public void TestNonPresenceItem()
    {
      var target = "aaaaaaaaaa@Sdx.SplittableText.Boundary@bbbbbbbbbb";
      var splittable = new Sdx.SplittableText(target);

      Assert.Equal(null, splittable.PartAt(999));
      Assert.Equal(null, splittable.PartAt(-1));
    }

    [Fact]
    public void TestTrim()
    {
      var target = "    aaaaaaaaaa    @Sdx.SplittableText.Boundary@    bbbbbbbbbb     ";
      var splittable = new Sdx.SplittableText(target);

      Assert.Equal("aaaaaaaaaa", splittable.First);
      Assert.Equal("bbbbbbbbbb", splittable.Last);
    }
  }
}

