using System;

namespace IISUtil.Entity
{
	public class ErrorCode
	{
		/// <summary>
		/// 成功
		/// </summary>
		public const int Succeed = 0;
		/// <summary>
		/// 参数错误
		/// </summary>
		public const int InvalidParameter = 302;
		/// <summary>
		/// 站点未找到
		/// </summary>
		public const int SiteNotFound = 400;
		/// <summary>
		/// 应用程序池未找到
		/// </summary>
		public const int AppPoolNotFound = 401;
		/// <summary>
		/// 应用程序未找到
		/// </summary>
		public const int AppNotFound = 402;
		/// <summary>
		/// 根应用未找到
		/// </summary>
		public const int RootAppNotFound = 403;
		/// <summary>
		/// 虚拟目录未找到
		/// </summary>
		public const int VirtualDirNotFound = 404;
		/// <summary>
		/// IIS版本未知
		/// </summary>
		public const int UnknownIISVer = 500;
		/// <summary>
		/// http端口已占用
		/// </summary>
		public const int HttpPortUsed = 501;
		/// <summary>
		/// https端口已占用
		/// </summary>
		public const int HttpsPortUsed = 502;
		/// <summary>
		/// 站点已存在
		/// </summary>
		public const int SiteExists = 503;
		/// <summary>
		/// 应用程序池已存在
		/// </summary>
		public const int AppPoolExists = 504;
		/// <summary>
		/// 应用程序已存在
		/// </summary>
		public const int AppExists = 505;
		/// <summary>
		/// 虚拟目录已存在
		/// </summary>
		public const int VirtualDirExists = 506;
		/// <summary>
		/// 未知错误
		/// </summary>
		public const int Unknown = 999;
	}
}
