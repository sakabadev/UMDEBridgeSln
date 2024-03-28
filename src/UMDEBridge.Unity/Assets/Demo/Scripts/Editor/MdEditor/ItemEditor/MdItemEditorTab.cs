using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace Demo.Scripts.Editor.MdEditor.ItemEditor
{
    public class MdItemEditorTab<T> : IMdEditorTab where T : class
    {
        protected List<T> MainItemList = null;
        protected T SelectedItem = null;
        protected ReorderableList MainReorderableList;
        
        protected Vector2 MainLeftScrollPos = Vector2.zero;
        protected Vector2 MainCenterScrollPos = Vector2.zero;
        
        protected MdItemEditor ParentEditor;

        public MdItemEditorTab(MdItemEditor editor)
        {
            ParentEditor = editor;
        }

        public virtual void OnTabSelected()
        {
        }

        public virtual void Draw()
        {
        }
    }
}