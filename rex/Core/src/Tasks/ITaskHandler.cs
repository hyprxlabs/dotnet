using Hyprx.Results;
using Hyprx.Rex.Collections;

namespace Hyprx.Rex.Tasks;

public interface ITaskHandler
{
    /// <summary>
    /// Runs the task handler asynchronously.
    /// </summary>
    /// <param name="context">The task context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the task execution.</returns>
    Task<Result<Outputs>> RunAsync(TaskContext context, CancellationToken cancellationToken = default);
}