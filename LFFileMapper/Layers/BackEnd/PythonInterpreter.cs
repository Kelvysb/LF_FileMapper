using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Utils;

namespace LFFileMapper.Layers.BackEnd
{
    class PythonInterpreter
    {
        #region Declarations
        private static ScriptEngine objPyEng = null;
        #endregion

        #region Constructor
        private PythonInterpreter()
        {
        }
        #endregion

        #region Functions
        public static List<string> ProcessString(string p_PythonCommand, string p_function, string p_Value)
        {

            List<string> strReturn;
            ScriptSource objSource;
            ScriptScope objScope;
            Func<string, IList<object>> objExecute;

            try
            {

                strReturn = new List<string>();

                if (objPyEng is null)
                {
                    objPyEng = Python.CreateEngine();
                }

                objSource = objPyEng.CreateScriptSourceFromString(p_PythonCommand, SourceCodeKind.Statements);
                objScope = objPyEng.CreateScope();

                objSource.Execute(objScope);

                objExecute = objScope.GetVariable<Func<string, IList<object>>>(p_function);

                strReturn = objExecute(p_Value).Cast<string>().ToList();

                strReturn.RemoveAll(s => s.Equals(""));

                return strReturn;

            }
            catch (Exception)
            {
                throw;
            }

        }

        public static string ValidatePython(string p_PythonCommand)
        {

            string strReturn;
            ScriptSource objSource;
            ScriptScope objScope;

            try
            {

                strReturn = "";

                if (objPyEng is null)
                {
                    objPyEng = Python.CreateEngine();
                }

                objSource = objPyEng.CreateScriptSourceFromString(p_PythonCommand, SourceCodeKind.Statements);
                objScope = objPyEng.CreateScope();

                objSource.Execute(objScope);


            }
            catch (Exception ex)
            {
                strReturn = ex.Message;
            }

            return strReturn;

        }
        #endregion
    }
}
