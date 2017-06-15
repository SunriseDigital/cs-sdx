using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Sdx.Db.Sql;
using System.Collections.Specialized;
using System.Web.Script.Serialization;
using System.Globalization;

namespace Sdx.Db
{
  public abstract class Record
  {
    public static string AutoCreateDateColumn { get; set; }
    public static string AutoUpdateDateColumn { get; set; }

    static Record()
    {
      AutoCreateDateColumn = "created_at";
      AutoUpdateDateColumn = "updated_at";
    }

    private Dictionary<string, object> recordCache = new Dictionary<string, object>();

    internal string ContextName { get; set; }
    internal Dictionary<string, object> UpdatedValues { get; private set; }
    internal List<Dictionary<string, object>> ValuesList { get; private set; }
    internal Select Select { get; set; }

    private Db.Connection connection;

    public Record()
    {
      this.UpdatedValues = new Dictionary<string, object>();
      this.ValuesList = new List<Dictionary<string, object>>();
      ValueWillUpdate = new Dictionary<string, Action<object, object, bool>>();
      ValueDidUpdate = new Dictionary<string, Action<object, object>>();
      Init();
    }

    public Sdx.Db.Connection Connection
    {
      set
      {
        connection = value;
      }

      get
      {
        if (connection != null)
        {
          return connection;
        }
        else
        {
          return Select.Connection;
        }
      }
    }

    /// <summary>
    /// カラムが更新される直前に呼ばれるActionをセットする。<seealso cref="Init()"/>でセットしてください。
    /// ValueWillUpdate["someColumn"] = (prevValue, nextValue, isRaw) => {}
    /// </summary>
    protected Dictionary<string, Action<object, object, bool>> ValueWillUpdate { get; private set; }

    /// <summary>
    /// カラムが更新された直後に呼ばれるActionをセットする。<seealso cref="Init()"/>でセットしてください。
    /// ValueDidUpdate["someColumn"] = (prevValue, nextValue) => {}
    /// </summary>
    protected Dictionary<string, Action<object, object>> ValueDidUpdate { get; private set; }

    /// <summary>
    /// Save/Deleteのフック
    /// </summary>
    protected virtual void RecordWillSave(Connection conn) { }
    protected virtual void RecordDidSave(Connection conn) { }
    protected virtual void RecordWillDelete(Connection conn) { }
    protected virtual void RecordDidDelete(Connection conn) { }

    protected virtual void Init()
    {

    }

    public TableMeta OwnMeta
    {
      get
      {
        var prop = this.GetType().GetProperty("Meta");
        if (prop == null)
        {
          throw new NotImplementedException("Missing Meta property in " + this.GetType());
        }

        var meta = prop.GetValue(null, null) as TableMeta;
        if (meta == null)
        {
          throw new NotImplementedException("Initialize TableMeta for " + this.GetType());
        }

        return meta;
      }
    }

    public string GetFormatedDateTime(string key, string format = null)
    {
      if(!HasValue(key))
      {
        return null;
      }

      var datetime = Convert.ToDateTime(GetValue(key));
      return datetime.ToString(format);
    }

    public DateTime GetDateTime(string key)
    {
      return Convert.ToDateTime(this.GetValue(key));
    }

    public decimal GetDecimal(string key)
    {
      return Convert.ToDecimal(this.GetValue(key));
    }

    public double GetDouble(string key)
    {
      return Convert.ToDouble(this.GetValue(key));
    }

    public float GetFloat(string key)
    {
      return float.Parse(this.GetString(key));
    }

    public short GetInt16(string key)
    {
      return Convert.ToInt16(this.GetValue(key));
    }

    public int GetInt(string key)
    {
      return GetInt32(key);
    }

    public int GetInt32(string key)
    {
      return Convert.ToInt32(this.GetValue(key));
    }

    public long GetInt64(string key)
    {
      return Convert.ToInt64(this.GetValue(key));
    }

