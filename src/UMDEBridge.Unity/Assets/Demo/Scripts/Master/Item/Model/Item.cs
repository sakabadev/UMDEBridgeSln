using System.Collections.Generic;
using System.Linq;
using MasterMemory;
using MessagePack;
using UMDEBridge.Annotations;
using UnityEditor;
using UnityEngine;

namespace Demo.Scripts.Master.Item.Model {
    public enum ItemType
    {
        Unit,
        Equip
    }
    
    [Union(0, typeof(UnitItem))]
    [Union(1, typeof(EquipItem))]
    [MemoryTable(nameof(Item)), MessagePackObject(true)]
	public abstract class Item
    {
        protected Item(string id, string name, string text, string icon) {
            Id = id;
            Name = name;
            Text = text;
            Icon = icon;
        }

        [PrimaryKey]
        [ColumnConfig(sortLabel = "キー", columnWidth = 20, preferExcel = 1)]
        public string Id { get; private set; }

        [ColumnConfig(sortLabel="名前", preferExcel = 1)]
        public string Name { get; private set; }
        
        [ColumnConfig(sortLabel = "説明", columnWidth = 200, preferExcel = 1)]
        public string Text { get; private set; }
        
        [ColumnConfig(sortLabel = "アイコンパス")]
        public string Icon { get; private set; }

        [SecondaryKey(0), NonUnique, IgnoreMember]
        public abstract ItemType Type { get; }

#if UNITY_EDITOR
        public virtual void Draw()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box, GUILayout.MaxWidth(500)))
            {
                EditorGUILayout.LabelField("id", Id);
                Name = EditorGUILayout.TextField("名前", Name);
                Text = EditorGUILayout.TextField("説明", Text);
                Icon = EditorGUILayout.TextField("アイコンパス", Icon);
            }
        }
#endif
    }

    [MessagePackObject(true)]
    public class UnitItem : Item
    {
        public UnitItem(string id, string name, string text, string icon, int hp, int attack) : base(id, name, text, icon) {
            Hp = hp;
            Attack = attack;
        }

        [ColumnConfig(sortLabel ="HP", columnWidth = 20, preferExcel = 1)]
        public int Hp { get; private set; }
        
        [ColumnConfig(sortLabel ="攻撃力", columnWidth = 20, preferExcel = 1)]
        public int Attack { get; private set; }
        
        [IgnoreMember]
        public override ItemType Type => ItemType.Unit;

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
    
    [MessagePackObject(true)]
    public class EquipItem : Item
    {
        public EquipItem(string id, string name, string text, string icon, StatModifier.StatModifier[] modifiers) : base(id, name, text, icon) {
            Modifiers = modifiers;
        }

        [ColumnConfig(sortLabel ="Stat変更Objリスト", columnWidth = 200)]
        public StatModifier.StatModifier[] Modifiers { get; private set; }
        
        [IgnoreMember]
        public override ItemType Type => ItemType.Equip;
        
#if UNITY_EDITOR
        public override void Draw()
        {
            base.Draw();
            GUILayout.Space(10);
            
            EditorGUILayout.LabelField("装備設定");
            GUILayout.Space(5);
            
            var mods = Modifiers?.ToList();
            if(mods == null)
                mods = new List<StatModifier.StatModifier>();

            using (new GUILayout.VerticalScope())
            {
                if (GUILayout.Button("Add"))
                    mods.Add(new StatModifier.StatModifier());
                GUILayout.Space(8);

                for (int i = mods.Count - 1; i >= 0; i--)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("X"))
                        {
                            mods.RemoveAt(i);
                            continue;
                        }

                        GUILayout.Space(8);

                        mods[i].Draw();
                        GUILayout.Space(8);
                    }
                }
            }

            Modifiers = mods.ToArray();
        }
#endif
    }
}