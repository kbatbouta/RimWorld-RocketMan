using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public static class Logger
    {
        private static string logsFolder;

        private readonly static StringBuilder StringBuilder = new StringBuilder();

        public static void Initialize()
        {
            Logger.logsFolder = RocketEnvironmentInfo.LogsFolderPath;
            if (!Directory.Exists(logsFolder))
                Directory.CreateDirectory(logsFolder);
            foreach (string filePath in Directory.GetFiles(logsFolder))
                File.Delete(filePath);
        }

        public static void Debug(string message, Exception exception = null, string file = null)
        {
            if (file == null)
            {
                file = "Rocket.log";
            }
            StringBuilder.Clear();
            StringBuilder.Append('\n');
            StringBuilder.Append(message);
            if (exception != null)
            {
                StringBuilder.AppendInNewLine($"<#-ERROR-#>");
                StringBuilder.AppendInNewLine(exception.GetType().FullName);
                StringBuilder.AppendInNewLine(exception.Message);
                StringBuilder.Append('\n');
                AddStackTrace(StringBuilder, new StackTrace(exception));
            }
            Flush(StringBuilder, file);
        }

        private static void Flush(StringBuilder stringBuilder, string file)
        {
            string fullPath = Path.Combine(logsFolder, file);
            stringBuilder.Append('\n');
            if (!File.Exists(fullPath))
            {
                File.WriteAllText(fullPath, "[LOGGER STARTED]\n");
            }
            File.AppendAllText(fullPath, stringBuilder.ToString());
            stringBuilder.Clear();
        }

        private static void AddStackTrace(StringBuilder stringBuilder, StackTrace trace)
        {
            StackFrame[] frames = trace.GetFrames();
            for (int i = 0; i < frames.Length; i += 2)
            {
                stringBuilder.Append("\n");
                for (int j = i; j < frames.Length && j < i + 2; j++)
                {
                    StackFrame frame = frames[j];
                    MethodBase method = frame.GetMethod();
                    string part = string.Format("{0}.{1}({2})", method.ReflectedType.FullName, method.Name, string.Join(",", method.GetParameters().Select(o => string.Format("{0} {1}", o.ParameterType, o.Name)).ToArray()));
                    stringBuilder.Append($"\tAt {part}[{frame.GetFileLineNumber()}]");
                }
            }
            stringBuilder.Append("\tStackTrace ended\n");
        }
    }
}
