using System;

namespace Hosts
{
	class MainClass
	{
		public static void Main (string[] args)
		{

			string[] files = new string[] {
				"http://adaway.org/hosts.txt",
				"http://winhelp2002.mvps.org/hosts.txt",
				"http://pgl.yoyo.org/adservers/serverlist.php?hostformat=hosts&showintro=0&mimetype=plaintext",
				"http://someonewhocares.org/hosts/hosts",
				"http://hosts-file.net/ad_servers.txt",	//Ads
				"http://hosts-file.net/grm.txt"			//Spam
			};

			hosts h = new hosts ("0.0.0.0", files);

			Console.WriteLine ("Downloading from sources...");
			h.GetHosts ();
			Console.WriteLine ("Merging and saving...");
			if (h.Merge ("/etc/hosts"))
				Console.WriteLine ("Done.");
		}
	}
}
