using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gantry_Control.Common
{
    internal class TaskWorkerBase
    {
        public TaskWorkerBase(string name, int intervalMs)
        {
            _name = name;
            _intervalMs = intervalMs;
        }

        protected string _name;
        protected int _intervalMs;

        protected virtual void WorkRoutine(CancellationToken ct) { }
        protected virtual Task WorkRoutineAsync(CancellationToken ct) { return Task.CompletedTask; }
        protected virtual void DoFinalize() { }

        public virtual Task RunAsync(CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                try
                {
                    while (ct.IsCancellationRequested == false)
                    {
                        try
                        {
                            WorkRoutine(ct);
                            await WorkRoutineAsync(ct);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[{_name}] Exception Occurred: {ex.Message}");
                        }
                        finally
                        {
                            await Task.Delay(_intervalMs, ct);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine($"[{_name}] Task Canceled");
                }
                finally
                {
                    DoFinalize();
                }
            });
        }
    }
}
