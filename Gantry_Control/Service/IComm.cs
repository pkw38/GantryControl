namespace Gantry_Control.Service
{
    internal interface IComm
    {
        bool IsConnected { get; }

        void Initialize(string ip, int port);
        Task RunAsync(CancellationToken ct);

        /// <summary>통신 워커 스레드에서 발생하므로 UI 갱신 시 Dispatcher 사용 필요</summary>
        event EventHandler<bool>? ConnectionChanged;
    }
}
