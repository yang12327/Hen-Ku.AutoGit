using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hen_Ku.AutoGit
{
    class CommandLine
    {
        public Process Process = new Process();
        public Action<string> Output = null;
        public Action<string> Error = null;
        public CommandLine(string FileName, string Args = "")
        {
            Process.StartInfo.FileName = FileName;
            Process.StartInfo.Arguments = Args;
            Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.RedirectStandardInput = true;
            Process.StartInfo.RedirectStandardError = true;
            Process.StartInfo.CreateNoWindow = true;
            Process.Start();
            Task.Run(ReadOutput);
            Task.Run(ReadError);
        }

        public bool Send(string command)
        {
            try { Process.StandardInput.WriteLine(command); }
            catch { return false; }
            return true;
        }

        void ReadOutput()
        {
            while (!Process.HasExited)
            {
                string Content = Process.StandardOutput.ReadLine();
                if (Output != null) Output(Content);
            }
            Process.WaitForExit();
        }
        void ReadError()
        {
            while (!Process.HasExited)
            {
                string Content = Process.StandardError.ReadLine();
                if (Error != null) Error(Content);
            }
            Process.WaitForExit();
        }
    }
}
