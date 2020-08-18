using System;
using System.Threading.Tasks;
using CDynamic.StdIODriver;

namespace CDynamic.StdIODriver.TestIn
{
    public class Program
    {

        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    for (int i = 0; i < 10000000; i++)
                    {
                        //list.Add(new MyObj2());
                        var v = new object();
                        //v.Text = "AABBCC";
                        //v.MyObj = new object();
                        //v.Dispose();
                        //list.Add(v);
                        Console.WriteLine(i);
                    }
                    await Task.Delay(100);
                  
                    //list.Clear();
                }

            });
            while (true)
            {
                Console.WriteLine(DateTime.Now);
            }

            //var testIn = @"C:\Users\Administrator\Desktop\应用工具\alihbase-2.0.9\bin\hbase.cmd";
            var testIn = @"E:\WorkSpace\opensource\DCliWrap\CMDDemo\bin\Debug\netcoreapp3.1\CMDDemo.exe";
            ProcessConnection processConnection = new ProcessConnection();
            processConnection.ConnectionStr = testIn;
            var connectMsg=processConnection.Open((msg) =>
            {
                Console.WriteLine(msg);
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
