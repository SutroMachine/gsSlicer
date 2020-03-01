using System;

namespace gsCore.FunctionalTests.Utility
{
    public class ConsoleLogger : ILogger
    {
        public void WriteLine(string s)
        {
            Console.WriteLine(s);
        }
    }
}