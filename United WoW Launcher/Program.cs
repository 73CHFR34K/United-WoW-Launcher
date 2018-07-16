using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace United_WoW_Launcher
{
    public class Program
    {
        private static void Finish()
        {
            Program.text.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                Environment.SetEnvironmentVariable("upd", "yes");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    FileName = "Wow.exe",
                    WindowStyle = ProcessWindowStyle.Normal,
                    Arguments = null
                };
                try
                {
                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "United WoW Launcher");
                }
                Application.Current.Shutdown();
            }), new object[0]);
        }

        private static void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (Program.downloads.Any<Download>())
            {
                Program.currentFile = Program.downloads[0].Key;
                Program.currentUrl = Program.downloads[0].URL;
                Program.downloads.RemoveAt(0);
                Program.downloaded++;
                try
                {
                    Program.wc.DownloadFileAsync(new Uri(Program.currentUrl + Program.currentFile), (Program.currentFile == "United WoW Launcher.exe") ? "United WoW Launcher.exe.tmp" : Program.currentFile);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "United WoW Launcher");
                    return;
                }
            }
            Program.Finish();
        }

        private static void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            long num = (e.BytesReceived + 1L) / 1024L;
            long num2 = (e.TotalBytesToReceive + 1L) / 1024L;
            string str = string.Format("[{2}/{3} files] Downloading {0}, {1}% downloaded, ({4}kb / {5}kb)", new object[]
            {
                Program.currentFile,
                e.ProgressPercentage,
                Program.downloaded,
                Program.downloadsCount,
                num,
                num2
            });
            Program.text.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                Program.text.Text = str;
            }), new object[0]);
        }

        public void Download()
        {
            if (!Program.downloads.Any<Download>())
            {
                return;
            }
            Program.wc = new WebClient();
            Program.wc.DownloadFileCompleted += Program.DownloadFileCompleted;
            Program.wc.DownloadProgressChanged += Program.DownloadProgressChanged;
            Program.currentFile = Program.downloads[0].Key;
            Program.currentUrl = Program.downloads[0].URL;
            Program.downloads.RemoveAt(0);
            Program.downloaded++;
            try
            {
                Program.wc.DownloadFileAsync(new Uri(Program.currentUrl + Program.currentFile), (Program.currentFile == "United WoW Launcher.exe") ? "United WoW Launcher.exe.tmp" : Program.currentFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "United WoW Launcher");
            }
        }

        public List<Download> GetDataFileInfo(string url)
        {
            List<Download> list = new List<Download>();
            using (XmlTextReader xmlTextReader = new XmlTextReader(url))
            {
                string text = string.Empty;
                while (xmlTextReader.MoveToNextAttribute() || xmlTextReader.Read())
                {
                    XmlNodeType nodeType = xmlTextReader.NodeType;
                    if (nodeType != XmlNodeType.Attribute)
                    {
                        if (nodeType == XmlNodeType.Text)
                        {
                            try
                            {
                                if (text.Length != 0)
                                {
                                    Download item = new Download
                                    {
                                        Key = xmlTextReader.Value,
                                        Hash = text,
                                        URL = url.Replace("/data.xml", "")
                                    };
                                    list.Add(item);
                                }
                            }
                            catch (Exception)
                            {
                            }
                            text = string.Empty;
                        }
                    }
                    else
                    {
                        text = xmlTextReader.Value;
                    }
                }
            }
            return list;
        }

        public void FileCheck(List<Download> list)
        {
            using (MD5 md = MD5.Create())
            {
                foreach (Download download in list)
                {
                    bool flag = !File.Exists(download.Key);
                    string directoryName = Path.GetDirectoryName(download.Key);
                    if (directoryName.Length != 0 && !Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    Download item = new Download
                    {
                        Key = download.Key,
                        Hash = download.Hash,
                        URL = download.URL
                    };
                    if (!flag)
                    {
                        using (FileStream fileStream = File.OpenRead(download.Key))
                        {
                            if (download.Hash != md.ComputeHash(fileStream).ToHex(false))
                            {
                                flag = true;
                            }
                        }
                    }
                    if (flag)
                    {
                        Program.downloads.Add(item);
                    }
                }
            }
        }

        public void Update()
        {
            try
            {
                foreach (string url in this.Servers)
                {
                    List<Download> dataFileInfo = this.GetDataFileInfo(url);
                    this.FileCheck(dataFileInfo);
                }
                if ((Program.downloadsCount = Program.downloads.Count) != 0)
                {
                    this.Download();
                }
                else
                {
                    Program.Finish();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "United WoW Launcher");
            }
        }

        private static string channel = "prerelease/";

        public static ProgressBar pbar;

        public static TextBlock text;

        private static List<Download> downloads = new List<Download>();

        private static WebClient wc;

        private static string currentFile;

        private static string currentUrl;

        private static int downloadsCount;

        private static int downloaded;

        private readonly List<string> Servers = new List<string>
        {
            string.Format("http://updater.united-wow.com/{0}/data.xml", Program.channel)
        };
    }
}
