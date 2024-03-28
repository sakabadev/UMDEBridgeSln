using UnityEngine;

namespace UMDEBridge.Editor.Settings.Repository {
	[CreateAssetMenu(fileName = "UMDEBridgeSettingsAsset", menuName = "UMDEBridge/Create Settings", order = 0)]
	public sealed class UMDEBridgeSettingsAsset : ScriptableObject {
		public Model.Settings settings = new Model.Settings();
	}
}