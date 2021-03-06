﻿using Newtonsoft.Json;
using NLog;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using SysTimer = System.Timers.Timer;

namespace AutoSSH
{
    public class SSHControl
    {
        public SSHControl()
        {
            _cancel = new CancellationTokenSource();
            // _worker_delay = TimeSpan.FromHours(1).TotalMilliseconds;

            int mins = 2;//Convert.ToInt32(ConfigurationManager.AppSettings["readcfg_interval_minutes"]);
            _worker_delay = TimeSpan.FromMinutes(mins).TotalMilliseconds;
            _worker = new SysTimer(_worker_delay);
            _worker.Elapsed += _worker_Elapsed;

            MyLog.initconfig();
            TheLog = LogManager.GetCurrentClassLogger();

            //QueuedCommands = new List<SSHJob>();
        }

        //private List<SSHJob> QueuedCommands { get; set; } = null;
        private Logger TheLog = null;
        
        private readonly double _worker_delay;
        private CancellationTokenSource _cancel = null;
        private SysTimer _worker { get; set; } = null;
        private volatile bool _running = false;

        public void Start()
        {
            //QueuedCommands = LoadConfig();
            _worker.Start();
        }

        public void Stop()
        {
            _cancel.Cancel();
            _worker.Dispose();
            _worker = null;
        }

        private void _worker_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_cancel.IsCancellationRequested) return;
            if (!(sender is SysTimer)) return;
            if (Debugger.IsAttached)
                ((SysTimer)sender).Enabled = false;

            if (_running) return;
            _running = true;
            try
            {
                var jobs_todo = LoadConfig().Where(x => x.RunNow);                
                if (!jobs_todo.Any())
                {
                    TheLog.Info($"No ready jobs found");
                    return;
                }
                
                TheLog.Info($"Found {jobs_todo.Count()} jobs!");
                foreach (var job in jobs_todo)
                    ExecSSHJob(job);
            }
            finally
            {
                TheLog.Info("");
                _running = false;
                if (Debugger.IsAttached)
                    ((System.Timers.Timer)sender).Enabled = true;
            }
        }

        public void ExecSSHJob(SSHJob job)
        {
            TheLog.Info($"Connecting to {job.IP} as {job.User}");
            try
            {
                using (var client = new SshClient(job.IP, job.User, job.PW))
                {
                    client.Connect();
                    foreach (var cmd in job.Commands)
                    {
                        TheLog.Info($"Running: [{cmd}]");
                        var result = client.RunCommand(cmd);
                        TheLog.Info($"Error: {result.Error}");
                        TheLog.Info($"Response: {result.Result}");
                    }
                    client.Disconnect();
                    TheLog.Info($"Disconnecting from {job.IP}");
                }
            }
            catch (Exception ex)
            {
                TheLog.Error($"Encountered an error! Removing current job from queue");
                string logmsg = string.Join(",\n", job.Commands);
                TheLog.Error($"job::ip[{job.IP}] user[{job.User}] commands[{logmsg}]");
                TheLog.Error($"{ex.Source}");
                TheLog.Error(ex);
                //QueuedCommands?.Remove(job);
            }
        }

        private List<SSHJob> LoadConfig()
        {
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\sshconfig.json");
            if (!File.Exists(file))
            {
                TheLog.Error($"Could not find sshconfig.json! expected path[{file}]");
                Stop();
                Environment.Exit(1);
                return null;
            }

            var json = File.ReadAllText(file);
            return JsonConvert.DeserializeObject<List<SSHJob>>(json);            
        }

        private void MakeExampleConfig()
       { 
            var QueuedCommands = new List<SSHJob>();
            for (int i = 0; i < 5; i++)
            {
                QueuedCommands.Add(new SSHJob
                {
                    Commands = new List<string> { "command1", "command2", "do something again" },
                    IP = $"192.168.1.{i}:8000",
                    User = "ssh_username_here",
                    PW = "abc123",
                    NextRun = DateTime.Now + TimeSpan.FromHours(5),
                });
            }
            

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
            };

            string json = JsonConvert.SerializeObject(QueuedCommands, Formatting.Indented, settings);
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\sshconfig.json");
            string path = Path.GetDirectoryName(file);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (var stream = File.CreateText(file))
            {
                stream.Write(json);
                stream.Close();
            }
        }
    }
}