using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MasterMemory;
using UnityEngine;

namespace UMDEBridge.Editor.Helper
{
    public static class EditorHelpers
    {
        static Dictionary<Type, List<Type>> _derivedTypeCache = new();
        
        
        internal static string GetTableNameFrom(Type type)
        {
            string[] arr = type.Name.Split('.');
            return arr[^1].ToSnakeCase();
        }

        internal static IReadOnlyList<Type> GetDerivedTypeList<T>()
        {
            if (!_derivedTypeCache.ContainsKey(typeof(T)))
                CacheDerivedTypes<T>();
            return _derivedTypeCache[typeof(T)].ToList();
        }

        /// <summary>
        /// 全てのAssemblyからTの実装を検索。重いので一度検索したTypeはキャッシュする
        /// </summary>
        /// <typeparam name="T"></typeparam>
        static void CacheDerivedTypes<T>()
        {
            _derivedTypeCache ??= new Dictionary<Type, List<Type>>();
            _derivedTypeCache.Add(typeof(T), new List<Type>());
            var types = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(T));
            foreach (var type in types)
            {
                // 実装されているもののみ有効
                if (!type.IsAbstract)
                    _derivedTypeCache[typeof(T)].Add(type);
            }
        }
        
        /// <summary>
        /// Dic keyのType = Book名
        /// IGroupingのType = Sheet名
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<Type, Dictionary<Type, IEnumerable<IGrouping<Type, MemberInfo>>>> GetMemoryTableTypes()
        {
            // <Book, <Sheet, Group<Class, Fields>>>
            Dictionary<Type, Dictionary<Type, IEnumerable<IGrouping<Type, MemberInfo>>>> res = new();
            List<Type> notAbstractTypes = new();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var t in types)
                    {
                        var attr = (MemoryTableAttribute)Attribute.GetCustomAttribute(t, typeof(MemoryTableAttribute));
                        if (attr != null)
                        {
                            // Abstractのクラスを前もって抽出
                            if (!t.IsAbstract)
                                notAbstractTypes.Add(t);
                            else
                                res.Add(t, new Dictionary<Type, IEnumerable<IGrouping<Type, MemberInfo>>>());
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"{assembly.FullName} failed GetTypes()");
                }
            }
            
            foreach (var t in notAbstractTypes)
            {
                // 親子関係にあるクラス毎にfieldをまとめる。
                
#if UMDEBRIDGE_USE_FIELD
                var group = t
                    .GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .GroupBy(x => x.DeclaringType);
#else
                var group = t
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .GroupBy(x => x.DeclaringType);
#endif

                bool isSubclass = false;
                foreach (var res2 in res)
                {
                    // 既にリストに載ってるどれかの実装クラスだったらそこにAdd
                    if (t.IsSubclassOf(res2.Key))
                    {
                        // Debug.Log($"<color=green>{t.Name} is SubClass {res2.Key.Name} and Add</color>");
                        res2.Value.Add(t, group);
                        isSubclass = true;
                        break;
                    }
                }
                
                if(isSubclass) continue;

                // 新参ものだったらKeyから作る
                res.Add(t, new Dictionary<Type, IEnumerable<IGrouping<Type, MemberInfo>>>());
                res[t].Add(t, group);
                // Debug.Log($"<color=blue>Add: {t.Name}</color>");
            }
            return res;
        }
    }
}