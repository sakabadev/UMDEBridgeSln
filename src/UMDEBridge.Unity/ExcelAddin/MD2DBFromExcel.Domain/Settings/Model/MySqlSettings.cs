using System;

namespace MD2DBFromExcel.Domain.Settings.Model {
	public sealed class MySqlSettings {
		public string Server { get; set; } = "127.0.0.1";
		public string Port { get; set; } = "3306";
		public string Database { get; set; } = "umdebridge-demo-db";
		public string User { get; set; } = "root";
		public string Pass { get; set; } = "root";
		public string Charset { get; set; } = "utf8";
		
		// Databaseは存在確認を行うため別で分けています。
		public string ConnectionCommand => $"server={Server};port={Port};userid={User};password={Pass};charset={Charset};";
	}
}