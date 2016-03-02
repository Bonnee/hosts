using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Hosts
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			List<string> hosts = new List<string> ();
			string[] files = new string[] {
				"http://adaway.org/hosts.txt",
				"http://winhelp2002.mvps.org/hosts.txt",
				"http://hosts-file.net/ad_servers.txt"
			};
			WebClient cl = new WebClient ();

			for (int i = 0; i < files.Length; i++) {
				foreach (string host in cl.DownloadString (files [i]).Split('\n')) {
					if (isRelevant (host)) {
						hosts.Add (Clean (host).Split (',') [1]);
						Console.WriteLine (Clean (host).Split (',') [1]);
					}
				}
			}
		}

		static bool isRelevant (string line)
		{
			line = Clean (line);
			if (line.StartsWith ("#") || line.Length == 0 || line.StartsWith ("\r"))
				return false;
			return true;
		}

		static string Clean (string s)
		{
			return s.Replace (" ", ",").Replace ("\t", ",");
		}
	}
}