    /// <summary>
    /// <see cref="GetValue"/>が例外にならない場合はtrue。
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool CanGetValue(string key)
    {
      if (!OwnMeta.HasColumn(key))
      {
        throw new InvalidOperationException("Missing " + key + " column. Check table settings.");
      }

      if (this.UpdatedValues.ContainsKey(key))
      {
        return true;
      }

      if (IsNew)
      {
        return false;
      }

      var keyWithContext = Record.BuildColumnAliasWithContextName(key, this.ContextName);
      return this.ValuesList[0].ContainsKey(keyWithContext);
    }

    private object GetOriginValue(string key)
    {
      if (IsNew)
      {
        throw new InvalidOperationException("No origin values. This is new record.");
      }

      var keyWithContext = Record.BuildColumnAliasWithContextName(key, this.ContextName);
      if (!this.ValuesList[0].ContainsKey(keyWithContext))
      {
        throw new InvalidOperationException("No origin values. Not loaded from db " + key + " on " + this.ContextName);
      }
      return this.ValuesList[0][keyWithContext];
    }

    /// <summary>
    /// カラムの値を取得します。
    /// またDBから読み込まなかったカラムを取得すると例外になります。これはGet系のユーティリティーメソッドで読み込んでいない値を使ってしまうと発見しづらいバグを生むからです。
    /// </summary>
    /// <param name="key"></param>
    /// <returns>新規レコードで値をsetしていない場合NULLが帰ります。DBの値がNULLの時はDBNullが帰ります。</returns>
    public object GetValue(string key)
    {
      if (this.UpdatedValues.ContainsKey(key))
      {
        return this.UpdatedValues[key];
      }

      if (IsNew)
      {
        return null;
      }

      return GetOriginValue(key);
    }

    public string GetString(string key)
    {
      return Convert.ToString(this.GetValue(key));
    }

    public bool GetBool(string key)
    {
      return (bool)GetValue(key);
    }

    public bool ContainsColumn(string key)
    {
      if(UpdatedValues.ContainsKey(key))
      {
        return true;
      }

      var keyWithContext = Record.BuildColumnAliasWithContextName(key, ContextName);
      if (this.ValuesList.Count > 0 && this.ValuesList[0].ContainsKey(keyWithContext))
      {
        return true;
      }

      return false;
    }

    public bool HasValue(string key)
    {
      var value = GetValue(key);
      return !(value is DBNull) && value != null;
    }

    public bool HasRecordCache(string contextName)
    {
      return recordCache.ContainsKey(contextName);
    }

    public Record ClearRecordCache(string contextName = null)
    {
      if (contextName != null)
      {
        if (this.recordCache.ContainsKey(contextName))
        {
          this.recordCache.Remove(contextName);
        }
      }
      else
      {
        this.recordCache.Clear();
      }

      return this;
    }

    private string GetRelationName<T>()
    {
      var recordType = typeof(T);
      var relations = OwnMeta.Relations.Where(kv => kv.Value.TableMeta.RecordType == recordType);
      var count = relations.Count();
      if (count == 0)
      {
        throw new NotImplementedException("Missing relation setting for " + recordType + " in " + OwnMeta.TableType);
      }
      else if (count > 1)
      {
        throw new NotImplementedException("Too many match relations for " + recordType + " in " + OwnMeta.TableType);
      }

      return relations.First().Key;
    }

    public T GetRecord<T>(Action<Select> selectHook = null) where T : Sdx.Db.Record
    {
      return GetRecord<T>(GetRelationName<T>(), selectHook);
    }

    public T GetRecord<T>(Connection connection, Action<Select> selectHook = null) where T : Sdx.Db.Record
    {
      return GetRecord<T>(GetRelationName<T>(), connection, selectHook);
    }

    public T GetRecord<T>(string contextName, Action<Select> selectHook = null) where T : Sdx.Db.Record
    {
      return this.GetRecord<T>(contextName, null, selectHook);
    }

