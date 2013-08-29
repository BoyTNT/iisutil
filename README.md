基本形式形式：
iisutil.exe 命令 /参数1:值1 /参数2:值2


进程返回值即为运行结果，0表示成功，其它值为出错，具体见后文错误码


===========================================

1、创建站点
	iisutil CreateSite /siteName:Rvsp /httpPort:80 /httpsPort:445 /physicalPath:C:\wwwroot /sslHash:5681154ac76ef9b73af44b08e4730933c633b26b

	参数：
		siteName：站点名，必填
		httpPort：端口，在IIS6下必填，IIS7非必填
		httpsPort：ssl端口，非必填
		physicalPath：物理路径，必填
		sslHash：证书hash，指定了httpsPort时必填，证书要放入LocalMachine的个人区


----------------------
2、删除站点
	iisutil RemoveSite /siteName:Rvsp

	参数：
		siteName：站点名，必填


----------------------
3、创建应用程序池
	iisutil CreateAppPool /poolName:RvspPool

	参数：
		poolName：池名，必填


----------------------
4、删除应用程序池
	iisutil RemoveAppPool /poolName:RvspPool

	参数：
		poolName：池名，必填


----------------------
5、创建虚拟目录
	iisutil CreateDir /siteName:Rvsp /virtualPath:/log /physicalPath:C:\wwwroot\log /enableAllMimeTypes:true

	参数：
		siteName：站点名，必填
		virtualPath：虚拟路径，以/开头，目前只支持一级，必填
		physicalPath：物理路径，必填
		enableAllMimeTypes: 是否允许下载任意文件，非必填


----------------------
6、删除虚拟目录
	iisutil RemoveDir /siteName:Rvsp /virtualPath:/log

	参数：
		siteName：站点名，必填
		virtualPath：虚拟路径，必填


----------------------
7、创建应用程序
	iisutil CreateApp /siteName:Rvsp /virtualPath:/log /physicalPath:C:\wwwroot\log /poolName:RvspPool /useSsl:true

	参数：
		siteName：站点名，必填
		virtualPath：虚拟路径，以/开头，目前只支持一级，必填
		physicalPath：物理路径，必填
		poolName：应用程序池名，必填
		useSsl：是否要求ssl，非必填


----------------------
8、删除应用程序
	iisutil RemoveApp /siteName:Rvsp /virtualPath:/log /physicalPath:C:\wwwroot\log /poolName:RvspPool /useSsl:true

	参数：
		siteName：站点名，必填
		virtualPath：虚拟路径，必填


----------------------
9、判断站点是否存在
	iisutil SiteExist /siteName:Rvsp

	参数：
		siteName：站点名，必填
	返回值：
		400		不存在
		503		存在


----------------------
10、更换站点ssl证书
	iisutil SetCert /siteName:Rvsp /sslHash:5681154ac76ef9b73af44b08e4730933c633b26b

	参数：
		siteName：站点名，必填
		sslHash：证书hash，必填，证书要放入LocalMachine的个人区


----------------------
11、更换站点端口
	iisutil SetPort /siteName:Rvsp /httpPort:8080 /httpsPort:8081

	参数：
		siteName：站点名，必填
		httpPort：端口
		httpsPort：ssl端口


===========================================


错误码：

	302:	参数错误
	400:	站点未找到
	401:	应用程序池未找到
	402:	应用程序未找到
	403:	根应用未找到
	404:	虚拟目录未找到
	500:	IIS版本未知
	501:	http端口已占用
	502:	https端口已占用
	503:	站点已存在
	504:	应用程序池已存在
	505:	应用程序已存在
	506:	虚拟目录已存在
	909:	未知错误

