using System;
using System.Collections.Generic;
using System.Linq;

namespace Sdx.Db
{
  /// <summary>
  /// SelectにおいてTableを表現するオブジェクト。メタ情報としてカラムや他テーブルとの関連情報を保持し、SELECTにセットする。
  /// またテーブルに付与されたエイリアス名を保持し、Recordに必要なカラム名のエイリアス生成も行う。
  /// </summary>
  public abstract class Table : ICloneable
  {
    public class Relation
    {
      public Relation(Type tableType, string foreignKey, string referenceKey)
      {
        this.TableType = tableType;
        this.ForeignKey = foreignKey;
        this.ReferenceKey = referenceKey;
      }

      public Relation(Type tableType, string foreignKey) : this(tableType, foreignKey, foreignKey)
      {

      }

      public TableMeta TableMeta
      {
        get
        {
          var prop = TableType.GetProperty("Meta");
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
      public Type TableType { get; private set; }
      public string ForeignKey { get; private set; }
      public string ReferenceKey { get; private set; }
      public string JoinCondition
      {
        get
        {
          return "{0}." + this.ForeignKey + " = {1}." + this.ReferenceKey;
        }
      }
    }

    public enum ColumnType
    {
      Integer,
      UnsignedInteger,
      Float,
      UnsignedFloat,
      String,
      DateTime,
      Date
    }

    public class Column
    {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="name"></param>
      /// <param name="type"></param>
      /// <param name="isNotNull"></param>
      /// <param name="isAutoIncrement"></param>
      /// <param name="maxLength">Stringの場合は文字数、Numberの場合はビット数</param>
      public Column(
        string name,
        ColumnType? type = null,
        bool isNotNull = true,
        bool isAutoIncrement = false,
        long? maxLength = null,
        bool isPkey = false
      )
      {
        this.Name = name;
        this.Type = type;
        this.IsNotNull = isNotNull;
        this.IsAutoIncrement = isAutoIncrement;
        this.MaxLength = maxLength;
        this.IsPkey = isPkey;
      }

      public string Name { get; private set; }
      public ColumnType? Type { get; private set; }
      public bool IsNotNull {get; private set;}
      public bool IsAutoIncrement { get; private set; }
      public long? MaxLength { get; private set; }

      public TableMeta Meta { get; internal set; }

      public bool IsPkey { get; private set; }

      public List<Validation.Validator> CreateValidatorList(Record record)
      {
        var list = new List<Validation.Validator>();
        if(IsNotNull)
        {
          //AutoFillでも編集時はNotEmptyをつける
          if (IsAutoFill && !record.IsNew)
          {
            list.Add(new Validation.NotEmpty());
          }
          else if (!IsAutoFill)
          {
            list.Add(new Validation.NotEmpty());
          }
        }

        if(Type == ColumnType.Integer || Type == ColumnType.UnsignedInteger || IsAutoIncrement)
        {
          list.Add(new Validation.Numeric());

          if(Type == ColumnType.UnsignedInteger)
          {
            var vGreater = new Validation.GreaterThan(0);
            vGreater.IsInclusive = true;
            list.Add(vGreater);
          }

          if(MaxLength != null)
          {
            var max = (long)Math.Pow((double)2, (double)MaxLength - 1) - 1;
            if(Type == ColumnType.UnsignedInteger)
            {
              max = max * 2 + 1;
            }
            else
            {
              var vGreater = new Validation.GreaterThan(-max - 1);
              vGreater.IsInclusive = true;
              list.Add(vGreater);
            }

            var vLess = new Validation.LessThan(max);
            vLess.IsInclusive = true;
            list.Add(vLess);
          }
        }
        else if(Type == ColumnType.Float)
        {
          // 浮動小数点も最大値出せそうな予感はするが需要がそれほどなさそうなので保留
          list.Add(new Validation.Numeric());
        }
        else if(Type == ColumnType.String)
        {
          if (MaxLength != null)
          {
            list.Add(new Validation.StringLength(max: MaxLength));
          }
        }
        else if (Type == ColumnType.DateTime)
        {
          list.Add(new Validation.DateTime());
        }
        else if (Type == ColumnType.Date)
        {
          list.Add(new Validation.Date());
        }

        return list;
      }

      public void AppendValidators(Html.FormElement element, Record record)
      {
        CreateValidatorList(record).ForEach(v => element.AddValidator(v, v is Validation.NotEmpty));
      }

      private bool IsAutoFill
      {
        get
        { 
          if(IsAutoIncrement)
          {
            return true;
          }

          return Name == Record.AutoCreateDateColumn || Name == Record.AutoUpdateDateColumn;
        }
      }
    }

    public Adapter.Base Adapter { get; set; }

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

    public Table ClearColumns()
    {
      this.Select.ClearColumns(this.Context.Name);
      return this;
    }

    public Table AddColumns(params object[] columns)
    {
      foreach (var column in columns)
      {
        Select.ColumnList.Add(new Sql.Column((dynamic)column, this.Context.Name, NormalizeAlias(column, null)));
      }
      return this;
    }

    public Table AddColumn(Sql.Select select, string alias = null)
    {
      Select.ColumnList.Add(new Sql.Column(select, this.Context.Name, NormalizeAlias(select, alias)));
      return this;
    }

    public Table AddColumn(Sql.Expr expr, string alias = null)
    {
      Select.ColumnList.Add(new Sql.Column(expr, this.Context.Name, NormalizeAlias(expr, alias)));
      return this;
    }

    public Table AddColumn(string columnName, string alias = null)
    {
      Select.ColumnList.Add(new Sql.Column(columnName, this.Context.Name, NormalizeAlias(columnName, alias)));
      return this;
    }

    private string NormalizeAlias(object columnName, string alias)
    {
      if (this.Context == null)
      {
        throw new InvalidOperationException("ContextName is null");
      }

      return Record.BuildColumnAliasWithContextName(alias != null ? alias : columnName.ToString(), this.Context.Name);
    }

    public Table SetColumns(params object[] columns)
    {
      this.ClearColumns();
      this.AddColumns(columns);
      return this;
    }

    public Table SetColumn(Sql.Select subquery, string alias = null)
    {
      this.ClearColumns();
      this.AddColumn(subquery, alias);
      return this;
    }

    public Table SetColumn(Sql.Expr expr, string alias = null)
    {
      this.ClearColumns();
      this.AddColumn(expr, alias);
      return this;
    }

    public Table SetColumn(string columnName, string alias = null)
    {
      this.ClearColumns();
      this.AddColumn(columnName, alias);
      return this;
    }

    public Sql.Context Context
    {
      get; set;
    }

    internal void AddAllColumnsFromMeta()
    {
      this.OwnMeta.Columns.ForEach(column =>
      {

        this.AddColumn(column.Name);
      });
    }

    /// <summary>
    /// テーブル経由でカラムを変更したとき、カラムの並び順が呼び出し順になってるのが自然だと思ったので、Table内でSelectを保持するようになっています。
    /// </summary>
    public Sql.Select Select
    {
      get
      {
        return this.Context.Select;
      }
    }

    public object Clone()
    {
      return this.MemberwiseClone();
    }

    public virtual Table SelectDefaultOrder(Sql.Select select)
    {
      return this;
    }

    public Db.Sql.Context FindSelfContext(Db.Sql.Select select)
    {
      return select.ContextList.First(c => c.Value.Table == this).Value;
    }

    public Db.RecordSet FetchRecordSetDefaultOrder(Db.Connection conn)
    {
      var select = conn.Adapter.CreateSelect();
      select.AddFrom(this);

      this.SelectDefaultOrder(select);

      return conn.FetchRecordSet(select);
    }

    public Db.Record FetchRecordByPkey(Db.Connection conn, Dictionary<string, object> pkeyValues)
    {
      var select = this.Adapter.CreateSelect();
      select.AddFrom(this);

      foreach (var col in OwnMeta.Pkeys)
      {
        select.Where.Add(col.Name, pkeyValues[col.Name]);
      }

      return conn.FetchRecord(select);
    }

    public Record FetchRecordByPkey(Db.Connection conn, string pkeyValue)
    {
      if (OwnMeta.Pkeys.Count() > 1)
      {
        throw new InvalidOperationException("This table has multiple pkeys.");
      }
      var select = conn.Adapter.CreateSelect();
      select.AddFrom(this);
      select.Where.Add(OwnMeta.Pkeys.First((col) => true).Name, pkeyValue);

      return conn.FetchRecord(select);
    }

    public Record FetchRecord(Db.Connection conn, Action<Sql.Select> action = null)
    {
      var select = conn.Adapter.CreateSelect();
      select.AddFrom(this);

      if (action != null)
      {
        action.Invoke(select);
      }

      return conn.FetchRecord(select);
    }

    public RecordSet FetchRecordSet(Db.Connection conn, Action<Sql.Select> action = null)
    {
      var select = conn.Adapter.CreateSelect();
      select.AddFrom(this);

      if (action != null)
      {
        action.Invoke(select);
      }

      return conn.FetchRecordSet(select);
    }
  }
}
