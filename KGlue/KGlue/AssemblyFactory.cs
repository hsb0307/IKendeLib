﻿using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Collections;
using System.IO;

namespace KGlue
{
    /// <summary>
    /// Generates an Assembly from a script filename
    /// </summary>
    class AssemblyFactory
    {
        private CompilerErrorCollection compilerErrors = null;

        /// <summary>
        /// Generates an Assembly from a script filename
        /// </summary>
        /// <param name="filename">The filename of the script</param>
        /// <returns>The generated assembly</returns>
        public Assembly CreateAssembly(string filename)
        {
            return CreateAssembly(filename, new ArrayList());
        }

        /// <summary>
        /// Generates an Assembly from a script filename
        /// </summary>
        /// <param name="filename">The filename of the script</param>
        /// <param name="references">Assembly references for the script</param>
        /// <returns>The generated assembly</returns>
        public Assembly CreateAssembly(string filename, IList references)
        {
            // ensure that compilerErrors is null
            compilerErrors = null;

            string extension = Path.GetExtension(filename);

            // Select the correct CodeDomProvider based on script file extension
            CodeDomProvider codeProvider = null;
            switch (extension)
            {
                case ".cs":
                    codeProvider = new Microsoft.CSharp.CSharpCodeProvider();
                    break;
                case ".vb":
                    codeProvider = new Microsoft.CSharp.CSharpCodeProvider();
                    break;
                default:
                    throw new InvalidOperationException("Script files must have a .cs or .vb.");
            }



            // Set compiler parameters
            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.CompilerOptions = "/target:library /optimize";
            compilerParams.GenerateExecutable = false;
            compilerParams.GenerateInMemory = true;
            compilerParams.IncludeDebugInformation = false;

            compilerParams.ReferencedAssemblies.Add("mscorlib.dll");
            compilerParams.ReferencedAssemblies.Add("System.dll");

            // Add custom references
            foreach (string reference in references)
            {
                if (!compilerParams.ReferencedAssemblies.Contains(reference))
                {
                    compilerParams.ReferencedAssemblies.Add(reference);
                }
            }

            // Do the compilation
            CompilerResults results = codeProvider.CompileAssemblyFromFile(compilerParams,
                filename);

            //Do we have any compiler errors
            if (results.Errors.Count > 0)
            {
                compilerErrors = results.Errors;
                throw new Exception(
                    "Compiler error(s) encountered and saved to AssemblyFactory.CompilerErrors");
            }

            Assembly createdAssembly = results.CompiledAssembly;
            return createdAssembly;
        }

        /// <summary>
        /// Generates an Assembly from a list of script filenames
        /// </summary>
        /// <param name="filenames">The filenames of the scripts</param>
        /// <returns>The generated assembly</returns>
        public Assembly CreateAssembly(IList filenames)
        {
            return CreateAssembly(filenames, new ArrayList());
        }

        /// <summary>
        /// Generates an Assembly from a list of script filenames
        /// </summary>
        /// <param name="filenames">The filenames of the scripts</param>
        /// <param name="references">Assembly references for the script</param>
        /// <returns>The generated assembly</returns>
        public Assembly CreateAssembly(IList filenames, IList references)
        {
            string fileType = null;
            foreach (string filename in filenames)
            {
                string extension = Path.GetExtension(filename);
                if (fileType == null)
                {
                    fileType = extension;
                }
                else if (fileType != extension)
                {
                    throw new ArgumentException("All files in the file list must be of the same type.");
                }
            }

            // ensure that compilerErrors is null
            compilerErrors = null;

            // Select the correct CodeDomProvider based on script file extension
            CodeDomProvider codeProvider = null;
            switch (fileType)
            {
                case ".cs":
                    codeProvider = new Microsoft.CSharp.CSharpCodeProvider();
                    break;
                case ".vb":
                    codeProvider = new Microsoft.CSharp.CSharpCodeProvider();
                    break;
                default:
                    throw new InvalidOperationException("Script files must have a .cs, .vb, or .js extension, for C#, Visual Basic.NET, or JScript respectively.");
            }


            // Set compiler parameters
            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.CompilerOptions = "/target:library /optimize";
            compilerParams.GenerateExecutable = false;
            compilerParams.GenerateInMemory = true;
            compilerParams.IncludeDebugInformation = false;
            compilerParams.ReferencedAssemblies.Add("mscorlib.dll");
            compilerParams.ReferencedAssemblies.Add("System.dll");

            // Add custom references
            foreach (string reference in references)
            {
                if (!compilerParams.ReferencedAssemblies.Contains(reference))
                {
                    compilerParams.ReferencedAssemblies.Add(reference);
                }
            }

            // Do the compilation
            CompilerResults results = codeProvider.CompileAssemblyFromFile(
                compilerParams, (string[])ArrayList.Adapter(filenames).ToArray(typeof(string)));


            if (results.Errors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (CompilerError item in results.Errors)
                {
                    sb.AppendFormat("{0} line:{1}   {2}\r\n", item.FileName, item.Line, item.ErrorText);
                }
                compilerErrors = results.Errors;
                throw new Exception(
                    "Compiler error(s)\r\n" + sb.ToString());
            }

            Assembly createdAssembly = results.CompiledAssembly;
            return createdAssembly;
        }

        /// <summary>
        /// The compiler errors for the last generated assembly.  Null if no compile errors.
        /// </summary>
        public CompilerErrorCollection CompilerErrors
        {
            get
            {
                return compilerErrors;
            }
        }
    }

}
