using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gantry_Control.Service
{
    internal interface IComm
    {
        Task Connect(String ip, int port);
        void SendManualMove(double x, double y, double angular);
        void SendManualMoveXY(string x, string y, double linearVelocity);
        Task WriteData(CancellationToken ct);

        event EventHandler<bool> ConnectionChanged;
    }
}
