using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using MessagePack;
using UMDEBridge.Annotations;
using UMDEBridge.Editor.Helper;
using UMDEBridge.Editor.Settings.Repository;
using UnityEditor;
using UnityEngine;

namespace UMDEBridge.Editor
{
    public class ExcelFileCreator : EditorWindow
    {
        const string SettingsFileName = "umdebridge_settings.json";
        
        private Dictionary<Type, Dictionary<Type, IEnumerable<IGrouping<Type, MemberInfo>>>> memoryTableTypes;
        private Type current;
        private UMDEBridgeSettingsAsset settingsAsset;
        private bool showConnectionInfo;
        
        [MenuItem("UMDEBridge/Open ExcelFileCreator", false, 1)]
        private static void Init()
        {
            var window = GetWindow(typeof(ExcelFileCreator));
            window.titleContent = new GUIContent("Excel File Creator");
            window.minSize = new Vector2(400, 200);
        }

        private void OnEnable() {
            var settingsRepository = new UMDEBridgeSettingsRepository();
            settingsAsset = settingsRepository.Find();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Load MemoryTable list"))
                memoryTableTypes = EditorHelpers.GetMemoryTableTypes();
            GUILayout.Space(10);
            
            if (memoryTableTypes == null || memoryTableTypes.Count <= 0)
            {
                GUILayout.Label("有効なMemoryTableクラスが読み込まれていません。");
                return;
            }

            List<Type> typeKeys = memoryTableTypes.Keys.ToList();
            string[] typeNames = memoryTableTypes.Select(x => x.Key.Name).ToArray();

            int typeIdx = typeKeys.IndexOf(current);
            if (typeIdx == -1) typeIdx = 0;
            current = typeKeys[EditorGUILayout.Popup(typeIdx, typeNames)];
            GUILayout.Space(20);
            
            if (GUILayout.Button ("create xlsx", GUILayout.Height(40)))
            {
                Debug.Log($"{nameof(CreateExcelFile)} : start");
                fileName = current.Name;
                filePath = Dir + fileName + ".xlsx";
                Directory.CreateDirectory(Dir);
                if (File.Exists(filePath))
                {
                    if (!EditorUtility.DisplayDialog("ファイルがすでに存在します。", "すでにあるファイルは削除して作成します。", "おーけー", "きゃんせる"))
                        return;

                    File.Delete(filePath);
                }
                
                CreateExcelFile(memoryTableTypes[current]);

                // AssetDatabase.ImportAsset (filePath);
                // AssetDatabase.Refresh (ImportAssetOptions.ForceUpdate);
                // Close ();
                Debug.Log($"{nameof(CreateExcelFile)} : end");
            }
            
            GUILayout.Space(20);
            showConnectionInfo = EditorGUILayout.Foldout(showConnectionInfo, "接続情報");
            if (showConnectionInfo) {
                EditorGUILayout.LabelField(settingsAsset.settings.mySqlSettings.ConnectionCommand);
                EditorGUILayout.LabelField("Database:", settingsAsset.settings.mySqlSettings.Database);
                GUILayout.Space(20);
            }
            
            GUILayout.Space(20);
            
            if (GUILayout.Button ("create table and options to MySql", GUILayout.Height(40))
            && EditorUtility.DisplayDialog("CREATEの前にDROP TABLEします", "フィールドを追加したい場合は各シートをコミットで行われます。", "おーけー", "きゃんせる"))
            {
                CreateTableToMySql(memoryTableTypes[current]);
            }
            
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Entityの構造は変更せず、Excelのオプションを変更する場合等に使います。");
            EditorGUILayout.LabelField("fieldからpropertyに変えた場合にも使います。");
            if (GUILayout.Button ("create options to MySql", GUILayout.Height(40))
                && EditorUtility.DisplayDialog("CREATEの前にDROP TABLEします", "オプションのテーブルのみ再作成します。", "おーけー", "きゃんせる"))
            {
                CreateTableOptionsToMySql(memoryTableTypes[current]);
            }
        }

        void CreateTableToMySql(Dictionary<Type, IEnumerable<IGrouping<Type, MemberInfo>>> sheetTypes) {
            CreateTableOptionsToMySql(sheetTypes);
            
            Debug.Log($"{nameof(CreateTableToMySql)} : start");
            foreach (var sheetType in sheetTypes)
            {
                string tableName = sheetType.Key.Name.ToSnakeCase();
                
                string sql = GenerateCreateTableSql(tableName, sheetType.Value);
                Debug.Log("CreateTable: "+sql);
                MySqlHelpers.Execute(sql, settingsAsset.settings.mySqlSettings, null);
            }
            Debug.Log($"{nameof(CreateTableToMySql)} : end");
        }
        
