using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using LaForgeFileMapper.Layers.BackEnd;
using LFFileMapper.Model;

namespace LaForgeFileMapper
{
    class Program
    {
        protected Program()
        {
        }

        static void Main(string[] args)
        {

            string[] input;

            try
            {

                initializeDirs();
                List<string> argsList;
                Dictionary<String, String> inputArgs;
                bool exit = true;
                input = args;

                do
                {

                    argsList = input.ToList();
                    inputArgs = new Dictionary<string, string>();

                    foreach (var arg in argsList)
                    {
                        if (arg.StartsWith("-"))
                        {
                            if (arg.Equals("--run", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (argsList.Count > argsList.IndexOf(arg) + 1 && !argsList[argsList.IndexOf(arg) + 1].StartsWith("-"))
                                {
                                    inputArgs.Add("RUN", argsList[argsList.IndexOf(arg) + 1]);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Must inform a mapper file, on --run");
                                }

                                if (argsList.Count > argsList.IndexOf(arg) + 2 && !argsList[argsList.IndexOf(arg) + 2].StartsWith("-"))
                                {
                                    inputArgs.Add("RUNPATH", argsList[argsList.IndexOf(arg) + 2]);
                                }

                            }
                            else if (arg.Equals("--run-seq", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (argsList.Count > argsList.IndexOf(arg) + 1 && !argsList[argsList.IndexOf(arg) + 1].StartsWith("-"))
                                {
                                    inputArgs.Add("RUNSEQ", argsList[argsList.IndexOf(arg) + 1]);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Must inform a sequence file, on --run-seq");
                                }

                                if (argsList.Count > argsList.IndexOf(arg) + 2 && !argsList[argsList.IndexOf(arg) + 2].StartsWith("-"))
                                {
                                    inputArgs.Add("RUNPATH", argsList[argsList.IndexOf(arg) + 2]);
                                }

                            }
                            else if (arg.Equals("--interactive", StringComparison.InvariantCultureIgnoreCase))
                            {
                                inputArgs.Add("INTERACTIVE", "");
                            }
                            else if (arg.Equals("--init", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (argsList.Count > argsList.IndexOf(arg) + 1 && !argsList[argsList.IndexOf(arg) + 1].StartsWith("-"))
                                {
                                    inputArgs.Add("INIT", argsList[argsList.IndexOf(arg) + 1]);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Must inform a file name on --init.");
                                }
                            }
                            else if (arg.Equals("--init-seq", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (argsList.Count > argsList.IndexOf(arg) + 1 && !argsList[argsList.IndexOf(arg) + 1].StartsWith("-"))
                                {
                                    inputArgs.Add("INITSEQ", argsList[argsList.IndexOf(arg) + 1]);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Must inform a file name on --init.");
                                }

                            }
                            else if (arg.Equals("--seq", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (argsList.Count > argsList.IndexOf(arg) + 1 && !argsList[argsList.IndexOf(arg) + 1].StartsWith("-"))
                                {                                    
                                    inputArgs.Add("SEQ", String.Join(" ", argsList.GetRange(argsList.IndexOf(arg) + 1, argsList.Count - (argsList.IndexOf(arg) + 1))));
                                }
                                else
                                {
                                    throw new InvalidOperationException("Must inform the sequence list on --seq.");
                                }
                            }
                            else if (arg.Equals("--replace", StringComparison.InvariantCultureIgnoreCase))
                            {
                                inputArgs.Add("REPLACE", "");
                            }
                            else if (arg.Equals("--open", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (argsList.Count > argsList.IndexOf(arg) + 1 && !argsList[argsList.IndexOf(arg) + 1].StartsWith("-"))
                                {
                                    inputArgs.Add("OPEN", argsList[argsList.IndexOf(arg) + 1]);
                                }
                                else
                                {
                                    inputArgs.Add("OPEN", "");
                                }
                            }
                            else if (arg.Equals("--open-seq", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (argsList.Count > argsList.IndexOf(arg) + 1 && !argsList[argsList.IndexOf(arg) + 1].StartsWith("-"))
                                {
                                    inputArgs.Add("OPENSEQ", argsList[argsList.IndexOf(arg) + 1]);
                                }
                                else
                                {
                                    inputArgs.Add("OPENSEQ", "");
                                }
                            }
                            else if (arg.Equals("--help", StringComparison.InvariantCultureIgnoreCase) ||
                                     arg.Equals("-h", StringComparison.InvariantCultureIgnoreCase))
                            {
                                inputArgs.Add("HELP", "");
                            }
                            else if (arg.Equals("--version", StringComparison.InvariantCultureIgnoreCase) ||
                                     arg.Equals("-v", StringComparison.InvariantCultureIgnoreCase))
                            {
                                inputArgs.Add("VERSION", "");
                            }
                            else if (arg.Equals("--env", StringComparison.InvariantCultureIgnoreCase) ||
                                     arg.Equals("-env", StringComparison.InvariantCultureIgnoreCase))
                            {
                                inputArgs.Add("ENV", "");
                            }
                            else if (arg.Equals("--no-exit", StringComparison.InvariantCultureIgnoreCase))
                            {
                                inputArgs.Add("NOEXIT", "");
                            }
                            else if (arg.Equals("--exit", StringComparison.InvariantCultureIgnoreCase))
                            {
                                inputArgs.Add("EXIT", "");
                            }
                            else
                            {
                                throw new InvalidOperationException("Invalid command use -h or --help to a list of valid commands.");
                            }
                        }
                    }


                    if (inputArgs.Count > 0 && !inputArgs.ContainsKey("EXIT"))
                    {

                        if (inputArgs.ContainsKey("NOEXIT"))
                        {
                            exit = false;
                        }

                        if (inputArgs.ContainsKey("INIT"))
                        {
                            if (inputArgs.ContainsKey("INTERACTIVE"))
                            {
                                initFileInteractive(inputArgs);
                            }
                            else
                            {
                                initFile(inputArgs);
                            }
                        }
                        else if (inputArgs.ContainsKey("INITSEQ"))
                        {
                            if (!inputArgs.ContainsKey("SEQ"))
                            {
                                throw new InvalidOperationException("Must inform the sequence list on --init-seq, use --seq.");
                            }
                            initSeq(inputArgs);
                        }
                        else if (inputArgs.ContainsKey("RUN"))
                        {
                            execute(inputArgs);
                        }
                        else if (inputArgs.ContainsKey("RUNSEQ"))
                        {
                            executeSeq(inputArgs);
                        }
                        else if (inputArgs.ContainsKey("VERSION"))
                        {
                            Console.WriteLine("LFFileMapper version: " + Assembly.GetEntryAssembly().GetName().Version);
                        }
                        else if (inputArgs.ContainsKey("ENV"))
                        {
                            Console.WriteLine("Current working directory: " + FileMapper.currentDirectory);
                        }
                        else if (inputArgs.ContainsKey("OPEN"))
                        {
                            OpenFiles(inputArgs.GetValueOrDefault("OPEN"));
                        }
                        else if (inputArgs.ContainsKey("OPENSEQ"))
                        {
                            OpenSeq(inputArgs.GetValueOrDefault("OPENSEQ"));
                        }
                        else if (inputArgs.ContainsKey("HELP"))
                        {
                            help();
                        }
                        else
                        {
                            Console.WriteLine("Invalid command, use -h or --help to see a list of valid commands.");
                        }

                    }
                    else if (inputArgs.ContainsKey("EXIT"))
                    {
                        exit = true;
                    }
                    else
                    {
                        Console.WriteLine("Invalid command, use -h or --help to see a list of valid commands.");
                    }

                    if (!exit)
                    {

                        input = Console.In.ReadLine().Split(" ");
                    }

                } while (!exit);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

 

        private static void executeSeq(Dictionary<string, string> inputArgs)
        {
            List<string> result;
            try
            {

                if (!inputArgs.ContainsKey("RUNPATH"))
                {
                    inputArgs.Add("RUNPATH", FileMapper.currentDirectory);
                }

                result = FileMapper.MapSequence(inputArgs.GetValueOrDefault("RUNSEQ"), inputArgs.GetValueOrDefault("RUNPATH"), inputArgs.ContainsKey("REPLACE"));

                foreach (string item in result)
                {
                    Console.WriteLine(item);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void execute(Dictionary<string, string> inputArgs)
        {
            List<string> result;
            try
            {

                if (!inputArgs.ContainsKey("RUNPATH"))
                {
                    inputArgs.Add("RUNPATH", FileMapper.currentDirectory);
                }

                result = FileMapper.MapFiles(inputArgs.GetValueOrDefault("RUN"), inputArgs.GetValueOrDefault("RUNPATH"), inputArgs.ContainsKey("REPLACE"));

                foreach (string item in result)
                {
                    Console.WriteLine(item);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void initFile(Dictionary<string, string> inputArgs)
        {
            FilePatternMapper file;
            StreamWriter patternFile;
            string patternFileName = "";
            string mapperFileName = "";
            string pythonFileName = "";
            try
            {
      
                mapperFileName = Path.GetFullPath(Path.Combine(FileMapper.workDirectory, inputArgs.GetValueOrDefault("INIT") + ".json"));
                patternFileName = Path.GetFullPath(Path.Combine(FileMapper.workDirectory, inputArgs.GetValueOrDefault("INIT") + ".txt"));
                pythonFileName = Path.GetFullPath(Path.Combine(FileMapper.workDirectory, inputArgs.GetValueOrDefault("INIT") + ".py"));
               
                file = new FilePatternMapper()
                {
                    Name = inputArgs.GetValueOrDefault("INIT"),
                    DirFilter = "",
                    FileFilter = "",
                    OutputFileName = "",
                    OutputFolder = "",
                    Variables = new List<FilePatternVariables>()
                    {
                        new FilePatternVariables()
                        {
                            Name = "Variabe_Name",
                            SearchLocation = ESearchLocation.Content,
                            SearchPatern = ""
                        }
                    }
                };
                file.save(mapperFileName);
                patternFile = new StreamWriter(patternFileName);
                patternFile.WriteLine("<%variable_name%>");
                patternFile.Close();
                patternFile.Dispose();
                Console.WriteLine("Saved: " + inputArgs.GetValueOrDefault("INIT"));

                Console.WriteLine("Do you want to open the generated files? (y or n)");
                if (Console.ReadLine().Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                {
                    var p1 = new Process();
                    p1.StartInfo = new ProcessStartInfo(@patternFileName)
                    {
                        UseShellExecute = true
                    };
                    p1.Start();

                    var p2 = new Process();
                    p2.StartInfo = new ProcessStartInfo(@mapperFileName)
                    {
                        UseShellExecute = true
                    };
                    p2.Start();

                    var p3 = new Process();
                    p3.StartInfo = new ProcessStartInfo(@pythonFileName)
                    {
                        UseShellExecute = true
                    };
                    p3.Start();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void initFileInteractive(Dictionary<string, string> inputArgs)
        {
            FilePatternMapper file;
            StreamWriter patternFile;
            bool hasNext = false;
            int auxSearchLocation = 0;
            string patternFileName = "";
            string mapperFileName = "";
            string pythonFileName = "";

            try
            {

                mapperFileName = Path.GetFullPath(Path.Combine(FileMapper.workDirectory, inputArgs.GetValueOrDefault("INIT") + ".json"));
                patternFileName = Path.GetFullPath(Path.Combine(FileMapper.workDirectory, inputArgs.GetValueOrDefault("INIT") + ".txt"));
                pythonFileName = Path.GetFullPath(Path.Combine(FileMapper.workDirectory, inputArgs.GetValueOrDefault("INIT") + ".py"));

                file = new FilePatternMapper()
                {
                    Name = inputArgs.GetValueOrDefault("INIT"),
                    DirFilter = "",
                    FileFilter = "",
                    OutputFileName = "",
                    OutputFolder = "",
                    Variables = new List<FilePatternVariables>()
                };

                Console.WriteLine("Directory filter (default ''): ");
                file.DirFilter = Console.ReadLine();
                Console.WriteLine("File filter (default ''): ");
                file.FileFilter = Console.ReadLine();

                Console.WriteLine("Directory exclusion filter (default ''): ");
                file.DirFilter = Console.ReadLine();
                Console.WriteLine("File exclusion filter (default ''): ");
                file.FileFilter = Console.ReadLine();

                Console.WriteLine("Inform variables? (y or n)");
                if (Console.ReadLine().Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                {
                    do
                    {
                        file.Variables.Add(new FilePatternVariables());

                        Console.WriteLine("Variable name:");
                        file.Variables.Last().Name = Console.ReadLine();

                        Console.WriteLine("Search location (0 - content, 1 - File name, 2 - File Directory, 3 - File Path, 4 - Literal):");
                        
                        if (int.TryParse(Console.ReadLine(), out auxSearchLocation))
                        {
                            file.Variables.Last().SearchLocation = (ESearchLocation)auxSearchLocation;
                        }
                        else
                        {
                            file.Variables.Last().SearchLocation = ESearchLocation.Content;
                        }

                        Console.WriteLine("Variable search pattern:");
                        file.Variables.Last().SearchPatern = Console.ReadLine();

                        hasNext = false;
                        Console.WriteLine("do you want to inform another variable? (y or n)");
                        if (Console.ReadLine().Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                        {
                            hasNext = true;
                        }
                    } while (hasNext);
                }



                file.save(mapperFileName);
                patternFile = new StreamWriter(patternFileName, false);
                foreach (FilePatternVariables filePatternVariable in file.Variables)
                {
                    patternFile.WriteLine("<%" + filePatternVariable.Name + "%>");
                }
                patternFile.Close();
                patternFile.Dispose();
                Console.WriteLine("Saved: " + inputArgs.GetValueOrDefault("INIT"));

                Console.WriteLine("Do you want to open the patern files? (y or n)");
                if (Console.ReadLine().Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                {
                    var p1 = new Process();
                    p1.StartInfo = new ProcessStartInfo(@patternFileName)
                    {
                        UseShellExecute = true
                    };
                    p1.Start();

                    var p2 = new Process();
                    p2.StartInfo = new ProcessStartInfo(@mapperFileName)
                    {
                        UseShellExecute = true
                    };
                    p2.Start();

                    var p3 = new Process();
                    p3.StartInfo = new ProcessStartInfo(@pythonFileName)
                    {
                        UseShellExecute = true
                    };
                    p3.Start();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void initSeq(Dictionary<string, string> inputArgs)
        {
            ExecutionSequence file;
            string sequenceFileName = "";
            List<string> mappersFilesNames;
            

            try
            {

                sequenceFileName = Path.GetFullPath(Path.Combine(FileMapper.sequenceDirectory, inputArgs.GetValueOrDefault("INITSEQ") + ".json"));
                mappersFilesNames = inputArgs.GetValueOrDefault("SEQ").Split(" ").ToList();

                file = new ExecutionSequence()
                {
                    Name = inputArgs.GetValueOrDefault("INITSEQ"),                   
                    Itens = (from item in mappersFilesNames
                             select new ExecutionItem() {
                                 Sequence = mappersFilesNames.IndexOf(item) + 1,
                                 Configuration = item}
                             ).ToList()
                };

                file.save(sequenceFileName);               
                Console.WriteLine("Saved: " + inputArgs.GetValueOrDefault("INITSEQ"));

                Console.WriteLine("Do you want to open the generated file? (y or n)");
                if (Console.ReadLine().Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                {
                    var p1 = new Process();
                    p1.StartInfo = new ProcessStartInfo(@sequenceFileName)
                    {
                        UseShellExecute = true
                    };
                    p1.Start();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void OpenFiles(string p_path)
        {
            StreamWriter file;

            try
            {
                if (string.IsNullOrEmpty(p_path))
                {
                    var p0 = new Process();
                    p0.StartInfo = new ProcessStartInfo(FileMapper.workDirectory)
                    {
                        UseShellExecute = true
                    };
                    p0.Start();
                }
                else
                {

                    if (File.Exists(Path.Combine(FileMapper.workDirectory, p_path + ".json")))
                    {

                        var p1 = new Process();
                        p1.StartInfo = new ProcessStartInfo(Path.Combine(FileMapper.workDirectory, p_path + ".json"))
                        {
                            UseShellExecute = true
                        };
                        p1.Start();

                        if (!File.Exists(Path.Combine(FileMapper.workDirectory, p_path + ".txt")))
                        {
                            file = new StreamWriter(Path.Combine(FileMapper.workDirectory, p_path + ".txt"));
                            file.WriteLine("");
                            file.Close();
                            file.Dispose();
                        }
                        var p2 = new Process();
                        p2.StartInfo = new ProcessStartInfo(Path.Combine(FileMapper.workDirectory, p_path + ".txt"))
                        {
                            UseShellExecute = true
                        };
                        p2.Start();

                        if (!File.Exists(Path.Combine(FileMapper.workDirectory, p_path + ".py")))
                        {
                            file = new StreamWriter(Path.Combine(FileMapper.workDirectory, p_path + ".py"));
                            file.WriteLine("");
                            file.Close();
                            file.Dispose();
                        }
                        var p3 = new Process();
                        p3.StartInfo = new ProcessStartInfo(Path.Combine(FileMapper.workDirectory, p_path + ".py"))
                        {
                            UseShellExecute = true
                        };
                        p3.Start();
                    }
                    else
                    {
                        Console.WriteLine("Configuration not found.");
                    }


                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void OpenSeq(string p_path)
        {
            try
            {
                if (string.IsNullOrEmpty(p_path))
                {
                    var p0 = new Process();
                    p0.StartInfo = new ProcessStartInfo(FileMapper.sequenceDirectory)
                    {
                        UseShellExecute = true
                    };
                    p0.Start();
                }
                else
                {

                    if (File.Exists(Path.Combine(FileMapper.sequenceDirectory, p_path + ".json")))
                    {
                        var p1 = new Process();
                        p1.StartInfo = new ProcessStartInfo(Path.Combine(FileMapper.sequenceDirectory, p_path + ".json"))
                        {
                            UseShellExecute = true
                        };
                        p1.Start();
                    }
                    else
                    {
                        Console.WriteLine("Sequence file not found.");
                    }


                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void help()
        {
            Console.WriteLine("Run:");
            Console.WriteLine(" --run <mapper file name> [optional directory path]");
            Console.WriteLine("     --replace (optional replace existing files)");
            Console.WriteLine("");

            Console.WriteLine("Run Sequence:");
            Console.WriteLine(" --run-seq <sequence file name> [optional directory path]");
            Console.WriteLine("     --replace (optional replace existing files)");
            Console.WriteLine("");

            Console.WriteLine("Initialize mapper:");
            Console.WriteLine(" --init <mapper file name>");
            Console.WriteLine("     --interactive (optional inform values)");
            Console.WriteLine("");

            Console.WriteLine("Initialize sequence:");
            Console.WriteLine(" --init-seq <sequence file name> ");
            Console.WriteLine("     --seq [Mappers List (Space separated)]");
            Console.WriteLine("");

            Console.WriteLine("Open Mapper Files:");
            Console.WriteLine(" --open [Mapper name optional]");
            Console.WriteLine("");

            Console.WriteLine("Open Sequence Files:");
            Console.WriteLine(" --open-seq [Sequence name optional]");
            Console.WriteLine("");

            Console.WriteLine(" Get current dir:");
            Console.WriteLine("     -env or --env");
            Console.WriteLine("");

            Console.WriteLine(" Program Version:");
            Console.WriteLine("     -v or --version");

            Console.WriteLine(" Help:");
            Console.WriteLine("     -h or --help");
        }

        private static void initializeDirs()
        {
            string auxPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            auxPath = Path.Combine(auxPath, "LaForgeFileMapper");
            if (!Directory.Exists(auxPath))
            {
                Directory.CreateDirectory(auxPath);
            }
            FileMapper.currentDirectory = Environment.CurrentDirectory;
            FileMapper.workDirectory = auxPath;
            FileMapper.sequenceDirectory = Path.Combine(auxPath, "seqs");
            if (!Directory.Exists(FileMapper.sequenceDirectory))
            {
                Directory.CreateDirectory(FileMapper.sequenceDirectory);
            }
        }
    }
}
