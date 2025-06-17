using System.Threading.Tasks.Dataflow;

using Hyprx.Rex.Deployments;
using Hyprx.Rex.Jobs;
using Hyprx.Rex.Tasks;

namespace Hyprx.Rex.Messaging;

public class ConsoleSink : IMessageSink
{
    public static string DeploySymbol()
    {
        switch (AnsiSettings.Current.Mode)
        {
            case AnsiMode.FourBit:
                return Ansi.Cyan("❯❯❯❯❯");

            case AnsiMode.EightBit:
                return Ansi.Cyan("❯❯❯❯❯");

            case AnsiMode.TwentyFourBit:
                return "\x1b[38;2;60;0;255m❯\x1b[39m\x1b[38;2;54;51;204m❯\x1b[39m\x1b[38;2;48;102;153m❯\x1b[39m\x1b[38;2;42;153;102m❯\x1b[39m\x1b[38;2;36;204;51m❯\x1b[39m";

            case AnsiMode.None:
            default:
                return "❯❯❯❯❯";
        }
    }

    public static string JobSymbol()
    {
        switch (AnsiSettings.Current.Mode)
        {
            case AnsiMode.FourBit:
                return Ansi.Red("❯❯❯❯❯");

            case AnsiMode.EightBit:
                return Ansi.Red("❯❯❯❯❯");

            case AnsiMode.TwentyFourBit:
                return "\x1b[38;2;65;0;140m❯\x1b[39m\x1b[38;2;113;0;105m❯\x1b[39m\x1b[38;2;160;0;70m❯\x1b[39m\x1b[38;2;208;0;35m❯\x1b[39m\x1b[38;2;255;0;0m❯\x1b[39m";

            case AnsiMode.None:
            default:
                return "❯❯❯❯❯";
        }
    }

    public static string Symbol()
    {
        switch (AnsiSettings.Current.Mode)
        {
            case AnsiMode.FourBit:
                return Ansi.Magenta("❯❯❯❯❯");

            case AnsiMode.EightBit:
                return Ansi.Magenta("❯❯❯❯❯");

            case AnsiMode.TwentyFourBit:
                return "\x1b[38;2;60;0;255m❯\x1b[39m\x1b[38;2;90;0;255m❯\x1b[39m\x1b[38;2;121;0;255m❯\x1b[39m\x1b[38;2;151;0;255m❯\x1b[39m\x1b[38;2;182;0;255m❯\x1b[39m";

            case AnsiMode.None:
            default:
                return "❯❯❯❯❯";
        }
    }

