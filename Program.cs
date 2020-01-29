using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PortScanner
{
    class Engine
    {
        static void Main(string[] args)
        {
            //set default values
            string host;
            int portstart = 1, portstop = 65535, ctrthread = 200;
            bool verbose = false;

            //set first argument to target host IP
            try
            {
                host = args[0];
            }
            catch
            {
                printUsage();
                return;
            }

            //parse command line options and set variables accordingly
            try
            {
                if (args.Length >= 0)
                {
                    try
                    {
                        for (int i = 0; i <= args.Length; i++)
                        {
                            string argument = args[i];
                            try
                            {
                                switch (argument)
                                {
                                    case "-b":
                                    case "-begin":
                                        portstart = int.Parse(args[i + 1]);
                                        break;

                                    case "-e":
                                    case "-end":
                                        portstop = int.Parse(args[i + 1]);
                                        break;

                                    case "-t":
                                    case "-threads":
                                        ctrthread = int.Parse(args[i + 1]);
                                        break;

                                    case "-v":
                                    case "-verbose":
                                        verbose = true;
                                        break;

                                    default:
                                        break;
                                }
                            }
                            catch
                            {
                                printUsage();
                            }
                        }
                    }
                    catch
                    {
                        printUsage();
                    }
                }
            }
            catch
            { 
                printUsage();
                return;
            }

            //Initialize scan
            Console.WriteLine("Scanning " + host + " from port " + portstart + " to " + portstop);
            PortScanner ps = new PortScanner(host, portstart, portstop, verbose, ctrthread);
            ps.start(ctrthread);
        }

        static void printUsage()
        {
            /* usage */
        }
    }

    public class PortScanner
    {
        private string host;
        private int portstart, portstop, ptr;
        private bool verbose;

        //Declarations
        public PortScanner(string host, int portstart, int portstop, bool verbose, int ctrthread)
        {
            this.host = host;
            this.portstart = portstart;
            this.portstop = portstop;
            this.verbose = verbose;
            this.ptr = portstart - 1;
        }
        public PortScanner(string host) : this(host, 1, 65535, false, 200) { }
        public PortScanner() : this("127.0.0.1") { }

        //Start run with multiple threads
        public void start(int threadctr)
        {
            for (int i = 0; i < threadctr; i++)
            {
                Thread th = new Thread(new ThreadStart(run));
                th.Start();
            }
        }

        //check if more ports to scan
        public bool hasmore()
        {
            return (portstop - ptr) > 0;
        }
        //if there are more integers available, continue
        public int getnext()
        {
            if (hasmore())
            {
                return ptr += 1;
            }
            return -1;
        }

        //run the port scan
        public void run()
        {
            int port = portstart;
            TcpClient tcp = new TcpClient();

            //While there are more ports to scan (utilizes getnext() and hasmore())
            while ((port = getnext()) != -1)
            {
                Thread.Sleep(1000);
                //try initailizing a tcp connection
                try
                {
                    //connect to port
                    if (verbose == true)
                    {
                        Console.WriteLine("trying " + port + " " + host);
                    }
                    tcp = new TcpClient(host, port);
                }
                catch
                {
                    //label closed if error caught and move to next port
                    if (verbose == true)
                    {
                        Console.WriteLine("Port " + port + " is Closed");
                    }
                    continue;
                }
                finally
                {
                    //close successful connection
                    try
                    {
                        tcp.Close();
                    }
                    catch { }
                }
                Console.WriteLine("TCP port " + port + " is open");
            }
        }
    }
}