    public T GetRecord<T>(string contextName, Connection connection, Action<Select> selectHook = null) where T : Sdx.Db.Record
    {
      var records = this.GetRecordSet(contextName, connection, selectHook);
      if (records.Count == 0)
      {
        return null;
      }

      return (T)records[0];
    }

    public Record GetRecord(string contextName, Action<Select> selectHook = null)
    {
      return this.GetRecord<Record>(contextName, null, selectHook);
    }

    public Record GetRecord(string contextName, Connection connection, Action<Select> selectHook = null)
    {
      return this.GetRecord<Record>(contextName, connection, selectHook);
    }

    public RecordSet GetRecordSet(string contextName, Action<Select> selectHook = null)
    {
      return this.GetRecordSet(contextName, null, selectHook);
    }

    public RecordSet GetRecordSet(string contextName, Connection connection, Action<Select> selectHook = null)
    {
      if (this.recordCache.ContainsKey(contextName))
      {
        if (selectHook != null)
        {
          throw new ArgumentException("You must clear record cache, before use selectHook.");
        }

        return (RecordSet)this.recordCache[contextName];
      }

      //ParentContext.Nameをチェックして自分にJOINされているかもチェックしています。
      if (this.Select != null && this.Select.HasContext(contextName) && Select.Context(contextName).ParentContext.Name == ContextName) //already joined
      {
        if (selectHook != null)
        {
          throw new ArgumentException("You can't use selectHook, because already joined " + contextName + " context.");
        }

        var resultSet = new RecordSet();
        resultSet.Build(this.ValuesList, this.Select, contextName);
        //キャッシュする
        this.recordCache[contextName] = resultSet;
        return resultSet;
      }
      else //no join
      {
        if (connection == null)
        {
          connection = Connection;
        }

        if (OwnMeta.Relations.ContainsKey(contextName))
        {
          var relations = OwnMeta.Relations[contextName];

          var sel = connection.Adapter.CreateSelect();
          sel.SetComment(this.GetType().Name + "::GetRecordSet(" + contextName  + ")");
          sel.AddFrom(relations.TableMeta.CreateTable())
            .Where.Add(relations.ReferenceKey, this.GetString(relations.ForeignKey));

          if (selectHook != null)
          {
            selectHook.Invoke(sel);
          }

          RecordSet resultSet = connection.FetchRecordSet(sel);

          //キャッシュする
          this.recordCache[contextName] = resultSet;
          return resultSet;
        }

        throw new NotImplementedException("Missing relation setting for " + contextName + " in " + OwnMeta.TableType);
      }
    }

    internal void AddRow(Dictionary<string, object> row)
    {
      this.ValuesList.Add(row);
    }

    internal static string BuildColumnAliasWithContextName(string columnName, string contextName)
    {
      return columnName + "@" + contextName;
    }

    public bool IsNew
    {
      get
      {
        return this.ValuesList.Count == 0;
      }
    }

    public bool IsUpdated
    {
      get
      {
        return this.UpdatedValues.Count != 0;
      }
    }

    public bool IsDeleted { get; internal set; }

    private bool EqualsToCurrent(string columnName, object value)
    {
      var current = GetValue(columnName);
      if(value == null)
      {
        return current == null;
      }

      if(current == null)
      {
        return value == null;
      }

      //DateTimeはミリ秒まで持っていてEquals更新していなくてもFalseを返し必ず更新されてしまうので秒までで比較します。
      if(current is DateTime)
      {
        return current.ToString().Equals(value.ToString());
      }

      if(current is DBNull || value is DBNull)
      {
        return current == value;
      }
      
      value = Convert.ChangeType(value, current.GetType());
      return current.Equals(value);
    }

