using System.Data.Common;

namespace Sdx.Db
{
  public interface INonQueryBuilder
  {
    DbCommand Build();
  }
}
