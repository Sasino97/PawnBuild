/*
 * *********************************************************************
 *                              PawnBuild
 * *********************************************************************
 *                  Copyright (c) 2021 - Sasinosoft Games
 * *********************************************************************
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * *********************************************************************
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace PawnBuild
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("Usage: PawnBuild <buildFileName.json> [options]");
                Console.WriteLine(@"
Options:
    -r (--run): executes the run instruction after building
    -v (--verbose): prints more information
    -f (--force): does not skip any file
");
                return;
            }

            string fname = args[0];
            if (!File.Exists(fname))
            {
                Console.WriteLine("The specified file does not exist.");
                return;
            }
            Directory.SetCurrentDirectory(new FileInfo(fname).Directory.FullName);

            // parse args
            bool run = args.Contains("-r") || args.Contains("--run");
            bool verbose = args.Contains("-v") || args.Contains("--verbose");
            bool force = args.Contains("-f") || args.Contains("--force");

            if (verbose)
                Console.WriteLine($"----- Parsing {fname}... -----");

            BuildFile buildFile = null;
            try
            {
                buildFile = JsonConvert.DeserializeObject<BuildFile>(File.ReadAllText(fname));
            }
            catch (JsonSerializationException e)
            {
                Console.WriteLine($"\nSyntax error: invalid build file format ({fname}).");
                Console.WriteLine(e.ToString());
            }

            try
            {
                if (verbose)
                {
                    Console.WriteLine("\n----- Source -> Output -----");

                    foreach (BuildFolder build in buildFile.BuildFolders)
                        Console.WriteLine($"{build.SourceFolder} ===> {build.OutputFolder}");

                    Console.WriteLine("\n----- Include Folders -----");

                    foreach (string inc in buildFile.IncludeFolders)
                        Console.WriteLine($"{inc}");

                    Console.WriteLine("\n----- Files to Compile -----");

                    foreach (string file in buildFile.Files)
                        Console.WriteLine($"{file}");

                    Console.WriteLine("\n----- Starting build... -----");
                }

                int succeeded = 0;
                int failed = 0;
                int skipped = 0;

                Console.WriteLine($"------ Build started - Project: {buildFile.ProjectName} ------");

                foreach (string file in buildFile.Files)
                {
                    foreach (BuildFolder build in buildFile.BuildFolders)
                    {
                        string srcPath = Path.Combine(build.SourceFolder, file);
                        if (File.Exists(srcPath))
                        {
                            string outPath = Path.Combine(build.OutputFolder, Path.GetFileNameWithoutExtension(file) + ".amx");

                            if (!force)
                            {
                                //
                                DateTime outTime = File.GetLastWriteTime(outPath);

                                // recursively check if include files changed
                                var checkedFiles = new List<string>();
                                DateTime srcTime = DateTime.MaxValue;

                                void checkLastCompileTimeRecursive(string path)
                                {
                                    // skip if already checked
                                    if (checkedFiles.Contains(path))
                                        return;

                                    if (!File.Exists(path))
                                        return;

                                    // check last write time
                                    var t = File.GetLastWriteTime(path);

                                    if (t > outTime)

                                    if (t < srcTime)
                                        srcTime = t;

                                    checkedFiles.Add(path);

                                    // check includes of this file
                                    using (var fileReader = new StreamReader(path))
                                    {
                                        string line;
                                        while ((line = fileReader.ReadLine()) != null)
                                        {
                                            if (!line.StartsWith('#') && !line.StartsWith(' ') && !line.StartsWith('\t'))
                                                continue;

                                            if (line.Contains("#include") || line.Contains("#tryinclude"))
                                            {
                                                var idx = line.IndexOf(' ', line.IndexOf('#')) + 1;
                                                var includePath = line
                                                    .Substring(idx)
                                                    .Trim()
                                                    .Replace("<", "")
                                                    .Replace(">", "")
                                                    .Replace("\"", "");

                                                checkLastCompileTimeRecursive(includePath);
                                            }
                                        }
                                    }
                                }

                                checkLastCompileTimeRecursive(srcPath);


                                if (srcTime < outTime)
                                {
                                    Console.WriteLine($"* Skipping {srcPath}");
                                    skipped++;
                                    continue;
                                }
                            }

                            string compilerArgs = $"\"{srcPath}\" -o\"{outPath}\" ";

                            foreach (string incPath in buildFile.IncludeFolders)
                                compilerArgs += $"-i\"{incPath}\" ";

                            if (!string.IsNullOrWhiteSpace(buildFile.Args))
                                compilerArgs += $"{buildFile.Args} ";

                            Console.WriteLine($"* Compiling {srcPath} -> {outPath}");

                            var pawnccOutput = new StringBuilder();
                            var startInfo = new ProcessStartInfo()
                            {
                                FileName = "pawncc",
                                Arguments = compilerArgs,
                                CreateNoWindow = true,
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true
                            };

                            using (var process = new Process() { StartInfo = startInfo })
                            {
                                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                                {
                                    pawnccOutput.AppendLine(e.Data);
                                };

                                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                                {
                                    pawnccOutput.AppendLine(e.Data);
                                };

                                process.Start();
                                process.BeginOutputReadLine();
                                process.BeginErrorReadLine();
                                process.WaitForExit(5000);

                                string finalOutput = pawnccOutput
                                    .ToString()
                                    .Insert(0, "    ")
                                    .Replace("\n", "\n    ")
                                    .TrimEnd()
                                ;

                                if (finalOutput.Contains(" Error.") || finalOutput.Contains(" Errors."))
                                    failed++;
                                else
                                    succeeded++;

                                if (finalOutput.Contains(" Error.") || finalOutput.Contains(" Errors.")
                                     || finalOutput.Contains(" Warning.") || finalOutput.Contains(" Warnings."))
                                {
                                    Console.WriteLine(finalOutput + "\n");
                                }
                            }
                        }
                    }
                }

                Console.WriteLine($"========== Build: {succeeded} succeeded, {failed} failed, {skipped} skipped ==========");

                if (run)
                {
                    if (buildFile.Run.Length > 0)
                    {
                        foreach (string runFile in buildFile.Run)
                        {
                            if (!string.IsNullOrWhiteSpace(runFile))
                            {
                                var runProcess = new Process()
                                {
                                    StartInfo = new ProcessStartInfo()
                                    {
                                        FileName = new FileInfo(runFile).FullName,
                                        WorkingDirectory = new FileInfo(runFile).Directory.FullName
                                    }
                                };
                                runProcess.Start();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nGeneric error:");
                Console.WriteLine(e.ToString());
            }
        }
    }
}