    /// <summary>
    /// テーブルに存在しないカラムは無視されます。
    /// NameValueCollectionにキーが存在しない場合は無視します。
    /// </summary>
    /// <param name="values"></param>
    /// <param name="isRaw"><see cref="SetValue"/></param>
    /// <returns></returns>
    public Record SetValues(NameValueCollection values, bool isRaw = false)
    {
      var allKeys = values.AllKeys;
      foreach (var column in OwnMeta.Columns)
      {
        if (allKeys.Contains(column.Name))
        {
          SetValue(column.Name, values[column.Name], isRaw);
        }
      }
      return this;
    }
    
    /// <summary>
    /// カラムにデータをセットする。nullあるいは空文字をセットした場合DbNullが入ります。
    /// 空文字を保存したいときは`isRaw`にtrueを渡してください。
    /// </summary>
    /// <param name="columnName"></param>
    /// <param name="value"></param>
    /// <param name="isRaw">DbNullへの変換を行うかどうか</param>
    public Record SetValue(string columnName, object value, bool isRaw = false)
    {
      var prevValue = GetValue(columnName);
      if (ValueWillUpdate.ContainsKey(columnName))
      {
        ValueWillUpdate[columnName](prevValue, value, isRaw);
      }

      this.OwnMeta.CheckColumn(columnName);

      if (!isRaw)
      {
        if (value == null || value.ToString() == "")
        {
          value = DBNull.Value;
        }
      }

      if (!EqualsToCurrent(columnName, value))
      {
        this.UpdatedValues[columnName] = value;
      }

      if (ValueDidUpdate.ContainsKey(columnName))
      {
        ValueDidUpdate[columnName](prevValue, GetValue(columnName));
      }

      return this;
    }

    internal void AppendPkeyWhere(Condition where)
    {
      //pkeyがなかったら例外
      if (!OwnMeta.Pkeys.Any())
      {
        throw new InvalidOperationException("Missing Pkey data in " + this.OwnMeta.Name + " table");
      }

      foreach(var column in OwnMeta.Pkeys)
      {
        var value = this.GetOriginValue(column.Name);
        if (value == null)
        {
          throw new InvalidOperationException("Primary key " + column + " is null.");
        }
        where.Add(column.Name, value);
      }
    }

    public override string ToString()
    {
      var builder = new StringBuilder();
      builder
        .Append(this.OwnMeta.Name)
        .Append(": {")
        ;
      foreach (var column in OwnMeta.Columns.Where(col => ContainsColumn(col.Name)))
      {
        builder
          .Append(column.Name)
          .Append(": ")
          .Append('"')
          .Append(this.GetString(column.Name))
          .Append("\", ")
          ;
      }

      builder
        .Remove(builder.Length - 2, 2)
        .Append("}");

      return builder.ToString();
    }

    public void Bind(NameValueCollection collection)
    {
      this.OwnMeta.Columns.ForEach((column) => {
        if(column.IsAutoIncrement)
        {
          return;
        }
        var value = collection[column.Name];
        if (value != null)
        {
          this.SetValue(column.Name, value);
        }
      });
    }

    /// <summary>
    /// FormにBindするためのNameValueCollectionを生成する。
    /// </summary>
    /// <param name="dateFormat">ColumnType.Date|DateTime型のカラムのフォーマット。省略すると<see cref="CultureInfo.CurrentCulture"/>より取得。</param>
    /// <returns></returns>
    public NameValueCollection ToNameValueCollection(string dateFormat = null)
    {
      var col = new NameValueCollection();
      
      OwnMeta.Columns.ForEach((column) => {
        if (HasValue(column.Name))
        {
          string value;
          if (column.Type == Table.ColumnType.Date)
          {
            if(dateFormat == null)
            {
              dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            }
            value = GetDateTime(column.Name).ToString(dateFormat);
          }
          else if(column.Type == Table.ColumnType.DateTime)
          {
            if(dateFormat == null)
            {
              dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " + CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
            }
            value = GetDateTime(column.Name).ToString(dateFormat);
          }
          else
          {
            value = GetString(column.Name);
          }

          col.Add(column.Name, value);
   
        }
      });

      return col;
    }

