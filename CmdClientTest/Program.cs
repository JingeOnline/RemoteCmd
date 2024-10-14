using System.Diagnostics;

namespace CmdClientTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //TestCmdMapping();
            string location = AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine(location);
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            Console.WriteLine(path);
            Console.ReadLine();
        }

        static void TestCmdMapping()
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "powershell.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;

            cmd.Start();
            cmd.OutputDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
            cmd.ErrorDataReceived += (sender, e) => { Console.WriteLine(e.Data); };

            cmd.BeginOutputReadLine();
            cmd.BeginErrorReadLine();
            while (true)
            {
                string s = Console.ReadLine();
                cmd.StandardInput.WriteLine(s);
                if (s == "exit")
                {
                    cmd.StandardInput.Close();
                    cmd.WaitForExit();
                    break;
                }
            }
        }
    }
}
