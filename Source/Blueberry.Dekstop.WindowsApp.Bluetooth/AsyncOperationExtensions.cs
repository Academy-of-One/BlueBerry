using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Blueberry.Dekstop.WindowsApp.Bluetooth
{
    /// <summary>
    /// Provides helper methods for the <see cref="IAsyncOperation"/>
    /// </summary>
    public static class AsyncOperationExtensions
    {
        /// <summary>
        /// This will convert a <see cref="IAsyncOperation{TResult}"/>
        /// into a <see cref="Task"/>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="operation"></param>
        /// <returns></returns>
        public static Task<TResult> AsTask<TResult>(this IAsyncOperation<TResult> operation)
        {
            // Create task completion result
            var tcs = new TaskCompletionSource<TResult>();

            // When the operation is completed...
            operation.Completed += delegate
            {
                switch (operation.Status)
                {
                    // If successful..
                    case AsyncStatus.Completed:
                        // Set result
                        tcs.TrySetResult(operation.GetResults());
                        break;
                    // If exception
                    case AsyncStatus.Error:
                        // Set exception
                        tcs.TrySetException(operation.ErrorCode);
                        break;
                    // If canceled...
                    case AsyncStatus.Canceled:
                        // set task as canceled
                        tcs.SetCanceled();
                        break;

                }
            };

            // Return the task
            return tcs.Task;
        }
    }
}
