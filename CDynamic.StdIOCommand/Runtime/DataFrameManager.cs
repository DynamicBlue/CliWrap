using CDynamic.Command.Defaults;
using Dynamic.Core.Extensions;
using Dynamic.Core.Models;
using Dynamic.Core.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CDynamic.StdIODriver.Runtime
{
    public class DataFrameManager
    {
        DataQueue<DataFrameStr> _DataRrameQueue = new DataQueue<DataFrameStr>();
        DataQueue<DataFrameGroup> _DataRrameGroupManager = new DataQueue<DataFrameGroup>();
        ConcurrentQueue<string> _CmdKeyQueue = new ConcurrentQueue<string>();
        private readonly object _lockObj = new object();
        private readonly object _FrameGroupLockObj = new object();
        private volatile bool IsStarted = false;

        public void PushCmdKey(string cmdKey)
        {
            _CmdKeyQueue.Enqueue(cmdKey);
        }
        public void Push(DataFrameStr dataFrameStr)
        {
            _DataRrameQueue.Push(new DataItem<DataFrameStr>() { Data = dataFrameStr, ReceivedTime = DateTime.Now });
        }
        public void Start()
        {
            if (IsStarted)
            {
                throw new Exception("系统已经初始化，不能重复初始化！");
            }
            TimerTasker timerTasker = new TimerTasker((obj) =>
            {
                while (_CmdKeyQueue.Count > 0)
                {
                    string cmdKey = null;
                    if (_CmdKeyQueue.TryDequeue(out cmdKey) && cmdKey.IsNotNullOrEmpty())
                    {
                        var dataFrameGroup = ProductDataGroup(cmdKey);
                        if (dataFrameGroup != null && dataFrameGroup.CmdKey.IsNotNullOrEmpty())
                        {
                            _DataRrameGroupManager.Push(new DataItem<DataFrameGroup>() { Data = dataFrameGroup, ReceivedTime = DateTime.Now });
                        }
                    }
                }
            }, 3000, 10000);
            IsStarted = true;
        }
        protected DataFrameGroup ProductDataGroup(string cmdKey)
        {
            DataFrameGroup dataFrameGroup = null;
            while (_DataRrameQueue.Count() > 0)
            {
                DataItem<DataFrameStr> currentDataFrame = null;
                lock (_lockObj)
                {
                    currentDataFrame = _DataRrameQueue.Get(TimeSpan.FromSeconds(3));
                }
                if (currentDataFrame != null && currentDataFrame.Data != null)
                {
                    if (currentDataFrame.Data.Context == cmdKey && dataFrameGroup == null)
                    {
                        dataFrameGroup = new DataFrameGroup(cmdKey);
                    }
                    var 
                    if()
                    dataFrameGroup.Add(currentDataFrame.Data);
                }
               
            }
            return dataFrameGroup;
        }
    }
}
