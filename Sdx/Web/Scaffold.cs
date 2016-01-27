using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Web
{
  public class Scaffold<T> where T: Sdx.Db.Record, new()
  {
    public const string CONTEXT_KEY = "SDX.WEB.SCAFFOLD.INSTANCES";
    public String Name { get; private set; }

    public Scaffold(Sdx.Db.Adapter db, string name)
    {
      this.Name = name;
      Dictionary<string, Scaffold<T>> instances = null;
      if (!Context.Current.Vars.ContainsKey(Scaffold<T>.CONTEXT_KEY))
      {
        instances = new Dictionary<string, Scaffold<T>>();
        Context.Current.Vars[Scaffold<T>.CONTEXT_KEY] = instances;
      }
      else
      {
        instances = Context.Current.Vars.As<Dictionary<string, Scaffold<T>>>(Scaffold<T>.CONTEXT_KEY);
      }

      if (instances.ContainsKey(name))
      {
        throw new InvalidOperationException("Already exists `area` Scaffold");
      }

      var prop = typeof(T).GetProperty("Meta");
      if (prop == null)
      {
        throw new NotImplementedException("Missing Meta property in " + typeof(T));
      }

      this.TableMeta = prop.GetValue(null, null) as Db.TableMeta;
      if (this.TableMeta == null)
      {
        throw new NotImplementedException("Initialize TableMeta for " + typeof(T));
      }

      instances[name] = this;

      Db = db;
    }


    private Db.TableMeta TableMeta { get; set; }

    private Db.Adapter Db { get; set; }

    public Db.RecordSet<T> List
    {
      get
      {
        var select = Db.CreateSelect();
        select.AddFrom(TableMeta.CreateTable<Db.Table>());

        Db.RecordSet<T> records;
        using (var conn = Db.CreateConnection())
        {
          conn.Open();
          records = conn.FetchRecordSet<T>(select);
        }

        return records;
      }
    }

    public List<Dictionary<string, string>> ListColumns { get; set; }

    public string Title { get; set; }
  }
}
