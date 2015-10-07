using System;
using System.Data.Common;

namespace Sdx.Db
{
  public interface INonQueryBuilder : ICloneable
  {
    DbCommand Build();
  }
}
