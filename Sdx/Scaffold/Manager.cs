using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace Sdx.Scaffold
{
  public class Manager
  {
    private int? perPage;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tableMeta"></param>
    /// <param name="db"></param>
    public Manager(Db.TableMeta tableMeta, Db.Adapter.Base db)
    {
      Db = db;
      TableMeta = tableMeta;
      DisplayList = new Config.List();
      FormList = new Config.List();
      SortingOrder = new Config.Item();
      OutlineRank = 1;
      postSaveHookList = new List<Action<Db.Record, NameValueCollection, Db.Connection, bool>>();
    }

    internal Db.TableMeta TableMeta { get; set; }

    public Db.Adapter.Base Db { get; private set; }

    private Db.Sql.Select CreateSelect()
    {
      var select = Db.CreateSelect();
      select.AddFrom(TableMeta.CreateTable());
      return select;
    }

    /// <summary>
    /// ページのタイトル
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// フォーム用の設定
    /// </summary>
    public Config.List FormList { get; private set; }

    /// <summary>
    /// リスト用の設定
    /// </summary>
    public Config.List DisplayList { get; private set; }

    /// <summary>
    /// 並び替えが必要な場合セットします。リストの並び順には影響しません。別途<see cref="ListSelectHook"/>で並び順を指定します。
    /// </summary>
    public Config.Item SortingOrder { get; private set; }

    private Html.FormElement CreateFormElement(Sdx.Html.Form form, Config.Item config, Db.Record record, Db.Connection conn)
    {
      Html.FormElement elem;

      MethodInfo method = null;
      if (config.ContainsKey("factory"))
      {
        method = config["factory"].GetMethodInfo(TableMeta.TableType);
        if (method == null)
        {
          throw new NotImplementedException("Missing " + config["factory"] + " method in " + TableMeta.TableType);
        }
      }
      else
      {
        method = TableMeta.TableType.GetMethod(
          "Create" + Sdx.Util.String.ToCamelCase(config.Name) + "Element"
        );
      }


      if (method != null)
      {
        var paramsCount = method.GetParameters().Count();
        if (paramsCount == 2)
        {
          elem = (Sdx.Html.FormElement)method.Invoke(null, new object[] { record, conn });
        }
        else if (paramsCount == 1)
        {
          elem = (Sdx.Html.FormElement)method.Invoke(null, new object[] { conn });
        }
        else
        {
          elem = (Sdx.Html.FormElement)method.Invoke(null, null);
        }
      }
      else
      {
        elem = new Sdx.Html.InputText();
        elem.Name = config.Name;
      }

      elem.Label = config["label"].ToString();

      form.SetElement(elem);

      if (config.ContainsKey("autoCurrentCheckbox"))
      {
        var group = new Sdx.Html.CheckBoxGroup(config["autoCurrentCheckbox"].ToString());
        var checkbox = new Sdx.Html.CheckBox();
        checkbox.Tag.Attr.Set("value", "1");
        group.AddCheckable(checkbox, Sdx.I18n.GetString("現在日時で更新"));
        form.SetElement(group);
      }

      return elem;
    }

    public Html.Form BuildForm(Db.Record record, Db.Connection conn)
    {
      var form = new Html.Form();

      var bind = new NameValueCollection();
      foreach (var config in FormList)
      {
        var elem = CreateFormElement(form, config, record, conn);

        //Validator
        MethodInfo method = null;
        if (config.ContainsKey("validators"))
        {
          method = config["validators"].GetMethodInfo(TableMeta.TableType);
          if (method == null)
          {
            throw new NotImplementedException("Missing " + config["validators"] + " method in " + TableMeta.TableType);
          }
        }
        else
        {
          method = TableMeta.TableType.GetMethod(
            "Create" + Sdx.Util.String.ToCamelCase(config.Name) + "Validators"
          );
        }

        if (method != null)
        {
          var paramsCount = method.GetParameters().Count();

          if(paramsCount == 3)
          {
            method.Invoke(null, new object[] { elem, record, conn });
          }
          else if (paramsCount == 2)
          {
            method.Invoke(null, new object[] { elem, conn });
          } 
          else if (paramsCount == 1)
          {
            method.Invoke(null, new object[] { elem });
          }
          else
          {
            throw new InvalidOperationException("Illegal parameter count for " + method);
          }
        }
        else
        {
          //Auto validator
          var column = TableMeta.Columns.Find(c => c.Name == config.Name);
          if (column != null)
          {
            column.AppendValidators(elem, record);
          }
        }


        if (!elem.Validators.Any(valid => valid is Sdx.Validation.NotEmpty))
        {
          elem.IsAllowEmpty = true;
        }

        if (config.ContainsKey("attributes"))
        {
          elem.Tag.Attr.Add(config["attributes"].ToArray());
        }

        //FormにDBから戻す値を生成
        if (config.ContainsKey("getter"))
        {
          var value = config["getter"].Invoke(record.GetType(), record, null);
          if (config.ContainsKey("multiple") && config["multiple"].ToBool())
          {
            foreach (var val in (string[])value)
            {
              bind.Add(
                config.Name,
                (string)val
              );
            }
          }
          else
          {
            bind.Set(
              config.Name,
              (string)value
            );
          }
        }
        else if(config.ContainsKey("relation"))
        {
          var columnName = config["column"].ToString();
          var relationName = config["relation"].ToString();
          var values = record.GetRecordSet(relationName, conn).ToStringArray(rec => rec.GetString(columnName));
          foreach(var val in values)
          {
            bind.Add(columnName, val);
          }
        }
        else if (config.ContainsKey("column"))
        {
          var columnName = config["column"].ToString();
          bind.Set(columnName, record.GetString(columnName));
        }

        if(config.ContainsKey("autoCurrentCheckbox"))
        {
          if(bind[config["column"].ToString()] == "")
          {
            bind.Set(config["autoCurrentCheckbox"].ToString(), "1");
          }
        }
      }

      form.Bind(bind);

      return form;
    }


    public Web.Url ListPageUrl { get; set; }
    public Web.Url EditPageUrl { get; set; }

    public Db.Record LoadRecord(NameValueCollection parameters, Sdx.Db.Connection conn)
    {
      var recordSet = FetchRecordSet(conn, (select) => {
        var exists = true;
        foreach (var column in TableMeta.Pkeys)
        {
          var values = parameters.GetValues(column.Name);
          if (values != null && values.Length > 0 && values[0].Length > 0)
          {
            select.Where.Add(column.Name, values[0]);
          }
          else
          {
            exists = false;
          }
        }

        if (!exists)
        {
          return false;
        }

        return true;

      });

      Db.Record record;
      
      if(recordSet == null || recordSet.Count == 0)
      {
        record = TableMeta.CreateRecord();
        if (Group != null && Group.TargetValue != null)
        {
          record.SetValue(Group.TargetColumnName, Group.TargetValue);
        }
      }
      else
      {
        record = recordSet[0];
      }

      return record;
    }

    private Group.Base group;

    public Group.Base Group
    {
      get { return group; }
      set
      {
        group = value;
        group.Manager = this;
      }
    }

    private Db.RecordSet FetchRecordSet(Sdx.Db.Connection conn, Func<Db.Sql.Select, bool> filter)
    {
      var select = CreateSelect();
      var ret = filter(select);

      Db.RecordSet records = null;

      if(ret)
      {
        records = conn.FetchRecordSet(select);
      }

      return records;
    }

    public Db.RecordSet FetchRecordSet(Sdx.Db.Connection conn, Sdx.Pager pager = null)
    {
      return FetchRecordSet(conn, (select) =>
      {
        var context = select.ContextList.First(kv => kv.Value.JoinType == Sdx.Db.Sql.JoinType.From).Value;
        if (ListSelectHook != null)
        {
          ListSelectHook.Invoke(TableMeta.TableType, context.Table, new object[] { select, conn });
        }
        
        if (Group != null)
        {
          if (Group.TargetValue != null)
          {
            context.Where.Add(Group.TargetColumnName, Group.TargetValue);
          }
        }

        if (pager != null)
        {
          pager.TotalCount = conn.CountRow(select);
          select.LimitPager(pager);
        }

        return true;
      });
    }

    public void Save(Sdx.Db.Record record, NameValueCollection values, Sdx.Db.Connection conn)
    {
      var relationList = new Config.List();
      foreach (var config in FormList)
      {
        var columnName = config.Name;
        if (config.ContainsKey("relation"))
        {
          relationList.Add(config);
        }
        else
        {
          if (values[columnName] != null)
          {
            List<object> args = new List<object>();
            if (config.ContainsKey("setter"))
            {
              if (config.ContainsKey("multiple") && config["multiple"].ToBool())
              {
                args.Add(values.GetValues(columnName));
              }
              else
              {
                args.Add(values[columnName]);
              }
              
              if (config["setter"].ToMethodInfo(record.GetType()) != null)
              {
                var methodInfo = config["setter"].ToMethodInfo(record.GetType());
             
                if (methodInfo.GetParameters().Count() == 2)
                {
                  args.Add(values);
                }

                methodInfo.Invoke(record, args.ToArray());
              }
              else if (config["setter"].ToPropertyInfo(record.GetType()) != null)
              {
                var propertyInfo = config["setter"].ToPropertyInfo(record.GetType());

                propertyInfo.SetValue(record, args[0]);
              }
              else
              {
                throw new NotImplementedException("Missing " + config["setter"] + " method or property in " + record.GetType());
              }
            }
            else
            {
              record.SetValue(columnName, values[columnName]);
            }
          }
          else if (config.ContainsKey("allowNull") && config["allowNull"].ToBool())
          {
            record.SetValue(columnName, DBNull.Value);
          }
        }
      }

      if(!SortingOrder.IsEmpty)
      {
        var column = SortingOrder["column"].ToString();
        //SQLインジェクション対策
        if(!TableMeta.HasColumn(column))
        {
          throw new InvalidOperationException("Missing " + column + " column in " + TableMeta.Name);
        }

        if(!record.HasValue(column))
        {
          var direction = SortingOrder["direction"].ToString().ToUpper();
          var select = Db.CreateSelect();
          select.AddFrom(TableMeta.CreateTable());

          if (direction == "DESC")
          {
            select.SetColumn(Sdx.Db.Sql.Expr.Wrap("MAX(" + column + ") + 1"));
          }
          else
          {
            select.SetColumn(Sdx.Db.Sql.Expr.Wrap("MIN(" + column + ") - 1"));
          }

          var value = conn.FetchOne(select);

          record.SetValue(column, value == DBNull.Value ? 1 : value);
        }
      }

      var isNew = record.IsNew;//フック内で使いたいので record.Save が呼ばれる前に控えておく
      record.Save(conn);

      foreach (var config in relationList)
      {
        var relName = config["relation"].ToString();
        var rel = TableMeta.Relations[relName];
        var currentRecords = record.GetRecordSet(relName, conn);
        var relValues = values.GetValues(config["column"].ToString());

        if (relValues != null)
        {
          foreach (var refId in values.GetValues(config["column"].ToString()))
          {
            var cRecord = currentRecords.Pop(crec => crec.GetString(config["column"].ToString()) == refId);
            if (cRecord == null)
            {
              var relRecord = rel.TableMeta.CreateRecord();
              relRecord.SetValue(rel.ReferenceKey, record.GetValue(rel.ForeignKey));
              relRecord.SetValue(config["column"].ToString(), refId);
              relRecord.Save(conn);
            }
          }
        }

        currentRecords.ForEach(crec => crec.Delete(conn));
      }

      record.ClearRecordCache();
      postSaveHookList.ForEach(action =>
      {
        action(record, values, conn, isNew);
      });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pkeyJson">`{id: 1}`のようなJSON文字列を想定しています</param>
    /// <param name="conn"></param>
    public void DeleteRecord(string pkeyJson, Sdx.Db.Connection conn)
    {
      var pkeyValues = Sdx.Util.Json.Decode<Dictionary<string, object>>(pkeyJson);
      var delete = Db.CreateDelete();

      delete.SetFrom(TableMeta.Name);
      foreach(var column in TableMeta.Pkeys)
      {
        delete.Where.Add(column.Name, pkeyValues[column.Name]);
      }

      conn.Execute(delete);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pkeyJsons">`{id: 1}`のようなJSON文字列の配列を想定しています</param>
    public void Sort(Db.RecordSet recordSet, string[] pkeyJsons, Sdx.Db.Connection conn)
    {
      var secValue = 0;
      var direction = SortingOrder["direction"].ToString().ToUpper();
      if (direction == "DESC")
      {
        secValue = pkeyJsons.Length - 1;
      }

      foreach (var pkeys in pkeyJsons)
      {
        var pkeyValues = Sdx.Util.Json.Decode<Dictionary<string, object>>(pkeys);
        var record = recordSet.First((rec) => pkeyValues.All(kv => rec.GetString(kv.Key) == kv.Value.ToString()));
        record.SetValue(SortingOrder["column"].ToString(), secValue);
        record.Save(conn);
        secValue = direction == "DESC" ? secValue - 1 : secValue + 1;
      }
    }

    /// <summary>
    /// リストの並び順の変更が必要な場合こちらで。
    /// </summary>
    public Config.Value ListSelectHook { get; set; }

    public bool HasPerPage
    {
      get
      {
        return perPage != null;
      }
    }

    /// <summary>
    /// ページングが必要な場合セットします。
    /// </summary>
    public int PerPage
    {
      get
      {
        return (int)perPage;
      }
      set
      {
        this.perPage = value;
      }
    }

    /// <summary>
    /// サブミットされた値をフォームにバインドする
    /// </summary>
    /// <param name="form"></param>
    /// <param name="values"></param>
    public void BindToForm(Html.Form form, NameValueCollection values)
    {
      var bindValues = new NameValueCollection(values);
      foreach(var config in FormList.Where(c => c.ContainsKey("autoCurrentCheckbox")))
      {
        if(bindValues[config["autoCurrentCheckbox"].ToString()] == "1")
        {
          bindValues.Set(config["column"].ToString(), DateTime.Now.ToString());
        }
      }
      
      form.Bind(bindValues);
    }

    public int OutlineRank { get; set; }

    public string Heading(int rank)
    {
      return "h" + (OutlineRank + rank - 1).ToString();
    }

    public object GetEditPagePath(Sdx.Db.Record record)
    {
      //pkeyの一部でブルーピングされていたら、既にURLについているので取り除く。
      return EditPageUrl.Build(
        record.GetPkeyValues()
          .Where(kv => Group == null || kv.Key != Group.TargetColumnName)
          .ToDictionary(kv => kv.Key, kv => kv.Value.ToString())
      );
    }

    /// <summary>
    /// Save() 後に何かさせたい場合は AddPostSaveHook でセットしてください
    /// </summary>
    private List<Action<Db.Record, NameValueCollection, Db.Connection, bool>> postSaveHookList;

    public void AddPostSaveHook(Action<Db.Record, NameValueCollection, Db.Connection, bool> callback)
    {
      postSaveHookList.Add(callback);
    }

  }
}
