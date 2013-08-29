using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using IISOle;

using IISUtil.Entity;

namespace IISUtil.Core
{
	/// <summary>
	/// IIS6工具类
	/// </summary>
	public class UtilForIIS6 : UtilBase, IUtil
	{
		public int CreateSite(string siteName, int httpPort, int httpsPort, string sslHash, string physicalPath)
		{
			int errorCode = ErrorCode.Succeed;

			try
			{
				var siteEntry = new DirectoryEntry("IIS://localhost/w3svc");

				//启用ASP4
				this.EnableAspNet4(siteEntry);

				//检查是否已存在，顺便确定端口占用及站点ID占用情况
				bool exists = false;
				var usedSiteIds = new List<int>();
				var usedHttpPorts = new List<int>();
				var usedHttpsPorts = new List<int>();
				foreach(DirectoryEntry site in siteEntry.Children)
				{
					if(!site.SchemaClassName.EqualsEx("IIsWebServer"))		//IIsWebServer代表站点
						continue;

					usedSiteIds.Add(Convert.ToInt32(site.Name));			//站点的Name是SiteId
					usedHttpPorts.Add(GetPort(site.Properties["ServerBindings"].Value as string));
					usedHttpsPorts.Add(GetPort(site.Properties["SecureBindings"].Value as string));

					string name = Convert.ToString(site.Properties["ServerComment"].Value);
					if(!name.IsNullOrEmpty() && name.EqualsEx(siteName))
						exists = true;
				}

				//不存在才创建
				if(!exists)
				{
					//检查端口占用情况
					if(httpPort > 0 && usedHttpPorts.Contains(httpPort))
					{
						//http端口占用
						errorCode = ErrorCode.HttpPortUsed;
					}
					else if(httpsPort > 0 && usedHttpsPorts.Contains(httpsPort))
					{
						//https端口占用
						errorCode = ErrorCode.HttpsPortUsed;
					}
					else
					{
						//确定可用的站点id
						int siteId = 0;
						for(siteId = 1;siteId < 65536;++siteId)
						{
							if(!usedSiteIds.Contains(siteId))
								break;
						}

						//建站
						var site = siteEntry.Children.Add(siteId.ToString(), "IIsWebServer");

						//设置绑定等信息
						site.Properties["ServerBindings"].Value = string.Format(":{0}:", httpPort);
						site.Properties["ServerComment"].Value = siteName;
						site.Properties["AccessScript"][0] = true;		//允许脚本执行
						site.Properties["AccessRead"][0] = true;		//允许读
						site.Properties["LogType"].Value = "0";			//不记录日志
						if(httpsPort > 0)
						{
							site.Properties["SecureBindings"].Value = string.Format(":{0}:", httpsPort);
							site.Properties["SSLStoreName"][0] = "MY";	//个人存储区

							//ssl信息必须以特殊的格式加入
							var hash = new object[sslHash.Length / 2];
							for(int i = 0;i < sslHash.Length / 2;++i)
								hash[i] = sslHash.Substring(i * 2, 2);
							site.Properties["SSLCertHash"].Clear();
							site.Properties["SSLCertHash"].Add(hash);
						}

						//建根目录
						var root = site.Children.Add("ROOT", "IIsWebVirtualDir");
						root.Properties["Path"].Value = physicalPath;

						root.CommitChanges();
						site.CommitChanges();
					}
				}
				else
				{
					errorCode = ErrorCode.SiteExists;
				}
			}
			catch(Exception ex)
			{
				errorCode = ErrorCode.Unknown;
				Console.WriteLine(ex.Message);
			}

			return errorCode;
		}

		public int RemoveSite(string siteName)
		{
			int errorCode = ErrorCode.Succeed;

			try
			{
				var siteEntry = new DirectoryEntry("IIS://localhost/w3svc");

				//找指定站点
				DirectoryEntry targetSite = null;
				foreach(DirectoryEntry site in siteEntry.Children)
				{
					if(!site.SchemaClassName.EqualsEx("IIsWebServer"))
						continue;

					//站点名在ServerComment中
					string name = Convert.ToString(site.Properties["ServerComment"].Value);
					if(!name.IsNullOrEmpty() && name.EqualsEx(siteName))
					{
						targetSite = site;
						break;
					}
				}

				//存在才移除
				if(targetSite != null)
				{
					siteEntry.Children.Remove(targetSite);
					siteEntry.CommitChanges();
				}
				else
				{
					errorCode = ErrorCode.SiteNotFound;
				}
			}
			catch(Exception ex)
			{
				errorCode = ErrorCode.Unknown;
				Console.WriteLine(ex.Message);
			}

			return errorCode;
		}