    public T Get<T>(string path, Connection conn = null)
    {
      return (T)GetDynamic(path, conn);
    }


    /// <summary>
    /// `.`で区切って深い階層のデータを取得可能。`@`はGetRecord、`#`はメソッドを検索、それ以外はカラムの取得を実行します。
    /// メソッドに引数は渡せません。
    /// e.g.
    /// record.GetDynamic("@some_record.#GetSomeMethod.name"); = record.GetRecord("some_record").GetSomeMethod().GetValue("name");
    /// </summary>
    /// <param name="path"></param>
    /// <param name="conn"></param>
    /// <returns></returns>
    public dynamic GetDynamic(string path, Connection conn = null)
    {
      dynamic result = this;
      var chunk = path.Split('.');
      foreach(var key in chunk)
      {
        //record
        if (key.StartsWith("@"))
        {
          result = result.GetRecord(key.Substring(1), conn);
        }
        else if(key.StartsWith("#"))
        {
          var method = key.Substring(1);
          System.Type type = result.GetType();
          var methodInfo = type.GetMethods().FirstOrDefault(m => m.Name == method && !m.IsStatic);
          if (methodInfo == null)
          {
            throw new NotImplementedException("Missing " + method + " method in " + result.GetType());
          }

          var paramsCount = methodInfo.GetParameters().Count();
          if (paramsCount == 0)
          {
            result = methodInfo.Invoke(result, null);
          }
          else if (paramsCount == 1)
          {
            result = methodInfo.Invoke(result, new object[] { conn });
          }
          else
          {
            throw new NotSupportedException(method + "'s parameter must be nothing or Sdx.Db.Connection.");
          }
        }
        else
        {
          result = result.GetValue(key);
        }

        if (result == null)
        {
          break;
        }
        
      }
      return result;
    }

    public Dictionary<string, object> GetPkeyValues()
    {
      var dic = new Dictionary<string, object>();
      foreach (var column in OwnMeta.Pkeys)
      {
        dic[column.Name] = GetValue(column.Name);
      }

      return dic;
    }

    public bool IsNull(string columnName)
    {
      return GetValue(columnName) == DBNull.Value;
    }

    private bool NeedsAutoUpdate(string columnName)
    {
      if (columnName == null)
      {
        return false;
      }

      if (!OwnMeta.HasColumn(columnName))
      {
        return false;
      }
      
      var first = UpdatedValues.FirstOrDefault(kv => kv.Key == columnName);
      if (first.Value == null)
      {
        return true;
      }

      if (first.Value.ToString() == "")
      {
        return true;
      }

      return false;
    }

