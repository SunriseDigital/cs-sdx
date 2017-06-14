using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Sdx.Data.TreeMapper
{
    public class Record<T>
        where T : Sdx.Data.TreeMapper.Record.Item, new()
    {
        private Tree tree;
        private Dictionary<string, RecordSetData> recordSetDictionary = null;

        public Record(Tree tree)
        {
            this.tree = tree;
        }
        
        public class RecordSetData
        {
            public Sdx.Db.RecordSet RecordSet { get; set;}
            public Func<T, Sdx.Db.Record, bool> Matcher { get; set; }
        }

        public void AddRecordSet(string key, Sdx.Db.RecordSet recordSet, Func<T, Sdx.Db.Record, bool> matcher) 
        {
            if (recordSetDictionary == null)
            {
                recordSetDictionary = new Dictionary<string,RecordSetData>{};
            }
            recordSetDictionary[key] = new RecordSetData
            {
                RecordSet = recordSet,
                Matcher = matcher
            };
        }

        public IEnumerable<Sdx.Data.TreeMapper.Record.Item> GetItems()
        {
            foreach (var childTree in tree)
            {
                var treeItem = new T();
                treeItem.AddTree(childTree);
                foreach (KeyValuePair<string, RecordSetData> pair in recordSetDictionary)
                {
                    var data = pair.Value;
                    treeItem.AddRecord(
                        pair.Key,
                        data.RecordSet
                            .Select(rec => rec)
                            .FirstOrDefault(rec => data.Matcher(treeItem, rec))
                    );
                }

                yield return treeItem;
            }
        }

        public int Count
        {
            get
            {
                //return 0;
                return tree.Count;
            }
        }
    }
}
