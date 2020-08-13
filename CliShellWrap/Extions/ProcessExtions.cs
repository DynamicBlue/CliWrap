using System;
using System.Collections.Generic;
using System.Text;

namespace CliWrap.Extions
{
    public static class ProcessExtions
    {
        private static Dictionary<string, string> _quietSwitchMap =
        new Dictionary<string, string> { { "rmdir", "/Q" }, { "xcopy", "/y" } };

        public static string AddQuietSwitch(this string input)
        {
            var trimmedInput = input.Trim();
            var cmd = trimmedInput.Substring(0, trimmedInput.IndexOf(" "));

            string sw;
            if (!_quietSwitchMap.TryGetValue(cmd, out sw)) { return input; }
            if (trimmedInput.IndexOf(sw, 0,
           StringComparison.InvariantCultureIgnoreCase) > 0) { return input; }

            return input += string.Format(" {0}", _quietSwitchMap[cmd]);
        }

    }
}
