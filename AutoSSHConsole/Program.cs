using AutoSSH;
using System;
using System.Collections.Generic;

namespace AutoSSHConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WindowWidth = 100;
            Console.WindowHeight = 50;

            SSHControl ssh = new SSHControl();
            //ssh.Start();
            SSHJob job = new SSHJob();
            job.Commands = new List<string>();
            Console.WriteLine("Enter IP:");
            job.IP = Console.ReadLine();

            Console.WriteLine("Enter Username:");
            job.User = Console.ReadLine();

            Console.WriteLine("Enter PW:");
            job.PW = Console.ReadLine();

            while (true)
            {
                Console.WriteLine("Enter command:");
                string cmd = Console.ReadLine();
                job.Commands.Add(cmd);
                ssh.ExecSSHJob(job);
                job.Commands.Remove(cmd);
            }

            //ssh.Stop();
        }
    }
}