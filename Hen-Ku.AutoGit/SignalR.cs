using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hen_Ku.AutoGit
{
    public class SignalR
    {
        public IHubProxy HubProxy;
        private HubConnection Connection;

        public SignalR(string Url, string HubName, Action OnDisconnected = null)
        {
            Connection = new HubConnection(Url);
            Connection.Closed += OnDisconnected;
            HubProxy = Connection.CreateHubProxy(HubName);
            while (true)
            {
                try
                {
                    Connection.Start().Wait();
                    break;
                }
                catch (Exception) {
                    new System.Net.WebClient().DownloadData(Url);
                }
            }
        }

        public void Close()
        {
            if (Connection != null)
            {
                Connection.Stop();
                Connection.Dispose();
            }
        }

        public void Send(string Function, string Message)
        {
            if (Connection.State != ConnectionState.Disconnected)
                HubProxy.Invoke(Function, Message);
        }
    }
}
