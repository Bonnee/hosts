using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Hosts
{
    public class HostsManager
    {
        List<string[]> entries;
        public List<string[]> Hosts { get { return entries; } }

        string addr;
        public string Address
        {
            get { return addr; }
            set { addr = value; }
        }

        public string Header { get; set; }

        public List<string> Sources { get; set; }

        public HostsManager()
        {
            entries = new List<string[]>();
            Sources = new List<string>();
        }

        public HostsManager(string address, string[] sources)
        {
            entries = new List<string[]>();
            Address = address;
            Sources = new List<string>(sources);
        }

        /// <summary>
        /// Fetches the hosts files from all the sources and adds them to "Hosts" list
        /// </summary>
        /// <returns>If the operation succedded.</returns>
        public bool GetHosts()
        {
            WebClient cl = new WebClient();
            cl.Proxy.Credentials = CredentialCache.DefaultCredentials;

            string[] entry;
            int index;
            int errors = 0;

            foreach (string source in Sources)
            {
                Console.Write(Sources.IndexOf(source) + 1 + "/" + Sources.Count + " " + source + "...");
                try
                {
                    string[] host = cl.DownloadString(source).Split('\n');
                    Console.Write("Adding...");
                    index = Console.CursorLeft;
                    int add = 0;

                    for (int i = 0; i < host.Length; i++)
                    {
                        if (isRelevant(host[i]))
                        {
                            entry = new string[] { addr, Clean(host[i]) };
                            if (!Exists(entries, entry))
                            {
                                entries.Add(entry);
                                Console.CursorLeft = index;
                                add++;
                                Console.Write(add + "/" + host.Length);
                            }
                        }
                    }
                    Console.Write("...Done.\n");
                    add = 0;

                }
                catch (WebException e)
                {
                    errors++;
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                }
            }
            if (errors >= Sources.Count)
                return false;
            entries = entries.OrderBy(o => o[1]).ToList();
            return true;
        }

        /// <summary>
        /// Check if the given host already exists in the given list
        /// </summary>
        /// <param name="list">The list to check</param>
        /// <param name="arr">The element to verify</param>
        /// <returns></returns>
        bool Exists(List<string[]> list, string[] arr)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i][1] == arr[1])
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks wether the string given is a valid host entry
        /// </summary>
        /// <param name="line">The line to check</param>
        /// <returns>If the line is valid</returns>
        bool isRelevant(string line)
        {
            line = line.Replace(" ", ",");
            if (line.StartsWith("#") || line.Length == 0 || line.StartsWith("\r") || line.Contains("localhost"))
                return false;
            return true;
        }

        /// <summary>
        /// Cleans the entry
        /// </summary>
        /// <param name="line">The string to clean</param>
        /// <returns>The cleaned string</returns>
        string Clean(string line)
        {
            return line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("\r", "");
        }

        /// <summary>
        /// Creates the header for the hosts file
        /// </summary>
        /// <returns>The header</returns>
        string CreateHeader()
        {
            string header = "# Host file courtesy of SuperBonny.\n# This list was updated on " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToShortTimeString() + "\n# There are " + entries.Count + " hosts present.\n# These are the sources:\n#\n# ";
            foreach (string s in Sources)
            {
                header += s + "\n# ";
            }
            header += "\n127.0.0.1 localhost\n::1 localhost\n\n# [START OF GENERATED ENTRIES]\n#\n";
            return header;
        }

        /// <summary>
        /// Writes the file to disk
        /// </summary>
        /// <param name="filename">The path of the file</param>
        /// <returns>If the operation succedded</returns>
        public bool Write(string filename)
        {
            try
            {
                if (File.Exists(filename))
                    File.Delete(filename);
                StreamWriter sw = new StreamWriter(filename);
                sw.Write(CreateHeader());
                foreach (string[] host in entries)
                    sw.Write(host[0] + "\t" + host[1] + "\n");
                sw.Close();
                Console.WriteLine(entries.Count + " entries written successfully.");
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("You have no permission to write in " + filename + ", moron.");
                Console.ResetColor();
                return false;
            }
        }
    }
}