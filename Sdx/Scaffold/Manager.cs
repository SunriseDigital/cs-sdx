using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;

namespace Sdx.Scaffold
{
  public class Manager
  {
    private const string CONTEXT_KEY = "SDX.SCAFFOLD.MANAGER.INSTANCES";
    private const string DEFAULT_NAME = "SDX.SCAFFOLD.MANAGER.DEFAULT_NAME";
    public String Name { get; private set; }

    public static void ClearContextCache()
    {
      Dictionary<string, Manager> instances = Context.Current.Vars.As<Dictionary<string, Manager>>(Manager.CONTEXT_KEY);
      instances.Clear();
    }

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

      DisplayList = new ConfigList();
      FormList = new ConfigList();
    }

    internal Db.TableMeta TableMeta { get; set; }

    public Db.Adapter.Base Db { get; private set; }

    private Db.Sql.Select CreateSelect()
    {
      var select = Db.CreateSelect();
      select.AddFrom(TableMeta.CreateTable());
      return select;
    }

    public string Title { get; set; }

    public ConfigList FormList { get; private set; }

    public ConfigList DisplayList { get; private set; }

    public Html.Form BuildForm(Db.Record record, Db.Connection conn)
    {
      var form = new Html.Form();

      var hasGetters = new List<ConfigItem>();
      foreach (var config in FormList)
      {
        Html.FormElement elem;
        var methodName = "Create" + Sdx.Util.String.ToCamelCase(config["column"].ToString()) + "Element";
        var method = TableMeta.TableType.GetMethod(methodName);
        if (method != null)
        {
          var paramsCount = method.GetParameters().Count();
          if(paramsCount == 1)
          {
            elem = (Sdx.Html.FormElement)method.Invoke(null, new object[] { conn });
          }
          else if (paramsCount == 2)
          {
            elem = (Sdx.Html.FormElement)method.Invoke(null, new object[] { conn, record });
          }
          else
          {
            elem = (Sdx.Html.FormElement)method.Invoke(null, null);
          }
        }
        else
        {
          //主キーはhidden
          if (TableMeta.Pkeys.Exists((column) => column == config["column"].Value))
          {
            elem = new Sdx.Html.InputHidden();
          }
          else
          {
            elem = new Sdx.Html.InputText();
          }
          
          elem.Name = config["column"].Value;
        }

        if(!config.ContainsKey("label"))
        {
          throw new InvalidOperationException("Missing label param");
        }

        elem.Label = config["label"].Value;

        form.SetElement(elem);

        if(config.ContainsKey("getter"))
        {
          hasGetters.Add(config);
        }
      }

      var binds = record.ToNameValueCollection();

      hasGetters.ForEach(config => {
        binds.Set(
          config["column"].Value,
          (string)config["getter"].Invoke(record, null, m => !m.IsStatic && m.GetParameters().Count() == 0)
        );
      });

      form.Bind(binds);


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

    public Db.RecordSet FetchRecordSet(Sdx.Db.Connection conn)
    {
      return FetchRecordSet(conn, (select) =>
      {
        if (Group != null)
        {
          if (Group.TargetValue != null)
          {
            select.Where.Add(Group.TargetColumnName, Group.TargetValue);
          }
        }

        return true;
      });
    }

    public void Save(Sdx.Db.Record record, NameValueCollection form, Sdx.Db.Connection conn)
    {
      var relationList = new ConfigList();
      var ownValues = new NameValueCollection();
      foreach (var config in FormList)
      {
        if(config.ContainsKey("relation"))
        {
          relationList.Add(config);
        }
        else if(config.ContainsKey("setter"))
        {
          config["setter"].Invoke(
            record,
            new object[] { form[config["column"].Value] },
            m => !m.IsStatic && m.GetParameters().Count() == 1
          );
        }
        else
        {
          var columnName = config["column"].Value;
          ownValues.Set(columnName, form[columnName]);
        }
      }

      record.Bind(ownValues);
      conn.Save(record);

      foreach (var param in relationList)
      {
        var rel = TableMeta.Relations[param["relation"].Value];
        var currentRecords = record.GetRecordSet(param["relation"].Value, conn);
        foreach (var refId in form.GetValues(param["column"].Value))
        {
          var cRecord = currentRecords.Pop(crec => crec.GetString(param["column"].Value) == refId);
          if(cRecord == null)
          {
            var relRecord = rel.TableMeta.CreateRecord();
            relRecord.SetValue(rel.ReferenceKey, record.GetValue(rel.ForeignKey));
            relRecord.SetValue(param["column"].Value, refId);
            conn.Save(relRecord);
          }
        }

        currentRecords.ForEach(crec => conn.Delete(crec));
      }
    }
  }
}