        void CreateTableOptionsToMySql(Dictionary<Type, IEnumerable<IGrouping<Type, MemberInfo>>> sheetTypes)
        {
            Debug.Log($"{nameof(CreateTableOptionsToMySql)} : start");
            foreach (var sheetType in sheetTypes)
            {
                string tableName = sheetType.Key.Name.ToSnakeCase();
                string sql = GenerateCreateConfigSql(tableName, sheetType.Value);
                Debug.Log("CreateConfigTable: "+sql);
                MySqlHelpers.Execute(sql, settingsAsset.settings.mySqlSettings, null);
                
                sql = GenerateInsertConfigSql(tableName, sheetType.Value);
                Debug.Log("InsertConfig: " + sql);
                MySqlHelpers.Execute(sql, settingsAsset.settings.mySqlSettings, null);
            }
            Debug.Log($"{nameof(CreateTableOptionsToMySql)} : end");
        }

        string GenerateCreateTableSql(string tableName, IEnumerable<IGrouping<Type, MemberInfo>> group) {
            StringBuilder sb = new StringBuilder();
            var reg = new Regex("^idx$|^id$", RegexOptions.IgnoreCase);
            
            // 値テーブルの作成
            sb.Append($"DROP TABLE IF EXISTS `{tableName}`;");
            sb.Append($@"
CREATE TABLE IF NOT EXISTS `{tableName}` (
`idx` int(11) NOT NULL AUTO_INCREMENT,
`id` varchar(50) NOT NULL,
");

            var columnConfigList = new Dictionary<string, ColumnConfigAttribute>();
            var reverseGroup = group.Reverse();
            foreach (var infos in reverseGroup)
            foreach (var info in infos)
            {
                if(reg.IsMatch(info.Name))
                    continue;

                string columnTypeSql = string.Empty;
                AddColumnTypeAttribute attr = (AddColumnTypeAttribute)Attribute.GetCustomAttribute(info, typeof(AddColumnTypeAttribute));
                // NOTE: AddColumnTypeAttributeが付いていればそちらを優先し、ついてなければsettingsから取得する。
                if (attr != null) {
                    columnTypeSql = string.Join(" ", attr.GetMembers());
                }
                else {
                    var map = settingsAsset.settings.columnTypeMapping;
                    bool isEnum = false;
#if UMDEBRIDGE_USE_FIELD
                    var castedInfo =  (FieldInfo)info;
                    string className = castedInfo.FieldType.Name;
                    isEnum = castedInfo.FieldType.IsEnum;
#else
                    var castedInfo = (PropertyInfo)info;
                    string className = castedInfo.PropertyType.Name;
                    isEnum = castedInfo.PropertyType.IsEnum;
#endif
                    string fieldName = info.Name;

                    if (isEnum) {
                        // Enumはシリアライズ時にintに変換されるので、int型として扱う。
                        columnTypeSql = "int(11) NOT NULL DEFAULT 0";
                    }
                    else {
                        columnTypeSql = map.GetColumnSql(className, fieldName);
                    }
                    Debug.Log($"[Debug] columnTypeSql:{columnTypeSql}");
                }

                ColumnConfigAttribute attr2 = (ColumnConfigAttribute)Attribute.GetCustomAttribute(info, typeof(ColumnConfigAttribute));
                if (attr2 == null){
                    var attr3 = (IgnoreMemberAttribute)Attribute.GetCustomAttribute(info, typeof(IgnoreMemberAttribute));
                    if (attr3 == null) {
                        Debug.LogError($"[ColumnConfig]を設定して下さい。 {info.Name}");
                        return string.Empty;
                    }
                    continue;
                }

                string columnName = info.Name.ToSnakeCase();
                sb.Append($"`{columnName}` {columnTypeSql},");
                columnConfigList.Add(columnName, attr2); 
            }

            sb.Append($@"
`disable` tinyint(1) NOT NULL DEFAULT 0,
PRIMARY KEY (`idx`) USING BTREE,
UNIQUE KEY (`id`)
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8 COLLATE = utf8_unicode_ci ROW_FORMAT = Compact;
");
            return sb.ToString();
        }
        
        string GenerateCreateConfigSql(string tableName, IEnumerable<IGrouping<Type, MemberInfo>> group) {
            StringBuilder sb = new StringBuilder();
            
            // configテーブルの作成
            sb.Append($"DROP TABLE IF EXISTS `{tableName}_config`;");
            sb.Append($@"
CREATE TABLE IF NOT EXISTS `{tableName}_config` (
`idx` int(11) NOT NULL AUTO_INCREMENT,
");
            
            foreach (var info in typeof(ColumnConfigAttribute).GetFields())
            {
                var attr = (AddColumnTypeAttribute)Attribute.GetCustomAttribute(info, typeof(AddColumnTypeAttribute));
                if (attr == null){
                    Debug.LogError($"[AddColumnType]を設定して下さい。 {info.Name}");
                    return string.Empty;
                }

                if (info.Name == "sortLabel")
                {
                    // sort_labelの手前に入れたいフィールド
                    sb.Append($"`mp_key` varchar(50) NOT NULL,");
                    sb.Append($"`column_name` varchar(50) NOT NULL,");
                }
                
                sb.Append($"`{info.Name.ToSnakeCase()}` {string.Join(" ", attr.GetMembers())},");
            }
            
            sb.Append($@"
PRIMARY KEY (`idx`) USING BTREE,
UNIQUE KEY (`column_name`)
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8 COLLATE = utf8_unicode_ci ROW_FORMAT = Compact;
");
            Debug.Log(sb.ToString());
            return sb.ToString();
        }
        
        string GenerateInsertConfigSql(string tableName, IEnumerable<IGrouping<Type, MemberInfo>> group) {
            StringBuilder sb = new StringBuilder();
            var reg = new Regex("^idx$", RegexOptions.IgnoreCase);
            
            List<string> configColNames = new List<string>{"column_name", "mp_key"};
            foreach (var info in typeof(ColumnConfigAttribute).GetFields())
                configColNames.Add(info.Name.ToSnakeCase());
            
            // configテーブルにInsert
            sb.Append($"INSERT INTO {tableName}_config (");
            sb.Append($"{string.Join(",", configColNames)}");
            sb.Append($") VALUES ");

            var reverseGroup = group.Reverse();
            foreach (var infos in reverseGroup)
            foreach (var info in infos)
            {
                if(reg.IsMatch(info.Name))
                    continue;
                
                ColumnConfigAttribute attr2 = (ColumnConfigAttribute)Attribute.GetCustomAttribute(info, typeof(ColumnConfigAttribute));
                if (attr2 == null){
                    var attr3 = (IgnoreMemberAttribute)Attribute.GetCustomAttribute(info, typeof(IgnoreMemberAttribute));
                    if (attr3 == null) {
                        Debug.LogError($"[ColumnConfig]を設定して下さい。 {info.Name}");
                        return string.Empty;
                    }
                    continue;
                }

                // column_name
                string columnName = info.Name.ToSnakeCase();
                sb.Append($"('{columnName}',");
                
                // mp_key
                var key = (KeyAttribute)Attribute.GetCustomAttribute(info, typeof(KeyAttribute));
                if (key != null) {
                    // KeyAttributeがあればそっちを使う
                    foreach (var prop in typeof(KeyAttribute).GetProperties()) {
                        sb.Append($"'{prop.GetValue(key)}',");
                        break;
                    }
                }
                else {
                    // KeyAttributeが無ければフィールド名（もしくはプロパティ名）を使う
                    sb.Append($"'{info.Name}',");
                }

                foreach (var field in typeof(ColumnConfigAttribute).GetFields())
                    sb.Append($"'{field.GetValue(attr2)}',");
                sb.Remove(sb.Length-1, 1); // カンマの削除
                sb.Append($"),");
            }
            sb.Remove(sb.Length-1, 1); // カンマの削除
            sb.Append($";");

            return sb.ToString();
        }

        string Dir => Application.dataPath + "/../Excels/";
        private string filePath = string.Empty;
        private string fileName = string.Empty;

        async void CreateExcelFile(Dictionary<Type, IEnumerable<IGrouping<Type, MemberInfo>>> sheetTypes)
        {
            XLWorkbook wb = new XLWorkbook();
            
            foreach (var sheetType in sheetTypes)
            {
                string sheetName = sheetType.Key.Name.ToSnakeCase();
                // シートの取得か作成
                var sheet = 
                    wb.Worksheets.FirstOrDefault(x => x.Name == sheetName) 
                    ?? wb.Worksheets.Add(sheetName);
                
                // セルに値を入れる
                var cell = sheet.Cell("A1");
                cell.Value = "アドインから同期して下さい。";
            }
            
            wb.SaveAs(filePath);
            
            // Excel Addin用設定ファイルが見つからなければ作成する。
            string addinSettingsPath = Path.Combine(Dir, SettingsFileName);
            if (!File.Exists(addinSettingsPath)) {
                var settings = new Settings.Model.Settings();
                string json = JsonUtility.ToJson(settings);
                File.WriteAllText(addinSettingsPath, json);
            }
        }
        
    }
}