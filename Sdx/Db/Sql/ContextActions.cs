using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Db.Sql
{
  /// <summary>
  /// 特定の名前のカラムを持ってる全てのテーブルに同じアクションを行いたいときに利用します。
  /// 例えば、stateというカラムを持っている全てのテーブルに`state = 1`を付与する。
  /// sequenceというカラムを持っている全てのテーブルにORDER BY sequence DESCを付与する、
  /// というときに便利です。SelectのCreateContextActionsから生成してください。
  /// select.CreateContextActions()
  ///   .Add("sequence", c => c.AddOrder("sequence", Sdx.Db.Sql.Order.DESC))
  ///   .Add("state", c => c.Where.Add("state", 1))
  ///   .Perform();
  /// </summary>
  public class ContextActions
  {
    public Select Select { get; private set; }

    private class ActionData
    {
      internal Action<Context> Action { get; set; }
      internal string ColumnName { get; set; }
    }

    private List<ActionData> actions;

    public ContextActions(Select select)
    {
      Select = select;
      actions = new List<ActionData>();
    }

    /// <summary>
    /// columnNameが存在するテーブルのcontextに対して実行したいActionを登録します。
    /// Perform()を呼ぶまで実行されませんので注意してください。
    /// </summary>
    /// <param name="columnName"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public ContextActions Add(string columnName, Action<Context> action)
    {
      var actionData = new ActionData();
      actionData.ColumnName = columnName;
      actionData.Action = action;
      actions.Add(actionData);
      return this;
    }

    public ContextActions Add(Action<Context> action)
    {
      var actionData = new ActionData();
      actionData.Action = action;
      actions.Add(actionData);
      return this;
    }

    /// <summary>
    /// Addしたアクションを実行します。
    /// </summary>
    public void Perform()
    {
      foreach(var kv in Select.ContextList)
      {
        var context = kv.Value;
        actions.ForEach(action => 
        {
          if (action.ColumnName == null)
          {
            action.Action(context);
          }
          else if(context.Table.OwnMeta.HasColumn(action.ColumnName))
          {
            action.Action(context);
          }
        });
      }
    }
  }
}
