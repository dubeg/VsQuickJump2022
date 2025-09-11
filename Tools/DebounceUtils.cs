using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace QuickJump2022.Tools;

public static class DebounceUtils {
    public static Action<T1, T2> Debounce<T1, T2>(this Action<T1, T2> func, TaskScheduler taskScheduler, int milliseconds = 300) {
        CancellationTokenSource? cancelTokenSource = null;
        return (t1, t2) => {
            cancelTokenSource?.Cancel();
            cancelTokenSource = new CancellationTokenSource();
            Task.Delay(milliseconds, cancelTokenSource.Token)
                .ContinueWith(t => {
                    if (!t.IsCanceled) {
                        func(t1, t2);
                    }
                }, taskScheduler);
        };
    }

    public static Action<T> Debounce<T>(this Action<T> func, TaskScheduler taskScheduler, int milliseconds = 300) {
        CancellationTokenSource? cancelTokenSource = null;
        return (args) => {
            cancelTokenSource?.Cancel();
            cancelTokenSource = new CancellationTokenSource();
            Task.Delay(milliseconds, cancelTokenSource.Token)
                .ContinueWith(t => {
                    if (!t.IsCanceled) {
                        func(args);
                    }
                }, taskScheduler);
        };
    }

    public static Action Debounce(this Action func, TaskScheduler taskScheduler, int milliseconds = 300) {
        CancellationTokenSource? cancelTokenSource = null;
        return () => {
            cancelTokenSource?.Cancel();
            cancelTokenSource = new CancellationTokenSource();
            Task.Delay(milliseconds, cancelTokenSource.Token)
                .ContinueWith(t => {
                    if (!t.IsCanceled) {
                        func();
                    }
                }, taskScheduler);
        };
    }

    public static Task<TaskScheduler> ToTaskSchedulerAsync(this Dispatcher dispatcher, DispatcherPriority priority = DispatcherPriority.Normal) {
        var taskCompletionSource = new TaskCompletionSource<TaskScheduler>();
        var invocation = dispatcher.BeginInvoke(new Action(() => taskCompletionSource.SetResult(TaskScheduler.FromCurrentSynchronizationContext())), priority);
        invocation.Aborted += (s, e) => taskCompletionSource.SetCanceled();
        return taskCompletionSource.Task;
    }
}
