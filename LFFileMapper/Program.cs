using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using LaForgeFileMapper.Layers.BackEnd;
using LaForgeFileMapper.Model;

namespace LaForgeFileMapper
{
    class Program
    {

        static void Main(string[] args)
        {

            string[] input;

            try
            {

                initializeDirs();
                List<string> argsList = args.ToList();
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
                                    throw new Exception("Must inform a mapper file, on --run");
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
                                    throw new Exception("Must inform a file name on --init.");
                                }

                                if (argsList.Count > argsList.IndexOf(arg) + 2 && !argsList[argsList.IndexOf(arg) + 2].StartsWith("-"))
                                {
                                    inputArgs.Add("INITPATH", argsList[argsList.IndexOf(arg) + 2]);
                                }
                                else
                                {
                                    inputArgs.Add("INITPATH", FileMapper.workDirectory);
                                }
                    
                            }
                            else if (arg.Equals("--replace", StringComparison.InvariantCultureIgnoreCase))
                            {
                                inputArgs.Add("REPLACE", "");
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
                                throw new Exception("Invalid command use -h or --help to a list of valid commands.");
                            }
                        }
                    }


                    if (inputArgs.Count > 0 && !inputArgs.ContainsKey("EXIT"))
                    {

                        if (inputArgs.ContainsKey("NOEXIT"))
                        {
                            exit = false;
                        }
                        else if (inputArgs.ContainsKey("INIT"))
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
                        else if (inputArgs.ContainsKey("RUN"))
                        {
                            execute(inputArgs);
                        }
                        else if (inputArgs.ContainsKey("VERSION"))
                        {
                            Console.WriteLine("CheckTestFiles version: " + Assembly.GetEntryAssembly().GetName().Version);
                        }
                        else if (inputArgs.ContainsKey("ENV"))
                        {
                            Console.WriteLine("Current working directory: " + FileMapper.currentDirectory);
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
                throw;
            }
        }

        private static void initFile(Dictionary<string, string> inputArgs)
        {
            FilePatternMapper file;
            StreamWriter patternFile;
            string patternFileName = "";
            string mapperFileName = "";

            try
            {

                if (!Path.IsPathRooted(inputArgs.GetValueOrDefault("INITPATH")))
                {
                    mapperFileName = Path.GetFullPath(Path.Combine(FileMapper.currentDirectory, inputArgs.GetValueOrDefault("INITPATH"), inputArgs.GetValueOrDefault("INIT") + ".json"));
                    patternFileName = Path.GetFullPath(Path.Combine(FileMapper.currentDirectory, inputArgs.GetValueOrDefault("INITPATH"), inputArgs.GetValueOrDefault("INIT") + ".txt"));
                }
                else
                {
                    mapperFileName = Path.Combine(inputArgs.GetValueOrDefault("INITPATH"), inputArgs.GetValueOrDefault("INIT") + ".json");
                    patternFileName = Path.Combine(inputArgs.GetValueOrDefault("INITPATH"), inputArgs.GetValueOrDefault("INIT") + ".txt");
                }

                file = new FilePatternMapper()
                {
                    Name = "Name",
                    DirFilter = "",
                    FileFilter = "",
                    OutputFileName = "",
                    OutputFolder = "",
                    OutputPatern = inputArgs.GetValueOrDefault("INIT") + ".txt",
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

                Console.WriteLine("Do you want to open the patern file and mapper file? (y or n)");
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
                }

            }
            catch (Exception)
            {
                throw;
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

            try
            {

                if (!Path.IsPathRooted(inputArgs.GetValueOrDefault("INITPATH")))
                {
                    mapperFileName = Path.GetFullPath(Path.Combine(FileMapper.currentDirectory, inputArgs.GetValueOrDefault("INITPATH"), inputArgs.GetValueOrDefault("INIT") + ".json"));
                    patternFileName = Path.GetFullPath(Path.Combine(FileMapper.currentDirectory, inputArgs.GetValueOrDefault("INITPATH"), inputArgs.GetValueOrDefault("INIT") + ".txt"));
                }
                else
                {
                    mapperFileName = Path.Combine(inputArgs.GetValueOrDefault("INITPATH") , inputArgs.GetValueOrDefault("INIT") + ".json");
                    patternFileName = Path.Combine(inputArgs.GetValueOrDefault("INITPATH") , inputArgs.GetValueOrDefault("INIT") + ".txt");
                }

                file = new FilePatternMapper()
                {
                    Name = "Name",
                    DirFilter = "",
                    FileFilter = "",
                    OutputFileName = "",
                    OutputFolder = "",
                    OutputPatern = inputArgs.GetValueOrDefault("INIT") + ".txt",
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

                        auxSearchLocation = 0;
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

                Console.WriteLine("Do you want to open the patern file? (y or n)");
                if (Console.ReadLine().Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                {
                    var p = new Process();
                    p.StartInfo = new ProcessStartInfo(@patternFileName)
                    {
                        UseShellExecute = true
                    };
                    p.Start();
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void help()
        {
            Console.WriteLine("Run:");
            Console.WriteLine(" --run <mapper file name> [optional directory path]");
            Console.WriteLine("     --replace (optional replace existing files)");
            Console.WriteLine("");

            Console.WriteLine("Initialize mapper:");
            Console.WriteLine(" --init <mapper file name> [mapper file path optional]");
            Console.WriteLine("     --interactive (optional inform values)");
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
        }
    }
}
