using Gantry_Control.Service;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Gantry_Control
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly CancellationTokenSource _appCts = new();
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            TcpComm.Instance.Initialize("127.0.0.1", 8999);   // 실제 PLC IP/포트로 교체
            TcpComm.Instance.RunAsync(_appCts.Token);
        }
    }

}
