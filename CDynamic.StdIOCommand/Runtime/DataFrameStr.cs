﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CDynamic.StdIODriver.Runtime
{

    public class DataFrameStr
    {
        public string Key { get; set; }
        public string Context { get; set; }

        public DateTime ReciveTime { get; set; }

        public DataFrameStr(string context, string key=null)
        {
            this.ReciveTime = DateTime.Now;
            this.Context = context;
            this.Key = key;
        }
      
    }
    public class DataFrameGroup
    {
        public List<DataFrameStr> DataFrameList { get; set; }

        public string CmdKey { get;private set; }

        public DataFrameGroup(string cmdKey)
        {
            this.DataFrameList = new List<DataFrameStr>();
            this.CmdKey = cmdKey;
        }
        public void Add(DataFrameStr dataFrame)
        {
            this.DataFrameList.Add(dataFrame);
        }
        public override string ToString()
        {
            if (this.DataFrameList != null&&this.DataFrameList.Count>0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var item in this.DataFrameList)
                {
                    stringBuilder.AppendLine(item.Context);
                }
                return stringBuilder.ToString();
            }
            return base.ToString();
        }
    }

   
}
