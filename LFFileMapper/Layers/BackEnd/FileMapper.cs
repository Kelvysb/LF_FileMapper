using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using LaForgeFileMapper.Model;
using LFFileMapper.Layers.BackEnd;

namespace LaForgeFileMapper.Layers.BackEnd
{
    class FileMapper
    {

        public static RegexOptions regexOptions = RegexOptions.Singleline;
        public static string currentDirectory { get; set; }
        public static string workDirectory { get; set; }
        private static string directory = "";
        private static string pythonScript = "";

        public static List<string> MapFiles(string p_mapperFile, string p_directory, bool p_replace)
        {
            List<string> result = new List<string>();
            List<string> files = new List<string>();
            FilePatternMapper pattern;
            StreamReader inputFile;

            try
            {

                directory = p_directory;

                if (!Path.IsPathRooted(directory))
                {
                    directory = Path.GetFullPath(Path.Combine(currentDirectory, directory));
                }

                pattern = FilePatternMapper.load(Path.Combine(workDirectory, p_mapperFile + ".json"));

                files = getAllFiles(directory, pattern.FileFilter, pattern.DirFilter, pattern.FileExclude, pattern.DirExclude);

                foreach (var file in files)
                {
                    inputFile = new StreamReader(file);
                    result.Add(ProcessFile(pattern, inputFile.ReadToEnd(), file, p_replace));
                    inputFile.Close();
                    inputFile.Dispose();
                }

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static string ProcessFile(FilePatternMapper pattern, string file, string filePath, bool replace)
        {
            string result = "";
            List<string> auxOutput = new List<string>();
            StreamWriter outputFile;
            List<VariablesResult> variables;
            string output = "";
            List<string> loopBlocks = new List<string>();
            string loopBlocksGroup = "";
            string outputName = "";
            string outputDir = "";
            StreamReader patternFile;
            

            try
            {
                //<%name%> = variable
                //<@name@> = repeat for each match of variable (begin of block)
                //<!name!> = value of current item in loop
                //</@name@> = repeat for each match of variable (end of block)
                //<%name.subname%> = sub variable


                pythonScript = LoadPythonScript(pattern.Name);

                //From body
                variables = CollectVariables(pattern.Variables.FindAll(patternVariables => patternVariables.SearchLocation == ESearchLocation.Content), file);

                //From file name
                variables.AddRange(CollectVariables(pattern.Variables.FindAll(patternVariables => patternVariables.SearchLocation == ESearchLocation.FileName), Path.GetFileName(filePath)));

                //From file path
                variables.AddRange(CollectVariables(pattern.Variables.FindAll(patternVariables => patternVariables.SearchLocation == ESearchLocation.FileDirectory), Path.GetDirectoryName(filePath)));

                patternFile = new StreamReader(Path.Combine(workDirectory, pattern.Name + ".txt"));
                output = patternFile.ReadToEnd();
                patternFile.Close();
                patternFile.Dispose();
                outputName = pattern.OutputFileName;
                outputDir = pattern.OutputFolder;
                

                //Replace Variables
                foreach (VariablesResult variablesResult in variables)
                {
                    //On Output
                    output = processVariable(variablesResult, output, "");

                    //On File Name
                    outputName = processVariable(variablesResult, outputName, "");

                    //On File path
                    outputDir = processVariable(variablesResult, outputDir, "");
                }

                //Replace loop blocks
                foreach (VariablesResult variablesResult in variables)
                {
                    loopBlocks = CollectLoopBlocks(output, variablesResult.Name);
                    loopBlocks.ForEach(block =>
                    {
                        loopBlocksGroup = processBlock(variablesResult, block, "")
                                            .Replace("<@" + variablesResult.Name + "@>", "")
                                            .Replace("</@" + variablesResult.Name + "@>", "");
                        output = output.Replace(block, loopBlocksGroup);
                    });
                }

                if (!Path.IsPathRooted(outputDir))
                {
                    outputDir = Path.GetFullPath(Path.Combine(directory, outputDir));
                }

                if ((!replace && !File.Exists(Path.Combine(outputDir, outputName))) || replace)
                {

                    if (File.Exists(Path.Combine(outputDir, outputName)) && replace)
                    {
                        File.Delete(Path.Combine(outputDir, outputName));
                        result = "File replaced: " + Path.Combine(outputDir, outputName);

                    }
                    else
                    {
                        result = "File created: " + Path.Combine(outputDir, outputName);
                    }
                    outputFile = new StreamWriter(Path.Combine(outputDir, outputName), false);
                    outputFile.Write(output);
                    outputFile.Close();
                    outputFile.Dispose();
                }
                else
                {
                    result = "File already exists: " + Path.Combine(outputDir, outputName);
                }


            }
            catch (Exception e)
            {
                result = "Error processing file: " + e.Message;
            }
            return result;
        }

        private static string processVariable(VariablesResult variable, string value, string superVariable)
        {
            string result = value;

            try
            {
                if (variable.Values != null && variable.Values.Count > 0)
                {
                    if (superVariable.Equals(""))
                    {
                        result = result.Replace("<%" + variable.Name + "%>", variable.Values.First().Value);
                    }
                    else
                    {
                        result = result.Replace("<%" + superVariable + "." + variable.Name + "%>", variable.Values.First().Value);
                    }

                    if (variable.Values.First().Variables != null && variable.Values.First().Variables.Count > 0)
                    {
                        variable.Values.First().Variables.ForEach(variablesResult =>
                        {
                            if (superVariable.Equals(""))
                            {
                                result = processVariable(variablesResult, result, variable.Name);
                            }
                            else
                            {
                                result = processVariable(variablesResult, result, superVariable + "." + variable.Name);
                            }
                        });
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static string processBlock(VariablesResult variable, string block, string superVariable)
        {
            string result = "";

            try
            {
                if (variable.Values != null && variable.Values.Count > 0)
                {
                    variable.Values.ForEach(value =>
                    {
                        if (superVariable.Equals(""))
                        {
                            result = result + block.Replace("<!" + variable.Name + "!>", value.Value);
                        }
                        else
                        {
                            result = result + block.Replace("<!" + superVariable + "." + variable.Name + "!>", value.Value);
                        }

                        if (value.Variables != null && value.Variables.Count > 0)
                        {
                            value.Variables.ForEach(variablesResult =>
                            {
                                if (superVariable.Equals(""))
                                {
                                    result = processBlock(variablesResult, result, variable.Name);
                                }
                                else
                                {
                                    result = processBlock(variablesResult, result, superVariable + "." + variable.Name);
                                }
                            });
                        }

                    });
                }
                else
                {
                    if (superVariable.Equals(""))
                    {
                        result = result + block.Replace("<!" + variable.Name + "!>", "");                        
                    }
                    else
                    {
                        result = result + block.Replace("<!" + superVariable + "." + variable.Name + "!>", "");
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static List<string> CollectLoopBlocks(string output, string name)
        {
            List<string> result = new List<string>();
            MatchCollection objMatchs;
            try
            {
                objMatchs = Regex.Matches(output, "(<@" + name + "@>)(.*\\s ?)*(<\\/@" + name + "@>)", regexOptions);
                foreach (Match match in objMatchs)
                {
                    if (match.Success)
                    {
                        result.Add(match.Value);
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static List<VariablesResult> CollectVariables(List<FilePatternVariables> variables, string file)
        {
            List<VariablesResult> result = new List<VariablesResult>();
            MatchCollection objMatchs;
            List<string> auxValues;
            try
            {
                foreach (FilePatternVariables filePatternVariablese in variables)
                {
                    result.Add(new VariablesResult() { Name = filePatternVariablese.Name, Values = new List<VariablesResultItem>() });
                    objMatchs = Regex.Matches(file, filePatternVariablese.SearchPatern, regexOptions);
                    foreach (Match match in objMatchs)
                    {

                        if (match.Success && (filePatternVariablese.ExclusionPatern == null
                                              || filePatternVariablese.ExclusionPatern.Equals("")
                                              || Regex.Matches(match.Value, @filePatternVariablese.ExclusionPatern).Count == 0))
                        {
                            if (!match.Value.Equals(""))
                            {
                                auxValues = new List<string>();
                                if (filePatternVariablese.Script != null
                                    && !filePatternVariablese.Script.Equals("") &&
                                    PythonInterpreter.ValidatePython(pythonScript) == "")
                                {
                                    auxValues.AddRange(PythonInterpreter.ProcessString(pythonScript, filePatternVariablese.Script, match.Value));
                                }
                                else
                                {
                                    auxValues.Add(match.Value);
                                }
                                result.Last().Values.AddRange(auxValues.Select(value => new VariablesResultItem() { Value = value }));
                            }
                        }
                    }

                    if (filePatternVariablese.Variables != null &&
                        filePatternVariablese.Variables.Count > 0)
                    {
                        result.Last().Values.ForEach(value =>
                        {
                            value.Variables = new List<VariablesResult>();
                            value.Variables.AddRange(CollectVariables(filePatternVariablese.Variables, value.Value));
                        });
                    }

                }
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static List<string> getAllFiles(string p_directory, string p_filesFilter, string p_dirFilter, string p_filesExclude, string p_dirExclude)
        {
            try
            {
                List<string> result = new List<string>();
                List<string> directories = new List<string>();
                List<string> files = new List<string>();
                string[] auxDirectories;
                string[] auxFiles;

                auxDirectories = Directory.GetDirectories(p_directory);

                if (auxDirectories != null && auxDirectories.Length > 0)
                {
                    directories = auxDirectories.ToList();
                }

                if (p_dirFilter != null && !p_dirFilter.Equals(""))
                {
                    directories.RemoveAll(dir => !Regex.IsMatch(dir, p_dirFilter, regexOptions));
                }

                if (p_dirExclude != null && !p_dirExclude.Equals(""))
                {
                    directories.RemoveAll(dir => Regex.IsMatch(dir, p_dirExclude, regexOptions));
                }

                directories.ForEach(dir =>
                {
                    result.AddRange(getAllFiles(dir, p_filesFilter, p_dirFilter, p_filesExclude, p_dirExclude));
                });

                auxFiles = Directory.GetFiles(p_directory);

                if (auxFiles != null && auxFiles.Length > 0)
                {
                    files = auxFiles.ToList();
                }

                if (p_filesFilter != null && !p_filesFilter.Equals(""))
                {
                    files.RemoveAll(file => !Regex.IsMatch(Path.GetFileName(file), p_filesFilter));
                }

                if (p_filesExclude != null && !p_filesExclude.Equals(""))
                {
                    files.RemoveAll(file => Regex.IsMatch(Path.GetFileName(file), p_filesExclude));
                }

                result.AddRange(files);

                return result;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private static string LoadPythonScript(string p_name)
        {
            string result = "";
            StreamReader file;

            try
            {
                if (File.Exists(Path.Combine(workDirectory, p_name + ".py")))
                {
                    file = new StreamReader(Path.Combine(workDirectory, p_name + ".py"));
                    result = file.ReadToEnd();
                    file.Close();
                    file.Dispose();
                }
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}
