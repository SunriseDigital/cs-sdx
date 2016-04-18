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
    private const string CONTEXT_KEY = "SDX.SCAFFOLD.MANAGER.INSTANCES";
    private const string DEFAULT_NAME = "SDX.SCAFFOLD.MANAGER.DEFAULT_NAME";
    public String Name { get; private set; }

    private int? perPage;

    /// <summary>
    /// 生成したMagegerはUserControlで参照するために<see cref="Name"/>をキーにContext.Currentにキャッシュされます。そのキャッシュを強制的にクリアします。通常の使用では必要ありません。ユニットテストで使いまいした。
    /// </summary>
    public static void ClearContextCache()
    {
      Dictionary<string, Manager> instances = Context.Current.Vars.As<Dictionary<string, Manager>>(Manager.CONTEXT_KEY);
      instances.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tableMeta"></param>
    /// <param name="db"></param>
    /// <param name="name">UserControl側で参照するための名前。基本デフォルトでOK。一ページに複数のScaffoldを使用する場合必要（テストしてません）。</param>
    public Manager(Db.TableMeta tableMeta, Db.Adapter.Base db, string name = Manager.DEFAULT_NAME)
    {
      this.Name = name;
      Dictionary<string, Manager> instances = null;
      if (!Context.Current.Vars.ContainsKey(Manager.CONTEXT_KEY))
      {
        instances = new Dictionary<string, Manager>();
        Context.Current.Vars[Manager.CONTEXT_KEY] = instances;
      }
      else
      {
        instances = Context.Current.Vars.As<Dictionary<string, Manager>>(Manager.CONTEXT_KEY);
      }

      if (instances.ContainsKey(this.Name))
      {
        throw new InvalidOperationException("Already exists " + this.Name + " Manager");
      }

      this.TableMeta = tableMeta;

      instances[name] = this;

      Db = db;

      DisplayList = new Config.List();
      FormList = new Config.List();
      SortingOrder = new Config.Item();
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

    private Html.FormElement CreateFormElement(Config.Item config, Db.Record record, Db.Connection conn)
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
          "Create" + Sdx.Util.String.ToCamelCase(config["column"].ToString()) + "Element"
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

        elem.Name = config["column"].ToString();

        if (!config.ContainsKey("label"))
        {
          throw new InvalidOperationException("Missing label param");
        }

        elem.Label = config["label"].ToString();
      }

      return elem;
    }

    public Html.Form BuildForm(Db.Record record, Db.Connection conn)
    {
      var form = new Html.Form();

      var bind = new NameValueCollection();
      //var hasGetters = new List<Config.Item>();
      foreach (var config in FormList)
      {
        var elem = CreateFormElement(config, record, conn);

        form.SetElement(elem);

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
            "Create" + Sdx.Util.String.ToCamelCase(config["column"].ToString()) + "Validators"
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
        else if (config.ContainsKey("column"))
        {
          var columnName = config["column"].ToString();
          var column = TableMeta.Columns.Find(c => c.Name == columnName);
          if (column != null)
          {
            column.AppendValidators(elem);
          }
        }


        if (!elem.Validators.Any(valid => valid is Sdx.Validation.NotEmpty))
        {
          elem.IsAllowEmpty = true;
        }

        if (config.ContainsKey("getter"))
        {
          bind.Set(
            config["column"].ToString(),
            (string)config["getter"].Invoke(record.GetType(), record, null)
          );
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
          if (record.HasValue(columnName))
          {
            bind.Set(columnName, record.GetString(columnName));
          }
        }
      }

      form.Bind(bind);

      return form;
    }


    public Web.Url ListPageUrl { get; set; }
    public Web.Url EditPageUrl { get; set; }

    public static Manager CurrentInstance(string key)
    {
      if(key == null)
      {
        key = Manager.DEFAULT_NAME;
      }
      var instances = Context.Current.Vars.As<Dictionary<string, Manager>>(Manager.CONTEXT_KEY);
      return instances[key];
    }

    public Db.Record LoadRecord(NameValueCollection parameters, Sdx.Db.Connection conn)
    {
      var recordSet = FetchRecordSet(conn, (select) => {
        var exists = false;
        TableMeta.Pkeys.ForEach((column) =>
        {
          var values = parameters.GetValues(column);
          if (values != null && values.Length > 0 && values[0].Length > 0)
          {
            exists = true;
            select.Where.Add(column, values[0]);
          }
        });

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
          ListSelectHook.Invoke(TableMeta.TableType, context.Table, new object[] { select });
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

    public void Save(Sdx.Db.Record record, NameValueCollection form, Sdx.Db.Connection conn)
    {
      var relationList = new Config.List();
      foreach (var config in FormList)
      {
        var columnName = config["column"].ToString();
        if(config.ContainsKey("relation"))
        {
          relationList.Add(config);
        }
        else if(config.ContainsKey("setter"))
        {
          if (form[columnName] != null)
          {
            config["setter"].Invoke(
              record.GetType(),
              record,
              new object[] { form[columnName] }
            );
          }
        }
        else
        {
          if (form[columnName] != null)
          {
            record.SetValue(columnName, form[columnName]);
          }
        }
      }

      record.Save(conn);

      foreach (var config in relationList)
      {
        var relName = config["relation"].ToString();
        var rel = TableMeta.Relations[relName];
        var currentRecords = record.GetRecordSet(relName, conn);
        var values = form.GetValues(config["column"].ToString());

        if (values != null)
        {
          foreach (var refId in form.GetValues(config["column"].ToString()))
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
      TableMeta.Pkeys.ForEach(column => delete.Where.Add(column, pkeyValues[column]));

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
  }
}
