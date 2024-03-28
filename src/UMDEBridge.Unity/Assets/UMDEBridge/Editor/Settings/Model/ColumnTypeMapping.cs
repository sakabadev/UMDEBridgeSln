using System;
using System.Collections.Generic;
using UnityEngine;

namespace UMDEBridge.Editor.Settings.Model {
	[Serializable]
	public sealed class ColumnTypeMapping {
		[Serializable]
		public sealed class TypeInfo {
			public string className;
			public string fieldName;
			public string columnSql;
			public string memo;
		}
		
		public List<TypeInfo> items = new List<TypeInfo> {
			new TypeInfo{className = "Int32", columnSql = "INT(11) NOT NULL DEFAULT -1", memo = "int"},
			new TypeInfo{className = "String", columnSql = "TEXT", memo = "string"},
			new TypeInfo{className = "Boolean", columnSql = "TINYINT(1) NOT NULL DEFAULT 0", memo = "bool"},
			new TypeInfo{className = "Single", columnSql = "DOUBLE(20,10) DEFAULT NULL DEFAULT 0", memo = "float"},
			new TypeInfo{className = "DateTime", columnSql = "DATETIME NOT NULL", memo = "DateTime"},
			new TypeInfo{className = "TimeSpan", columnSql = "TIMESPAN NOT NULL", memo = "TimeSpan"},
		};

		public string GetColumnSql(string className, string fieldName = "") {
			Debug.Log($"[Debug] className:{className}, fieldName:{fieldName}");
			
			foreach (var item in items) {
				// fieldNameとclassNameが一致しているitemのcolumnSqlを返す。
				if (item.className == className && item.fieldName == fieldName) {
					return item.columnSql;
				}
				// 一致するものがない場合は、classNameが一致し、fieldNameは空のitemのcolumnSqlを返す。
				if (item.className == className && string.IsNullOrWhiteSpace(item.fieldName)) {
					return item.columnSql;
				}
			}
			// 当てはまるものが無い場合、"TEXT"を返す。
			return "TEXT";
		}
	}
	
	/*
	 例えばC#のfloat型はMySQLではFLOATで保存すべきでないなど、微妙に使うべき型が違う物があるので、参考程度にしてください。
	 
C# プリミティブ型	MySQL 型
bool	BOOL or TINYINT(1)
byte	TINYINT UNSIGNED
sbyte	TINYINT
short	SMALLINT
ushort	SMALLINT UNSIGNED
int	INT
uint	INT UNSIGNED
long	BIGINT
ulong	BIGINT UNSIGNED
float	FLOAT
double	DOUBLE
decimal	DECIMAL
char	CHAR
string	VARCHAR
DateTime	DATETIME
TimeSpan	TIME
byte[]	BLOB


| C# プリミティブ型 | GetType().Name での型名 |
|-------------------|------------------------|
| `bool`            | `Boolean`              |
| `byte`            | `Byte`                 |
| `sbyte`           | `SByte`                |
| `short`           | `Int16`                |
| `ushort`          | `UInt16`               |
| `int`             | `Int32`                |
| `uint`            | `UInt32`               |
| `long`            | `Int64`                |
| `ulong`           | `UInt64`               |
| `float`           | `Single`               |
| `double`          | `Double`               |
| `decimal`         | `Decimal`              |
| `char`            | `Char`                 |
| `string`          | `String`               |
	 
	 */
}