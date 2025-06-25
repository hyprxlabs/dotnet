using Hyprx.Results;
using Hyprx.Rex.Collections;

namespace Hyprx.Rex.Tasks;

public class DelegateTask : CodeTask, ITaskHandler
{
    private readonly RunTaskAsync func;

    public DelegateTask(string id, RunTaskAsync taskFunc)
        : base(id)
    {
        this.func = taskFunc ?? throw new ArgumentNullException(nameof(taskFunc));
    }

    public DelegateTask(string id, Func<TaskContext, Task<Result<Outputs>>> func)
        : base(id)
    {
        this.func = async (context, cancellationToken) => await func(context);
    }

    public DelegateTask(string id, Func<TaskContext, CancellationToken, Outputs> func)
        : base(id)
    {
        this.func = (context, cancellationToken) =>
        {
            try
            {
                var outputs = func(context, cancellationToken);
                var result = new Result<Outputs>(outputs);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                var result = new Result<Outputs>(ex);
                return Task.FromResult(result);
            }
        };
    }

    public DelegateTask(string id, Func<TaskContext, CancellationToken, Task<Outputs>> func)
        : base(id)
    {
        this.func = async (context, cancellationToken) =>
        {
            try
            {
                var outputs = await func(context, cancellationToken);
                var result = new Result<Outputs>(outputs);
                return result;
            }
            catch (Exception ex)
            {
                var result = new Result<Outputs>(ex);
                return result;
            }
        };
    }

    public DelegateTask(string id, Func<TaskContext, Outputs> func)
        : base(id)
    {
        this.func = (context, cancellationToken) =>
        {
            try
            {
                var outputs = func(context);
                var result = new Result<Outputs>(outputs);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                var result = new Result<Outputs>(ex);
                return Task.FromResult(result);
            }
        };
    }

    public DelegateTask(string id, Action<TaskContext> func)
        : base(id)
    {
        this.func = (context, cancellationToken) =>
        {
            try
            {
                func(context);
                var result = new Result<Outputs>(new Outputs());
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                var result = new Result<Outputs>(ex);
                return Task.FromResult(result);
            }
        };
    }

    public DelegateTask(string id, Action func)
        : base(id)
    {
        this.func = (context, cancellationToken) =>
        {
            try
            {
                func();
                var result = new Result<Outputs>(new Outputs());
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                var result = new Result<Outputs>(ex);
                return result;
            }
        };
    }

    public virtual async Task<Result<Outputs>> RunAsync(TaskContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await this.func(context, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}

public class TaskBuilder
{
    private readonly CodeTask task;

    public TaskBuilder(CodeTask task)
    {
        this.task = task;
    }

    public TaskBuilder WithEnv(IEnumerable<KeyValuePair<string, string>> env)
    {
        var map = new StringMap(env!);
        this.task.Env = map;
        return this;
    }

    public TaskBuilder WithNeeds(IEnumerable<string> needs)
    {
        if (needs == null)
        {
            throw new ArgumentNullException(nameof(needs));
        }

        this.task.Needs = needs.ToArray();
        return this;
    }

    public TaskBuilder WithEnv(DeferredTaskValue<StringMap> env)
    {
        this.task.Env = env;
        return this;
    }

    public TaskBuilder WithName(string name)
    {
        this.task.Name = name;
        return this;
    }

    public TaskBuilder WithDescription(string description)
    {
        this.task.Description = description;
        return this;
    }

    public TaskBuilder Set(Action<CodeTask> action)
    {
        action(this.task);
        return this;
    }

    public TaskBuilder WithIf(DeferredTaskValue<bool> condition)
    {
        this.task.If = condition;
        return this;
    }

    public TaskBuilder WithIf(bool condition)
    {
        this.task.If = condition;
        return this;
    }

    public TaskBuilder WithTimeout(DeferredTaskValue<int> timeout)
    {
        this.task.Timeout = timeout;
        return this;
    }

    public TaskBuilder WithTimeout(int timeout)
    {
        this.task.Timeout = timeout;
        return this;
    }

    public TaskBuilder WithCwd(DeferredTaskValue<string> cwd)
    {
        this.task.Cwd = cwd;
        return this;
    }

    public TaskBuilder WithCwd(string cwd)
    {
        this.task.Cwd = cwd;
        return this;
    }

    public TaskBuilder WithForce(DeferredTaskValue<bool> force)
    {
        this.task.Force = force;
        return this;
    }

    public TaskBuilder WithForce(bool force)
    {
        this.task.Force = force;
        return this;
    }
}