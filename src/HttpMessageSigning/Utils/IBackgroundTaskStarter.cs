using System;
using System.Threading.Tasks;

namespace Dalion.HttpMessageSigning.Utils {
    /// <summary>
    ///     Represents an object that can start background tasks, without waiting for their completion.
    /// </summary>
    public interface IBackgroundTaskStarter {
        /// <summary>
        ///     Start a background task.
        /// </summary>
        /// <param name="task">The work to be done.</param>
        /// <remarks>The background task will be started immediately.</remarks>
        void Start(Func<Task> task);

        /// <summary>
        ///     Start a background task.
        /// </summary>
        /// <param name="task">The work to be done.</param>
        /// <param name="delay">The time after which the work should be started.</param>
        void Start(Func<Task> task, TimeSpan delay);
    }
}