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
		public UserInfo(string eml, string password, string[] qs)
		{
			email = eml;
			pass = password;
			questions = qs;
		}
		private String email;
		private String[] questions;
		public string Email { get { return email; } set { email = value; } }
		private String pass;
		public string Password { get { return pass; } set { pass = value; } }
		public string[] SecurityInfo { get { return questions; } set { questions = value; } }

	}
}
