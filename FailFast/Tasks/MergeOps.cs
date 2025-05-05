using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace FailFast.Tasks
{
    public class MergeOps : Task
    {
        [Required]
        public string Header { get; set; }
        
        [Required]
        public ITaskItem[] FileList { get; set; }
        
        [Required]
        public string Footer { get; set; }
        
        [Required]
        public string OutFile { get; set; }
        
        [Required]
        public string Version { get; set; }

        private string License => $@"
// ============================================================================
// Author: Joshua Howard
// Project: FailFast
// Version: {Version}
// License: MIT License
// Source: https://github.com/JD-Howard/FailFast
// 
// Description: A NetStandard developer library for wrapping defensive code
//              into something that can either be inspected, logged or ignored
//              depending on your need/environment. Effort went into avoiding
//              development side effects being pushed to production. In most
//              cases, the FailFast invocations should be okay in production.
//
// TIPS:
//   1. You must have your CSPROJ configured for LangVersion 8 or higher:
//      <LangVersion>8</LangVersion>
//   2. Async is fine, but avoid FailFast calls in overly parallel tasks.
//   3. Highly recommend changing the namespace to match your project root.
//   4. If your project references another project using FailFast and you are
//      not in a solution containing its code, then you should reference a
//      production build DLL that has FailFast turned off.
//   5. FailFast could be functional as a separate compiled dependency, but it
//      is absolutely critical the consumer provides the FFTryCatch delegate.
// 
// Copyright (c) {DateTime.UtcNow.Year} Joshua Howard
// ============================================================================
".Trim();

        public override bool Execute()
        {
            var dir = Path.GetDirectoryName(OutFile) ?? string.Empty;
            if (Directory.Exists(dir) == false)
                Directory.CreateDirectory(dir);
            
            if (File.Exists(OutFile))
                File.Delete(OutFile);


            List<string> result = new List<string>(1000) {License, ""};
            result.AddRange(File.ReadAllLines(Header).Select(x => x.Replace("global using", "using")).ToList());
            result.Add("");
            result.Add("namespace System.Diagnostics");
            result.Add("{");
            result.Add("    internal static class FailFast");
            result.Add("    {");
            foreach (var file in FileList)
            {
                result.Add("");
                result.Add($"        #region {Path.GetFileName(file.ItemSpec)}");
                result.Add("");
                var lines = System.IO.File.ReadAllLines(file.ItemSpec);
                var start = FindStartIndex(lines) + 1;
                var stop = FindStopIndex(lines) + 1;
                result.AddRange(lines.Take(stop).Skip(start));
                result.Add("");
                result.Add($"        #endregion // {Path.GetFileName(file.ItemSpec)}");
                result.Add("");
            } 
            
            result.Add("    }");
            result.Add("}");
               
            result.Add("");
            result.Add($"#region {Path.GetFileName(Footer)}");
            result.Add("");
            result.AddRange(File.ReadAllLines(Footer));
            result.Add("");
            result.Add($"#endregion // {Path.GetFileName(Footer)}");
            result.Add("");
                
            File.WriteAllLines(OutFile, result);
            return true;
        }


        private int FindStartIndex(string[] lines)
        {
            var found = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (found == 0 && lines[i].Contains("partial class"))
                    found++;

                if (found > 0 && lines[i].Trim() == "{")
                    return i;
            }
            return 0;
        }
        
        private int FindStopIndex(string[] lines)
        {
            int found = 0;
            for (int i = lines.Length - 1; i >= 0 ; i--)
            {
                if (lines[i].Trim() == "}")
                    found++;

                if (found >= 2)
                    return i - 1;
            }
            
            return lines.Length - 1;
        }
        
    }
}