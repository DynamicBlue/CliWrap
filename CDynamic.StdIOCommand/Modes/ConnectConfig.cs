using Dynamic.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace CDynamic.StdIODriver.Modes
{
    public class ConnectConfig
    {
        public static string _ConnecitonStrTemplate = "SubProcessPath={SubProcessPath};Args={Args};ConPoolNum={ConPoolNum};AliveTimeOut={AliveTimeOut}";
        public ConnectConfig()
        {
            this.ConPoolNum = 10;
            //default value is 20 minutues
            this.AliveTimeOut = 1000 * 60 * 20;

        }
       
        public string OriConnectionStr { get; set; }

        public string SubProcessPath { get; set; }
        /// <summary>
        ///  process start argumt
        /// </summary>
        public string Args { get; set; }

        public int ConPoolNum { get; set; }

        public int AliveTimeOut { get; set; }

        public static ConnectConfig GetConnectionConfig(string connectionString)
        {
            if (connectionString.IsNullOrEmpty())
            {
                return null;
            }
            ConnectConfig connectConfig = new ConnectConfig();
            try
            {
                var configItemStrArr = connectionString.Split(';');
                if (configItemStrArr != null && configItemStrArr.Length >= 1)
                {
                    connectConfig.OriConnectionStr = connectionString;
                    foreach (var configItemStr in configItemStrArr)
                    {
                        if (configItemStr.Contains('='))
                        {
                            var configItemKV = configItemStr.Split('=');
                            string keyStr = configItemKV[0];
                            string valueStr = configItemKV[1];
                            typeof(ConnectConfig).GetProperty(keyStr).SetValue(connectConfig, valueStr);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new FormatException($"连接字符串格式错误！【请参照{_ConnecitonStrTemplate}】+{ex.ToString()}");
            }
            return connectConfig;

        }

    }
}
