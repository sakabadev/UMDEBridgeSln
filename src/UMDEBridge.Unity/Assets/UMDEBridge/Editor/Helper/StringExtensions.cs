using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UMDEBridge.Editor.Helper {
	public static class StringExtensions {
		public static string ToCamelCase(this string str, bool keepUnderscore = false) {
			StringBuilder result = new StringBuilder();
			if (str.StartsWith("_")) {
				str = str.Substring(1);
				if (keepUnderscore)
					result.Append('_');
			}
			string[] strArray = str.Split('_');
			result.Append(char.ToLower(strArray[0][0]) + strArray[0].Substring(1));
			for (int i = 1; i < strArray.Length; i++) {
				// 最初の一文字を大文字にして、ワードを連結する
				result.Append(char.ToUpper(strArray[i][0]) + strArray[i].Substring(1));
			}
			return result.ToString();
		}
		
		public static string ToPascalCase(this string str, bool keepUnderscore = false) {
			StringBuilder result = new StringBuilder();
			if (str.StartsWith("_")) {
				str = str.Substring(1);
				if (keepUnderscore)
					result.Append('_');
			}
			string[] strArray = str.Split('_');
			for (int i = 0; i < strArray.Length; i++) {
				// 最初の一文字を大文字にして、ワードを連結する
				result.Append(char.ToUpper(strArray[i][0]) + strArray[i].Substring(1));
			}
			return result.ToString();
		}

		public static string ToSnakeCase(this string str, bool keepUnderscore = false) {
			if (string.IsNullOrEmpty(str))
				return str;
			if (str.Length == 1)
				return str.ToLower();
			
			StringBuilder result = new StringBuilder();
			if (str.StartsWith("_")) {
				str = str.Substring(1);
				if (keepUnderscore)
					result.Append('_');
			}
			
			var pattern = new Regex(@"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+",
				RegexOptions.IgnorePatternWhitespace);
			result.Append(string.Join("_", pattern.Matches(str).Cast<Match>().Select(m => m.Value)).ToLower());
			return result.ToString();
		}
	}
}