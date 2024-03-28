namespace UMDEBridge.Editor.Settings.Repository {
	public interface IUMDEBridgeSettingsRepository {
		void ClearCache();
		UMDEBridgeSettingsAsset Find();
	}
}