    public void Save(Db.Connection conn)
    {
      if (IsDeleted)
      {
        throw new InvalidOperationException("This record is already deleted.");
      }

      RecordWillSave(conn);

      if (IsNew)
      {
        var insert = conn.Adapter.CreateInsert();
        insert.SetInto(OwnMeta.Name);

        //自動登録日時更新
        if (NeedsAutoUpdate(AutoCreateDateColumn))
        {
          SetValue(AutoCreateDateColumn, DateTime.Now);
        }

        //自動更新日時更新
        if (NeedsAutoUpdate(AutoUpdateDateColumn))
        {
          SetValue(AutoUpdateDateColumn, DateTime.Now);
        }

        foreach (var columnValue in UpdatedValues)
        {
          insert.AddColumnValue(columnValue.Key, columnValue.Value);
        }

        conn.Execute(insert);

        //値を保存後も取得できるようにする
        var newValues = new Dictionary<string, object>();
        foreach (var columnValue in UpdatedValues)
        {
          var key = Record.BuildColumnAliasWithContextName(columnValue.Key, ContextName);
          newValues[key] = columnValue.Value;
        }

        //AutoincrementのPkeyを取得できるようにしておく。
        //Autoincrementは通常テーブルに１つしか作れないはず（MySQLとSQLServerはそうだった）
        var firstPkey = OwnMeta.Pkeys.FirstOrDefault();
        if (firstPkey != null)
        {
          var pkeyValue = GetValue(firstPkey.Name);
          //保存に成功し、PkeyがNullだったらAutoincrementのはず。
          //IsAutoincrementを見ると強引に挿入していることもあるので。
          if (pkeyValue == null)
          {
            var key = Record.BuildColumnAliasWithContextName(firstPkey.Name, ContextName);
            newValues[key] = conn.FetchLastInsertId();
          }
        }

        OwnMeta.Columns.ForEach(column => { 
          var key = Record.BuildColumnAliasWithContextName(column.Name, ContextName);
          if(!newValues.ContainsKey(key))
          {
            newValues[key] = DBNull.Value;
          }
        });

        ValuesList.Add(newValues);
      }
      else
      {
        if (UpdatedValues.Count == 0)
        {
          return;
        }

        var update = conn.Adapter.CreateUpdate();
        update.SetTable(OwnMeta.Name);
        foreach (var columnValue in UpdatedValues)
        {
          update.AddColumnValue(columnValue.Key, columnValue.Value);
        }

        AppendPkeyWhere(update.Where);

        //自動更新日時更新
        if (
          UpdatedValues.Count > 0
          &&
          NeedsAutoUpdate(AutoUpdateDateColumn)
        )
        {
          SetValue(AutoUpdateDateColumn, DateTime.Now);
        }

        conn.Execute(update);

        //値を保存後も取得できるようにする
        foreach (var row in ValuesList)
        {
          foreach (var columnValue in UpdatedValues)
          {
            var key = Record.BuildColumnAliasWithContextName(columnValue.Key, ContextName);
            row[key] = columnValue.Value;
          }
        }
      }

      UpdatedValues.Clear();

      RecordDidSave(conn);
    }

    public void Delete(Db.Connection conn)
    {
      if (IsNew)
      {
        return;
      }

      RecordWillDelete(conn);

      var delete = conn.Adapter.CreateDelete();
      delete.From = OwnMeta.Name;
      AppendPkeyWhere(delete.Where);

      conn.Execute(delete);

      IsDeleted = true;

      RecordDidDelete(conn);
    }

    /// <summary>
    /// データベースがロールバックしたときにDB以外に元に戻したいものがある場合はオーバーライドしてください。
    /// Scaffoldでは自動で呼ばれますが、独自の実装ではロールバック時に個別に呼び出す必要があります。
    /// </summary>
    public virtual void DisposeOnRollback()
    {
      
    }

    public bool IsAnyColumnUpdated(params string[] columns)
    {
      foreach(var column in columns)
      {
        if(UpdatedValues.ContainsKey(column))
        {
          return true;
        }
      }

      return false;
    }

    private Collection.Holder vars = new Collection.Holder();

    public Collection.Holder Vars
    {
      get
      {
        return vars;
      }
    }

    private Collection.Holder internalCache = new Collection.Holder();

    protected Collection.Holder InternalCache
    {
      get
      {
        return internalCache;
      }
    }

    public string PkeyStringValue
    {
      get
      {
        return GetString(PkeyName);
      }
    }

    public int PkeyIntValue
    {
      get
      {
        return GetInt(PkeyName);
      }
    }

    public object PkeyValue
    {
      get
      {
        return GetValue(PkeyName);
      }
    }

    private string PkeyName
    {
      get
      {
        if (OwnMeta.Pkeys.Count() != 1)
        {
          throw new InvalidOperationException("Illegal pkey count " + OwnMeta.Pkeys.Count());
        }

        return OwnMeta.Pkeys.First().Name;
      }
    }

    public Dictionary<string, object> ToDictionary(params string[] columns)
    {
      var dic = new Dictionary<string, object>(){};

      return dic;
    }

    public Dictionary<string, T> ToDictionary<T>(params string[] columns)
    {
      var dic = new Dictionary<string, T>() { };

      return dic;
    }
  }
}
