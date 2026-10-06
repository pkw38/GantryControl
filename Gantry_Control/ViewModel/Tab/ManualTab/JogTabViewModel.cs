using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gantry_Control.Service;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;

namespace Gantry_Control.ViewModel.Tab.ManualTab
{
    public partial class JogTabViewModel : ManualTabViewModel
    {
        public override string Header => "Jog";

        public class MoveButton
        {
            public PackIconKind Icon { get; init; }
            public string? Direction { get; init; }
            public bool ShowDirection { get; init; }

            public string DirectionKey { get; init; } = "stop";
            public bool IsStop => DirectionKey == "stop";
        }

        private enum Direction : byte
        {
            Stop = 0,
            Forward = 1,
            Backward = 2,
            Left = 3,
            Right = 4,
            Up = 5,
            Down = 6,
        }

        public const int MinSpeed = 0;
        public const int MaxSpeed = byte.MaxValue;

        public ObservableCollection<MoveButton> XYMoveButtonList { get; } = new()
        {
            new() { DirectionKey = "upperleft",     Icon = PackIconKind.None },
            new() { DirectionKey = "left",          Icon = PackIconKind.ArrowUpThick,        Direction = "좌",  ShowDirection = true },
            new() { DirectionKey = "upperright",    Icon = PackIconKind.None },
            new() { DirectionKey = "backward",      Icon = PackIconKind.ArrowLeftThick,      Direction = "후진",  ShowDirection = true },
            new() { DirectionKey = "stop",          Icon = PackIconKind.None },
            new() { DirectionKey = "forward",       Icon = PackIconKind.ArrowRightThick,     Direction = "전진", ShowDirection = true },
            new() { DirectionKey = "lowerleft",     Icon = PackIconKind.None },
            new() { DirectionKey = "right",         Icon = PackIconKind.ArrowDownThick,      Direction = "우", ShowDirection = true },
            new() { DirectionKey = "lowerright",    Icon = PackIconKind.None },
        };

        public ObservableCollection<MoveButton> ZMoveButtonList { get; } = new()
        {
            new() { DirectionKey = "up",            Icon = PackIconKind.ArrowUpThick,       Direction = "업",  ShowDirection = true },
            new() { DirectionKey = "down",          Icon = PackIconKind.ArrowDownThick,     Direction = "다운",  ShowDirection = true }
        };

        private bool _isPressActive;

        [ObservableProperty]
        private int setSpeed;

        public int SetHome { get; set; }

        partial void OnSetSpeedChanged(int value)
        {
            var clamped = Math.Clamp(value, MinSpeed, MaxSpeed);
            if (clamped != value)
            {
                SetSpeed = clamped;
            }
        }

        [RelayCommand]
        private void DirectionPress(string direction)
        {
            if (TryGetDirection(direction, out var dir) == false || dir == Direction.Stop) return;

            _isPressActive = true;
            SendJog(dir);
        }

        [RelayCommand]
        private void DirectionRelease()
        {
            if (!_isPressActive) return;

            _isPressActive = false;
            SendJog(Direction.Stop);
        }

        /// <summary>화면 전환, 창 비활성화 등 누름 상태와 무관하게 무조건 정지</summary>
        public override void StopMotion()
        {
            _isPressActive = false;
            SendJog(Direction.Stop);
        }

        private static bool TryGetDirection(string key, out Direction direction)
        {
            switch (key)
            {
                case "stop": direction = Direction.Stop; return true;
                case "forward": direction = Direction.Forward; return true;
                case "backward": direction = Direction.Backward; return true;
                case "left": direction = Direction.Left; return true;
                case "right": direction = Direction.Right; return true;
                case "up": direction = Direction.Up; return true;
                case "down": direction = Direction.Down; return true;
                default: direction = Direction.Stop; return false;
            }
        }

        private void SendJog(Direction direction)
        {
            // TcpComm이 매 주기 PlcData의 현재 값을 송신하므로 값만 갱신하면 된다
            var speed = direction == Direction.Stop ? (byte)0 : (byte)SetSpeed;
            PlcData.Instance.SetJog((byte)direction, speed);
        }
    }
}
