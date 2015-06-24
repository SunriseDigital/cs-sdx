﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Sdx.DebugTool;

namespace UnitTest
{
  public class DebugTool
  {
    [Fact]
    public void DumpString()
    {
      Assert.Equal("String(4) aaaa", Debug.Dump("aaaa"));
      Assert.Equal("String(3) 日本語", Debug.Dump("日本語"));
      Assert.Equal("String(0) ", Debug.Dump(""));
    }

    [Fact]
    public void DumpArray()
    {
      Assert.Equal(
        "System.String[](1)" + Environment.NewLine +
        " String(4) aaaa",
        Debug.Dump(new String[] { "aaaa" })
      );

      Assert.Equal(
        "System.String[](2)" + Environment.NewLine +
        " String(4) aaaa" + Environment.NewLine +
        " String(3) 日本語",
        Debug.Dump(new String[] { "aaaa", "日本語" })
      );

      Assert.Equal(
        "System.String[](3)" + Environment.NewLine +
        " String(4) aaaa" + Environment.NewLine +
        " String(3) 日本語" + Environment.NewLine +
        " String(0) ",
        Debug.Dump(new String[] { "aaaa", "日本語", "" })
      );

      Assert.Equal(
        "System.Array[](2)" + Environment.NewLine +
        " System.String[](1)" + Environment.NewLine +
        "  String(3) bbb" + Environment.NewLine +
        " System.String[](2)" + Environment.NewLine +
        "  String(3) aaa" + Environment.NewLine +
        "  String(3) ccc",
        Debug.Dump(new Array[] {
          new String[] { "bbb" },
          new String[] { "aaa", "ccc" },
        })
      );
    }

    [Fact]
    public void DumpDictionary()
    {
      Dictionary<String, String> dic = new Dictionary<string, string>();
      dic.Add("foo", "bar");
      Assert.Equal(
        "System.Collections.Generic.Dictionary`2(1)" + Environment.NewLine +
        " foo : String(3) bar",
        Debug.Dump(dic)
      );
    }
  }
}