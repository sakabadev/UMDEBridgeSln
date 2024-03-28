using System.Collections.Generic;

namespace UMDEBridge.Editor.Helper {
	internal struct StringKeyFieldNameResponse
	{
		public StringKeyFieldNameResponse(IReadOnlyList<string> nameList)
		{
			this.NameList = nameList;
		}
		public IReadOnlyList<string> NameList { get; }
	}
}