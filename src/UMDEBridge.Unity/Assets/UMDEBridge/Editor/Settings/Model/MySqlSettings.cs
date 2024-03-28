using System;

namespace UMDEBridge.Editor.Settings.Model {
	[Serializable]
	public sealed class MySqlSettings {
		public string Server = "127.0.0.1"; // localhostは指定できない。Windowsの場合、名前解決に時間がかかったり、上手く解決出来なかったりするため。
		public string Port = "3306";
		public string Database = "umdebridge-demo-db";
		public string User = "root";
		public string Pass = "root";
		public string Charset = "utf8";
		
		// Databaseは存在確認を行うため別で分けています。
		public string ConnectionCommand => $"server={Server};port={Port};userid={User};password={Pass};charset={Charset};";
	}
}