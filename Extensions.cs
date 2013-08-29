using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IISUtil
{
	public static class Extensions
	{
		/// <summary>
		/// 判断字符串是否相等
		/// </summary>
		/// <param name="text1"></param>
		/// <param name="text2"></param>
		/// <returns></returns>
		public static bool EqualsEx(this string text1, string text2)
		{
			return string.Equals(text1, text2, StringComparison.CurrentCultureIgnoreCase);
		}

		/// <summary>
		/// 判断是否包含字符串
		/// </summary>
		/// <param name="text"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool ContainsEx(this string text, string value)
		{
			return text.IndexOf(value, StringComparison.CurrentCultureIgnoreCase) >= 0;
		}

		/// <summary>
		/// 判断是否以指定字符串开头
		/// </summary>
		/// <param name="text"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool StartWithEx(this string text, string value)
		{
			return text.StartsWith(value, StringComparison.CurrentCultureIgnoreCase);
		}

		/// <summary>
		/// 判断是否以指定字符串结尾
		/// </summary>
		/// <param name="text"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool EndWithEx(this string text, string value)
		{
			return text.EndsWith(value, StringComparison.CurrentCultureIgnoreCase);
		}

		/// <summary>
		/// 判断字符串是否空
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty(this string text)
		{
			return string.IsNullOrEmpty(text);
		}

	}
}
