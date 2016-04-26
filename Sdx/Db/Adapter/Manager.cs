using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Db.Adapter
{
  /// <summary>
  /// DB負荷分散用Class
  /// 複数のAdapterをセットしておきランダムで取得する。
  /// </summary>
  public class Manager
  {
    private List<Base> writeAdapters = new List<Base>();
    private List<Base> readAdapters = new List<Base>();

    private static Dictionary<string, Object> managerDic = new Dictionary<string, Object>();

    private static Random random = new Random();

    public void AddCommonAdapter(Base adapter)
    {
      writeAdapters.Add(adapter);
      readAdapters.Add(adapter);
    }

    public void AddReadAdapter(Base adapter)
    {
      readAdapters.Add(adapter);
    }

    public void AddWriteAdapter(Base adapter)
    {
      writeAdapters.Add(adapter);
    }

    private static Base GetRandom(List<Base> list)
    {
      if (list.Count == 0)
      {
        throw new InvalidOperationException("Adapter list is empty.");
      }
      else if (list.Count == 1)
      {
        return list[0];
      }
      else
      {
        //Random.Next(maxValue)はmaxValueを含みません
        return list[random.Next(list.Count)];
      }
    }

    public Base Read
    {
      get
      {
        return Manager.GetRandom(readAdapters);
      }
    }

    public Base Write
    {
      get
      {
        return Manager.GetRandom(writeAdapters);
      }
    }

    public static void Set(string key, Manager manager)
    {
      managerDic[key] = manager;
    }

    public static void Set(string key, Func<Manager> getter)
    {
      managerDic[key] = getter;
    }

    public static Manager Get(string key)
    {
      var target = managerDic[key];
      if (target is Manager)
      {
        return (Manager)target;
      }
      else if(target is Func<Manager>)
      {
        return ((Func<Manager>) target)();
      }
      else
      {
        throw new InvalidOperationException("Invalid type " + target.GetType());
      }
    }
  }
}
