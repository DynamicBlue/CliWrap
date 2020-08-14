using System;
using System.IO;
using System.Threading;

namespace CMDDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            BinaryWriter writer = new BinaryWriter(Console.OpenStandardOutput());
            byte p = 0;
            do
            {
                writer.Write(p);
                Thread.Sleep(50);
                p++;
            }
            while (p <= 100);
            writer.Close();

            //Console.WriteLine("hello CliShellWrap!");

            //while (true)
            //{
            //    var abc = Console.ReadLine();
            //    Console.WriteLine("接收到输入"+abc);

            //}
            //Console.ReadLine();
        }
    }
}
