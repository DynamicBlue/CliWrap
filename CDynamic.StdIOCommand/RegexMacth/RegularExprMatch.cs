using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CDynamic.StdIODriver.RegexMacth
{
    public class RegularExprMatch : IMatch
    {
        public string MatchTemplate => throw new NotImplementedException();

        public bool Match(string oriStri)
        {
            Regex regex = new Regex(this.MatchTemplate);
            return regex.IsMatch(oriStri);
        }
    }
}
