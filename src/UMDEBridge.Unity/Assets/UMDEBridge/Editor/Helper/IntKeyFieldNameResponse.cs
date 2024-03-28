using System.Collections.Generic;

namespace UMDEBridge.Editor.Helper {
	internal struct IntKeyFieldNameResponse
	{
		public IntKeyFieldNameResponse(IReadOnlyList<(int, string)> nameList)
		{
			this.NameList = nameList;
		}
		public IReadOnlyList<(int, string)> NameList { get; }
	}
}