using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Web.Administration;

using IISUtil.Entity;

namespace IISUtil.Core
{
	/// <summary>
	/// IIS7工具类(IIS8适用)
	/// </summary>
	public class UtilForIIS7 : UtilBase, IUtil
	{
		public int CreateSite(string siteName, int httpPort, int httpsPort, string sslHash, string physicalPath)
		{
			int errorCode = ErrorCode.Succeed;

			try
			{
				var server = new ServerManager();

				//检查是否已存在，顺便确定端口占用情况
				bool exists = false;
				var usedHttpPorts = new List<int>();		//已使用的http端口
				var usedHttpsPorts = new List<int>();		//已使用的https端口
				foreach(var site in server.Sites)
				{
					foreach(var binding in site.Bindings)
					{
						if(binding.Protocol.EqualsEx("http"))
							usedHttpPorts.Add(binding.EndPoint.Port);
						else if(binding.Protocol.EqualsEx("https"))
							usedHttpsPorts.Add(binding.EndPoint.Port);
					}

					if(site.Name.EqualsEx(siteName))
					{
						exists = true;
					}
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
						//开始创建站点
						Site site = null;
						if(httpPort > 0)
						{
							//建http站点
							site = server.Sites.Add(siteName, physicalPath, httpPort);
							if(httpsPort > 0)
							{
								//加https绑定
								site.Bindings.Add(string.Format("*:{0}:", httpsPort), HexString2Bytes(sslHash), "MY");
							}
						}
						else
						{
							//直接建https站点
							site = server.Sites.Add(siteName, string.Format("*:{0}:", httpsPort), physicalPath, HexString2Bytes(sslHash));
						}
						site.ServerAutoStart = true;
						site.LogFile.Enabled = false;			//不记录日志

						server.CommitChanges();
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
				var server = new ServerManager();

				//找指定站点
				Site targetSite = null;
				foreach(var site in server.Sites)
				{
					if(site.Name.EqualsEx(siteName))
					{
						targetSite = site;
						break;
					}
				}

				//存在才移除
				if(targetSite != null)
				{
					server.Sites.Remove(targetSite);
					server.CommitChanges();
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
				var server = new ServerManager();

				//先检查是否已存在
				bool exists = false;
				foreach(var pool in server.ApplicationPools)
				{
					if(pool.Name.EqualsEx(poolName))
					{
						exists = true;
						break;
					}
				}

				//不存在才创建
				if(!exists)
				{
					var appPool = server.ApplicationPools.Add(poolName);
					appPool.ManagedRuntimeVersion = "v4.0";							//指定dotnet4.0，集成模式
					appPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
					appPool.QueueLength = 10000;
					server.CommitChanges();
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
				var server = new ServerManager();

				//找指定的应用程序池
				ApplicationPool targetPool = null;
				foreach(var pool in server.ApplicationPools)
				{
					if(pool.Name.EqualsEx(poolName))
					{
						targetPool = pool;
						break;
					}
				}

				//存在才移除
				if(targetPool != null)
				{
					server.ApplicationPools.Remove(targetPool);
					server.CommitChanges();
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
				var server = new ServerManager();

				//先找站点
				Site targetSite = null;
				foreach(var site in server.Sites)
				{
					if(site.Name.EqualsEx(siteName))
					{
						targetSite = site;
						break;
					}
				}

				if(targetSite != null)
				{
					//找根应用
					Application rootApp = null;
					foreach(var app in targetSite.Applications)
					{
						if(app.Path.Equals("/"))
						{
							rootApp = app;
							break;
						}
					}

					//存在根应用才能创建
					if(rootApp != null)
					{
						bool exists = false;
						foreach(var dir in rootApp.VirtualDirectories)
						{
							if(dir.Path.EqualsEx(virtualPath))
							{
								exists = true;
								break;
							}
						}

						if(!exists)
						{
							var dir = rootApp.VirtualDirectories.Add(virtualPath, physicalPath);

							//处理Mime Types
							if(enableAllMimeTypes)
							{
								/*  路径映射存在问题，会向错误的位置写web.config，未解决，改用直接写文件的方法
								var config = server.GetWebConfiguration(targetSite.Name, dir.Path);
								var mimes = config.GetSection("system.webServer/staticContent").GetCollection();

								var mimeTypeSection = mimes.CreateElement("mimeMap");
								mimeTypeSection["fileExtension"] = ".*";
								mimeTypeSection["mimeType"] = "application/*";
								mimes.AddAt(0, mimeTypeSection);
								*/
								string path = Path.Combine(physicalPath, "web.config");
								File.WriteAllText(path, IISResource.ResourceManager.GetString("webconfig"), Encoding.UTF8);
							}

							server.CommitChanges();
						}
						else
						{
							errorCode = ErrorCode.VirtualDirExists;
						}
					}
					else
					{
						errorCode = ErrorCode.RootAppNotFound;
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
			int errorCode = ErrorCode.Succeed;

			try
			{
				var server = new ServerManager();

				//先找站点
				Site targetSite = null;
				foreach(var site in server.Sites)
				{
					if(site.Name.EqualsEx(siteName))
					{
						targetSite = site;
						break;
					}
				}

				if(targetSite != null)
				{
					//找根应用
					Application rootApp = null;
					foreach(var app in targetSite.Applications)
					{
						if(app.Path.Equals("/"))
						{
							rootApp = app;
							break;
						}
					}

					//存在根应用才能删除
					if(rootApp != null)
					{
						VirtualDirectory targetDir = null;
						foreach(var dir in rootApp.VirtualDirectories)
						{
							if(dir.Path.EqualsEx(virtualPath))
							{
								targetDir = dir;
								break;
							}
						}

						if(targetDir != null)
						{
							rootApp.VirtualDirectories.Remove(targetDir);
							server.CommitChanges();
						}
						else
						{
							errorCode = ErrorCode.VirtualDirNotFound;
						}
					}
					else
					{
						errorCode = ErrorCode.RootAppNotFound;
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

		public int CreateApp(string siteName, string virtualPath, string physicalPath, string poolName, bool useSsl)
		{
			int errorCode = ErrorCode.Succeed;

			try
			{
				var server = new ServerManager();

				//先找站点
				Site targetSite = null;
				foreach(var site in server.Sites)
				{
					if(site.Name.EqualsEx(siteName))
					{
						targetSite = site;
						break;
					}
				}

				if(targetSite != null)
				{
					//再找应用
					bool exists = false;
					foreach(var app in targetSite.Applications)
					{
						if(app.Path.Equals(virtualPath))
						{
							exists = true;
							break;
						}
					}

					//不存在才创建
					if(!exists)
					{
						var app = targetSite.Applications.Add(virtualPath, physicalPath);

						//处理poolName
						if(!poolName.IsNullOrEmpty())
							app.ApplicationPoolName = poolName;

						//处理ssl
						if(useSsl)
						{
							var root = server.GetApplicationHostConfiguration();
							var location = root.GetSection("system.webServer/security/access", string.Format("{0}{1}", targetSite.Name, app.Path));
							location["sslFlags"] = 8;
						}

						server.CommitChanges();
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
				var server = new ServerManager();

				//先找站点
				Site targetSite = null;
				foreach(var site in server.Sites)
				{
					if(site.Name.EqualsEx(siteName))
					{
						targetSite = site;
						break;
					}
				}

				if(targetSite != null)
				{
					//再找应用
					Application targetApp = null;
					foreach(var app in targetSite.Applications)
					{
						if(app.Path.Equals(virtualPath))
						{
							targetApp = app;
							break;
						}
					}

					//存在才删除
					if(targetApp != null)
					{
						targetSite.Applications.Remove(targetApp);
						server.CommitChanges();
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
				var server = new ServerManager();

				//找指定站点
				foreach(var site in server.Sites)
				{
					if(site.Name.EqualsEx(siteName))
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
				var server = new ServerManager();

				//找指定站点
				Site targetSite = null;
				foreach(var site in server.Sites)
				{
					if(site.Name.EqualsEx(siteName))
					{
						targetSite = site;
						break;
					}
				}

				//存在才后续处理
				if(targetSite != null)
				{
					foreach(var binding in targetSite.Bindings)
					{
						//只处理https协议
						if(binding.Protocol.EqualsEx("https"))
						{
							binding.CertificateHash = HexString2Bytes(sslHash);
							server.CommitChanges();
							break;
						}
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

		public int SetPort(string siteName, int httpPort, int httpsPort)
		{
			int errorCode = ErrorCode.Succeed;

			try
			{
				var server = new ServerManager();

				//找指定站点
				Site targetSite = null;
				foreach(var site in server.Sites)
				{
					if(site.Name.EqualsEx(siteName))
					{
						targetSite = site;
						break;
					}
				}

				//存在才后续处理
				if(targetSite != null)
				{
					foreach(var binding in targetSite.Bindings)
					{
						if(httpPort > 0 && binding.Protocol.EqualsEx("http"))
						{
							binding.BindingInformation = string.Format("*:{0}:", httpPort);
						}

						if(httpsPort > 0 && binding.Protocol.EqualsEx("https"))
						{
							binding.BindingInformation = string.Format("*:{0}:", httpsPort);
						}
					}
					server.CommitChanges();
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
	}
}
