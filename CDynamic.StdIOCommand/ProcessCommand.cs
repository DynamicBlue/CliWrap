using System;
using System.Collections.Generic;
using System.Text;

namespace CDynamic.StdIODriver
{
    public class ProcessCommand
    {
        public string BeginSplitChar { get;private set; }
        public string EndSplitChar { get; set; }

        public ProcessConnection Connection { get;private set; }

        public string CommandText { get; set; }

        public void Send()
        {
            Connection.SendData(CommandText);
        }
    }
}
