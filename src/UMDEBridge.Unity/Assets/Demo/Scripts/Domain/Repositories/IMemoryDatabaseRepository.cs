using MD;

namespace Demo.Scripts.Domain.Repositories {
	public interface IMemoryDatabaseRepository
    {
	    void ClearCache();
        MemoryDatabase Find();
#if UNITY_EDITOR
	    // MemoryDatabaseの更新を行うのはUnityEditor上のみ
	    void UpdateCache(MemoryDatabase updateDatabase);
        void Save();
#endif
    }
}