using System;

namespace UMDEBridge.Editor.Settings.Model {
	/// <summary>
	/// ExcelAddinで使う設定ファイルと同じ構造を持つようにしています。
	/// </summary>
	[Serializable]
	public sealed class Settings {
		public MySqlSettings mySqlSettings = new MySqlSettings();
		public ColumnTypeMapping columnTypeMapping = new ColumnTypeMapping();
	}
}