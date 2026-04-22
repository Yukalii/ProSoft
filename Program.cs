using System;

public class Class1
namespace EasySave
{
    internal class Program
    {
        // TODO: var app = new ConsoleView(mainVM, jobListVM, settingsVM, execVM);

        // Command : "EasySave.exe [args]" or "dotnet run -- [args]"
        if (args.Length > 0)
        {
            string arg = args[0];

            if (arg.Contains('-'))
            {
                // "1-3" → run jobs 1, 2, 3
                var parts = arg.Split('-');
                int from = int.Parse(parts[0]);
                int to = int.Parse(parts[1]);
                for (int i = from; i <= to; i++)
                    jobManager.ExecuteJob(i);
            }
            else if (arg.Contains(';'))
            {
                // "1;3" → run jobs 1 and 3
                foreach (var index in arg.Split(';'))
                    jobManager.ExecuteJob(int.Parse(index.Trim()));
            }
            else
            {
                // Single job: "EasySave.exe 2"
                jobManager.ExecuteJob(int.Parse(arg));
            }
        }
        else
        {
            // TODO: app.Run(); // Interactive menu
        }
    }
}
