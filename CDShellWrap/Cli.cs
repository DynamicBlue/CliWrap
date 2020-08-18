using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CDShellWrap.Exceptions;
using CDShellWrap.Internal;
using CDShellWrap.Models;

namespace CDShellWrap
{
    /// <summary>
    /// Command line interface wrapper.
    /// </summary>
    public partial class Cli : ICli
    {
        private readonly string _filePath;

        private string? _workingDirectory;
        private string? _arguments;
        private Stream _standardInput = Stream.Null;
        private readonly IDictionary<string, string> _environmentVariables = new Dictionary<string, string>();
        private Encoding _standardOutputEncoding = Console.OutputEncoding;
        private Encoding _standardErrorEncoding = Console.OutputEncoding;
        private Action<string>? _standardOutputObserver;
        private Action? _standardOutputClosedObserver;
        private Action<string>? _standardErrorObserver;
        private Action? _standardErrorClosedObserver;
        private CancellationToken _cancellationToken;
        private bool _killEntireProcessTree;
        private bool _exitCodeValidation = true;
        private bool _standardErrorValidation;

        /// <inheritdoc />
        public int? ProcessId { get; private set; }

        internal CliProcess CurrentProcess { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="Cli"/> on the target executable.
        /// </summary>
        public Cli(string filePath, string args = null)
        {
            _filePath = filePath;
            if (!string.IsNullOrEmpty(args))
            {
                _arguments = args;
            }
        }

        #region Options

        /// <inheritdoc />
        public ICli SetWorkingDirectory(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
            return this;
        }

        /// <inheritdoc />
        public ICli SetArguments(string arguments)
        {
            _arguments = arguments;
            return this;
        }

        /// <inheritdoc />
        public ICli SetArguments(IReadOnlyList<string> arguments)
        {
            var buffer = new StringBuilder();

            foreach (var argument in arguments)
            {
                // If buffer has something in it - append a space
                if (buffer.Length != 0)
                    buffer.Append(' ');

                // If argument is clean and doesn't need escaping - append it directly
                if (argument.Length != 0 && argument.All(c => !char.IsWhiteSpace(c) && c != '"'))
                {
                    buffer.Append(argument);
                }
                // Otherwise - escape problematic characters
                else
                {
                    // Escaping logic taken from CoreFx source code

                    buffer.Append('"');

                    for (var i = 0; i < argument.Length;)
                    {
                        var c = argument[i++];

                        if (c == '\\')
                        {
                            var numBackSlash = 1;
                            while (i < argument.Length && argument[i] == '\\')
                            {
                                numBackSlash++;
                                i++;
                            }

                            if (i == argument.Length)
                            {
                                buffer.Append('\\', numBackSlash * 2);
                            }
                            else if (argument[i] == '"')
                            {
                                buffer.Append('\\', numBackSlash * 2 + 1);
                                buffer.Append('"');
                                i++;
                            }
                            else
                            {
                                buffer.Append('\\', numBackSlash);
                            }
                        }
                        else if (c == '"')
                        {
                            buffer.Append('\\');
                            buffer.Append('"');
                        }
                        else
                        {
                            buffer.Append(c);
                        }
                    }

                    buffer.Append('"');
                }
            }

            return SetArguments(buffer.ToString());
        }

        /// <inheritdoc />
        public ICli SetStandardInput(Stream standardInput)
        {
            if (standardInput == Stream.Null)
            {
                return this;
            }
            _standardInput = standardInput;
            if (this.CurrentProcess != null)
            {
                this.CurrentProcess.PipeStandardInput(standardInput);
            }

            return this;
        }

        /// <inheritdoc />
        public ICli SetStandardInput(string standardInput, Encoding encoding)
        {
            // Represent string as stream
            var data = encoding.GetBytes(standardInput);
            var stream = new MemoryStream(data, false);

            return SetStandardInput(stream);
        }

        /// <inheritdoc />
        public ICli SetStandardInput(string standardInput) => SetStandardInput(standardInput, Console.InputEncoding);

        /// <inheritdoc />
        public ICli SetEnvironmentVariable(string key, string value)
        {
            _environmentVariables[key] = value;
            return this;
        }

        /// <inheritdoc />
        public ICli SetStandardOutputEncoding(Encoding encoding)
        {
            _standardOutputEncoding = encoding;
            return this;
        }

        /// <inheritdoc />
        public ICli SetStandardErrorEncoding(Encoding encoding)
        {
            _standardErrorEncoding = encoding;
            return this;
        }

        /// <inheritdoc />
        public ICli SetStandardOutputCallback(Action<string> callback)
        {
            _standardOutputObserver = callback;
            return this;
        }

        /// <inheritdoc />
        public ICli SetStandardOutputClosedCallback(Action callback)
        {
            _standardOutputClosedObserver = callback;
            return this;
        }

        /// <inheritdoc />
        public ICli SetStandardErrorCallback(Action<string> callback)
        {
            _standardErrorObserver = callback;
            return this;
        }

        /// <inheritdoc />
        public ICli SetStandardErrorClosedCallback(Action callback)
        {
            _standardErrorClosedObserver = callback;
            return this;
        }

#if NET45 || NETCOREAPP3_0
        /// <inheritdoc />
        public ICli SetCancellationToken(CancellationToken token, bool killEntireProcessTree)
        {
            _cancellationToken = token;
            _killEntireProcessTree = killEntireProcessTree;
            return this;
        }
#endif

        /// <inheritdoc />
        public ICli SetCancellationToken(CancellationToken token)
        {
            _cancellationToken = token;
            return this;
        }

        /// <inheritdoc />
        public ICli EnableExitCodeValidation(bool isEnabled = true)
        {
            _exitCodeValidation = isEnabled;
            return this;
        }

        /// <inheritdoc />
        public ICli EnableStandardErrorValidation(bool isEnabled = true)
        {
            _standardErrorValidation = isEnabled;
            return this;
        }

        #endregion

        #region Execute

        private CliProcess StartProcess()
        {
            // Create process start info
            var startInfo = new ProcessStartInfo
            {
                FileName = _filePath,
                WorkingDirectory = _workingDirectory,
                Arguments = _arguments,
                StandardOutputEncoding = _standardOutputEncoding,
                StandardErrorEncoding = _standardErrorEncoding
            };

            // Set environment variables
#if NET45
            foreach (var variable in _environmentVariables)
                startInfo.EnvironmentVariables[variable.Key] = variable.Value;
#else
            foreach (var variable in _environmentVariables)
                startInfo.Environment[variable.Key] = variable.Value;
#endif

            // Create and start process
            var process = new CliProcess(
                startInfo,
                _standardOutputObserver,
                _standardErrorObserver,
                _standardOutputClosedObserver,
                _standardErrorClosedObserver
            );
            process.Start();

            return process;
        }

        private void ValidateExecutionResult(ExecutionResult result)
        {
            // Validate exit code if needed
            if (_exitCodeValidation && result.ExitCode != 0)
                throw new ExitCodeValidationException(result);

            // Validate standard error if needed
            if (_standardErrorValidation && !string.IsNullOrWhiteSpace(result.StandardError))
                throw new StandardErrorValidationException(result);
        }

        /// <inheritdoc />
        public async Task<ExecutionResult> ListenAsync(bool reuse = true)
        {
          return  await Task.Run(async ()=> {
                CliProcess process = StartProcess();
                try
                {
                    if (reuse)
                    {
                        this.CurrentProcess = process;
                    }
                    using (_cancellationToken.Register(() => process.TryKill(_killEntireProcessTree)))
                    {
                        ProcessId = process.Id;

                        // Pipe stdin
                       process.PipeStandardInput(_standardInput);

                        // Wait for exit
                       await process.WaitForExitAsync();

                        // Throw if cancelled
                        _cancellationToken.ThrowIfCancellationRequested();

                        // Create execution result
                        var result = new ExecutionResult(process.ExitCode,
                            process.StandardOutput,
                            process.StandardError,
                            process.StartTime,
                            process.ExitTime);

                        // Validate execution result
                        ValidateExecutionResult(result);
                        return result;
                    }
                }
                finally
                {
                    if (!reuse)
                    {
                        process.Dispose();
                    }

                }
            });
            // Set up execution context

            

        }
        public  ExecutionResult Execute()
        {
            // Set up execution context
            using var process = StartProcess();
            using (_cancellationToken.Register(() => process.TryKill(_killEntireProcessTree)))
            {
                ProcessId = process.Id;

                // Pipe stdin
                 process.PipeStandardInput(_standardInput);

                // Wait for exit
                 process.WaitForExit();

                // Throw if cancelled
                _cancellationToken.ThrowIfCancellationRequested();

                // Create execution result
                var result = new ExecutionResult(process.ExitCode,
                    process.StandardOutput,
                    process.StandardError,
                    process.StartTime,
                    process.ExitTime);

                // Validate execution result
                ValidateExecutionResult(result);

                return result;
            }
        }
        /// <inheritdoc />
        public async Task<ExecutionResult> ExecuteAsync()
        {
            // Set up execution context
            using var process = StartProcess();
            using (_cancellationToken.Register(() => process.TryKill(_killEntireProcessTree)))
            {
                ProcessId = process.Id;

                // Pipe stdin
                await process.PipeStandardInputAsync(_standardInput);

                // Wait for exit
                await process.WaitForExitAsync();

                // Throw if cancelled
                _cancellationToken.ThrowIfCancellationRequested();

                // Create execution result
                var result = new ExecutionResult(process.ExitCode,
                    process.StandardOutput,
                    process.StandardError,
                    process.StartTime,
                    process.ExitTime);

                // Validate execution result
                ValidateExecutionResult(result);

                return result;
            }
        }

        /// <inheritdoc />
        public void ExecuteAndForget()
        {
            // Set up execution context
            using var process = StartProcess();
            ProcessId = process.Id;

            // Write stdin
            process.PipeStandardInput(_standardInput);
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    if (this.CurrentProcess != null)
                    {
                        this.CurrentProcess.Dispose();
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~Cli()
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

        #endregion
    }

    public partial class Cli
    {
        /// <summary>
        /// Initializes an instance of <see cref="ICli"/> on the target executable.
        /// </summary>
        public static ICli Wrap(string filePath, string arguments = null) => new Cli(filePath, arguments);
    }
}