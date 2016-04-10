using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hosts
{
	class MainClass
	{
		static object locker = new object ();

		static string dest;
		static HostsManager h;

		static List<string> sources;
		static List<string[]> entries;
		static string address = "0.0.0.0";

		public static void Main (string[] args)
		{
			h = new HostsManager ();
			sources = new List<string> ();
			entries = new List<string[]> ();

			Load (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData) + @"/hosts/settings");

			Console.WriteLine ("Downloading from sources...");
			List<Task> workers = new List<Task> ();

			for (int i = 0; i < sources.Count; i++) {
				int ind = i;
				workers.Add (new Task (() => Do (ind)));
				workers [ind].Start ();
			}

			foreach (Task worker in workers) {
				worker.Wait ();
			}

			h.Write (dest, entries, CreateHeader ());
		}

		static void Do (int index)
		{
			Console.WriteLine (sources [index]);

			foreach (string[] entry in h.Parse (h.Download (sources[index]),address))
				if (!h.Exists (entries, entry)) {
					lock (locker)
						entries.Add (entry);
				}
		}

		static string[] CreateHeader ()
		{
			string header = "# Host file courtesy of SuperBonny.\n# This list was updated on " + DateTime.Now.ToShortDateString () + " at " + DateTime.Now.ToShortTimeString () + "\n# There are " + entries.Count + " hosts present.\n# These are the sources:\n#\n# ";
			foreach (string s in sources) {
				header += s + "\n# ";
			}
			header += "\n127.0.0.1 localhost " + System.Environment.MachineName + "\n::1 localhost\n\n# [START OF GENERATED ENTRIES]\n#\n";
			return header.Split ('\n');
		}

		static void Load (string path)
		{
			path = Path.GetFullPath (path);
			if (File.Exists (path)) {
				Console.WriteLine ("Loading config from: " + path);
				string[] settings = File.ReadAllText (path).Split (new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

				foreach (string setting in settings) {
					string[] s = setting.Split (new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

					if (s [0] == "source")
						sources.Add (s [1]);
					else if (s [0] == "redirect")
						address = s [1];
					else if (s [0] == "destination")
						dest = s [1];
				}
			} else {
				Console.WriteLine ("Loading default configuration in " + path);
				LoadDefaults (path);
			}
		}

		static void LoadDefaults (string path)
		{
			string file = "destination /etc/hosts\nredirect 0.0.0.0\n# Pro tip: order the sources by file size from the biggest to the smallest.\nsource http://hosts-file.net/ad_servers.txt\nsource http://winhelp2002.mvps.org/hosts.txt\nsource http://someonewhocares.org/hosts/hosts\nsource http://pgl.yoyo.org/adservers/serverlist.php?hostformat=hosts&showintro=0&mimetype=plaintext\nsource http://adaway.org/hosts.txt";
			if (!Directory.Exists (Directory.GetParent (path).ToString ()))
				Directory.CreateDirectory (Directory.GetParent (path).ToString ());
			File.WriteAllText (path, file);

			Load (path);
		}
	}
}
