using System;
using System.IO;

namespace Hosts
{
    class MainClass
    {
        static string dest;
        static HostsManager h;

        public static void Main(string[] args)
        {
            h = new HostsManager();
            Load(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"/hosts/settings");

            Console.WriteLine("Downloading from sources...");
            if (h.GetHosts())
            {
                Console.WriteLine("Merging and saving...");
                if (h.Write(dest))
                    Console.WriteLine("Done.");
            }
        }

        static void Load(string path)
        {
            path = Path.GetFullPath(path);
            if (File.Exists(path))
            {
                Console.WriteLine("Loading config from: " + path);
                string[] settings = File.ReadAllText(path).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string setting in settings)
                {
                    string[] s = setting.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (s[0] == "source")
                        h.Sources.Add(s[1]);
                    else if (s[0] == "redirect")
                        h.Address = s[1];
                    else if (s[0] == "destination")
                        dest = s[1];
                }
            }
            else
            {
                Console.WriteLine("Loading default configuration in " + path);
                LoadDefaults(path);
            }
        }

        static void LoadDefaults(string path)
        {
            string file = "destination /etc/hosts\nredirect 0.0.0.0\n# Pro tip: order the sources by file size from the biggest to the smallest.\nsource http://hosts-file.net/ad_servers.txt\nsource http://winhelp2002.mvps.org/hosts.txt\nsource http://someonewhocares.org/hosts/hosts\nsource http://pgl.yoyo.org/adservers/serverlist.php?hostformat=hosts&showintro=0&mimetype=plaintext\nsource http://adaway.org/hosts.txt";
            if (!Directory.Exists(Directory.GetParent(path).ToString()))
                Directory.CreateDirectory(Directory.GetParent(path).ToString());
            File.WriteAllText(path, file);

            Load(path);
        }
    }
}
