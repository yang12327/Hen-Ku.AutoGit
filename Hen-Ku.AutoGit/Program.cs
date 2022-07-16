using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace Hen_Ku.AutoGit
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            LoadResourceDll.RegistDLL();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GitForm(args?.Length > 0));
        }
    }

    /// <summary> 載入資源中的動態連結庫(dll)檔案
    /// </summary>
    static class LoadResourceDll
    {
        static Dictionary<string, Assembly> Dlls = new Dictionary<string, Assembly>();
        static Dictionary<string, object> Assemblies = new Dictionary<string, object>();

        static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //程式集
            Assembly ass;
            //獲取載入失敗的程式集的全名
            var assName = new AssemblyName(args.Name).FullName;
            //判斷Dlls集合中是否有已載入的同名程式集
            if (Dlls.TryGetValue(assName, out ass) && ass != null)
            {
                Dlls[assName] = null;//如果有則置空並返回
                return ass;
            }
            else
            {
                throw new DllNotFoundException(assName);//否則丟擲載入失敗的異常
            }
        }

        /// <summary> 註冊資源中的dll
        /// </summary>
        public static void RegistDLL()
        {
            //獲取呼叫者的程式集
            var ass = new System.Diagnostics.StackTrace(0).GetFrame(1).GetMethod().Module.Assembly;
            //判斷程式集是否已經處理
            if (Assemblies.ContainsKey(ass.FullName))
            {
                return;
            }
            //程式集加入已處理集合
            Assemblies.Add(ass.FullName, null);
            //繫結程式集載入失敗事件(這裡我測試了,就算重複綁也是沒關係的)
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            //獲取所有資原始檔檔名
            var res = ass.GetManifestResourceNames();
            foreach (var r in res)
            {
                //如果是dll,則載入
                if (r.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var s = ass.GetManifestResourceStream(r);
                        var bts = new byte[s.Length];
                        s.Read(bts, 0, (int)s.Length);
                        var da = Assembly.Load(bts);
                        //判斷是否已經載入
                        if (Dlls.ContainsKey(da.FullName))
                        {
                            continue;
                        }
                        Dlls[da.FullName] = da;
                    }
                    catch
                    {
                        //載入失敗就算了...
                    }
                }
            }
        }
    }
}
