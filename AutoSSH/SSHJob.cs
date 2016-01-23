using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSSH
{
    public class SSHJob
    {
        public string IP { get; set; }
        public string User { get; set; }
        public string PW { get; set; }
        public List<string> Commands { get; set; }
        public string RunTime { get; set; }
        
        [JsonIgnore]
        public DateTime NextRun
        {
            get
            {
                return DateTime.Parse(RunTime);
            }
            set
            {
                RunTime = value.GetDateTimeFormats()[106];
            }
        }

        [JsonIgnore]
        public bool RunNow => DateTime.Compare(DateTime.Now, NextRun) > 0;
    }
}
