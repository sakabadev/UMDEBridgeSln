using System;

namespace UMDEBridge.Annotations {
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class ColumnConfigAttribute : Attribute {
		[AddColumnType("tinyint(1)", "NOT NULL DEFAULT 0")]
		public int preferExcel;

		[AddColumnType("int(11)", "NOT NULL DEFAULT 100")]
		public int columnWidth = 100;

		[AddColumnType("varchar(50)", "NOT NULL")]
		public string sortLabel = "ラベル";
	}
}