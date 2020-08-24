using System;
using System.Collections.Generic;
using System.Text;

namespace CDynamic.StdIODriver.RegexMacth
{
    public class CmdEndMatch : IMatch
    {
        public string MatchTemplate => "Took 0.7730 seconds";

        public bool Match(string oriStr)
        {
            bool isMatch = false;
            if (!string.IsNullOrEmpty(oriStr))
            {
                return oriStr.IndexOf("Took")>=0&&oriStr.IndexOf("seconds")>=0;
            }
            return isMatch;
        }
    }
}
