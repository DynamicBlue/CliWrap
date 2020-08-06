using CliShellWrap;
using System;

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
                    var cli = Cli.Wrap(testIn,"shell");
                  
                    cli.SetStandardOutputCallback((r)=> {
                        Console.WriteLine(r);
                    });
                    var result = cli.Execute();

                    while (true)
                    {
                        var cmd = Console.ReadLine();
                        cli.SetStandardInput(cmd);
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
