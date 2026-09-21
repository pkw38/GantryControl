using Gantry_Control.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Gantry_Control.Service
{
    internal class TcpComm : TaskWorkerBase
    {
        private static readonly TcpComm _instance = new TcpComm();
        public static TcpComm Instance => _instance;
        private TcpClient _tcpClient = new TcpClient();

        private string _ip;
        private int _port;

        private readonly object _writeLock = new();

        private byte[] _writeBuffer = new byte[10];
        private byte[] _readBuffer = new byte[10];

        private byte _jogDirection;
        private byte _jogSpeed;

        public event EventHandler<bool> ConnectionChanged;

        public TcpComm() : base("TcpComm", 20) { }

        public void Initialize(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public void SetJog(byte direction, byte speed)
        {
            lock (_writeLock)
            {
                _jogDirection = direction;
                _jogSpeed = speed;
            }
        }

        protected override async Task WorkRoutineAsync(CancellationToken ct)
        {
            if(_tcpClient.Connected == false)
            {
                SafeReconnect();
            }

            if (_tcpClient.Connected == true) 
            {
                if (ReadPLC(ct) == true)
                {
                    WritePLC();
                }
                else
                {
                    _tcpClient.Close();
                }
            }
        }

        private void SafeReconnect()
        {
            try
            {
                var tcpClient = _tcpClient;
                _tcpClient = new TcpClient();
                if (tcpClient?.Connected ?? false)
                {
                    tcpClient.Close();
                }

                var tcpCt = new CancellationTokenSource();
                tcpCt.CancelAfter(1000);
                _tcpClient.ConnectAsync(_ip, _port, tcpCt.Token).AsTask().Wait();
            }
            catch (Exception ex)
            {

            }
        }

        public void WritePLC()
        {
            try
            {
                if (_tcpClient.Connected == false)
                {
                    return;
                }

                var stream = _tcpClient.GetStream();
                if (stream == null || stream.CanWrite == false)
                {
                    return;
                }

                _writeBuffer[0] = 0x02; //stx
                _writeBuffer[1] = _jogDirection; //jog direction
                _writeBuffer[2] = _jogSpeed; //jog speed
                _writeBuffer[3] = 10;
                _writeBuffer[4] = 10;
                _writeBuffer[5] = 10;
                _writeBuffer[6] = 10;
                _writeBuffer[7] = 10;
                _writeBuffer[8] = 10;
                _writeBuffer[9] = 0x03; //etx

                stream.Write(_writeBuffer, 0, _writeBuffer.Length);
                stream.Flush();
            }
            catch (Exception ex) { 
                
            }
        }

        private bool ReadPLC(CancellationToken ct)
        {
            try
            {
                if ( _tcpClient.Connected == false)
                {
                    return false;
                }

                _tcpClient.ReceiveTimeout = 300;
                int readCount = _tcpClient.Client.Receive(_readBuffer, _readBuffer.Length, SocketFlags.None);

                var position = (_readBuffer[1] << 8) + _readBuffer[2];

                if (readCount > 0)
                {
                    // TODO: 여기서 _readBuffer를 실제 프로토콜에 맞게 파싱
                    Debug.WriteLine($"수신: {string.Join(",", _readBuffer.Take(readCount))}, position: {position}");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine("read fail");
                return false;
            }
        }
    }
}
