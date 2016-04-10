using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Hosts
{
	public class HostsManager
	{
		public string Header { get; set; }

		public HostsManager ()
		{
		}

		/// <summary>
		/// Downloads the file from the specified URL
		/// </summary>
		/// <param name="source">The URL</param>
		/// <returns></returns>
		public string[] Download (string source)
		{
			WebClient cl = new WebClient ();
			cl.Proxy.Credentials = CredentialCache.DefaultCredentials;
			try {
				return cl.DownloadString (source).Split ('\n');
			} catch (WebException e) {
				Console.BackgroundColor = ConsoleColor.Red;
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine (e.Message);
				Console.ResetColor ();
			}
			return default(string[]);
		}

		/// <summary>
		/// Parses each line of the array and returns a host-formatted list
		/// </summary>
		/// <param name="source">The list of entries </param>
		/// <returns>The formatted and cleaned list of entries</returns>
		public List<string[]> Parse (string[] source, string address)
		{
			List<string[]> outp = new List<string[]> ();
			string[] entry;

			for (int i = 0; i < source.Length; i++) {
				if (isRelevant (source [i])) {
					entry = new string[] { address, Clean (source [i]) };
					outp.Add (entry);
				}
			}
			return outp;
		}

		/// <summary>
		/// Check if the given host already exists in the given list
		/// </summary>
		/// <param name="list">The list to check</param>
		/// <param name="arr">The element to verify</param>
		/// <returns></returns>
		public bool Exists (List<string[]> list, string[] arr)
		{
			for (int i = 0; i < list.Count; i++)
				if (list [i] [1] == arr [1])
					return true;
			return false;
		}

		/// <summary>
		/// Checks wether the string given is a valid host entry
		/// </summary>
		/// <param name="line">The line to check</param>
		/// <returns>If the line is valid</returns>
		bool isRelevant (string line)
		{
			line = line.Replace (" ", ",");
			if (line.StartsWith ("#") || line.Length == 0 || line.StartsWith ("\r") || line.Contains ("localhost"))
				return false;
			return true;
		}

		/// <summary>
		/// Cleans the entry
		/// </summary>
		/// <param name="line">The string to clean</param>
		/// <returns>The cleaned string</returns>
		string Clean (string line)
		{
			return line.Split (new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries) [1].Replace ("\r", "");
		}

		/// <summary>
		/// Writes the file to disk
		/// </summary>
		/// <param name="filename">The path of the file</param>
		/// <returns>If the operation succedded</returns>
		public bool Write (string filename, List<string[]> entries, string[] header)
		{
			try {
				if (File.Exists (filename))
					File.Delete (filename);
				StreamWriter sw = new StreamWriter (filename);
				sw.Write (header);
				foreach (string[] host in entries)
					sw.Write (host [0] + "\t" + host [1] + "\n");
				sw.Close ();
				Console.WriteLine (entries.Count + " entries written successfully.");
				return true;
			} catch (UnauthorizedAccessException) {
				Console.BackgroundColor = ConsoleColor.Red;
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine ("You have no permission to write in " + filename + ", moron.");
				Console.ResetColor ();
				return false;
			}
		}
	}
}