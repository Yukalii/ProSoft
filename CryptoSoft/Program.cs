namespace CryptoSoft;

public static class Program
{
    // Unique mutex name: a GUID prevents collision with any other process
    private const string MutexName = "Global\\CryptoSoft_SingleInstance_{F3A2B1C0-9D4E-4F7A-8B6C-2E1D0A5F3C9B}";

    public static void Main(string[] args)
    {
        // Try to acquire the global mutex (createdNew = true means we are first)
        using var mutex = new Mutex(initiallyOwned: true, MutexName, out bool createdNew);

        if (!createdNew)
        {
            // Another instance is already running: report and exit immediately
            Console.Error.WriteLine("[CryptoSoft] Error: another instance is already running.");
            Environment.Exit(-1); // EasySave can detect exit code -1 as "already running"
            return;
        }

        // We own the mutex: proceed normally
        try
        {
            foreach (var arg in args)
                Console.WriteLine(arg);

            var fileManager = new FileManager(args[0], args[1]);
            int elapsedTime = fileManager.TransformFile();
            Environment.Exit(elapsedTime);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            Environment.Exit(-99);
        }
        finally
        {
            // Always release the mutex so the next call can run immediately after
            mutex.ReleaseMutex();
        }
    }
}