using Gantry_Control.Common;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace Gantry_Control.Service
{
    internal class TcpComm : TaskWorkerBase, IComm
    {
        private static readonly TcpComm _instance = new TcpComm();
        public static TcpComm Instance => _instance;

        private const byte Stx = 0x02;
        private const byte Etx = 0x03;
        private const int FrameLength = 10;
        private const int ConnectTimeoutMs = 1000;
        private const int RxTimeoutMs = 300;

        private TcpClient? _tcpClient;
        private NetworkStream? _stream;

        private string _ip = "127.0.0.1";
        private int _port;

        private readonly byte[] _writeBuffer = new byte[FrameLength];
        private readonly byte[] _readChunk = new byte[256];
        private readonly List<byte> _rxBuffer = new();
        private long _lastRxTick;

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            private set
            {
                if (_isConnected == value) return;
                _isConnected = value;
                Debug.WriteLine($"[{_name}] Connection {(value ? "established" : "lost")}");
                ConnectionChanged?.Invoke(this, value);
            }
        }

        public event EventHandler<bool>? ConnectionChanged;

        private TcpComm() : base("TcpComm", 20) { }

        public void Initialize(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        protected override async Task WorkRoutineAsync(CancellationToken ct)
        {
            if (IsConnected == false)
            {
                await ReconnectAsync(ct);
                if (IsConnected == false) return;
            }

            try
            {
                // PLC는 한 주기에 1프레임만 읽으므로, 앱이 더 빨리 보내면 PLC 수신 버퍼에 명령이 쌓여 반응이 밀림
                if (ReadPLC() > 0)
                {
                    _lastRxTick = Environment.TickCount64;
                    WritePLC();
                }
                else if (Environment.TickCount64 - _lastRxTick > RxTimeoutMs)
                {
                    // PLC가 응답하지 않는 연결은 끊고 재연결
                    Debug.WriteLine($"[{_name}] No data from PLC for {RxTimeoutMs}ms");
                    CloseConnection();
                }
            }
            catch (Exception ex) when (ex is IOException || ex is SocketException || ex is ObjectDisposedException)
            {
                Debug.WriteLine($"[{_name}] I/O failed: {ex.Message}");
                CloseConnection();
            }
        }

        protected override void DoFinalize()
        {
            // 종료 시 반드시 정지 명령을 한 번 보내고 연결을 닫는다
            PlcData.Instance.StopJog();
            try
            {
                if (IsConnected) WritePLC();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{_name}] Final stop write failed: {ex.Message}");
            }
            CloseConnection();
        }

        private async Task ReconnectAsync(CancellationToken ct)
        {
            CloseConnection();

            var client = new TcpClient();
            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(ConnectTimeoutMs);
                await client.ConnectAsync(_ip, _port, timeoutCts.Token);

                client.NoDelay = true;
                _tcpClient = client;
                _stream = client.GetStream();
                _rxBuffer.Clear();
                _lastRxTick = Environment.TickCount64;
                IsConnected = true;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                client.Dispose();
                throw;
            }
            catch (Exception ex)
            {
                client.Dispose();
                Debug.WriteLine($"[{_name}] Connect to {_ip}:{_port} failed: {ex.Message}");
            }
        }

        private void CloseConnection()
        {
            _stream?.Dispose();
            _stream = null;
            _tcpClient?.Dispose();
            _tcpClient = null;
            IsConnected = false;
        }

        private void WritePLC()
        {
            if (_stream == null) return;

            var cmd = PlcData.Instance.Command;

            _writeBuffer[0] = Stx;
            _writeBuffer[1] = cmd.JogDirection; //jog direction
            _writeBuffer[2] = cmd.JogSpeed;     //jog speed
            _writeBuffer[3] = cmd.CmdCode;      //1회성 명령 코드
            _writeBuffer[4] = cmd.CmdSeq;       //1회성 명령 번호
            _writeBuffer[5] = 10;
            _writeBuffer[6] = 10;
            _writeBuffer[7] = 10;
            _writeBuffer[8] = 10;
            _writeBuffer[9] = Etx;

            _stream.Write(_writeBuffer, 0, _writeBuffer.Length);
        }

        /// <returns>이번에 수신한 완전한 프레임 수</returns>
        private int ReadPLC()
        {
            if (_stream == null || _tcpClient == null) return 0;

            // 읽을 수 있다고 표시되는데 데이터가 0이면 상대가 연결을 끊은 것
            var socket = _tcpClient.Client;
            if (socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0)
            {
                throw new IOException("Remote host closed the connection");
            }

            while (_stream.DataAvailable)
            {
                int readCount = _stream.Read(_readChunk, 0, _readChunk.Length);
                if (readCount <= 0)
                {
                    throw new IOException("Remote host closed the connection");
                }
                _rxBuffer.AddRange(_readChunk.AsSpan(0, readCount));
            }

            return ParseFrames();
        }

        /// <summary>STX로 시작하고 ETX로 끝나는 고정 길이 프레임을 수신 버퍼에서 추출</summary>
        /// <returns>추출한 프레임 수</returns>
        private int ParseFrames()
        {
            int frameCount = 0;
            while (true)
            {
                int stxIndex = _rxBuffer.IndexOf(Stx);
                if (stxIndex < 0)
                {
                    _rxBuffer.Clear();
                    return frameCount;
                }
                if (stxIndex > 0)
                {
                    _rxBuffer.RemoveRange(0, stxIndex);
                }
                if (_rxBuffer.Count < FrameLength)
                {
                    return frameCount;
                }
                if (_rxBuffer[FrameLength - 1] != Etx)
                {
                    // 잘못된 프레임: 현재 STX를 버리고 다음 STX부터 재동기화
                    _rxBuffer.RemoveAt(0);
                    continue;
                }

                var frame = _rxBuffer.GetRange(0, FrameLength);
                _rxBuffer.RemoveRange(0, FrameLength);
                HandleFrame(frame);
                frameCount++;
            }
        }

        private void HandleFrame(List<byte> frame)
        {
            // TODO: 나머지 필드는 실제 프로토콜에 맞게 파싱
            int x = (frame[1] << 8) + frame[2];
            int y = (frame[3] << 8) + frame[4];
            int z = (frame[5] << 8) + frame[6];
            byte result = frame[8];     // bit0 = 거부, bit1 = 홈 실행 중, bit2 = 홈 완료, bit3 = 홈 실패
            PlcData.Instance.UpdateStatus(new PlcStatus
            {
                X = x,
                Y = y,
                Z = z,
                AckSeq = frame[7],
                CmdRejected = (result & 0x01) != 0,
                HomeBusy = (result & 0x02) != 0,
                HomeDone = (result & 0x04) != 0,
                HomeError = (result & 0x08) != 0,
            });
            Debug.WriteLine($"수신: {string.Join(",", frame)}, position: {x}");
        }
    }
}
