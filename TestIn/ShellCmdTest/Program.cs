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
                    var cli = Cli.Wrap(testIn);
                    cli.SetStandardOutputCallback((r)=> {
                        Console.WriteLine(r);
                    });
                    var result = cli.Execute();
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
