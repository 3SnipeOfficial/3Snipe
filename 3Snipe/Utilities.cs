using System;
using System.Collections.Generic;
using System.Text;

namespace _3Snipe
{
	class ThreadInfo
	{
		private int threadNum;
		public int ThreadID { get { return threadNum; } set { threadNum = value; } }
		public ThreadInfo(int tid)
		{
			threadNum = tid;
		}
	}
	class UserInfo
	{
		public UserInfo(string eml, string password)
		{
			email = eml;
			pass = password;
		}
		private String email;
		public string Email { get { return email; } set { email = value; } }
		private String pass;
		public string Password { get { return pass; } set { pass = value; } }
	}
}
