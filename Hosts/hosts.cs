using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Linq;

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
			for (int i = 0; i < Sources.Count; i++) {
				Console.WriteLine (Sources [i]);
				try {
					string[] host = cl.DownloadString (Sources [i]).Split ('\n');
					for (int k = 0; k < host.Length; k++) {
						if (isRelevant (host [k]))
							entries.Add (new string[] { addr, Clean (host [k]) });
					}
				} catch (WebException e) {
					Console.WriteLine (e.Message);
				}
			}

			entries = entries.OrderBy (o => o [1]).ToList ();
		}

		bool isRelevant (string line)
		{
			line = line.Replace (" ", ",");
			if (line.StartsWith ("#") || line.Length == 0 || line.StartsWith ("\r") || line.Contains ("localhost") || line.Contains ("::1"))
				return false;
			return true;
		}

		string Clean (string line)
		{
			return line.Split (new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries) [1];
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
			} catch (UnauthorizedAccessException e) {
				Console.WriteLine ("You have no permission to write in " + filename + ", moron.");
				return false;
			}
		}
	}
}