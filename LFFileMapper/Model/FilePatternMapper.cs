using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace LFFileMapper.Model
{
    public class FilePatternMapper
    {
        public string Name { get; set; }
        public List<FilePatternVariables> Variables { get; set; }
        public string OutputFolder { get; set; }
        public string OutputFileName { get; set; }
        public string DirFilter { get; set; }
        public string FileFilter { get; set; }
        public string DirExclude { get; set; }
        public string FileExclude { get; set; }
        public string RelativePath { get; set; }
        public void save(string p_path)
        {
            StreamWriter file;
            try
            {
                file = new StreamWriter(p_path);
                file.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
                file.Close();
                file.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public static FilePatternMapper load(string p_path)
        {
            FilePatternMapper result;
            StreamReader file;
            string auxFile;
            try
            {
                file = new StreamReader(p_path);
                auxFile = file.ReadToEnd();
                result = JsonConvert.DeserializeObject<FilePatternMapper>(auxFile);
                file.Close();
                file.Dispose();
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public class FilePatternVariables
    {
        public string Name { get; set; }

        public string SearchPatern { get; set; }

        public string ExclusionPatern { get; set; }

        public string Script { get; set; }

        public List<FilePatternVariables> Variables { get; set; }

        public ESearchLocation SearchLocation { get; set; }

    }

    public class VariablesResult
    {
        public string Name { get; set; }

        public List<VariablesResultItem> Values { get; set; }

    }

    public class VariablesResultItem
    {
        public string Value { get; set; }

        public List<VariablesResult> Variables { get; set; }

    }

    public enum ESearchLocation
    {
        Content,
        FileName,
        FileDirectory,
        FilePath,
        Literal
    }


}
