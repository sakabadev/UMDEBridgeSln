using System;
using System.Collections.Generic;
using System.Reflection;
using MessagePack;
using UMDEBridge.Annotations;

namespace UMDEBridge.Editor.Helper {
	public static class FieldHelpers {
		/// <summary>
		/// Typeが持つフィールド名を取得します。
		/// ポリモーフィズムを使用している基底タイプだった場合にDictionaryのカウントが増えます。
		/// 
		/// シンプルにTypeからフィールド名が欲しい場合と、
		/// isKeyAsPropertyName: trueのシリアライズで使用を想定。
		/// </summary>
		internal static Dictionary<Type, StringKeyFieldNameResponse> GetFieldNamesFrom<T>(bool toSnakeCase = true)
		{
			var keyDic = new Dictionary<Type, StringKeyFieldNameResponse>();
			var t = typeof(T);
			if (t.IsAbstract)
			{
				var list = EditorHelpers.GetDerivedTypeList<T>();
				// Typeのフィールド名を特定
				foreach (var type in list)
					keyDic.Add(type, GetFieldNameList(type, toSnakeCase));
			}
			else
			{
				keyDic.Add(t, GetFieldNameList(t, toSnakeCase));
			}
			return keyDic;
		}
        
		/// <summary>
		/// Typeが持つフィールド名を取得します。
		/// ポリモーフィズムを使用している基底タイプだった場合にDictionaryのカウントが増えます。
		/// 
		/// フィールドにKeyAttributeが設定されている事を想定。
		/// </summary>
		internal static Dictionary<Type, IntKeyFieldNameResponse> GetFieldNamesWithKeyFrom<T>(bool toSnakeCase = true)
		{
			var keyDic = new Dictionary<Type, IntKeyFieldNameResponse>();
			var t = typeof(T);
			if (t.IsAbstract)
			{
				var list = EditorHelpers.GetDerivedTypeList<T>();
				// Typeのフィールド名を特定
				foreach (var type in list)
					keyDic.Add(type, GetFieldNameListWithKey(type, toSnakeCase));
			}
			else
			{
				keyDic.Add(t, GetFieldNameListWithKey(t, toSnakeCase));
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
		static StringKeyFieldNameResponse GetFieldNameList(Type t, bool toSnakeCase = true)
		{
			List<string> result = new();
			foreach (var field in t.GetFields(BindingFlags.Instance | BindingFlags.Public))
			{
				IgnoreMemberAttribute ignoreAttr = (IgnoreMemberAttribute)Attribute.GetCustomAttribute(field, typeof(IgnoreMemberAttribute));
				// [IgnoreMember]じゃなく、publicなフィールドのみDBに入れる
				if (ignoreAttr == null)
					result.Add(
						toSnakeCase ? field.Name.ToSnakeCase() : field.Name);
			}
			return new StringKeyFieldNameResponse(result);
		}
        
		/// <summary>
		/// Typeが持つフィールド名を取得します。
		/// 具象クラスであることが前提。
		/// 
		/// フィールドにKeyAttributeが設定されている事を想定。
		/// </summary>
		static IntKeyFieldNameResponse GetFieldNameListWithKey(Type t, bool toSnakeCase = true)
		{
			var result = new List<(int, string)>();
			foreach (var field in t.GetFields(BindingFlags.Instance | BindingFlags.Public))
			{
				var ignoreAttr = (IgnoreMemberAttribute)Attribute.GetCustomAttribute(field, typeof(IgnoreMemberAttribute));
				// [IgnoreMember]じゃなく、publicなフィールドのみDBに入れる
				if (ignoreAttr == null)
				{
					var key = (KeyAttribute)Attribute.GetCustomAttribute(field, typeof(KeyAttribute));
					if (key == null)
					{
						throw new NotImplementedException($"[Key]が設定されていません。 {field.Name}");
					}

					int keyNum = -1;
					foreach (var prop in typeof(KeyAttribute).GetProperties())
					{
						if(prop.Name != "IntKey") continue;
						keyNum = (int)prop.GetValue(key);
					}
					result.Add(
						(keyNum, toSnakeCase ? field.Name.ToSnakeCase() : field.Name));
				}
			}
			return new IntKeyFieldNameResponse(result);
		}

		internal static IReadOnlyDictionary<string, List<(string, string[])>> GetFieldNamesAndTypesFrom<T>()
		{
			var res = new Dictionary<string, List<(string, string[])>>();
			var t = typeof(T);
			if (t.IsAbstract)
			{
				var list = EditorHelpers.GetDerivedTypeList<T>();
				// Typeのフィールド名を特定
				foreach (var type in list)
				{
					string tableName = EditorHelpers.GetTableNameFrom(type);
					res.Add(tableName, GetFieldNamesAndTypesFrom(type));
				}
			}
			else
			{
				res.Add(EditorHelpers.GetTableNameFrom(t), GetFieldNamesAndTypesFrom(t));
			}
			return res;
		}

		static List<(string, string[])> GetFieldNamesAndTypesFrom(Type t)
		{
			List<(string, string[])> res = new();
			foreach (var field in t.GetFields(BindingFlags.Instance | BindingFlags.Public))
			{
				AddColumnTypeAttribute attr =
					(AddColumnTypeAttribute) Attribute.GetCustomAttribute(field,
						typeof(AddColumnTypeAttribute));
				// [IgnoreMember]じゃなく、publicなフィールドのみDBに入れる
				if (attr != null)
					res.Add((field.Name.ToSnakeCase(), attr.GetMembers()));
			}
			return res;
		}
	}
}