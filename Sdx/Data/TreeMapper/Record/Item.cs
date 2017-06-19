using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Data.TreeMapper.Record
{
    public class Item
    {
      protected Dictionary<RecordKey, Sdx.Db.Record> treeRecords = new Dictionary<RecordKey, Db.Record> { };
        protected Sdx.Data.Tree tree = null;

        public class RecordKey
        {
          public string Code { get; private set; }
          public RecordKey (string code)
          {
            Code = code;
          }

          public override int GetHashCode()
          {
            return Code.GetHashCode();
          }

          public override bool Equals(object obj)
          {
            return Equals(obj as RecordKey);
          }

          public bool Equals(RecordKey recordKey)
          {
            return recordKey != null && recordKey.Code == this.Code;
          }
        }

        public bool HasRecordKey(RecordKey recordKey)
        {
          if (recordKey == null)
          {
            throw new ArgumentNullException("The argument 'recordKey' null is not allowed.");
          }

          return this.GetType().GetFields()
            .Where(prop => prop.IsStatic)
            .Where(prop => prop.FieldType == typeof(RecordKey))
            .Any(prop => recordKey.Equals(prop.GetValue(null)));
        }

        public void AddTree(Sdx.Data.Tree tree)
        {
          this.tree = tree;
        }

        public void AddRecord(RecordKey key, Sdx.Db.Record record)
        {
          if(!HasRecordKey(key))
          {
            throw new ArgumentException("Missing `" + key.Code + "` RecordKey property in this item.");
          }

          treeRecords.Add(key, record);
        }
    }
}
