using Gantry_Control.Service;
using System.Windows;

namespace Gantry_Control
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly CancellationTokenSource _appCts = new();
        private Task? _commTask;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var settings = CommSettings.Load();
            TcpComm.Instance.Initialize(settings.Ip, settings.Port);
            _commTask = TcpComm.Instance.RunAsync(_appCts.Token);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 워커 종료 시 DoFinalize에서 정지 명령 송신 후 연결 종료
            PlcData.Instance.StopJog();
            _appCts.Cancel();
            try
            {
                _commTask?.Wait(TimeSpan.FromSeconds(2));
            }
            catch (AggregateException) { }
            _appCts.Dispose();

            base.OnExit(e);
        }
    }
}
