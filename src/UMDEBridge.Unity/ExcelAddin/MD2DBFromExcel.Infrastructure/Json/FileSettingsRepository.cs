using System;
using System.IO;
using System.Management;
using System.Text.Json;
using System.Windows.Forms;
using MD2DBFromExcel.Domain.Settings.Model;
using MD2DBFromExcel.Domain.Settings.Repository;

namespace MD2DBFromExcel.Infrastructure.Json {
	/// <summary>
	/// Excelファイルと同じ階層にあるsettings.jsonから設定を読み取ります。
	/// </summary>
	public sealed class FileSettingsRepository : ISettingsRepository {
		const string SettingsFileName = "umdebridge_settings.json";
		
		private Settings _cached;
		
		public void CacheClear() {
			_cached = null;
		}
		
		public void Save(string dirPath) {
			if (_cached == null) {
				throw new InvalidOperationException("settingsファイルを一度読み込む必要があります。");
			}
            string json = JsonSerializer.Serialize(_cached);
			MessageBox.Show(Path.Combine(dirPath, SettingsFileName));
			File.WriteAllText(Path.Combine(dirPath, SettingsFileName), json);
		}

		public Settings Find(string dirPath) {
			if (_cached != null) 
				return _cached;

			if (!File.Exists(SettingsFileName)) {
				_cached = new Settings();
				Save(dirPath);
				// NOTE: _cachedを返してもいいが、正常に設定ファイルが作られたか確認の意味も込めて、ファイル読み出しに進む。
			}

            string json = File.ReadAllText(Path.Combine(dirPath, SettingsFileName));
            _cached = JsonSerializer.Deserialize<Settings>(json);
            return _cached;
		}
	}
}