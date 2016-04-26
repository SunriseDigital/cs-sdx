using Xunit;
using UnitTest.DummyClasses;

#if ON_VISUAL_STUDIO
using FactAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestClassAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using ClassInitializeAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
using ClassCleanupAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;
#endif

using System;

using Sdx.Collection;
using System.Collections.Generic;

namespace UnitTest
{
  [TestClass]
  public class Collection_OrderedDictionary : BaseTest
  {
    [ClassInitialize]
    public static void InitilizeClass(TestContext context)
    {
      
    }

    [Fact]
    public void TestOrderedDictionary()
    {
      var dic = new OrderedDictionary<string, int>();

      for(var i=0; i<10; i++)
      {
        this.RunOrderedDictionary(dic);
      }
    }

    private void RunOrderedDictionary(OrderedDictionary<string, int> dic)
    {
      //追加一個目
      dic["key1"] = 11;
      Assert.Equal(11, dic["key1"]);
      Assert.Equal(1, dic.Count);

      //追加二個目
      dic["key2"] = 22;
      Assert.Equal(22, dic["key2"]);
      Assert.Equal(2, dic.Count);

      //追加三個目
      dic["key3"] = 33;
      Assert.Equal(33, dic["key3"]);
      Assert.Equal(3, dic.Count);

      //Enumlator
      var expected = new List<string>();

      expected.Clear();
      expected.Add("key1");
      expected.Add("key2");
      expected.Add("key3");
      var i = 0;
      foreach (var kv in dic)
      {
        Assert.Equal(expected[i], kv.Key);
        i++;
      }

      //更新
      dic["key1"] = 111;
      Assert.Equal(111, dic["key1"]);
      Assert.Equal(3, dic.Count);


      expected.Clear();
      expected.Add("key1");
      expected.Add("key2");
      expected.Add("key3");
      i = 0;
      foreach (var kv in dic)
      {
        Assert.Equal(expected[i], kv.Key);
        i++;
      }

      i = 0;
      dic.ForEach((key, value) => {
        Assert.Equal(expected[i], key);
        i++;
      });

      //削除
      dic.Remove("key2");
      Assert.False(dic.ContainsKey("key2"));
      Assert.Equal(2, dic.Count);
      expected.Clear();
      expected.Add("key1");
      expected.Add("key3");
      i = 0;
      foreach (var kv in dic)
      {
        Assert.Equal(expected[i], kv.Key);
        i++;
      }

      dic.Remove("key1");
      Assert.False(dic.ContainsKey("key1"));
      Assert.Equal(1, dic.Count);
      expected.Clear();
      expected.Add("key3");
      i = 0;
      foreach (var kv in dic)
      {
        Assert.Equal(expected[i], kv.Key);
        i++;
      }

      dic.Remove("key3");
      Assert.False(dic.ContainsKey("key3"));
      Assert.Equal(0, dic.Count);

      //Addで追加してみる
      dic.Add("key4", 44);
      Assert.Equal(44, dic["key4"]);
      Assert.Equal(1, dic.Count);
      expected.Clear();
      expected.Add("key4");
      i = 0;
      foreach (var kv in dic)
      {
        Assert.Equal(expected[i], kv.Key);
        i++;
      }

      dic.Add(new KeyValuePair<string, int>("key5", 55));
      Assert.Equal(55, dic["key5"]);
      Assert.Equal(2, dic.Count);
      expected.Clear();
      expected.Add("key4");
      expected.Add("key5");
      i = 0;
      foreach (var kv in dic)
      {
        Assert.Equal(expected[i], kv.Key);
        i++;
      }

      dic.Clear();
      Assert.Equal(0, dic.Count);
      for (var j = 1; j <= 5; j++)
      {
        Assert.False(dic.ContainsKey("key" + i));
      }
    }
  }
}
