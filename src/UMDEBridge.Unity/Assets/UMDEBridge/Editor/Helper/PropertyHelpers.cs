using System;
using System.Collections.Generic;
using System.Reflection;
using MessagePack;
using UMDEBridge.Annotations;

namespace UMDEBridge.Editor.Helper {
	public static class PropertyHelpers {
		/// <summary>
		/// Typeが持つフィールド名を取得します。
		/// ポリモーフィズムを使用している基底タイプだった場合にDictionaryのカウントが増えます。
		/// 
		/// シンプルにTypeからフィールド名が欲しい場合と、
		/// isKeyAsPropertyName: trueのシリアライズで使用を想定。
		/// </summary>
		internal static Dictionary<Type, StringKeyFieldNameResponse> GetPropertyNamesFrom<T>(bool toSnakeCase = true) {
			var keyDic = new Dictionary<Type, StringKeyFieldNameResponse>();
			var t = typeof(T);
			if (t.IsAbstract) {
				var list = EditorHelpers.GetDerivedTypeList<T>();
				// Typeのフィールド名を特定
				foreach (var type in list)
					keyDic.Add(type, GetPropertyNameList(type, toSnakeCase));
			}
			else {
				keyDic.Add(t, GetPropertyNameList(t, toSnakeCase));
			}

			return keyDic;
		}

		/// <summary>
		/// Typeが持つフィールド名を取得します。
		/// ポリモーフィズムを使用している基底タイプだった場合にDictionaryのカウントが増えます。
		/// 
		/// フィールドにKeyAttributeが設定されている事を想定。
		/// </summary>
		internal static Dictionary<Type, IntKeyFieldNameResponse> GetPropertyNamesWithKeyFrom<T>(bool toSnakeCase = true) {
			var keyDic = new Dictionary<Type, IntKeyFieldNameResponse>();
			var t = typeof(T);
			if (t.IsAbstract) {
				var list = EditorHelpers.GetDerivedTypeList<T>();
				// Typeのフィールド名を特定
				foreach (var type in list)
					keyDic.Add(type, GetPropertyNameListWithKey(type, toSnakeCase));
			}
			else {
				keyDic.Add(t, GetPropertyNameListWithKey(t, toSnakeCase));
			}

			return keyDic;
		}

		/// <summary>
		/// Typeが持つフィールド名を取得します。
		/// 具象クラスであることが前提
		/// 
		/// シンプルにTypeからフィールド名が欲しい場合と、
		/// isKeyAsPropertyName: trueのシリアライズで使用を想定。
		/// </summary>
		static StringKeyFieldNameResponse GetPropertyNameList(Type t, bool toSnakeCase = true) {
			List<string> result = new();
			foreach (PropertyInfo property in t.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
				IgnoreMemberAttribute ignoreAttr =
					(IgnoreMemberAttribute)Attribute.GetCustomAttribute(property, typeof(IgnoreMemberAttribute));
				// [IgnoreMember]じゃなく、publicなフィールドのみDBに入れる
				if (ignoreAttr == null)
					result.Add(
						toSnakeCase ? property.Name.ToSnakeCase() : property.Name);
			}

			return new StringKeyFieldNameResponse(result);
		}

		/// <summary>
		/// Typeが持つフィールド名を取得します。
		/// 具象クラスであることが前提。
		/// 
		/// フィールドにKeyAttributeが設定されている事を想定。
		/// </summary>
		static IntKeyFieldNameResponse GetPropertyNameListWithKey(Type t, bool toSnakeCase = true) {
			var result = new List<(int, string)>();
			foreach (var property in t.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
				var ignoreAttr =
					(IgnoreMemberAttribute)Attribute.GetCustomAttribute(property, typeof(IgnoreMemberAttribute));
				// [IgnoreMember]じゃなく、publicなフィールドのみDBに入れる
				if (ignoreAttr == null) {
					var key = (KeyAttribute)Attribute.GetCustomAttribute(property, typeof(KeyAttribute));
					if (key == null) {
						throw new NotImplementedException($"[Key]が設定されていません。 {property.Name}");
					}

					int keyNum = -1;
					foreach (var prop in typeof(KeyAttribute).GetProperties()) {
						if (prop.Name != "IntKey") continue;
						keyNum = (int)prop.GetValue(key);
					}

					result.Add(
						(keyNum, toSnakeCase ? property.Name.ToSnakeCase() : property.Name));
				}
			}

			return new IntKeyFieldNameResponse(result);
		}

		internal static IReadOnlyDictionary<string, List<(string, string[])>> GetPropertyNamesAndTypesFrom<T>() {
			var res = new Dictionary<string, List<(string, string[])>>();
			var t = typeof(T);
			if (t.IsAbstract) {
				var list = EditorHelpers.GetDerivedTypeList<T>();
				// Typeのフィールド名を特定
				foreach (var type in list) {
					string tableName = EditorHelpers.GetTableNameFrom(type);
					res.Add(tableName, GetPropertyNamesAndTypesFrom(type));
				}
			}
			else {
				res.Add(EditorHelpers.GetTableNameFrom(t), GetPropertyNamesAndTypesFrom(t));
			}

			return res;
		}

		static List<(string, string[])> GetPropertyNamesAndTypesFrom(Type t) {
			List<(string, string[])> res = new();
			foreach (var property in t.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
				AddColumnTypeAttribute attr =
					(AddColumnTypeAttribute)Attribute.GetCustomAttribute(property,
						typeof(AddColumnTypeAttribute));
				// [IgnoreMember]じゃなく、publicなフィールドのみDBに入れる
				if (attr != null)
					res.Add((property.Name.ToSnakeCase(), attr.GetMembers()));
			}

			return res;
		}
	}
}