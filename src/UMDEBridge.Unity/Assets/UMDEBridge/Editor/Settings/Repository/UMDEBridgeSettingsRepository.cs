using UnityEngine;

namespace UMDEBridge.Editor.Settings.Repository {
	public sealed class UMDEBridgeSettingsRepository : IUMDEBridgeSettingsRepository {
		
		UMDEBridgeSettingsAsset _cached;
		
		public void ClearCache() {
			_cached = null;
		}

		public UMDEBridgeSettingsAsset Find() {
			if (_cached != null)
				return _cached;

			_cached = Resources.Load<UMDEBridgeSettingsAsset>("UMDEBridgeSettingsAsset");
			return _cached;
		}
	}
}