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
          return this.GetType().GetFields()
            .Where(prop => prop.IsStatic)
            .Where(prop => prop.FieldType == typeof(RecordKey))
            .Any(prop => {
              var val = prop.GetValue(null) as RecordKey;
              return val != null && val.Equals(recordKey);
            });
        }



        public void AddTree(Sdx.Data.Tree tree)
        {
          this.tree = tree;
        }

        public void AddRecord(RecordKey key, Sdx.Db.Record record)
        {
          if(!HasRecordKey(key))
          {
            throw new ArgumentException("Argument 'key' is invalid. It must be of type 'RecordKey'.");
          }

          treeRecords.Add(key, record);
        }
    }
}
