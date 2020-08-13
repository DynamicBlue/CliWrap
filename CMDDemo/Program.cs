using System;

namespace CMDDemo
{
    class Program
    {
        static void Main(string[] args)
        {
           
            Console.WriteLine("hello CliShellWrap!");

            while (true)
            {
                var abc = Console.ReadLine();
                Console.WriteLine("接收到输入"+abc);

            }
            Console.ReadLine();
        }
    }
}
