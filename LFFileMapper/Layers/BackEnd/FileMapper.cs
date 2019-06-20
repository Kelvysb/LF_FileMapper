using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using LFFileMapper.Layers.BackEnd;
using LFFileMapper.Model;

namespace LaForgeFileMapper.Layers.BackEnd
{
    public class FileMapper
    {

        private static RegexOptions regexOptions = RegexOptions.Singleline;
        public static string currentDirectory { get; set; }
        public static string sequenceDirectory { get; set; }
        public static string workDirectory { get; set; }
        private static string directory = "";
        private static string pythonScript = "";

        protected FileMapper()
        {

        }

        public static List<string> MapFiles(string p_mapperFile, string p_directory, bool p_replace)
        {
            List<string> result = new List<string>();
            List<string> files;
            FilePatternMapper pattern;
            StreamReader inputFile;

            try
            {

                directory = p_directory;

                if (!Path.IsPathRooted(directory))
                {
                    directory = Path.GetFullPath(Path.Combine(currentDirectory, directory));
                }

                if (!File.Exists(Path.Combine(workDirectory, p_mapperFile + ".json")) ||
                    !File.Exists(Path.Combine(workDirectory, p_mapperFile + ".txt")))
                {
                    throw new FileNotFoundException("Configuration not found: " + p_mapperFile);
                }

                pattern = FilePatternMapper.load(Path.Combine(workDirectory, p_mapperFile + ".json"));

                if (!string.IsNullOrEmpty(pattern.RelativePath) && !Path.IsPathRooted(pattern.RelativePath))
                {
                    directory = Path.GetFullPath(Path.Combine(directory, pattern.RelativePath));
                }

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

        internal static List<string> MapSequence(string p_sequenceFile, string p_directory, bool p_replace)
        {

            List<string> result = new List<string>();
            string auxPath;
            ExecutionSequence sequence;

            if (!File.Exists(Path.Combine(sequenceDirectory, p_sequenceFile + ".json")))
            {
                throw new FileNotFoundException("Sequence file not found: " + p_sequenceFile);
            }

            sequence = ExecutionSequence.load(Path.Combine(sequenceDirectory, p_sequenceFile + ".json"));
            sequence.Itens.Sort((i1, i2) => i1.Sequence.CompareTo(i2.Sequence));

            sequence.Itens.ForEach((item) =>
            {
                if (!string.IsNullOrEmpty(item.RelativePath) && !Path.IsPathRooted(item.RelativePath))
                {
                    auxPath = Path.GetFullPath(Path.Combine(p_directory, item.RelativePath));
                }
                else
                {
                    auxPath = p_directory;
                }
                result.AddRange(MapFiles(item.Configuration, auxPath, p_replace));
            });

            return result;
        }

        private static string ProcessFile(FilePatternMapper pattern, string file, string filePath, bool replace)
        {
            string result = "";
            StreamWriter outputFile;
            List<VariablesResult> variables;
            string output = "";
            List<string> loopBlocks;
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
                    if (string.IsNullOrEmpty(superVariable))
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
                            if (string.IsNullOrEmpty(superVariable))
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
                        if (string.IsNullOrEmpty(superVariable))
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
                                if (string.IsNullOrEmpty(superVariable))
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
                    if (string.IsNullOrEmpty(superVariable))
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
            MatchCollection objClosingMatchs;
            string auxOutput;

            try
            {
                objMatchs = Regex.Matches(output, "(<@" + name + "@>)", regexOptions);
                foreach (Match match in objMatchs)
                {

                    if (match.Success)
                    {

                        auxOutput = output.Substring(match.Index);

                        //Find first closing Tag
                        objClosingMatchs = Regex.Matches(auxOutput, "(</@" + name + "@>)", regexOptions);

                        if (objClosingMatchs.Count > 0 && objClosingMatchs.First().Success)
                        {
                            result.Add(auxOutput.Substring(0, objClosingMatchs.First().Index + ("(</@" + name + "@>)").Length));
                        }
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

                        if (match.Success
                            && (filePatternVariablese.ExclusionPatern == null
                            || filePatternVariablese.ExclusionPatern.Equals("")
                            || Regex.Matches(match.Value, @filePatternVariablese.ExclusionPatern).Count == 0)
                            && !string.IsNullOrEmpty(match.Value))
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

                if (p_dirFilter != null && !string.IsNullOrEmpty(p_dirFilter))
                {
                    directories.RemoveAll(dir => !Regex.IsMatch(dir, p_dirFilter, regexOptions));
                }

                if (p_dirExclude != null && !string.IsNullOrEmpty(p_dirExclude))
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

                if (p_filesFilter != null && !string.IsNullOrEmpty(p_filesFilter))
                {
                    files.RemoveAll(file => !Regex.IsMatch(Path.GetFileName(file), p_filesFilter));
                }

                if (p_filesExclude != null && !string.IsNullOrEmpty(p_filesExclude))
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
