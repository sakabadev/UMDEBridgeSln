using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Demo.Scripts.Domain.Repositories;
using Demo.Scripts.Master.Item.Model;
using Demo.Scripts.Master.Item2;
using Demo.Scripts.MasterRepository;
using MasterMemory;
using MD;
using MessagePack;
using UMDEBridge.Editor.Repository;
using UMDEBridge.Editor.Settings.Model;
using UnityEngine;

namespace Demo.Scripts.Editor.MdEditor {
	public class MdEditorUseCase {
		private readonly IMemoryDatabaseRepository mdRepository;
		private readonly IMemoryTableRepository tableRepository;

		public MdEditorUseCase(MySqlSettings mySqlSettings) : this(new FileMemoryDatabaseRepository(),
			new MySqlMemoryTableRepository(mySqlSettings)) { }

		public MdEditorUseCase(
			IMemoryDatabaseRepository mdRepository,
			IMemoryTableRepository tableRepository) {
			this.mdRepository = mdRepository;
			this.tableRepository = tableRepository;
		}

		/// <summary>
		/// MySQL等のDBにキャッシュしてあるマスターデータを読み込み、Unityで保存しているデータを上書きします。
		/// Unity上でセーブしてDBに保存していないデータは上書きされるので注意。
		/// </summary>
		/// <param name="onComplete"></param>
		/// <typeparam name="T">マスターデータテーブルとなるモデル。IIdentifiableが付いていることが前提です。</typeparam>
		/// <exception cref="Exception"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public async void ImportTable<T>(System.Action onComplete) {
			if (typeof(T).GetCustomAttribute<MemoryTableAttribute>() == null)
				throw new InvalidOperationException($"{nameof(T)}にはMemoryTableAttributeが付いている必要があります。");
			
			Debug.Log($"[{nameof(ImportTable)}] Start");

			// DBからテーブル情報を取得
			var items = tableRepository.FindAll<T>();
			if (items == null || !items.Any())
				throw new Exception($"{typeof(T).Name}はDBに登録された情報がありません。");
			
			Debug.Log(MessagePackSerializer.SerializeToJson(items));

			// Unityに読み込んであるテーブル情報を引き出す
			var db = mdRepository.Find();
			var builder = db.ToImmutableBuilder();
			ReplaceTableItems(items, db, builder);

			// Unityでキャッシュしているデータを上書き保存。
			mdRepository.UpdateCache(builder.Build());
			mdRepository.Save();

			onComplete.Invoke();
			Debug.Log($"[{nameof(ImportTable)}] End");
		}

		private void ReplaceTableItems<T>(IReadOnlyList<T> items, MemoryDatabase db, ImmutableBuilder builder) {
			switch (typeof(T)) {
				case Type t when t == typeof(Item): {
					var castedList = items.Cast<Item>().ToArray();
					var excepts = db.ItemTable.All.Select(x => x.Id).ToArray();
					excepts = excepts.Except(castedList.Select(x => x.Id).ToArray()).ToArray();
					// DBから引き出したテーブルを真とし、UnityのテーブルにないIdのインスタンスを削除
					builder.RemoveItem(excepts);
					// データ差し替え
					builder.ReplaceAll(castedList.ToArray());
				}
					break;
				case Type t when t == typeof(ItemTier): {
					var castedList = items.Cast<ItemTier>().ToArray();
					var excepts = db.ItemTierTable.All.Select(x => x.Id).ToArray();
					excepts = excepts.Except(castedList.Select(x => x.Id).ToArray()).ToArray();
					builder.RemoveItem(excepts);
					builder.ReplaceAll(castedList.ToArray());
				}
					break;
				case Type t when t == typeof(Item2): {
					var castedList = items.Cast<Item2>().ToArray();
					var excepts = db.Item2Table.All.Select(x => x.Id).ToArray();
					excepts = excepts.Except(castedList.Select(x => x.Id).ToArray()).ToArray();
					builder.RemoveItem(excepts);
					builder.ReplaceAll(castedList.ToArray());
				}
					break;
				default:
					throw new InvalidOperationException($"({typeof(T).Name})は未作成のタイプです。");
			}
		}

		/// <summary>
		/// Unity上でセーブボタンでセーブした状態のマスターデータを、DBに保存します。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <exception cref="InvalidOperationException"></exception>
		public void ExportTable<T>() {
			if (typeof(T).GetCustomAttribute<MemoryTableAttribute>() == null)
				throw new InvalidOperationException($"{nameof(T)}にはMemoryTableAttributeが付いている必要があります。");
			
			Debug.Log($"[{nameof(ExportTable)}] Start");
			MemoryDatabase db = GetMemoryDatabase();
			List<T> list = GetMemoryTableItemList<T>(db);
			
			tableRepository.SaveAll(list);
			Debug.Log($"[{nameof(ExportTable)}] End");
		}

		private List<T> GetMemoryTableItemList<T>(MemoryDatabase db) {
			
			switch (typeof(T)) {
				case Type t when t == typeof(Item):
					return db.ItemTable.All.Cast<T>().ToList();
				case Type t when t == typeof(ItemTier):
					return db.ItemTierTable.All.Cast<T>().ToList();
				case Type t when t == typeof(Item2):
					return db.Item2Table.All.Cast<T>().ToList();
				default:
					throw new InvalidOperationException($"({typeof(T).Name})は未作成のタイプです。");
			}
		}


		public MemoryDatabase GetMemoryDatabase()
			=> mdRepository.Find();

		public void SaveMemoryDatabase() {
			//NOTE: あんまり良くないコードかも。そのうち直せたら直す。
			//cache内のインスタンスは適宜書き換えられているが、下記の手順をふまないとDirtyフラグが立たないようにしているため、こうしてる。
			var db = mdRepository.Find();
			mdRepository.UpdateCache(db);
			mdRepository.Save();
		}
	}
}