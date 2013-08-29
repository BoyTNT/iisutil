using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IISUtil.Core
{
	/// <summary>
	/// IIS操作接口
	/// </summary>
	public interface IUtil
	{
		/// <summary>
		/// 创建站点
		/// </summary>
		/// <param name="siteName">站点名，必填</param>
		/// <param name="httpPort">http端口，IIS6下必填</param>
		/// <param name="httpsPort">https端口</param>
		/// <param name="sslHash">ssl证书hash，指定httpsPort时必填</param>
		/// <param name="physicalPath">物理路径，必填</param>
		/// <returns>状态码</returns>
		int CreateSite(string siteName, int httpPort, int httpsPort, string sslHash, string physicalPath);

		/// <summary>
		/// 移除站点
		/// </summary>
		/// <param name="siteName">站点名，必填</param>
		/// <returns>状态码</returns>
		int RemoveSite(string siteName);

		/// <summary>
		/// 创建应用程序池
		/// </summary>
		/// <param name="poolName">池名称，必填</param>
		/// <returns>状态码</returns>
		int CreateAppPool(string poolName);

		/// <summary>
		/// 移除应用程序池
		/// </summary>
		/// <param name="poolName">池名称，必填</param>
		/// <returns>状态码</returns>
		int RemoveAppPool(string poolName);

		/// <summary>
		/// 创建虚拟目录
		/// </summary>
		/// <param name="siteName">站点名，必填</param>
		/// <param name="virtualPath">虚拟路径，以/开头，目前只支持一级，必填</param>
		/// <param name="physicalPath">物理路径，必填</param>
		/// <param name="enableAllMimeTypes">是否允许所有类型文件的下载</param>
		/// <returns>状态码</returns>
		int CreateDir(string siteName, string virtualPath, string physicalPath, bool enableAllMimeTypes);

		/// <summary>
		/// 移除虚拟目录
		/// </summary>
		/// <param name="siteName">站点名，必填</param>
		/// <param name="virtualPath">虚拟路径，以/开头，目前只支持一级，必填</param>
		/// <returns>状态码</returns>
		int RemoveDir(string siteName, string virtualPath);

		/// <summary>
		/// 创建应用程序
		/// </summary>
		/// <param name="siteName">站点名，必填</param>
		/// <param name="virtualPath">虚拟路径，以/开头，目前只支持一级，必填</param>
		/// <param name="physicalPath">物理路径，必填</param>
		/// <param name="poolName">应用程序池名称</param>
		/// <param name="useSsl">是否只允许ssl访问</param>
		/// <returns>状态码</returns>
		int CreateApp(string siteName, string virtualPath, string physicalPath, string poolName, bool useSsl);

		/// <summary>
		/// 移除应用程序
		/// </summary>
		/// <param name="siteName">站点名，必填</param>
		/// <param name="virtualPath">虚拟路径，以/开头，目前只支持一级，必填</param>
		/// <returns>状态码</returns>
		int RemoveApp(string siteName, string virtualPath);

		/// <summary>
		/// 站点是否存在
		/// </summary>
		/// <param name="siteName"></param>
		/// <returns></returns>
		int SiteExist(string siteName);

		/// <summary>
		/// 设置站点的证书
		/// </summary>
		/// <param name="siteName">站点名，必填</param>
		/// <param name="sslHash">ssl证书hash，必填</param>
		/// <returns>状态码</returns>
		int SetCertificate(string siteName, string sslHash);

		/// <summary>
		/// 设置站点端口
		/// </summary>
		/// <param name="siteName">站点名，必填</param>
		/// <param name="httpPort">http端口，IIS6下必填</param>
		/// <param name="httpsPort">https端口</param>
		/// <returns></returns>
		int SetPort(string siteName, int httpPort, int httpsPort);
	}
}
