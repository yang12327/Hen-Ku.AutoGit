﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Hen_Ku.AutoGit
{
    class Git
    {
        CommandLine CMD;
        public Git()
        {
            CMD = new CommandLine("cmd.exe", "/k");
            CMD.Output = Result;
            CMD.Error = Fail;
        }

        private delegate void invoke(string Content = null);
        string LastCommand = "";
        List<string> TempContent = new List<string>();

        void Result(string Content)
        {
            if (Content.Trim() == "") return;
            if (Content.Contains(">") && Content.Contains("#end"))
            {
                TempContent.Clear();
                LastCommand = Content.Substring(Content.IndexOf(">") + 1);
                ConsoleLog = $">{LastCommand}";
                return;
            }
            //ConsoleLog = Content;
            if (Content == "#end")
            {
                if (LastCommand.Contains("git branch"))
                {
                    var Branch = new List<string>();
                    string select = null;
                    foreach (var item in TempContent)
                    {
                        var S = item.Substring(2).Split('/');
                        var name = S[S.Length - 1];
                        if (item.Substring(0, 1) == "*") select = name;
                        if (!Branch.Contains(name)) Branch.Add(name);
                    }
                    if (updateBranch != null)
                        updateBranch(Branch, select);
                }
                else if (LastCommand.Contains("git pull origin"))
                {
                    var Error = TempContent.FindAll(x => x.Contains("error:") || x.Contains("fatal:"));
                    if (Error.Count() > 0) ConsoleLog = $"更新失敗 {Error[0]}";
                    else ConsoleLog = "更新成功";
                }
                return;
            }
            else
            {
                TempContent.Add(Content);
            }
        }
        void Fail(string Content)
        {
            if (LastCommand.Contains("git branch"))
            {
                ConsoleLog = "無法取得Git資料";
            }
            else
                ConsoleLog = " *** " + Content;
        }

        public Action<string> consoleLog = null;
        string ConsoleLog
        {
            set
            {
                if (consoleLog != null)
                    consoleLog(value);
            }
        }

        public Action<List<string>, string> updateBranch = null;
        public void SelectProject(string path)
        {
            CMD.Send($"cd {path}");
            CMD.Send($"git branch -a & echo #end");
        }
        public void SelectBranch(string name)
        {
            CMD.Send($"git checkout {name} & echo #end");
        }
        public void UpdateBranch()
        {
            CMD.Send($"git pull origin & echo #end");
        }
    }
}
