using CDynamic.Command.Defaults;
using Dynamic.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CDynamic.StdIODriver.Runtime
{
    public class DataFrameManager
    {
        DataQueue<DataFrameStr> _DataRrameQueue = new DataQueue<DataFrameStr>();
        private readonly object _lockObj = new object();

        public void Push(DataFrameStr dataFrameStr)
        {
            _DataRrameQueue.Push(new DataItem<DataFrameStr>() { Data = dataFrameStr, ReceivedTime = DateTime.Now });
        }

        public DataFrameGroup Get(string cmdKey)
        {
            DataFrameGroup dataFrameGroup = null;
            while (true)
            {
                DataItem<DataFrameStr> currentDataFrame = null;
                lock (_lockObj)
                {
                    currentDataFrame = _DataRrameQueue.Get(TimeSpan.FromSeconds(5));
                }
                if (currentDataFrame != null&&currentDataFrame.Data!=null)
                {
                    if (currentDataFrame.Data.Context == cmdKey && dataFrameGroup == null)
                    {
                        dataFrameGroup = new DataFrameGroup(cmdKey);
                    }
                    dataFrameGroup.Add(currentDataFrame.Data);
                }
                break;
            }
            return dataFrameGroup;
        }
    }
}
