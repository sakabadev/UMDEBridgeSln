using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using MessagePack;
using MySql.Data.MySqlClient;
using UMDEBridge.Editor.Helper;
using UMDEBridge.Editor.Settings.Model;
using UnityEngine;

namespace UMDEBridge.Editor.Repository {
	public sealed class MySqlMemoryTableRepository : IMemoryTableRepository {
		public MySqlMemoryTableRepository(MySqlSettings settings) {
			this.Settings = settings;
		}

		private MySqlSettings Settings { get; }

		public void SaveAll<T>(List<T> items) {
			MySqlHelpers.ExecuteAddColumnIfNotExistQuery<T>(Settings);
			foreach (T item in items)
				Save(item);
		}

		void Save<T>(T item) {
			var (insert, update, sqlParams) = GetInsertAndUpdateQuery(item);
			MySqlHelpers.Execute(insert, update, Settings, sqlParams.ToArray());
		}
		
		(string insert, string update, List<MySqlParameter> sqlParams) GetInsertAndUpdateQuery<T>(T item) {
			// Unionで派生しているfieldだった場合にunionの形式にjsonを作り変えるためのdic;
			Dictionary<Type, Dictionary<Type, int>> unionDic = new Dictionary<Type, Dictionary<Type, int>>();
			// Unionで派生していないからチェックがいらないリスト
			List<Type> ignoreUnionCheckList = new List<Type>();

			Type thisType = item.GetType();

			List<MySqlParameter> sqlParams = new List<MySqlParameter>();
			
#if UMDEBRIDGE_USE_FIELD
			var keyDic = FieldHelpers.GetFieldNamesFrom<T>();
#else
			var keyDic = PropertyHelpers.GetPropertyNamesFrom<T>();
#endif
			
			string[] names = keyDic[thisType].NameList.ToArray();

			StringBuilder valuesBuilder = new StringBuilder();
			StringBuilder updateValuesBuilder = new StringBuilder();
			var reg = new Regex("^idx$|^id$", RegexOptions.IgnoreCase);
			
			// シナリオとして、MemoryTableのClassのフィールドをそれぞれMessagePackでjson化し、それをDBに保存します。
			foreach (var n in names) {
#if UMDEBRIDGE_USE_FIELD
				// シリアライズ対象のフィールドはキャメルケースになっている前提。
				string fieldName = n.ToCamelCase();
				FieldInfo gotInfo = thisType.GetField(fieldName);
				object gotValue = gotInfo.GetValue(item);
#else
				string propName = n.ToPascalCase();
				PropertyInfo gotInfo = thisType.GetProperty(propName);
				object gotValue = gotInfo.GetValue(item);
#endif
				
				var json = MessagePackSerializer.SerializeToJson(gotValue);
				json = json.TrimStart('"').TrimEnd('"');
				// boolをtinyintに入れる場合、0と1にして入れる
				// TODO AddColumnTypeがtinyintかどうか調べてからこのチェックをする
				if (json.Equals("false")) json = "0";
				if (json.Equals("true")) json = "1";

				if (gotValue != null) {
#if UMDEBRIDGE_USE_FIELD
					Type gotType = gotInfo.FieldType;
#else
					Type gotType = gotInfo.PropertyType;
#endif
					// union check
					if (!unionDic.ContainsKey(gotType)
					    && !ignoreUnionCheckList.Contains(gotType)) {
						var unions =
							(UnionAttribute[])Attribute.GetCustomAttributes(gotType, typeof(UnionAttribute));
						if (unions.Length > 0)
							unionDic.Add(gotType, unions.ToDictionary(x => x.SubType, x => x.Key));
						else
							ignoreUnionCheckList.Add(gotType);
					}

					// このフィールドがUnionの一つだったら、Unionの型に整形する
					if (unionDic.TryGetValue(gotType, out var value)) {
						Debug.Log(gotType.Name);
						Debug.Log(gotValue.GetType().Name);
						int union = value[gotValue.GetType()];
						json = $"[{union}, {json}]";
					}
				}

				valuesBuilder.Append($"@{n}");

				if (!reg.IsMatch(n))
					updateValuesBuilder.Append($"{n} = @{n}");

				sqlParams.Add(new MySqlParameter($"@{n}", $"{json}"));

				if (n != names[^1]) {
					valuesBuilder.Append(", ");
					if (!reg.IsMatch(n))
						updateValuesBuilder.Append(", ");
				}
			}

			string tableName = EditorHelpers.GetTableNameFrom(thisType);

			string insert = $@"
INSERT INTO {tableName} ({string.Join(",", names).TrimEnd(',')})
VALUES ({valuesBuilder.ToString()});
";

			string update = $@"
UPDATE {tableName}
SET {updateValuesBuilder.ToString()}
WHERE id = @id;
";
			return (insert, update, sqlParams);
		}

