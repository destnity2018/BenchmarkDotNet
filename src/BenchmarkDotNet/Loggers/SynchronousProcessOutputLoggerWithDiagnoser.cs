﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;

namespace BenchmarkDotNet.Loggers
{
    internal class SynchronousProcessOutputLoggerWithDiagnoser
    {
        private readonly ILogger logger;
        private readonly Process process;
        private readonly IDiagnoser diagnoser;
        private readonly DiagnoserActionParameters diagnoserActionParameters;

        public SynchronousProcessOutputLoggerWithDiagnoser(ILogger logger, Process process, IDiagnoser diagnoser, BenchmarkCase benchmarkCase, BenchmarkId benchmarkId, IConfig config)
        {
            if (!process.StartInfo.RedirectStandardOutput)
                throw new NotSupportedException("set RedirectStandardOutput to true first");
            if (!process.StartInfo.RedirectStandardInput)
                throw new NotSupportedException("set RedirectStandardInput to true first");

            this.logger = logger;
            this.process = process;
            this.diagnoser = diagnoser;
            diagnoserActionParameters = new DiagnoserActionParameters(process, benchmarkCase, benchmarkId, config);

            LinesWithResults = new List<string>();
            LinesWithExtraOutput = new List<string>();
        }

        internal List<string> LinesWithResults { get; }

        internal List<string> LinesWithExtraOutput { get; }

        internal void ProcessInput()
        {
            string line;

            // Peek -1 or 0 can indicate internal error.
            while (!process.StandardOutput.EndOfStream && process.StandardOutput.Peek() > 0)
            {
                // ReadLine() can actually return string.Empty and null as valid values.
                line = process.StandardOutput.ReadLine();

                if (!string.IsNullOrEmpty(line)) // Skip bad data.
                {
                    logger.WriteLine(LogKind.Default, line);

                    if (!line.StartsWith("//"))
                    { LinesWithResults.Add(line); }
                    else if (Engine.Signals.TryGetSignal(line, out var signal))
                    {
                        diagnoser?.Handle(signal, diagnoserActionParameters);
                        process.StandardInput.WriteLine(Engine.Signals.Acknowledgment);
                    }
                    else
                    { LinesWithExtraOutput.Add(line); }
                }
            }
        }
    }
}