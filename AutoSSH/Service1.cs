using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AutoSSH
{
    public partial class Service1 : ServiceBase
    {
        private SSHControl _ssh = null;
        public Service1()
        {
            InitializeComponent();
            _ssh = new SSHControl();
        }

        protected override void OnStart(string[] args)
        {
            _ssh.Start();
        }

        protected override void OnStop()
        {
            _ssh.Stop();
        }
    }
}
