using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NamedPipe
{
    public class Promise<T>
    {
        private TaskCompletionSource<T> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public static implicit operator Task<T>(Promise<T> promise) => promise.tcs.Task;

        public Promise(Action<Action<T>, Action<Exception>> action) =>
            action(tcs.SetResult, tcs.SetException);
    }
}
