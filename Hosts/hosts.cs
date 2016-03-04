using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace Hosts
{
	public class hosts
	{
		List<string[]> entries;

		public List<string[]> Hosts { get { return entries; } }

		string addr;

		public string Address {
			get { return addr; }
			set { addr = value; }
		}

		public string Header { get; set; }

		public List<string> Sources { get; set; }

		public hosts (string address, string[] sources)
		{
			entries = new List<string[]> ();
			Address = address;
			Sources = new List<string> (sources);
		}

		public void GetHosts ()
		{
			WebClient cl = new WebClient ();
			cl.Proxy.Credentials = CredentialCache.DefaultCredentials;

			string[] entry;
			int index;

			foreach (string source in Sources) {
				Console.Write (source + "...");

				try {
					
					string[] host = cl.DownloadString (source).Split ('\n');
					Console.Write ("Adding...");
					index = Console.CursorLeft;

					for (int i = 0; i < host.Length; i++) {
						
						if (isRelevant (host [i])) {
							entry = new string[] { addr, Clean (host [i]) };
							if (!Exists (entries, entry)) {
								entries.Add (entry);
								Console.CursorLeft = index;
								Console.Write (i + 1 + "/" + host.Length);
							}
						}
					}
					Console.Write ("...Done.\n");

				} catch (WebException e) {
					Console.WriteLine (e.Message);
				}

			}

			entries = entries.OrderBy (o => o [1]).ToList ();
		}

		bool Exists (List<string[]> list, string[] arr)
		{
			for (int i = 0; i < list.Count; i++) {
				if (list [i] [1] == arr [1])
					return true;
			}
			return false;
		}

		bool isRelevant (string line)
		{
			line = line.Replace (" ", ",");
			if (line.StartsWith ("#") || line.Length == 0 || line.StartsWith ("\r") || line.Contains ("localhost"))
				return false;
			return true;
		}

		string Clean (string line)
		{
			return line.Split (new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries) [1].Replace ("\r", "");
		}

		string CreateHeader ()
		{
			string header = "# Host file courtesy of SuperBonny.\n# This list was updated on " + DateTime.Now.ToShortDateString () + " at " + DateTime.Now.ToShortTimeString () + "\n# There are " + entries.Count + " hosts present.\n# These are the sources:\n#\n# ";
			foreach (string s in Sources) {
				header += s + "\n# ";
			}
			header += "\n127.0.0.1 localhost\n::1 localhost\n\n# [START OF GENERATED ENTRIES]\n#\n";
			return header;
		}

		public bool Merge (string filename)
		{
			try {
				if (File.Exists (filename))
					File.Delete (filename);
				StreamWriter sw = new StreamWriter (filename);
				sw.Write (CreateHeader ());
				foreach (string[] host in entries)
					sw.Write (host [0] + "\t" + host [1] + "\n");
				sw.Close ();
				return true;
			} catch (UnauthorizedAccessException) {
				Console.WriteLine ("You have no permission to write in " + filename + ", moron.");
				return false;
			}
		}
	}
}