		public IReadOnlyList<T> FindAll<T>() {
			List<T> items = new List<T>();
			if (typeof(T).IsAbstract) {
#if UMDEBRIDGE_USE_FIELD
				var keyDic = FieldHelpers.GetFieldNamesFrom<T>();
#else
				var keyDic = PropertyHelpers.GetPropertyNamesFrom<T>();
#endif
				foreach (var key in keyDic.Keys) {
					items.AddRange(FindByTypeIsAbstract<T>(key));
				}
			}
			else {
				items.AddRange(FindByType<T>());
			}

			return items.AsReadOnly();
		}

		IReadOnlyList<T> FindByType<T>() {
			var type = typeof(T);
			string sql = $@"SELECT * FROM {EditorHelpers.GetTableNameFrom(type)} WHERE disable IS NULL OR disable=0;";
			Debug.Log(sql);
			
			// MessagePackObjectを調べて、Keyを使ってるかどうか確認する。
			// なお、Keyがstringで定義されているケースは未対応とする。
			var mpo = (MessagePackObjectAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(MessagePackObjectAttribute));
			bool isKeyAsPropertyName = mpo.KeyAsPropertyName;
			
			return MySqlHelpers.Query(sql, Settings,null,
				reader => {
					StringBuilder jsonBuilder = null;
					if (isKeyAsPropertyName) {
						// json
#if UMDEBRIDGE_USE_FIELD
						var fieldNameDic = FieldHelpers.GetFieldNamesFrom<T>(false);
						var tableNameDic = FieldHelpers.GetFieldNamesFrom<T>();
#else
						var fieldNameDic = PropertyHelpers.GetPropertyNamesFrom<T>(false);
						var tableNameDic = PropertyHelpers.GetPropertyNamesFrom<T>();
#endif
						jsonBuilder = ToJson(reader, type, fieldNameDic, tableNameDic);
					}
					else {
						// array
#if UMDEBRIDGE_USE_FIELD
						var fieldNameDic = FieldHelpers.GetFieldNamesWithKeyFrom<T>(false);
						var tableNameDic = FieldHelpers.GetFieldNamesWithKeyFrom<T>();
#else
						var fieldNameDic = PropertyHelpers.GetPropertyNamesWithKeyFrom<T>(false);
						var tableNameDic = PropertyHelpers.GetPropertyNamesWithKeyFrom<T>();
#endif
						jsonBuilder = ToArray(reader, type, fieldNameDic, tableNameDic);
					}

					// jsonへ加工おわり
					Debug.Log(jsonBuilder.ToString());
					var temp2 = MessagePackSerializer.ConvertFromJson(jsonBuilder.ToString());
					return MessagePackSerializer.Deserialize<T>(temp2);
				});
		}