		public int CreateAppPool(string poolName)
		{
			int errorCode = ErrorCode.Succeed;

			try
			{
				var poolEntry = new DirectoryEntry("IIS://localhost/w3svc/apppools");

				//先检查是否已存在
				bool exists = false;
				foreach(DirectoryEntry pool in poolEntry.Children)
				{
					//IIsApplicationPool表示应用程序池，Name即为名称
					if(pool.Name.EqualsEx(poolName) && pool.SchemaClassName.EqualsEx("IIsApplicationPool"))
					{
						exists = true;
						break;
					}
				}

				//不存在才创建
				if(!exists)
				{
					var pool = poolEntry.Children.Add(poolName, "IIsApplicationPool");
					pool.CommitChanges();
				}
				else
				{
					errorCode = ErrorCode.AppPoolExists;
				}
			}
			catch(Exception ex)
			{
				errorCode = ErrorCode.Unknown;
				Console.WriteLine(ex.Message);
			}

			return errorCode;
		}

		public int RemoveAppPool(string poolName)
		{
			int errorCode = ErrorCode.Succeed;

			try
			{
				var poolEntry = new DirectoryEntry("IIS://localhost/w3svc/apppools");

				//找指定的应用程序池
				DirectoryEntry targetPool = null;
				foreach(DirectoryEntry pool in poolEntry.Children)
				{
					if(pool.Name.EqualsEx(poolName) && pool.SchemaClassName.EqualsEx("IIsApplicationPool"))
					{
						targetPool = pool;
						break;
					}
				}

				//存在才移除
				if(targetPool != null)
				{
					poolEntry.Children.Remove(targetPool);
					poolEntry.CommitChanges();
				}
				else
				{
					errorCode = ErrorCode.AppPoolNotFound;
				}
			}
			catch(Exception ex)
			{
				errorCode = ErrorCode.Unknown;
				Console.WriteLine(ex.Message);
			}

			return errorCode;
		}

		public int CreateDir(string siteName, string virtualPath, string physicalPath, bool enableAllMimeTypes)
		{
			int errorCode = ErrorCode.Succeed;

			try
			{
				var siteEntry = new DirectoryEntry("IIS://localhost/w3svc");

				//先找站点
				DirectoryEntry targetSite = null;
				foreach(DirectoryEntry site in siteEntry.Children)
				{
					if(!site.SchemaClassName.EqualsEx("IIsWebServer"))
						continue;

					string name = Convert.ToString(site.Properties["ServerComment"].Value);
					if(!name.IsNullOrEmpty() && name.EqualsEx(siteName))
					{
						targetSite = site;
						break;
					}
				}

				if(targetSite != null)
				{
					//根目录
					var rootEntry = new DirectoryEntry(string.Format("IIS://localhost/w3svc/{0}/ROOT", targetSite.Name));
					bool exists = false;

					foreach(DirectoryEntry dir in rootEntry.Children)
					{
						//IIsWebVirtualDir表示虚拟目录
						if(!dir.SchemaClassName.EqualsEx("IIsWebVirtualDir"))
							continue;

						//名称不包含/，不会是多级
						if(dir.Name.Equals(virtualPath.Substring(1)))
						{
							exists = true;
							break;
						}
					}

					if(!exists)
					{
						var dir = rootEntry.Children.Add(virtualPath.Substring(1), "IIsWebVirtualDir");
						dir.Properties["Path"][0] = physicalPath;								//物理路径
						dir.Properties["AppFriendlyName"][0] = virtualPath.Substring(1);		//名称
						dir.Properties["AccessRead"][0] = true;									//读权限
						dir.Properties["DontLog"][0] = true;

						if(enableAllMimeTypes)
						{
							new IISOle.MimeMapClass();
							var mime = new IISOle.MimeMapClass();
							mime.Extension = "*";
							mime.MimeType = "application/octet-stream";
							dir.Properties["MimeMap"].Clear();
							dir.Properties["MimeMap"].Add(mime);
						}

						dir.CommitChanges();
						rootEntry.CommitChanges();
					}
					else
					{
						errorCode = ErrorCode.AppExists;
					}
				}
				else
				{
					errorCode = ErrorCode.SiteNotFound;
				}
			}
			catch(Exception ex)
			{
				errorCode = ErrorCode.Unknown;
				Console.WriteLine(ex.Message);
			}

			return errorCode;
		}

