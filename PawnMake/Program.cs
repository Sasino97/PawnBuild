using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace PawnMake
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("Usage: PawnMake <makefilename.json> [options]");
                Console.WriteLine("\nOptions:\n\t-r run");
                return;
            }

            string fname = args[0];
            if (!File.Exists(fname))
            {
                Console.WriteLine("The specified file does not exist.");
                return;
            }
            Directory.SetCurrentDirectory(new FileInfo(fname).Directory.FullName);

            #if VERBOSE
            Console.WriteLine($"----- Parsing {fname}... -----");
            #endif

            MakeFile makeFile = null;
            try
            {
                makeFile = JsonConvert.DeserializeObject<MakeFile>(File.ReadAllText(fname));
            }
            catch (JsonSerializationException e)
            {
                Console.WriteLine("\nSyntax error: invalid file format.");
                Console.WriteLine(e.ToString());
            }

            try
            {
                #if VERBOSE
                Console.WriteLine("\n----- Source -> Output -----");

                foreach (BuildFolder build in makeFile.BuildFolders)
                    Console.WriteLine($"{build.SourceFolder} ===> {build.OutputFolder}");

                Console.WriteLine("\n----- Include Folders -----");

                foreach (string inc in makeFile.IncludeFolders)
                    Console.WriteLine($"{inc}");

                Console.WriteLine("\n----- Files to Compile -----");

                foreach (string file in makeFile.Files)
                    Console.WriteLine($"{file}");

                Console.WriteLine("\n----- Starting build... -----");
                #endif

                foreach (string file in makeFile.Files)
                {
                    foreach (BuildFolder build in makeFile.BuildFolders)
                    {
                        string srcPath = Path.Combine(build.SourceFolder, file);
                        if (File.Exists(srcPath))
                        {
                            string outPath = Path.Combine(build.OutputFolder, Path.GetFileNameWithoutExtension(file) + ".amx");
                            string compilerArgs = $"\"{srcPath}\" -o\"{outPath}\" ";

                            foreach (string incPath in makeFile.IncludeFolders)
                                compilerArgs += $"-i\"{incPath}\" ";

                            if(!string.IsNullOrWhiteSpace(makeFile.Args))
                                compilerArgs += $"{makeFile.Args} ";

                            Console.WriteLine($"\nCompiling {srcPath} ...");

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

                                Console.WriteLine(pawnccOutput.ToString());
                            }
                        }
                    }
                }

                if(args.Length > 1)
                {
                    string parameter = args[1];
                    if(parameter == "-r")
                    {
                        if(makeFile.Run.Length > 0)
                        {
                            foreach(string run in makeFile.Run)
                            {
                                if (!string.IsNullOrWhiteSpace(run))
                                {
                                    var runProcess = new Process()
                                    {
                                        StartInfo = new ProcessStartInfo()
                                        {
                                            FileName = new FileInfo(run).FullName,
                                            WorkingDirectory = new FileInfo(run).Directory.FullName
                                        }
                                    };
                                    runProcess.Start();
                                }
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
