namespace MD2DBFromExcel.Domain.Settings.Repository {
	public interface ISettingsRepository {
		void CacheClear();
		void Save(string dirPath);
		Model.Settings Find(string dirPath);
	}
}