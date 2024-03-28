using System;

namespace MD2DBFromExcel.Domain.Settings.Model {
	/// <summary>
	/// アプリでは、このモデルをExcelファイルと同じ階層にあるsettings.jsonから読み取ります。
	/// </summary>
	public sealed class Settings {
		public MySqlSettings mySqlSettings { get; set; } = new MySqlSettings();
	}
}