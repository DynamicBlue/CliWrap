using System;
using System.Threading.Tasks;
using CDynamic.StdIODriver;
using Dynamic.Core.Log;

namespace CDynamic.StdIODriver.TestIn
{
    public class Program
    {

        static void Main(string[] args)
        {
            LoggerManager.InitLogger(new LogConfig());
            var testIn = @"C:\Users\Administrator\Desktop\应用工具\alihbase-2.0.9\bin\hbase.cmd";
            //var testIn = @"E:\WorkSpace\opensource\DCliWrap\CMDDemo\bin\Debug\netcoreapp3.1\CMDDemo.exe";
            ProcessConnection processConnection = new ProcessConnection();
            processConnection.ConnectionStr = testIn;
            var connectMsg=processConnection.Open((msg) =>
            {
                //Console.WriteLine(msg);
            }, "shell");
            Console.WriteLine(connectMsg);
            while (processConnection.Status!=ConnStatus.Disposed)
            {
                var cmd = Console.ReadLine();
                processConnection.SendDataAsync(cmd);
                //cli.ExecuteAsync();
            }


        }

    }
}
