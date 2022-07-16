﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Hen_Ku.AutoGit
{
    public partial class GitForm : Form
    {
        Dictionary<string, string> ProjectPath = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Settings.Default.ProjectPath);
        Dictionary<string, string> ProjectInfo = new Dictionary<string, string>();
        void SaveProject()
        {
            Properties.Settings.Default.ProjectPath = JsonConvert.SerializeObject(ProjectPath);
            Properties.Settings.Default.Save();
            //Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Hen-Ku.DC\AutoGit", "ProjectInfo", );
        }

        Git Git;
        MqttClient client;
        string clientId;
        public GitForm(bool hide)
        {
            InitializeComponent();
            //var Temp = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Hen-Ku.DC\AutoGit", "ProjectInfo", null);
            //if (Temp == null) ProjectPath = new Dictionary<string, string>();
            //else ProjectPath = ;
            Git = new Git();
            Git.consoleLog = ConsoleLog;
            Git.updateBranch = updateBranch;
            foreach (var item in ProjectPath)
            {
                Git.SelectProject(item.Value);
                ProjectInfo[item.Key] = Git.GetUrl();
            }
            UpdateList();
            //Task.Run(Timer);
            client = new MqttClient("zxcv.cx");//MQTTServer在本機
            client.MqttMsgPublishReceived += GitEvent;//當接收到訊息時處理函式
            clientId = Guid.NewGuid().ToString();//取得唯一碼
            client.Connect(clientId);//建立連線
            client.Subscribe(new string[] { "AutoGit" }, new byte[] { 0 });
            WindowState = hide ? FormWindowState.Minimized : FormWindowState.Normal;
        }
        private void GitForm_Shown(object sender, EventArgs e)
        {
            Visible = WindowState != FormWindowState.Minimized;
        }

        List<string> Branch;
        void updateBranch(List<string> branch, string select)
        {
            Branch = branch;
            BeginInvoke(new invoke(updateBranch), select);
        }
        void updateBranch(string select)
        {
            comboBoxBranch.Items.Clear();
            comboBoxBranch.Items.AddRange(Branch.ToArray());
            if (select != null) comboBoxBranch.SelectedItem = select;
        }

        private delegate void invoke(string Content = null);
        void ConsoleLog(string value)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new invoke(ConsoleLog), value);
                return;
            }
            Log.Text += $"{DateTime.Now.ToString("d:HH:mm:ss")} {value}\r\n";
            Log.SelectionStart = Log.Text.Length;
            Log.ScrollToCaret();
        }

        void UpdateList()
        {
            var list = ProjectPath.Keys.ToList();
            list.Add("瀏覽其他專案");
            comboBoxProject.Items.Clear();
            comboBoxProject.Items.AddRange(list.ToArray());
            comboBoxProject_SelectedIndexChanged(this, null);
        }

        //void Timer()
        //{
        //    var Git = new Git();
        //    Git.consoleLog = ConsoleLog;
        //    while (true)
        //    {
        //        Task.Delay((60 - DateTime.Now.Second) * 1000).Wait();
        //        try
        //        {
        //            foreach (var item in ProjectPath)
        //                try
        //                {
        //                    Git.SelectProject(item.Value);
        //                    Git.UpdateBranch();
        //                }
        //                catch (Exception ex) { ConsoleLog(" *** 專案錯誤：" + ex.Message); }
        //        }
        //        catch (Exception ex) { ConsoleLog(" *** 循環錯誤：" + ex.Message); }
        //    }
        //}
        #region 選擇專案
        private void comboBoxProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxBranch.Items.Clear();
            if (comboBoxProject.SelectedIndex == -1)
            {
                buttonDel.Enabled = false;
                buttonSync.Enabled = false;
                return;
            }
            else if (comboBoxProject.SelectedItem.ToString() == "瀏覽其他專案")
            {
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    List<string> ProjectPath = folderBrowserDialog1.SelectedPath.Split('\\').ToList();
                    string name = ProjectPath[ProjectPath.Count - 1];
                    string path = folderBrowserDialog1.SelectedPath;
                    while (this.ProjectPath.ContainsKey(name) && this.ProjectPath[name] != path)
                        name += "+";
                    this.ProjectPath[name] = path;
                    SaveProject();
                    UpdateList();
                    comboBoxProject.SelectedItem = name;
                }
                else
                    comboBoxProject.SelectedIndex = -1;
            }
            else
            {
                buttonDel.Enabled = true;
                buttonSync.Enabled = true;
                var name = comboBoxProject.SelectedItem.ToString();
                Git.SelectProject(ProjectPath[name]);
                Git.GetBranch();
                ProjectInfo[name] = Git.GetUrl();
            }
        }

        #endregion
        #region 變更分支
        private void comboBoxBranch_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxBranch.SelectedIndex >= 0)
                Git.SelectBranch(comboBoxBranch.SelectedItem.ToString());
        }

        #endregion
        #region 刪除專案
        private void buttonDel_Click(object sender, EventArgs e)
        {
            if (comboBoxProject.SelectedIndex >= 0 &&
                MessageBox.Show($"您確定要停止自動更新專案「{comboBoxProject.SelectedItem}」",
                "刪除專案", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ProjectPath.Remove(comboBoxProject.SelectedItem.ToString());
                SaveProject();
                UpdateList();
            }
        }

        #endregion
        #region 更新專案
        private void buttonSync_Click(object sender, EventArgs e)
        {
            if (comboBoxBranch.SelectedIndex >= 0)
                Git.UpdateBranch();
        }

        #endregion

        private void 還原視窗ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            Focus();
        }

        private void 關閉程式ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Environment.Exit(0);
        }

        private void GitForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            Visible = !Visible;
            if (Visible)
            {
                WindowState = FormWindowState.Normal;
                Focus();
            }
        }

        void GitEvent(object sender, MqttMsgPublishEventArgs e)
        {
            string CloneUrl = System.Text.Encoding.UTF8.GetString(e.Message);
            var list = ProjectInfo.Where(x => x.Value == CloneUrl || x.Value == CloneUrl + ".git").ToList();
            if (list.Count == 0) return;
            var Git = new Git();
            Git.consoleLog = ConsoleLog;
            foreach (var item in list)
                try
                {
                    Git.SelectProject(ProjectPath[item.Key]);
                    Git.UpdateBranch();
                }
                catch (Exception ex) { ConsoleLog(" *** 更新發生錯誤：" + ex.Message); }
        }
    }
}
