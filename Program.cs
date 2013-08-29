using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IISUtil.Core;
using IISUtil.Entity;

namespace IISUtil
{
	class Program
	{
		static int Main(string[] args)
		{
			int errorCode = ErrorCode.Succeed;

			try
			{
				if(args.Length <= 1)
				{
					errorCode = ErrorCode.InvalidParameter;
					Console.WriteLine("Invalid Parameters.");
				}
				else 
				{
					var util = UtilFactory.GetUtil();
					if(util != null)
					{
						switch(args[0].ToUpper())
						{
							//新建站点
							case "CREATESITE":
								errorCode = CreateSite(util, args);
								break;

							//移除站点
							case "REMOVESITE":
								errorCode = RemoveSite(util, args);
								break;

							//新建应用程序池
							case "CREATEAPPPOOL":
								errorCode = CreateAppPool(util, args);
								break;

							//移除应用程序池
							case "REMOVEAPPPOOL":
								errorCode = RemoveAppPool(util, args);
								break;

							//新建虚拟目录
							case "CREATEDIR":
								errorCode = CreateDir(util, args);
								break;

							//移除虚拟目录
							case "REMOVEDIR":
								errorCode = RemoveDir(util, args);
								break;

							//创建应用程序
							case "CREATEAPP":
								errorCode = CreateApp(util, args);
								break;

							//移除应用程序
							case "REMOVEAPP":
								errorCode = RemoveApp(util, args);
								break;

							//站点是否存在
							case "SITEEXIST":
								errorCode = SiteExist(util, args);
								break;

							//变更站点证书
							case "SETCERT":
								errorCode = SetCertificate(util, args);
								break;

							//变更站点端口
							case "SETPORT":
								errorCode = SetPort(util, args);
								break;
						}
					}
					else
					{
						//IIS版本未知
						errorCode = ErrorCode.UnknownIISVer;
					}
				}
			}
			catch(Exception ex)
			{
				errorCode = ErrorCode.Unknown;
				Console.WriteLine(ex.Message);
			}

			Console.WriteLine("Execute Result: {0}", errorCode);
			return errorCode;
		}

		/// <summary>
		/// 从参数中取指定值
		/// </summary>
		/// <param name="args"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		private static string GetValue(string[] args, string key)
		{
			string value = string.Empty;

			foreach(string p in args)
			{
				string prefix = string.Format("/{0}:", key);
				if(p.StartWithEx(prefix) && p.Length > prefix.Length)
				{
					value = p.Substring(prefix.Length);
				}
			}

			return value;
		}

		private static int CreateSite(IUtil util, string[] args)
		{
			int errorCode = ErrorCode.Succeed;
			string siteName = GetValue(args, "siteName");
			string httpPort = GetValue(args, "httpPort");
			string httpsPort = GetValue(args, "httpsPort");
			string sslHash = GetValue(args, "sslHash");
			string physicalPath = GetValue(args, "physicalPath");

			//参数基本检查
			if(siteName.IsNullOrEmpty() || physicalPath.IsNullOrEmpty())			//站点名、物理路径不可为空
				errorCode = ErrorCode.InvalidParameter;
			else if(httpPort.IsNullOrEmpty() && httpsPort.IsNullOrEmpty())			//两个端口号不可同时为空
				errorCode = ErrorCode.InvalidParameter;
			else if(!httpsPort.IsNullOrEmpty() && sslHash.IsNullOrEmpty())			//如果启用ssl，必须指定证书hash
				errorCode = ErrorCode.InvalidParameter;
			else
				errorCode = util.CreateSite(siteName,
									httpPort.IsNullOrEmpty() ? 0 : Convert.ToInt32(httpPort),
									httpsPort.IsNullOrEmpty() ? 0 : Convert.ToInt32(httpsPort),
									sslHash,
									physicalPath
								);

			return errorCode;
		}

		private static int RemoveSite(IUtil util, string[] args)
		{
			int errorCode = ErrorCode.Succeed;
			string siteName = GetValue(args, "siteName");

			//必须指定站点名
			if(siteName.IsNullOrEmpty())
				errorCode = ErrorCode.InvalidParameter;
			else
				errorCode = util.RemoveSite(siteName);

			return errorCode;
		}

		private static int CreateAppPool(IUtil util, string[] args)
		{
			int errorCode = ErrorCode.Succeed;
			string poolName = GetValue(args, "poolName");

			//必须指定池名称
			if(poolName.IsNullOrEmpty())
				errorCode = ErrorCode.InvalidParameter;
			else
				errorCode = util.CreateAppPool(poolName);

			return errorCode;
		}

