using System;
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
        Assert.Equal("aaaa", Debug.Dump("aaaa"));
        Assert.Equal("日本語", Debug.Dump("日本語"));
        Assert.Equal("", Debug.Dump(""));
      }

      [Fact]
      public void DumpArray()
      {
        Assert.Equal(
          "System.String[]" + Environment.NewLine +
          " aaaa",
          Debug.Dump(new String[] { "aaaa" })
        );

        Assert.Equal(
          "System.String[]" + Environment.NewLine +
          " aaaa" + Environment.NewLine +
          " 日本語",
          Debug.Dump(new String[] { "aaaa", "日本語" })
        );

        Assert.Equal(
          "System.String[]" + Environment.NewLine +
          " aaaa" + Environment.NewLine +
          " 日本語" + Environment.NewLine +
          " ",
          Debug.Dump(new String[] { "aaaa", "日本語", "" })
        );

        Assert.Equal(
          "System.Array[]" + Environment.NewLine +
          " System.String[]" + Environment.NewLine +
          "  bbb" + Environment.NewLine +
          " System.String[]" + Environment.NewLine +
          "  aaa" + Environment.NewLine +
          "  ccc",
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
          "System.Collections.Generic.Dictionary`2" + Environment.NewLine +
          " foo : bar",
          Debug.Dump(dic)
        );
      }
    }
}
