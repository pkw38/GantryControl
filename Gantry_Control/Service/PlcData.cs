namespace Gantry_Control.Service
{
    // PLC와 주고받는 데이터. UI 스레드가 쓰고 통신 스레드가 읽으므로 lock으로 보호
    internal class PlcData
    {
        private static readonly PlcData _instance = new PlcData();
        public static PlcData Instance => _instance;

        private readonly object _lock = new();

        // 앱 → PLC
        private byte _jogDirection;
        private byte _jogSpeed;

        // PLC → 앱
        private int _x;
        private int _y;
        private int _z;

        private PlcData() { }

        // 방향과 속도는 한 프레임에 같이 반영되어야 하므로 함께 설정
        public void SetJog(byte direction, byte speed)
        {
            lock (_lock)
            {
                _jogDirection = direction;
                _jogSpeed = speed;
            }
        }

        public void StopJog() => SetJog(0, 0);

        public (byte Direction, byte Speed) GetJog()
        {
            lock (_lock)
            {
                return (_jogDirection, _jogSpeed);
            }
        }

        public void SetPosition(int x, int y, int z)
        {
            lock (_lock)
            {
                _x = x;
                _y = y;
                _z = z;
            }
        }

        public (int X, int Y, int Z) GetPosition()
        {
            lock (_lock)
            {
                return (_x, _y, _z);
            }
        }
    }
}
