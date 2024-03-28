using UMDEBridge.Editor.Settings.Repository;
using UnityEditor;

namespace Demo.Scripts.Editor.MdEditor
{
    public class MdEditorBase : EditorWindow
    {
        public static bool IsDirty;
        public static MdEditorUseCase UseCase;
        
        void OnEnable() {
            if (UseCase == null) {
                var settingsRepository = new UMDEBridgeSettingsRepository();
                var settingsAsset = settingsRepository.Find();
                UseCase = new MdEditorUseCase(settingsAsset.settings.mySqlSettings);
            }
            
            OnEnableAfter();
        }

        protected virtual void OnEnableAfter()
        {
        }
        
        // NOTE: 関数名は元々ここでScriptableObjectに書き込んでた名残
        public void SetMDDirty(string reason) {
            UseCase.SaveMemoryDatabase();
            IsDirty = false;
        }
    }
}