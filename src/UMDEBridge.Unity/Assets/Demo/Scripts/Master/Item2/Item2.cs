using MasterMemory;
using MessagePack;
using UMDEBridge.Annotations;
using UnityEditor;
using UnityEngine;

namespace Demo.Scripts.Master.Item2 {
    [Union(0, typeof(UnitItem2))]
    [MemoryTable(nameof(Item2)), MessagePackObject]
	public abstract class Item2
    {
        protected Item2(string id, string name, string text, string icon, SomeType someType) {
            Id = id;
            Name = name;
            Text = text;
            Icon = icon;
            SomeType = someType;
        }

        [PrimaryKey]
        [ColumnConfig(sortLabel = "キー", columnWidth = 20, preferExcel = 1)]
        [Key(0)]
        public string Id { get; private set; }

        [ColumnConfig(sortLabel="名前", preferExcel = 1)]
        [Key(1)]
        public string Name { get; private set; }
        
        [ColumnConfig(sortLabel = "説明", columnWidth = 200, preferExcel = 1)]
        [Key(2)]
        public string Text { get; private set; }
        
        [ColumnConfig(sortLabel = "アイコンパス")]
        [Key(3)]
        public string Icon { get; private set; }
        
        [ColumnConfig(sortLabel = "SomeType")]
        [Key(6)]
        public SomeType SomeType { get; private set; }

#if UNITY_EDITOR
        public virtual void Draw()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box, GUILayout.MaxWidth(500)))
            {
                EditorGUILayout.LabelField("id", Id);
                Name = EditorGUILayout.TextField("名前", Name);
                Text = EditorGUILayout.TextField("説明", Text);
                Icon = EditorGUILayout.TextField("アイコンパス", Icon);
                SomeType = (SomeType)EditorGUILayout.EnumPopup("SomeType", SomeType);
            }
        }
#endif
    }

    [MessagePackObject]
    public class UnitItem2 : Item2
    {
        public UnitItem2(string id, string name, string text, string icon, int hp, int attack, SomeType someType) : base(id, name, text, icon, someType) {
            Hp = hp;
            Attack = attack;
        }

        [ColumnConfig(sortLabel = "HP", columnWidth = 20, preferExcel = 1)]
        [Key(4)]
        public int Hp { get; private set; }

        [ColumnConfig(sortLabel ="攻撃力", columnWidth = 20, preferExcel = 1)]
        [Key(5)]
        public int Attack { get; private set; }
        
#if UNITY_EDITOR
        public override void Draw()
        {
            base.Draw();
            GUILayout.Space(10);
            
            EditorGUILayout.LabelField("ユニット設定");
            GUILayout.Space(5);
            Hp = EditorGUILayout.IntField("基礎HP", Hp, GUILayout.Width(200));
            Attack = EditorGUILayout.IntField("基礎攻撃力", Attack, GUILayout.Width(200));
        }
#endif
    }
}