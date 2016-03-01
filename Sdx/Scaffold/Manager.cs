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

      DisplayList = new Config.List();
      FormList = new Config.List();
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

    public Config.List FormList { get; private set; }

    public Config.List DisplayList { get; private set; }

    public Html.Form BuildForm(Db.Record record, Db.Connection conn)
    {
      var form = new Html.Form();

      var hasGetters = new List<Config.Item>();
      foreach (var config in FormList)
      {
        Html.FormElement elem;

        MethodInfo method = null;
        if(config.ContainsKey("factory"))
        {
          method = config["factory"].GetMethodInfo(TableMeta.TableType);
          if(method == null)
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
          if (TableMeta.Pkeys.Exists((column) => column == config["column"].ToString()))
          {
            elem = new Sdx.Html.InputHidden();
          }
          else
          {
            elem = new Sdx.Html.InputText();
          }
        }

        elem.Name = config["column"].ToString();

        if(!config.ContainsKey("label"))
        {
          throw new InvalidOperationException("Missing label param");
        }

        elem.Label = config["label"].ToString();

        form.SetElement(elem);

        if(config.ContainsKey("getter"))
        {
          hasGetters.Add(config);
        }
      }

      var binds = record.ToNameValueCollection();

      hasGetters.ForEach(config => {
        binds.Set(
          config["column"].ToString(),
          (string)config["getter"].Invoke(record.GetType(), record, null)
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

      conn.Save(record);

      foreach (var config in relationList)
      {
        var rel = TableMeta.Relations[config["relation"].ToString()];
        var currentRecords = record.GetRecordSet(config["relation"].ToString(), conn);
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
              conn.Save(relRecord);
            }
          }
        }

        currentRecords.ForEach(crec => conn.Delete(crec));
      }
    }
  }
}
