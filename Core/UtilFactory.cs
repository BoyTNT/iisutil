using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace IISUtil.Core
{
	public class UtilFactory
	{
		public static IUtil GetUtil()
		{
			IUtil instance = null;

			int majorVersion = GetIISMajorVersion();
			switch(majorVersion)
			{
				case 6:
					instance = new UtilForIIS6();
					break;
				case 7:
				case 8:
					instance = new UtilForIIS7();
					break;
			}

			return instance;
		}

		private static int GetIISMajorVersion()
		{
			int majorVersion = 0;

			try
			{
				var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\INetStp");
				if(key != null)
					majorVersion = Convert.ToInt32(key.GetValue("MajorVersion"));
			}
			catch{}

			return majorVersion;
		}
	}
}
