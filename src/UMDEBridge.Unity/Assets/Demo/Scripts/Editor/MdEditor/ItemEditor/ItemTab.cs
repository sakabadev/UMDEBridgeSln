using System;
using System.Linq;
using Demo.Scripts.Master.Item.Model;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Demo.Scripts.Editor.MdEditor.ItemEditor
{
    public class ItemTab : MdItemEditorTab<Item>
    {
        ItemType selectedItemType;
        
        public ItemTab(MdItemEditor editor) : base(editor)
        {
        }
        
        public override void OnTabSelected()
        {
            CreateMainList();
        }
        
        void CreateMainList()
        {
            var master = MdEditorBase.UseCase.GetMemoryDatabase();

            MainItemList = master.ItemTable.FindByType(selectedItemType).OrderBy(x => x.Id).ToList();
            MainReorderableList = new ReorderableList(MainItemList, typeof(Item), false, true, false, false);
            // ヘッダーの描画設定
            MainReorderableList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Item");
            };
            // エレメントの描画設定
            MainReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), MainItemList[index].Name);
            };
            // 要素を選択した時
            MainReorderableList.onSelectCallback = (ReorderableList l) =>
            {
                SelectedItem = MainItemList[MainReorderableList.index];
            };
        }

        public override void Draw()
        {
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Import", GUILayout.Width(140), GUILayout.Height(28)))
                    MdEditorBase.UseCase.ImportTable<Item>(() =>
                        {
                            SelectedItem = null;
                            CreateMainList();
                        });

                if (GUILayout.Button("Export", GUILayout.Width(140), GUILayout.Height(28)))
                    MdEditorBase.UseCase.ExportTable<Item>();  // 4
            }

            DrawTypeSelector();
            
            using (new GUILayout.HorizontalScope())
            {
                // 左側
                using (new GUILayout.VerticalScope(GUILayout.Width(160)))
                {
                    MainLeftScrollPos = EditorGUILayout.BeginScrollView(MainLeftScrollPos, GUI.skin.box);

                    if (MainReorderableList == null)
                    {
                        CreateMainList();
                    }
                    MainReorderableList?.DoLayoutList();

                    EditorGUILayout.EndScrollView();
                }

                if (SelectedItem != null)
                {
                    // 真ん中
                    using (new GUILayout.VerticalScope())
                    {
                        MainCenterScrollPos = EditorGUILayout.BeginScrollView(MainCenterScrollPos, GUI.skin.box);
                        using (new GUILayout.VerticalScope()) {
                            SelectedItem?.Draw();
                        }
                        GUILayout.Space(10);
                        EditorGUILayout.EndScrollView();
                    }
                }
            }
        }

        void DrawTypeSelector() {
            GUILayout.Space(8);
            // タイプの選択を切り替えたらリストを作り直し
            var prevType = selectedItemType;
            EditorGUILayout.LabelField($"{nameof(ItemType)}:");
            selectedItemType = (ItemType)GUILayout.Toolbar((int)selectedItemType, Enum.GetNames(typeof(ItemType)), GUILayout.MaxWidth(500), GUILayout.Height(40));
            if (selectedItemType != prevType)
            {
                SelectedItem = null;
                CreateMainList();
            }
        }
    }
}