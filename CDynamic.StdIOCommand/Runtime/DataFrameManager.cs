using CDynamic.Command.Defaults;
using CDynamic.StdIODriver.RegexMacth;
using Dynamic.Core.Excuter;
using Dynamic.Core.Extensions;
using Dynamic.Core.Models;
using Dynamic.Core.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CDynamic.StdIODriver.Runtime
{
    public class DataFrameManager
    {
        DataQueue<DataFrameStr> _DataRrameQueue = new DataQueue<DataFrameStr>();
        IDataList<DataFrameGroup> _DataRrameGroupManager = new DataList<DataFrameGroup>();
        ConcurrentQueue<string> _CmdKeyQueue = new ConcurrentQueue<string>();
        private readonly object _lockObj = new object();
        private readonly object _FrameGroupLockObj = new object();
        private volatile bool IsStarted = false;
        ManualResetEvent _reciveARE = new ManualResetEvent(false);
        public void PushCmdKey(string cmdKey)
        {
            _CmdKeyQueue.Enqueue(cmdKey);
        }
        public void Push(DataFrameStr dataFrameStr)
        {
            _DataRrameQueue.Push(new DataItem<DataFrameStr>() { Data = dataFrameStr, ReceivedTime = DateTime.Now });
           // _reciveARE.Set();
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
            }, 10, 10000);
            IsStarted = true;
        }
        protected DataFrameGroup ProductDataGroup(string cmdKey)
        {
             IMatch cmdEndMatch = new CmdEndMatch();
            DataFrameGroup dataFrameGroup = null;
            while (true)
            {
                if (_DataRrameQueue.Count() <= 0)
                {
                    _reciveARE.WaitOne(10000);
                }
                DataItem<DataFrameStr> currentDataFrame = null;
                lock (_lockObj)
                {
                    currentDataFrame = _DataRrameQueue.Get(TimeSpan.FromSeconds(20));
                }
                if (currentDataFrame != null && currentDataFrame.Data != null)
                {
                    if (currentDataFrame.Data.Context == cmdKey && dataFrameGroup == null)
                    {
                        dataFrameGroup = new DataFrameGroup(cmdKey);
                    }
                    else
                    {
                        if (dataFrameGroup == null)
                        {
                            continue;
                        }
                        if (cmdEndMatch.Match(currentDataFrame.Data.Context))
                        {
                            //this is frameend
                            break;
                        }
                        else
                        {
                            dataFrameGroup.Add(currentDataFrame.Data);
                        }
                    }
                }
               
            }
            return dataFrameGroup;
        }

        public DataFrameGroup GetResponse(string cmdKey)
        {
            lock (_FrameGroupLockObj)
            {
                var dataRG = _DataRrameGroupManager.Get(f => f.Data.CmdKey == cmdKey, TimeSpan.FromMilliseconds(3000));
                if (dataRG != null)
                {
                    return dataRG.Data;
                }
                return null;
            }
        }
    }
}
