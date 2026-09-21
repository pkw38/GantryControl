using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gantry_Control.Service;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Gantry_Control.ViewModel
{
    public partial class ManualViewModel : ObservableObject
    {
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

        private readonly DispatcherTimer _moveTimer;
        private bool _isPressActive;
        private string _selectedDirection = "stop";
        public byte SetSpeed { get; set; }
        public int SetHome {  get; set; }

        public ManualViewModel()
        {
            _moveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _moveTimer.Tick += (_, __) => SendMove(_selectedDirection);
        }

        [RelayCommand]
        private void DirectionPress(string direction)
        {
            if (direction == "stop") return;

            _selectedDirection = direction;
            _isPressActive = true;
            SendMove(direction);
            _moveTimer.Start();
        }

        [RelayCommand]
        private void DirectionRelease()
        {
            if (!_isPressActive) return;

            _moveTimer.Stop();
            _isPressActive = false;
            SendMove("stop");
        }

        private void SendMove(string direction)
        {
            switch (direction)
            {
                case "stop":
                    SendJog(Direction.Stop);
                    break;
                case "forward":
                    SendJog(Direction.Forward);
                    break;
                case "backward":
                    SendJog(Direction.Backward);
                    break;
                case "left":
                    SendJog(Direction.Left);
                    break;
                case "right":
                    SendJog(Direction.Right);
                    break;
                case "up":
                    SendJog(Direction.Up);
                    break;
                case "down":
                    SendJog(Direction.Down);
                    break;
            }
        }

        private void SendJog(Direction direction)
        {
            TcpComm.Instance.SetJog((byte)direction, SetSpeed);
            TcpComm.Instance.WritePLC();
        }
    }
}
