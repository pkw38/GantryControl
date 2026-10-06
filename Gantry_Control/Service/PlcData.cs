namespace Gantry_Control.Service
{
    // 앱 → PLC. PLC의 GVL_Hmi.Jog / GVL_Hmi.Cmd 에 대응
    internal readonly record struct PlcCommand
    {
        public byte JogDirection { get; init; }
        public byte JogSpeed { get; init; }
        public byte CmdCode { get; init; }      // 1회성 명령 코드 (PlcData.CmdXxx)
        public byte CmdSeq { get; init; }       // 1회성 명령 번호. 값이 바뀌면 PLC가 새 명령으로 처리
    }

    // PLC → 앱. PLC의 GVL_Hmi.Status 에 대응
    internal readonly record struct PlcStatus
    {
        public int X { get; init; }
        public int Y { get; init; }
        public int Z { get; init; }
        public byte AckSeq { get; init; }       // PLC가 마지막으로 받은 명령 번호
        public bool CmdRejected { get; init; }  // 마지막 명령 거부
        public bool HomeBusy { get; init; }
        public bool HomeDone { get; init; }     // 다음 홈 명령까지 유지
        public bool HomeError { get; init; }    // 다음 홈 명령까지 유지
    }

    // PLC와 주고받는 데이터. UI 스레드가 쓰고 통신 스레드가 읽으므로 lock으로 보호
    internal class PlcData
    {
        private static readonly PlcData _instance = new PlcData();
        public static PlcData Instance => _instance;

        // 명령 코드 (PLC의 GVL_Hmi.CMD_xxx 와 일치해야 함)
        public const byte CmdNone = 0;
        public const byte CmdHome = 1;

        private readonly object _lock = new();
        private PlcCommand _command;
        private PlcStatus _status;

        private PlcData() { }

        public PlcCommand Command
        {
            get { lock (_lock) return _command; }
        }

        public PlcStatus Status
        {
            get { lock (_lock) return _status; }
        }

        // 여러 값을 한 번에 바꿔도 송신 프레임에 섞여 나가지 않음
        public void UpdateCommand(Func<PlcCommand, PlcCommand> update)
        {
            lock (_lock)
            {
                _command = update(_command);
            }
        }

        public void UpdateStatus(PlcStatus status)
        {
            lock (_lock)
            {
                _status = status;
            }
        }

        public void StopJog() => UpdateCommand(c => c with { JogDirection = 0, JogSpeed = 0 });

        // 1회성 명령 전송. 번호를 올려서 PLC가 새 명령으로 인식하게 한다. 보낸 번호를 반환
        public byte SendCommand(byte code)
        {
            lock (_lock)
            {
                var seq = unchecked((byte)(_command.CmdSeq + 1));
                _command = _command with { CmdCode = code, CmdSeq = seq };
                return seq;
            }
        }
    }
}
