using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.SqlClient;

using Xunit;

namespace UnitTest
{
  public class DbTest
  {
    [Fact]
    public void UtilSqlCommandToSql()
    {
      SqlCommand cmd;

      cmd = new SqlCommand("SELECT * FROM user WHERE city = @City");
      cmd.Parameters.AddWithValue("@City", "東京");
      Assert.Equal("SELECT * FROM user WHERE city = '東京'", Sdx.Db.Util.SqlCommandToSql(cmd));

      cmd = new SqlCommand("SELECT * FROM user WHERE city = @City AND city_code = @CityCode");
      cmd.Parameters.AddWithValue("@City", "東京");
      cmd.Parameters.AddWithValue("@CityCode", "tokyo");
      Assert.Equal("SELECT * FROM user WHERE city = '東京' AND city_code = 'tokyo'", Sdx.Db.Util.SqlCommandToSql(cmd));
    }

    [Fact]
    public void Where()
    {
      Sdx.Db.Where where = new Sdx.Db.Where();

      where.add("id", "1");

      Assert.Equal("[id] = '1'", Sdx.Db.Util.SqlCommandToSql(where.build()));

      where.add("type", 2);
      Assert.Equal("[id] = '1' AND [type] = '2'", Sdx.Db.Util.SqlCommandToSql(where.build()));
    }
  }
}
