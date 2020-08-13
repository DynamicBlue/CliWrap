using CliShellWrap;
using System;
using System.Threading;

namespace ShellCmdTest
{
    class Program
    {
        static void Main(string[] args)
        {


            do
            {

                try
                {
                    Console.WriteLine("请输入您要执行的批处理名字！");
                    var testIn = Console.ReadLine();

                    testIn = @"C:\Users\Administrator\Desktop\应用工具\alihbase-2.0.9\bin\hbase.cmd";
                    //testIn = @"E:\WorkSpace\opensource\DCliWrap\CMDDemo\bin\Debug\netcoreapp3.1\CMDDemo.exe";
                    var cli = Cli.Wrap(testIn,"shell");
                  
                    cli.SetStandardOutputCallback((r)=> {
                        Console.WriteLine(r);
                    });
                    ThreadPool.QueueUserWorkItem((obj)=> {
                        var result = cli.Execute(true);
                    });
                    Thread.Sleep(5000);
                    while (true)
                    {
                        var cmd = Console.ReadLine();
                        cli.SetStandardInput(cmd);
                        //cli.ExecuteAsync();
                    }

                   // cli.SetStandardInput("list");
                   // Console.WriteLine(result.StandardOutput);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            } while (true);

                Console.ReadLine();

            
        }
    }
}
