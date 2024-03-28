using System.IO;
using Demo.Scripts.Domain.Repositories;
using Demo.Scripts.Master.Item.Model;
using Demo.Scripts.Master.Item2;
using MD;
using MD.Tables;
using UnityEditor;
using UnityEngine;

namespace Demo.Scripts.MasterRepository {
	public sealed class FileMemoryDatabaseRepository : IMemoryDatabaseRepository {
		private const string ResourcesPath = "Demo/Resources/";
		private const string DatabaseName = "MD";
		static string WriteFilePath => Path.Combine(Application.dataPath, ResourcesPath, DatabaseFilename);
		static string DatabaseFilename => $"{DatabaseName}.bytes";

		// Editorで編集時に、このキャッシュに溜まっているItem等のインスタンスを参照して書き換えます。
		private MemoryDatabase _cache;
		
#if UNITY_EDITOR
		private bool _isDirty;
		
		public void ClearCache() {
			_cache = null;
			_isDirty = false;
		}
#else
		public void ClearCache() {
			_cache = null;
		}
#endif

		public MemoryDatabase Find() {
			if (_cache != null) {
				Debug.Log($"[Find] from cache.");
				return _cache;
			}
			Debug.Log($"[Find] from {WriteFilePath}");
			
			TextAsset ta;
#if UNITY_EDITOR
			// Scriptのリビルド直後など、ResourcesだとAssetが読み込めないパターンが何回かあったためEditorではこちらで読み込む。
			ta = AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/{ResourcesPath}{DatabaseFilename}");
#else
            ta = Resources.Load<TextAsset>(DatabaseName);
#endif
			
			if (ta == null) {
#if UNITY_EDITOR
				UpdateCache(CreateMD());
#else
				return null;
#endif
			}
			else {
				_cache = new MemoryDatabase(ta.bytes);
			}
			return _cache;
		}
		
#if UNITY_EDITOR
		public void UpdateCache(MemoryDatabase md) {
			_cache = md;
			_isDirty = true;
		}
		
		public void Save() {
			if (_cache == null) {
				_cache = CreateMD();
			}

			if (!_isDirty) {
				Debug.Log($"[Save] まだdirtyではありません。Saveが必要な場合は先にUpdateCache()でキャッシュを更新してください。");
				return;
			}
			
			var builder = _cache.ToDatabaseBuilder();
			Debug.Log($"[Save] to {WriteFilePath}");
			using (var fs = new FileStream(WriteFilePath, FileMode.OpenOrCreate, FileAccess.Write)) {
				builder.WriteToStream(fs);
			}

			_isDirty = false;
			AssetDatabase.Refresh();
		}

		MemoryDatabase CreateMD() {
			Debug.Log($"[CreateMD] create new MD");
			return new MemoryDatabase(
				new ItemTable(new Item[] {}),
				new Item2Table(new Item2[] {}),
				new ItemTierTable(new ItemTier[] {}));
		}
#endif
	}
}