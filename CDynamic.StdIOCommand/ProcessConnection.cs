using CDShellWrap;
using CDynamic.StdIODriver.Modes;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;


namespace CDynamic.StdIODriver
{
    /// <summary>
    /// 通过改方法连接子进程
    /// </summary>
    public class ProcessConnection : IDisposable
    {
        /// <summary>
        /// 连接字符串(连接子进程的路径)
        /// </summary>
        public string ConnectionStr { get; set; }
        public ConnectConfig ConConfig { get; set; }

        protected ICli CliDriver { get; set; }

        public ConnStatus Status { get; set; }
        ManualResetEvent _ConnectionMREvent = new ManualResetEvent(false);

        public ConnStatus Open(Action<string> reciveOnlineData, string args = null)
        {

            this.Status = ConnStatus.Opening;
            if (CliDriver == null)
            {
                CliDriver = Cli.Wrap(ConnectionStr, args);
            }
            CliDriver.ListenAsync();
            
            CliDriver.SetStandardOutputCallback((msg) =>
            {
                if (reciveOnlineData != null)
                {
                    reciveOnlineData.Invoke(msg);
                }
                if (this.Status == ConnStatus.Opening)
                {
                    this.Status = ConnStatus.Opened;
                    this._ConnectionMREvent.Set();
                }
            });
            CliDriver.SetStandardErrorCallback((msg) =>
            {
            
              //  this.Dispose();
              //  throw new Exception(msg);
            });
            CliDriver.SetStandardErrorClosedCallback(() =>
            {
                this.Dispose();
            });
            _ConnectionMREvent.WaitOne(TimeSpan.FromSeconds(180));
            return this.Status;
        }
        public async Task SendDataAsync(string sata)
        {
            await Task.Run(() =>
            {
                SendData(sata);
            });
        }
        public void SendData(string sata)
        {
            if (this.Status != ConnStatus.Opened)
            {
                throw new Exception($"连接池状态异常Status={this.Status}");
            }
            CliDriver.SetStandardInput(sata);
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            this.Status = ConnStatus.Closed;
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    CliDriver.Dispose();
                    this.Status = ConnStatus.Disposed;
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~ProcessConnection()
        // {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion


    }
}
