using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Db.Adapter
{
  public class Manager
  {
    private List<Base> writeAdapters = new List<Base>();
    private List<Base> readAdapters = new List<Base>();

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

    public Base Read
    {
      get
      {
        if(readAdapters.Count == 0)
        {
          throw new InvalidOperationException("Read Adapter is empty.");
        }
        else if(readAdapters.Count == 1)
        {
          return readAdapters[0];
        }
        else
        {
          //Random.Next(maxValue)はmaxValueを含みません
          return readAdapters[random.Next(readAdapters.Count)];
        }
      }
    }

    public Base Write
    {
      get
      {
        if (writeAdapters.Count == 0)
        {
          throw new InvalidOperationException("Read Adapter is empty.");
        }
        else if (writeAdapters.Count == 1)
        {
          return writeAdapters[0];
        }
        else
        {
          return writeAdapters[random.Next(writeAdapters.Count)];
        }
      }
    }

    public Base Share
    {
      get
      {
        var totalCount = writeAdapters.Count + readAdapters.Count;
        var seed = random.Next(totalCount);

        if(seed < writeAdapters.Count)
        {
          return writeAdapters[seed];
        }
        else
        {
          return readAdapters[seed - writeAdapters.Count];
        }
      }
    }
  }
}