		public int RemoveDir(string siteName, string virtualPath)
		{
			//移除虚拟目录的方法与移除应用程序一样
			return RemoveApp(siteName, virtualPath);
		}

		public int CreateApp(string siteName, string virtualPath, string physicalPath, string poolName, bool useSsl)
		{
			int errorCode = ErrorCode.Succeed;

			try
			{
				var siteEntry = new DirectoryEntry("IIS://localhost/w3svc");

				//启用ASP4
				this.EnableAspNet4(siteEntry);

				//先找站点
				DirectoryEntry targetSite = null;
				foreach(DirectoryEntry site in siteEntry.Children)
				{
					if(!site.SchemaClassName.EqualsEx("IIsWebServer"))
						continue;

					string name = Convert.ToString(site.Properties["ServerComment"].Value);
					if(!name.IsNullOrEmpty() && name.EqualsEx(siteName))
					{
						targetSite = site;
						break;
					}
				}

				if(targetSite != null)
				{
					//根目录
					var rootEntry = new DirectoryEntry(string.Format("IIS://localhost/w3svc/{0}/ROOT", targetSite.Name));
					bool exists = false;

					foreach(DirectoryEntry dir in rootEntry.Children)
					{
						if(!dir.SchemaClassName.EqualsEx("IIsWebDirectory") && !dir.SchemaClassName.EqualsEx("IIsWebVirtualDir"))
							continue;

						if(dir.Name.Equals(virtualPath.Substring(1)))
						{
							exists = true;
							break;
						}
					}

					if(!exists)
					{
						var dir = rootEntry.Children.Add(virtualPath.Substring(1), "IIsWebVirtualDir");
						dir.Properties["Path"][0] = physicalPath;
						dir.Properties["AppFriendlyName"][0] = virtualPath.Substring(1);
						dir.Properties["AccessRead"][0] = true;
						dir.Properties["AccessScript"][0] = true;			//脚本执行权限
						dir.Properties["DontLog"][0] = true;

						//处理poolName
						if(!poolName.IsNullOrEmpty())
							dir.Properties["AppPoolId"].Value = poolName;

						//处理ssl
						if(useSsl)
							dir.Properties["AccessSSL"][0] = true;

						//IIS6要处理通配符映射
						string systemDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
						string scriptMap = string.Format("*,{0}\\Microsoft.NET\\Framework\\v4.0.30319\\aspnet_isapi.dll,0,GET,HEAD,POST", systemDir);
						dir.Properties["ScriptMaps"].Add(scriptMap);

						dir.Invoke("AppCreate", true);						//创建应用程序
						dir.CommitChanges();
						rootEntry.CommitChanges();
					}
					else
					{
						errorCode = ErrorCode.AppExists;
					}
				}
				else
				{
					errorCode = ErrorCode.SiteNotFound;
				}
			}
			catch(Exception ex)
			{
				errorCode = ErrorCode.Unknown;
				Console.WriteLine(ex.Message);
			}

			return errorCode;
		}

		public int RemoveApp(string siteName, string virtualPath)
		{
			int errorCode = ErrorCode.Succeed;

			try
			{
				var siteEntry = new DirectoryEntry("IIS://localhost/w3svc");

				//先找站点
				DirectoryEntry targetSite = null;
				foreach(DirectoryEntry site in siteEntry.Children)
				{
					if(!site.SchemaClassName.EqualsEx("IIsWebServer"))
						continue;

					string name = Convert.ToString(site.Properties["ServerComment"].Value);
					if(!name.IsNullOrEmpty() && name.EqualsEx(siteName))
					{
						targetSite = site;
						break;
					}
				}

				if(targetSite != null)
				{
					//根目录
					var rootEntry = new DirectoryEntry(string.Format("IIS://localhost/w3svc/{0}/ROOT", targetSite.Name));
					
					DirectoryEntry targetApp = null;
					foreach(DirectoryEntry app in rootEntry.Children)
					{
						if(!app.SchemaClassName.EqualsEx("IIsWebDirectory") && !app.SchemaClassName.EqualsEx("IIsWebVirtualDir"))
							continue;

						if(app.Name.EqualsEx(virtualPath.Substring(1)))
						{
							targetApp = app;
							break;
						}
					}

					if(targetApp != null)
					{
						rootEntry.Children.Remove(targetApp);
						rootEntry.CommitChanges();
					}
					else
					{
						errorCode = ErrorCode.AppNotFound;
					}
				}
				else
				{
					errorCode = ErrorCode.SiteNotFound;
				}
			}
			catch(Exception ex)
			{
				errorCode = ErrorCode.Unknown;
				Console.WriteLine(ex.Message);
			}

			return errorCode;
		}

