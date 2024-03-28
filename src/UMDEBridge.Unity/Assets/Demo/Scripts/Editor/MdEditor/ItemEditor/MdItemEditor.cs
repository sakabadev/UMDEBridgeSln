using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Demo.Scripts.Editor.MdEditor.ItemEditor
{
    public class MdItemEditor : MdEditorBase
    {
        private readonly List<IMdEditorTab> tabs = new List<IMdEditorTab>();
        
        private int selectedTabIndex = -1;
        private int prevSelectedTabIndex = -1;
        
        [MenuItem("UMDEBridge/Demo/Master Editor/Item", false, 1)]
        private static void Init()
        {
            var window = GetWindow(typeof(MdItemEditor));
            window.titleContent = new GUIContent("Item Editor");
            window.minSize = new Vector2(800, 600);
        }

        protected override void OnEnableAfter()
        {
            tabs.Add(new ItemTab(this));
            tabs.Add(new ItemTierTab(this));
            tabs.Add(new Item2Tab(this));
            selectedTabIndex = 0;
        }

        private void OnGUI()
        {
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, tabs.Select(x => x.GetType().Name).ToArray());
            if (selectedTabIndex >= 0 && selectedTabIndex < tabs.Count)
            {
                var selectedEditor = tabs[selectedTabIndex];
                if (selectedTabIndex != prevSelectedTabIndex)
                {
                    selectedEditor.OnTabSelected();
                    GUI.FocusControl(null);
                }
                
                EditorGUI.BeginChangeCheck();

                selectedEditor.Draw();
                
                if (EditorGUI.EndChangeCheck())
                {
                    IsDirty = true;
                }
                
                prevSelectedTabIndex = selectedTabIndex;
                
                if (IsDirty && GUILayout.Button("Save", GUILayout.Width(140), GUILayout.Height(28)))
                    SetMDDirty("Standard Editor Change");
            }
        }
    }
}