    public static string DeployEmoji(DeploymentAction action)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "deploy" => $"{Ansi.BrightBlack("deploy")} {Ansi.Emoji("🚀")}",
            "rollback" => $"{Ansi.BrightBlack("rollback")} {Ansi.Emoji("🪂")}",
            "destroy" => $"{Ansi.BrightBlack("destroy")} {Ansi.Emoji("💥")}",
            _ => Ansi.Emoji("🚀"),
        };
    }

    public static string RexPrefix()
    {
        switch(AnsiSettings.Current.Mode)
        {
            case AnsiMode.FourBit:
                return Ansi.Magenta("[rex]");
            case AnsiMode.EightBit:
                return Ansi.Rgb8(53, "[rex]");

            case AnsiMode.TwentyFourBit:
                return Ansi.Rgb(0x8200ff, "[rex]");

            case AnsiMode.None:
            default:
                return "[rex]";
        }
    }

    public Task<bool> ReceiveAsync(IMessage message)
    {
        switch (message)
        {
            case DeployStarted deployStarted:
                Console.WriteLine($"{RexPrefix()} {Symbol()} {DeployEmoji(deployStarted.Action)} {deployStarted.Data.Name}");
                return Task.FromResult(false);

            case DeployCompleted deployCompleted:
                Console.WriteLine($"{RexPrefix()} {Symbol()} {DeployEmoji(deployCompleted.Action)} {deployCompleted.Data.Name} {Ansi.Green("completed")}");
                return Task.FromResult(false);

            case DeployFailed deployFailed:
                Console.WriteLine($"{RexPrefix()} {Symbol()} {DeployEmoji(deployFailed.Action)} {deployFailed.Data.Name} {Ansi.Red("failed")}");
                if (deployFailed.Exception != null)
                {
                    Console.WriteLine(Ansi.Red(deployFailed.Exception.ToString()));
                }

                return Task.FromResult(false);

            case DeployCancelled deployCancelled:
                Console.WriteLine($"{RexPrefix()} {Symbol()} {DeployEmoji(deployCancelled.Action)} {deployCancelled.Data.Name} (cancelled)");
                return Task.FromResult(false);

            case DeploySkipped deploySkipped:
                Console.WriteLine($"{RexPrefix()} {Symbol()} {DeployEmoji(deploySkipped.Action)} {deploySkipped.Data.Name} (skipped)");
                return Task.FromResult(false);

            case JobStarted jobStarted:
                Console.WriteLine($"{RexPrefix()} {Symbol()} {Ansi.BrightBlack("batch")} {jobStarted.Data.Name}");
                return Task.FromResult(false);

            case JobCompleted jobCompleted:
                Console.WriteLine($"{RexPrefix()} {Symbol()} {Ansi.BrightBlack("batch")} {jobCompleted.Data.Name} {Ansi.Green("completed")}");
                return Task.FromResult(false);

            case JobFailed jobFailed:
                Console.WriteLine($"{RexPrefix()} {Symbol()} {Ansi.BrightBlack("batch")} {jobFailed.Data.Name} {Ansi.Red("failed")}");
                return Task.FromResult(false);

            case JobCancelled jobCancelled:
                Console.WriteLine($"{RexPrefix()} {Symbol()} {Ansi.BrightBlack("batch")} {jobCancelled.Data.Name} (cancelled)");
                return Task.FromResult(false);

            case JobSkipped jobSkipped:
                Console.WriteLine($"{RexPrefix()} {Symbol()} {Ansi.BrightBlack("batch")} {jobSkipped.Data.Name} (skipped)");
                return Task.FromResult(false);

            case JobFoundMissingDependencies jobFoundMissingDependencies:
                {
                    Console.WriteLine($"{RexPrefix()} {Symbol()} Found {Ansi.Red("missing")} dependencies in jobs:");
                    foreach (var (job, missing) in jobFoundMissingDependencies.Tasks)
                    {
                        Console.WriteLine($"{RexPrefix()} - {job.Name} is missing: {string.Join(", ", missing)}");
                    }

                    return Task.FromResult(false);
                }

            case JobFoundCyclycalReferences jobsFoundCyclycalReferences:
                {
                    Console.WriteLine($"{RexPrefix()} {Symbol()} Found {Ansi.Cyan("cyclical")} references in jobs:");
                    foreach (var job in jobsFoundCyclycalReferences.Job)
                    {
                        Console.WriteLine($"{RexPrefix()} - {job.Name}");
                    }

                    return Task.FromResult(false);
                }

            case TaskCompleted taskCompleted:
                Console.WriteLine($"{RexPrefix()} {Symbol()} {Ansi.BrightBlack("run")} {taskCompleted.Data.Name} {Ansi.Green("completed")}");
                Console.WriteLine();
                return Task.FromResult(false);

            case TaskSkipped taskSkipped:
                Console.WriteLine($"{RexPrefix()} {Symbol()} {Ansi.BrightBlack("run")} {taskSkipped.Data.Name} (skipped)");
                return Task.FromResult(false);

            case TaskCancelled taskCancelled:
                Console.WriteLine($"{RexPrefix()} {Symbol()} {Ansi.BrightBlack("run")} {taskCancelled.Data.Name} (cancelled)");
                return Task.FromResult(false);

            case TaskFailed taskFailed:
                Console.WriteLine($"{RexPrefix()} {Symbol()} {Ansi.BrightBlack("run")} {taskFailed.Data.Name} {Ansi.Red("failed")}");
                return Task.FromResult(false);

            case TaskStarted taskStarted:
                Console.WriteLine();
                Console.WriteLine($"{RexPrefix()} {Symbol()} {Ansi.BrightBlack("run")} {taskStarted.Data.Name}");
                return Task.FromResult(false);

            case TasksFoundCyclycalReferences tasksFoundCyclycalReferences:
                {
                    Console.WriteLine($"{RexPrefix()} {Symbol()} Found {Ansi.Cyan("cyclical")} references in tasks:");
                    foreach (var task in tasksFoundCyclycalReferences.Tasks)
                    {
                        Console.WriteLine($"{RexPrefix()} - {task.Name}");
                    }

                    return Task.FromResult(false);
                }

            case TasksFoundMissingDependencies tasksFoundMissingDependencies:
                {
                    Console.WriteLine($"{RexPrefix()} {Symbol()} Found {Ansi.Red("missing")} dependencies in tasks:");
                    foreach (var (task, missing) in tasksFoundMissingDependencies.Tasks)
                    {
                        Console.WriteLine($"{RexPrefix()} - {task.Name} is missing: {string.Join(", ", missing)}");
                    }

                    return Task.FromResult(false);
                }

            case DiagnosticMessage diagnosticMessage:
                {
                    Console.WriteLine($"{RexPrefix()} [{diagnosticMessage.Level}] {diagnosticMessage.Message}");
                    return Task.FromResult(false);
                }

            default:
                return Task.FromResult(false);
        }
    }
}