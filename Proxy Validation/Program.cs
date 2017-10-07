using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace Proxy_Validation {
    class Program {
        static int curIP = 0;
        static int totalScanned = 0;
        static HashSet<string> ips = new HashSet<string>();
        static void Main(string[] args) {
            // Just in case the proxies doesn't exist.
            if (!File.Exists("./proxies.txt")) {
                Console.WriteLine("Couldn't find proxies.txt!");
                Console.ReadKey();
                return;
            }
            string url = "https://www.google.com";
            string proxiesLocation = "./proxies.txt";
            foreach (string s in File.ReadAllLines(proxiesLocation)) {
                string proxy = s.Split(':')[0];
                int port = int.Parse(s.Split(':')[1]);
                new Thread(f => Work(url, proxy, port)).Start();
                Thread.Sleep(50);
                totalScanned++;
            }
            File.WriteAllLines("./aliveproxies.txt", ips);
            Console.ReadKey();
        }

        // Multithreaded worker.
        static void Work(string url, string proxy, int port) {
            string rv = connect(url, true, proxy, port, 5000);
            string proxyText = proxy + ":" + port;
            int ms;
            if (int.TryParse(rv, out ms)) {
                curIP++;
                string output = curIP + ".) " + proxyText +
                    new string(' ', 25 - proxyText.Length) + "=> " + ms;
                Console.WriteLine(output);
                ips.Add(proxyText);
            }
            Console.Title = curIP + " / " + totalScanned;
        }

        // Connect to the remote server.
        static string connect(string url, bool useProxy, string proxy = "", int port = -1, 
            int timeout = 3000) {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                if (useProxy) {
                    WebProxy myproxy = new WebProxy(proxy, port);
                    myproxy.BypassProxyOnLocal = false;
                    request.Proxy = myproxy;
                }
                request.Method = "GET";
                request.Timeout = timeout;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            } catch (Exception ex) {
                return ex.Message;
            }
            sw.Stop();
            return sw.ElapsedMilliseconds.ToString();
        }
    }
}
