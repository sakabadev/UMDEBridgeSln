using System;
using System.Collections.Generic;
using UnityEngine;

namespace UMDEBridge.Editor.Helper {
	public static class TypeExtensions {
		/// <summary>
		/// Typeを継承しているクラスを探索する
		/// </summary>
		/// <param name="aAppDomain"></param>
		/// <param name="aType"></param>
		/// <returns></returns>
		public static Type[] GetAllDerivedTypes(this AppDomain aAppDomain, Type aType) {
			var result = new List<Type>();
			var assemblies = aAppDomain.GetAssemblies();
			foreach (var assembly in assemblies) {
				// assemblyがMySql.Dataの時などにGetTypesに失敗したので、一部は無視する
				// 多分、MySql Connectorをインポートした時にエラーが出なかったDllはプロジェクトに入れなかったからかな？
				try {
					var types = assembly.GetTypes();
					foreach (var type in types) {
						if (type.IsSubclassOf(aType))
							result.Add(type);
					}
				}
				catch (Exception e) {
					Debug.Log($"{assembly.FullName} failed GetTypes()");
				}
			}
			return result.ToArray();
		}
	}
}