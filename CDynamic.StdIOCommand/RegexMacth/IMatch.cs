using System;
using System.Collections.Generic;
using System.Text;

namespace CDynamic.StdIODriver.RegexMacth
{
    public interface IMatch
    {
        public string MatchTemplate { get;}
        bool Match(string oriStri);
    }
}