		public int SiteExist(string siteName)
		{
			int errorCode = ErrorCode.SiteNotFound;

			try
			{
				var siteEntry = new DirectoryEntry("IIS://localhost/w3svc");

				//找指定站点
				foreach(DirectoryEntry site in siteEntry.Children)
				{
					if(!site.SchemaClassName.EqualsEx("IIsWebServer"))
						continue;

					//站点名在ServerComment中
					string name = Convert.ToString(site.Properties["ServerComment"].Value);
					if(!name.IsNullOrEmpty() && name.EqualsEx(siteName))
					{
						errorCode = ErrorCode.SiteExists;
						break;
					}
				}
			}
			catch(Exception ex)
			{
				errorCode = ErrorCode.Unknown;
				Console.WriteLine(ex.Message);
			}

			return errorCode;
		}

		public int SetCertificate(string siteName, string sslHash)
		{
			int errorCode = ErrorCode.Succeed;

			try
			{
				var siteEntry = new DirectoryEntry("IIS://localhost/w3svc");

				//找指定站点
				DirectoryEntry targetSite = null;
				foreach(DirectoryEntry site in siteEntry.Children)
				{
					if(!site.SchemaClassName.EqualsEx("IIsWebServer"))
						continue;

					//站点名在ServerComment中
					string name = Convert.ToString(site.Properties["ServerComment"].Value);
					if(!name.IsNullOrEmpty() && name.EqualsEx(siteName))
					{
						targetSite = site;
						break;
					}
				}

				//存在才做后续处理
				if(targetSite != null)
				{
					var hash = new object[sslHash.Length / 2];
					for(int i = 0;i < sslHash.Length / 2;++i)
						hash[i] = sslHash.Substring(i * 2, 2);
					targetSite.Properties["SSLCertHash"].Clear();
					targetSite.Properties["SSLCertHash"].Add(hash);

					targetSite.CommitChanges();
				}
				else
				{
					errorCode = ErrorCode.SiteNotFound;
				}
			}
			catch(Exception ex)
			{
				errorCode = ErrorCode.Unknown;
				Console.WriteLine(ex.Message);
			}

			return errorCode;
		}

		public int SetPort(string siteName, int httpPort, int httpsPort)
		{
			int errorCode = ErrorCode.Succeed;

			try
			{
				var siteEntry = new DirectoryEntry("IIS://localhost/w3svc");

				//找指定站点
				DirectoryEntry targetSite = null;
				foreach(DirectoryEntry site in siteEntry.Children)
				{
					if(!site.SchemaClassName.EqualsEx("IIsWebServer"))
						continue;

					//站点名在ServerComment中
					string name = Convert.ToString(site.Properties["ServerComment"].Value);
					if(!name.IsNullOrEmpty() && name.EqualsEx(siteName))
					{
						targetSite = site;
						break;
					}
				}

				//存在才做后续处理
				if(targetSite != null)
				{
					if(httpPort > 0)
						targetSite.Properties["ServerBindings"].Value = string.Format(":{0}:", httpPort);
					if(httpsPort > 0)
						targetSite.Properties["SecureBindings"].Value = string.Format(":{0}:", httpsPort);

					targetSite.CommitChanges();
				}
				else
				{
					errorCode = ErrorCode.SiteNotFound;
				}
			}
			catch(Exception ex)
			{
				errorCode = ErrorCode.Unknown;
				Console.WriteLine(ex.Message);
			}

			return errorCode;
		}

		protected int GetPort(string binding)
		{
			if(binding.IsNullOrEmpty())
				return 0;

			//IIS6的binding是“:80:”或者“local:80:”这样的形式
			int start = binding.IndexOf(":");
			int end = binding.IndexOf(":", start + 1);
			string port = binding.Substring(start + 1, end - start - 1);

			return Convert.ToInt32(port);
		}

		protected void EnableAspNet4(DirectoryEntry siteEntry)
		{
			try
			{
				siteEntry.Invoke("EnableWebServiceExtension", "ASP.NET v4.0.30319");
			}
			catch { }
		}
	}
}
