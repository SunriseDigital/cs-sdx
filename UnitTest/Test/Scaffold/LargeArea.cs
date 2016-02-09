using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Scaffold
{
  public class LargeArea
  {
    public static Sdx.Scaffold.Manager Create()
    {
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.LargeArea.Meta, Test.Db.CreateDb());
      scaffold.Title = "大エリア";

      scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/large-area/edit.aspx");
      scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/large-area/list.aspx");

      scaffold.DisplayList
        .Add(Sdx.Scaffold.Param.Create()
          .Set("column", "name")
        ).Add(Sdx.Scaffold.Param.Create()
          .Set("column", "code")
        );

      
      scaffold.FormList
        .Add(Sdx.Scaffold.Param.Create()
          .Set("column", "id")
          .Set("label", "ID")
        ).Add(Sdx.Scaffold.Param.Create()
          .Set("column", "name")
          .Set("label", "名称")
        ).Add(Sdx.Scaffold.Param.Create()
          .Set("column", "code")
          .Set("label", "コード")
        );


      return scaffold;
    }
  }
}
