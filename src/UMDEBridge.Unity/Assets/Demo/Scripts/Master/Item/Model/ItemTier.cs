using MasterMemory;
using MessagePack;
using UMDEBridge.Annotations;
using UnityEditor;

namespace Demo.Scripts.Master.Item.Model
{
    [MemoryTable(nameof(ItemTier)), MessagePackObject(true)]
    public class ItemTier
    {
        public ItemTier(string id, string name, int price) {
            Id = id;
            Name = name;
            Price = price;
        }

        [PrimaryKey]
        [ColumnConfig(sortLabel="キー", columnWidth = 20, preferExcel = 1)]
        public string Id { get; private set; }
        
        [ColumnConfig(sortLabel="名前", preferExcel = 1)]
        public string Name { get; private set; }
        
        [ColumnConfig(sortLabel="基礎値段", preferExcel = 1, columnWidth = 20)]
        public int Price { get; private set; }
        
#if UNITY_EDITOR
        public void Draw()
        {
            EditorGUILayout.LabelField(Id);
            EditorGUILayout.LabelField("名前", Name);
            EditorGUILayout.LabelField("基礎価格", Price.ToString());
        }
#endif
    }
}