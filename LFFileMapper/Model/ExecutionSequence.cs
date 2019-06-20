using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LFFileMapper.Model
{
    class ExecutionSequence
    {
        public string Name { get; set; }
        public List<ExecutionItem> Itens { get; set; }

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
        public static ExecutionSequence load(string p_path)
        {
            ExecutionSequence result;
            StreamReader file;
            string auxFile;
            try
            {
                file = new StreamReader(p_path);
                auxFile = file.ReadToEnd();
                result = JsonConvert.DeserializeObject<ExecutionSequence>(auxFile);
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

    class ExecutionItem
    {
        public int Sequence { get; set; }
        public string Configuration { get; set; }
        public string RelativePath { get; set; }
    }
}
