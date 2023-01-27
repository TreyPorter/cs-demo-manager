﻿using System;
using System.Collections.Generic;
using System.IO;
using Core.Models.Source;
using System.Linq;

namespace CLI
{
    internal abstract class ExportCommand : BaseCommand
    {
        protected Source _source;
        protected readonly List<string> _demoPaths;
        protected string _outputFolderPath;
        protected bool _forceAnalyze = false;
        protected long _focusSteamId = 0;

        public ExportCommand(string commandName, string description) : base(commandName, description)
        {
            _demoPaths = new List<string>();
        }

        protected void ParseArgs(string[] args, string[] allowedOptions)
        {
            base.ParseArgs(args);

            for (int index = 0; index < args.Length; index++)
            {
                string arg = args[index];
                bool isOption = arg.StartsWith("--");
                if (isOption)
                {
                    switch (arg)
                    {
                        case "--source":
                            if (args.Length > index + 1)
                            {
                                index += 1;
                                string sourceName = args[index];
                                _source = Source.Factory(sourceName);
                                if (_source == null)
                                {
                                    Console.WriteLine($@"Invalid source: {sourceName}");
                                    Environment.Exit(1);
                                }
                            }
                            else
                            {
                                Console.WriteLine(@"Missing --source argument value");
                                Environment.Exit(1);
                            }

                            break;
                        case "--output":
                            if (args.Length > index + 1)
                            {
                                index += 1;
                                _outputFolderPath = Path.GetFullPath(args[index]).TrimEnd(Path.DirectorySeparatorChar);
                                bool folderExists = Directory.Exists(_outputFolderPath);
                                if (!folderExists)
                                {
                                    Console.WriteLine(@"The output folder doesn't exists");
                                    Environment.Exit(1);
                                }

                                if (!IsDirectoryWritable(_outputFolderPath))
                                {
                                    Console.WriteLine(@"The output folder is not writable");
                                    Environment.Exit(1);
                                }
                            }
                            else
                            {
                                Console.WriteLine(@"Missing --output argument value");
                                Environment.Exit(1);
                            }

                            break;
                        case "--force-analyze":
                            _forceAnalyze = true;
                            break;
                        case "--steamid":
                            if (args.Length > index + 1)
                            {
                                index += 1;
                                bool success = long.TryParse(args[index], out long steamId);
                                if (success)
                                {
                                    _focusSteamId = steamId;
                                }
                                else
                                {
                                    Console.WriteLine(@"Invalid SteamID");
                                    Environment.Exit(1);
                                }
                            }
                            else
                            {
                                Console.WriteLine(@"Missing --steamid argument value");
                                Environment.Exit(1);
                            }
                            break;
                        default:
                            if (!allowedOptions.Contains(arg))
                            {
                                Console.WriteLine($@"Unknown option {arg}");
                                Environment.Exit(1);
                            }

                            break;
                    }
                }
                else
                {
                    bool isDemoFile = arg.EndsWith(".dem");
                    if (isDemoFile)
                    {
                        bool fileExists = File.Exists(arg);
                        if (!fileExists)
                        {
                            Console.WriteLine($@"The file doesn't exists: {arg}");
                            Environment.Exit(1);
                        }

                        if (_demoPaths.Contains(arg))
                        {
                            continue;
                        }

                        _demoPaths.Add(arg);
                    }
                    else
                    {
                        try
                        {
                            if (arg.EndsWith("\""))
                            {
                                arg = arg.Substring(0, arg.Length - 1) + "\\";
                            }

                            string directoryPath = Path.GetFullPath(arg);
                            bool directoryExists = Directory.Exists(directoryPath);
                            if (directoryExists)
                            {
                                string[] files = Directory.GetFiles(directoryPath, "*.dem");
                                foreach (string file in files)
                                {
                                    if (_demoPaths.Contains(file))
                                    {
                                        continue;
                                    }

                                    _demoPaths.Add(file);
                                }
                            }
                            else
                            {
                                Console.WriteLine($@"The directory doesn't exists: {arg}");
                                Environment.Exit(1);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($@"Invalid directory: {ex.Message}");
                            Environment.Exit(1);
                        }
                    }
                }
            }
        }

        protected string BuildOutputFolderPathFromDemoPath(string demoPath)
        {
            if (!string.IsNullOrEmpty(_outputFolderPath))
            {
                return _outputFolderPath;
            }

            string demoFolderPath = Path.GetDirectoryName(demoPath);
            if (IsDirectoryWritable(demoFolderPath))
            {
                return demoFolderPath;
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }
    }
}