		IReadOnlyList<T> FindByTypeIsAbstract<T>(Type type) {
			if (!type.IsSubclassOf(typeof(T))) {
				Debug.LogWarning($"誤ったタイプです。 {type.FullName}");
				return new List<T>().AsReadOnly();
			}

			// Union番号を特定
			var unions = (UnionAttribute[])Attribute.GetCustomAttributes(typeof(T), typeof(UnionAttribute));
			var unionPair = unions.ToDictionary(x => x.SubType.Name, x => x.Key);
			int union = unionPair[type.Name];

			string sql = $@"SELECT * FROM {EditorHelpers.GetTableNameFrom(type)} WHERE disable IS NULL OR disable = 0;";
			Debug.Log(sql);
			
			// MessagePackObjectを調べて、Keyを使ってるかどうか確認する。
			// なお、Keyがstringで定義されているケースは未対応とする。
			var mpo = (MessagePackObjectAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(MessagePackObjectAttribute));
			bool isKeyAsPropertyName = mpo.KeyAsPropertyName;

			return MySqlHelpers.Query(sql, Settings, null,
				reader => {
					// 加工開始
					StringBuilder jsonBuilder = new StringBuilder();
					string json;
					if (isKeyAsPropertyName) {
						// json
#if UMDEBRIDGE_USE_FIELD
						var fieldNameDic = FieldHelpers.GetFieldNamesFrom<T>(false);
						var tableNameDic = FieldHelpers.GetFieldNamesFrom<T>();
#else
						var fieldNameDic = PropertyHelpers.GetPropertyNamesFrom<T>(false);
						var tableNameDic = PropertyHelpers.GetPropertyNamesFrom<T>();
#endif
						json = ToJson(reader, type, fieldNameDic, tableNameDic)?.ToString();
					}
					else {
						// array
#if UMDEBRIDGE_USE_FIELD
						var fieldNameDic = FieldHelpers.GetFieldNamesWithKeyFrom<T>(false);
						var tableNameDic = FieldHelpers.GetFieldNamesWithKeyFrom<T>();
#else
						var fieldNameDic = PropertyHelpers.GetPropertyNamesWithKeyFrom<T>(false);
						var tableNameDic = PropertyHelpers.GetPropertyNamesWithKeyFrom<T>();
#endif
						json = ToArray(reader, type, fieldNameDic, tableNameDic)?.ToString();
					}

					jsonBuilder.Append($"[{union},{json}]");
					Debug.Log(jsonBuilder.ToString());
					var temp2 = MessagePackSerializer.ConvertFromJson(jsonBuilder.ToString());
					return MessagePackSerializer.Deserialize<T>(temp2);
				});
		}


		StringBuilder ToArray(MySqlDataReader reader, Type type, Dictionary<Type, IntKeyFieldNameResponse> fieldNameDic,
			Dictionary<Type, IntKeyFieldNameResponse> tableNameDic) {
			StringBuilder builder = new StringBuilder();
			// 1. フィールド名とMessagePackのKey番号が一致しているリストを用意
			// 2. stringのリストを用意し、カラム名とフィールド名が一致している場所に値を挿入
			// 3. 配列の見た目に整える
			builder.Append("[");
			var valueList = new List<string>();
			var tableNames = tableNameDic[type].NameList.ToArray();
			int i = 0;
			foreach (var (key, name) in fieldNameDic[type].NameList) {
				try {
					var value = reader[tableNames[i].Item2];
					var str = ToValidString(value);
					// Debug.Log($"table name is {name}, key:{key} value:{str}");
					// リストがkeyよりも小さかったらリストを伸ばす
					if (valueList.Count <= key) {
						int count = key - valueList.Count + 1;
						for (int j = 0; j < count; j++) valueList.Add("null");
					}

					valueList[key] = $"{str}";
				}
				catch (Exception e) {
					Debug.LogException(e);
					Debug.LogWarning($"key:{key} or name:{name} is Not Found");
				}

				i++;
			}

			builder.Append(string.Join(",", valueList));
			builder.Append("]");
			return builder;
		}

		StringBuilder ToJson(MySqlDataReader reader, Type type,
			Dictionary<Type, StringKeyFieldNameResponse> fieldNameDic,
			Dictionary<Type, StringKeyFieldNameResponse> tableNameDic) {
			// jsonへ加工開始
			StringBuilder jsonBuilder = new StringBuilder();
			jsonBuilder.Append("{");
			var tableNames = tableNameDic[type].NameList.ToArray();
			int i = 0;
			foreach (var name in fieldNameDic[type].NameList) {
				try {
					var value = reader[tableNames[i]];
					var str = ToValidString(value);
					jsonBuilder.Append($"\"{name}\" : {str},");
				}
				catch (Exception e) {
					Debug.LogException(e);
					Debug.LogWarning($"{tableNames[i]} is Not Found");
				}

				i++;
			}

			jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
			jsonBuilder.Append("}");
			return jsonBuilder;
		}

		object ToValidString(object value) {
			if (value is string str) {
				if (!(str.Trim().StartsWith("[") || str.Trim().StartsWith("{"))
				    && !str.Trim().StartsWith("\""))
					value = $"\"{str}\"";
				if (string.IsNullOrWhiteSpace(str) || str == "\"null\"" || str == "null")
					value = "null";
			}
			else if (value is bool flag) {
				value = flag ? "true" : "false";
			}
			else if (value == DBNull.Value) {
				value = "null";
			}

			return value;
		}
	}
}