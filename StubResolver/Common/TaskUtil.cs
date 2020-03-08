using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bns.StubResolver.Common
{
    public static class TaskUtil
    {
        public static async Task RunAndWaitForCancel(Task task, CancellationToken cancellationToken, Action cleanup)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(() => taskCompletionSource.TrySetResult(true)))
            {
                try
                {
                    var cancelTask = taskCompletionSource.Task;
                    var completedTask = await Task.WhenAny(task, cancelTask).ConfigureAwait(false);

                    if (completedTask == cancelTask)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                catch (OperationCanceledException)
                {
                    // close the listener and exit
                }
                finally
                {
                    cleanup?.Invoke();
                }
            }
        }
    }
}
