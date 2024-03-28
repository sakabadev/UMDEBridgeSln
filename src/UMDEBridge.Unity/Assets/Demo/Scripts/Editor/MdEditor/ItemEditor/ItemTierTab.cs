using System.Collections.Generic;
using Demo.Scripts.Master.Item.Model;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Demo.Scripts.Editor.MdEditor.ItemEditor
{
    public class ItemTierTab : MdItemEditorTab<ItemTier>
    {
        public ItemTierTab(MdItemEditor editor) : base(editor)
        {
        }
        
        public override void OnTabSelected()
        {
            CreateMainList();
        }
        
        void CreateMainList()
        {
            var master = MdEditorBase.UseCase.GetMemoryDatabase();
            MainItemList = new List<ItemTier>(master.ItemTierTable.All);

            MainReorderableList = new ReorderableList(MainItemList, typeof(ItemTier), false, true, false, false);
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
                    MdEditorBase.UseCase.ImportTable<ItemTier>(() =>
                        {
                            SelectedItem = null;
                            CreateMainList();
                        });

                if (GUILayout.Button("Export", GUILayout.Width(140), GUILayout.Height(28)))
                    MdEditorBase.UseCase.ExportTable<ItemTier>();  // 4
            }

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
    }
}