		private static int RemoveAppPool(IUtil util, string[] args)
		{
			int errorCode = ErrorCode.Succeed;
			string poolName = GetValue(args, "poolName");

			//必须指定池名称
			if(poolName.IsNullOrEmpty())
				errorCode = ErrorCode.InvalidParameter;
			else
				errorCode = util.RemoveAppPool(poolName);

			return errorCode;
		}

		private static int CreateDir(IUtil util, string[] args)
		{
			int errorCode = ErrorCode.Succeed;
			string siteName = GetValue(args, "siteName");
			string virtualPath = GetValue(args, "virtualPath");
			string physicalPath = GetValue(args, "physicalPath");
			string enableAllMimeTypes = GetValue(args, "enableAllMimeTypes");

			//站点名、虚拟路径、物理路径不可为空
			if(siteName.IsNullOrEmpty() || virtualPath.IsNullOrEmpty() || physicalPath.IsNullOrEmpty())
				errorCode = ErrorCode.InvalidParameter;
			else
				errorCode = util.CreateDir(siteName,
									virtualPath,
									physicalPath,
									enableAllMimeTypes.IsNullOrEmpty() ? false : Convert.ToBoolean(enableAllMimeTypes)
								);

			return errorCode;
		}

		private static int RemoveDir(IUtil util, string[] args)
		{
			int errorCode = ErrorCode.Succeed;
			string siteName = GetValue(args, "siteName");
			string virtualPath = GetValue(args, "virtualPath");

			//站点名、虚拟路径不可为空
			if(siteName.IsNullOrEmpty() || virtualPath.IsNullOrEmpty())
				errorCode = ErrorCode.InvalidParameter;
			else
				errorCode = util.RemoveDir(siteName, virtualPath);

			return errorCode;
		}

		private static int CreateApp(IUtil util, string[] args)
		{
			int errorCode = ErrorCode.Succeed;
			string siteName = GetValue(args, "siteName");
			string virtualPath = GetValue(args, "virtualPath");
			string physicalPath = GetValue(args, "physicalPath");
			string poolName = GetValue(args, "poolName");
			string useSsl = GetValue(args, "useSsl");

			//站点名、虚拟路径、物理路径、池名称不可为空
			if(siteName.IsNullOrEmpty() || virtualPath.IsNullOrEmpty() || physicalPath.IsNullOrEmpty())
				errorCode = ErrorCode.InvalidParameter;
			else
				errorCode = util.CreateApp(siteName,
									virtualPath,
									physicalPath,
									poolName,
									useSsl.IsNullOrEmpty() ? false : Convert.ToBoolean(useSsl)
								);

			return errorCode;
		}

		private static int RemoveApp(IUtil util, string[] args)
		{
			int errorCode = ErrorCode.Succeed;
			string siteName = GetValue(args, "siteName");
			string virtualPath = GetValue(args, "virtualPath");

			//站点名、虚拟路径不可为空
			if(siteName.IsNullOrEmpty() || virtualPath.IsNullOrEmpty())
				errorCode = ErrorCode.InvalidParameter;
			else
				errorCode = util.RemoveApp(siteName, virtualPath);

			return errorCode;
		}

		private static int SiteExist(IUtil util, string[] args)
		{
			int errorCode = ErrorCode.Succeed;
			string siteName = GetValue(args, "siteName");

			//站点名不可为空
			if(siteName.IsNullOrEmpty())
				errorCode = ErrorCode.InvalidParameter;
			else
				errorCode = util.SiteExist(siteName);

			return errorCode;
		}

		private static int SetCertificate(IUtil util, string[] args)
		{
			int errorCode = ErrorCode.Succeed;
			string siteName = GetValue(args, "siteName");
			string sslHash = GetValue(args, "sslHash");

			//站点名、证书hash不可为空
			if(siteName.IsNullOrEmpty() || sslHash.IsNullOrEmpty())
				errorCode = ErrorCode.InvalidParameter;
			else
				errorCode = util.SetCertificate(siteName, sslHash);

			return errorCode;
		}

		private static int SetPort(IUtil util, string[] args)
		{
			int errorCode = ErrorCode.Succeed;
			string siteName = GetValue(args, "siteName");
			string httpPort = GetValue(args, "httpPort");
			string httpsPort = GetValue(args, "httpsPort");

			//参数基本检查
			if(siteName.IsNullOrEmpty())										//站点名不可为空
				errorCode = ErrorCode.InvalidParameter;
			else if(httpPort.IsNullOrEmpty() && httpsPort.IsNullOrEmpty())		//两个端口号不可同时为空
				errorCode = ErrorCode.InvalidParameter;
			else
				errorCode = util.SetPort(siteName,
									httpPort.IsNullOrEmpty() ? 0 : Convert.ToInt32(httpPort),
									httpsPort.IsNullOrEmpty() ? 0 : Convert.ToInt32(httpsPort)
								);

			return errorCode;
		}
	}
}
