������ʽ��ʽ��
iisutil.exe ���� /����1:ֵ1 /����2:ֵ2


���̷���ֵ��Ϊ���н����0��ʾ�ɹ�������ֵΪ������������Ĵ�����


===========================================

1������վ��
	iisutil CreateSite /siteName:Rvsp /httpPort:80 /httpsPort:445 /physicalPath:C:\wwwroot /sslHash:5681154ac76ef9b73af44b08e4730933c633b26b

	������
		siteName��վ����������
		httpPort���˿ڣ���IIS6�±��IIS7�Ǳ���
		httpsPort��ssl�˿ڣ��Ǳ���
		physicalPath������·��������
		sslHash��֤��hash��ָ����httpsPortʱ���֤��Ҫ����LocalMachine�ĸ�����


----------------------
2��ɾ��վ��
	iisutil RemoveSite /siteName:Rvsp

	������
		siteName��վ����������


----------------------
3������Ӧ�ó����
	iisutil CreateAppPool /poolName:RvspPool

	������
		poolName������������


----------------------
4��ɾ��Ӧ�ó����
	iisutil RemoveAppPool /poolName:RvspPool

	������
		poolName������������


----------------------
5����������Ŀ¼
	iisutil CreateDir /siteName:Rvsp /virtualPath:/log /physicalPath:C:\wwwroot\log /enableAllMimeTypes:true

	������
		siteName��վ����������
		virtualPath������·������/��ͷ��Ŀǰֻ֧��һ��������
		physicalPath������·��������
		enableAllMimeTypes: �Ƿ��������������ļ����Ǳ���


----------------------
6��ɾ������Ŀ¼
	iisutil RemoveDir /siteName:Rvsp /virtualPath:/log

	������
		siteName��վ����������
		virtualPath������·��������


----------------------
7������Ӧ�ó���
	iisutil CreateApp /siteName:Rvsp /virtualPath:/log /physicalPath:C:\wwwroot\log /poolName:RvspPool /useSsl:true

	������
		siteName��վ����������
		virtualPath������·������/��ͷ��Ŀǰֻ֧��һ��������
		physicalPath������·��������
		poolName��Ӧ�ó������������
		useSsl���Ƿ�Ҫ��ssl���Ǳ���


----------------------
8��ɾ��Ӧ�ó���
	iisutil RemoveApp /siteName:Rvsp /virtualPath:/log /physicalPath:C:\wwwroot\log /poolName:RvspPool /useSsl:true

	������
		siteName��վ����������
		virtualPath������·��������


----------------------
9���ж�վ���Ƿ����
	iisutil SiteExist /siteName:Rvsp

	������
		siteName��վ����������
	����ֵ��
		400		������
		503		����


----------------------
10������վ��ssl֤��
	iisutil SetCert /siteName:Rvsp /sslHash:5681154ac76ef9b73af44b08e4730933c633b26b

	������
		siteName��վ����������
		sslHash��֤��hash�����֤��Ҫ����LocalMachine�ĸ�����


----------------------
11������վ��˿�
	iisutil SetPort /siteName:Rvsp /httpPort:8080 /httpsPort:8081

	������
		siteName��վ����������
		httpPort���˿�
		httpsPort��ssl�˿�


===========================================


�����룺

	302:	��������
	400:	վ��δ�ҵ�
	401:	Ӧ�ó����δ�ҵ�
	402:	Ӧ�ó���δ�ҵ�
	403:	��Ӧ��δ�ҵ�
	404:	����Ŀ¼δ�ҵ�
	500:	IIS�汾δ֪
	501:	http�˿���ռ��
	502:	https�˿���ռ��
	503:	վ���Ѵ���
	504:	Ӧ�ó�����Ѵ���
	505:	Ӧ�ó����Ѵ���
	506:	����Ŀ¼�Ѵ���
	909:	δ֪����

