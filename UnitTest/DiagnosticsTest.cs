using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sdx.Diagnostics;

using Xunit;
using UnitTest.DummyClasses;

#if ON_VISUAL_STUDIO
using FactAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestClassAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using ClassInitializeAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
using ClassCleanupAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;
#endif

namespace UnitTest
{
  [TestClass]
  public class DiagnosticsTest : BaseTest
  {
    [Fact]
    public void TestDumpString()
    {
      Assert.Equal("String(4) aaaa", Debug.Dump("aaaa"));
      Assert.Equal("String(3) 日本語", Debug.Dump("日本語"));
      Assert.Equal("String(0) ", Debug.Dump(""));
    }

    [Fact]
    public void TestDumpArray()
    {
      Assert.Equal(
        "System.String[](1)" + Environment.NewLine +
        Debug.DumpIndent + "String(4) aaaa",
        Debug.Dump(new String[] { "aaaa" })
      );

      Assert.Equal(
        "System.String[](2)" + Environment.NewLine +
        Debug.DumpIndent + "String(4) aaaa" + Environment.NewLine +
        Debug.DumpIndent + "String(3) 日本語",
        Debug.Dump(new String[] { "aaaa", "日本語" })
      );

      Assert.Equal(
        "System.String[](3)" + Environment.NewLine +
        Debug.DumpIndent + "String(4) aaaa" + Environment.NewLine +
        Debug.DumpIndent + "String(3) 日本語" + Environment.NewLine +
        Debug.DumpIndent + "String(0) ",
        Debug.Dump(new String[] { "aaaa", "日本語", "" })
      );

      Assert.Equal(
        "System.Array[](2)" + Environment.NewLine +
        Debug.DumpIndent + "System.String[](1)" + Environment.NewLine +
        Debug.DumpIndent + Debug.DumpIndent + "String(3) bbb" + Environment.NewLine +
        Debug.DumpIndent + "System.String[](2)" + Environment.NewLine +
        Debug.DumpIndent + Debug.DumpIndent + "String(3) aaa" + Environment.NewLine +
        Debug.DumpIndent + Debug.DumpIndent + "String(3) ccc",
        Debug.Dump(new Array[] {
          new String[] { "bbb" },
          new String[] { "aaa", "ccc" },
        })
      );
    }

    [Fact]
    public void TestDumpDictionary()
    {
      Dictionary<String, String> dic = new Dictionary<string, string>();
      dic.Add("foo", "bar");
      Assert.Equal(
        "System.Collections.Generic.Dictionary`2(1)" + Environment.NewLine +
        Debug.DumpIndent + "foo : String(3) bar",
        Debug.Dump(dic)
      );
    }

    [Fact]
    public void DumpInt()
    {
      Assert.Equal(
        "System.Int32 10",
        Debug.Dump(10)
      );
    }

    [Fact]
    public void TestLog()
    {
      Sdx.Context.Current.Timer.Start();

      Sdx.Context.Current.Debug.Out = Console.Out;
      Sdx.Context.Current.Debug.Log("aaaa");

      Sdx.Context.Current.Debug.Out = new Sdx.Diagnostics.DebugHtmlWriter();
      Sdx.Context.Current.Debug.Log("aaaa");

      //秒数が出るのでAssertできません。とりあえず。例外が出ないかと空っぽじゃないかをテストしてるだけです。
      var result = Sdx.Context.Current.Debug.Out.ToString();
      Console.WriteLine(result.Length);
      Assert.True(result.Length > 0);
    }

    [Fact]
    public void DumpNestedDictionary()
    {
      var dic = new Dictionary<string, object>();
      dic.Add("string", "foobar");
      dic.Add("array", new string[] { "arr1", "arr2" });
      dic.Add("dic", new Dictionary<string, string>() {
        { "key1", "value1"},
        { "key2", "value2"},
      });
      Assert.Equal(
        @"
System.Collections.Generic.Dictionary`2(3)
  string : String(6) foobar
  array : System.String[](2)
   String(4) arr1
   String(4) arr2
  dic : System.Collections.Generic.Dictionary`2(2)
   key1 : String(6) value1
   key2 : String(6) value2".Trim(),
        Debug.Dump(dic)
      );
    }

    [Fact]
    public void DumpNull()
    {
      string str = null;
      Assert.Equal(
        "NULL",
        Debug.Dump(str)
      );
    }
  }
}
