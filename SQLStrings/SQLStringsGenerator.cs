using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using System.Globalization;
using Microsoft.CodeAnalysis.Text;
using System.Linq;

namespace SQLStrings
{
    [Generator]
    public class SQLStringsGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {        
            var sqlFiles = context.AdditionalFiles.Where(at => at.Path.EndsWith(".sql")); 

            //Process opt-in files only
            foreach (var file in sqlFiles)
            {
                bool process = false;

                if (context.AnalyzerConfigOptions.GetOptions(file)
                    .TryGetValue("build_metadata.AdditionalFiles.SQLStrings_Enable", out string conf))
                {
                    process = conf.Equals("true", StringComparison.OrdinalIgnoreCase);
                }

                if (process)
                {
                    ProcessEntry(file, context);
                }           
            }
        }

        public void Initialize(GeneratorInitializationContext context) { }

        /// <summary>
        /// Process AdditionalFiles entries from .csproj ItemGroup elements and add code to context
        /// </summary>
        /// <param name="file">AdditionalFiles entry</param>
        /// <param name="context">GeneratorExecutionContext</param>
        public static void ProcessEntry(AdditionalText file, GeneratorExecutionContext context)
        {
            TextInfo textInfo = new CultureInfo("en-US",false).TextInfo;
            var className = textInfo.ToTitleCase(Path.GetFileNameWithoutExtension(file.Path));

            var cb = new StringBuilder()
                .AppendLine("namespace SQLStrings")
                .AppendLine("{")
                .AppendLine($"\tpublic static class {className}")
                .AppendLine("\t{");

            using (StreamReader reader = new StreamReader(file.Path))
            {
                foreach (QueryInfo q in ParseText(reader))
                {
                    cb.AppendLine("\t\t///<summary>");

                    foreach (var c in q.Comments)
                        cb.AppendLine($"\t\t///{c}");

                    cb.AppendLine("\t\t///</summary>")
                      .Append("\t\tpublic static readonly string ")
                      .Append(q.Name)
                      .Append(" = @\"")
                      .Append(q.SQL.Replace("\"", "\"\""))
                      .AppendLine("\";")
                      .AppendLine("");
                }
            }

            cb.AppendLine("\t}")
              .AppendLine("}");

            context.AddSource($"SQLStrings_{className}.cs", SourceText.From(cb.ToString(), Encoding.UTF8));
        }

        /// <summary>
        /// Parse SQL text for individual queries.
        /// </summary>
        /// <param name="reader">TextReader instance</param>
        /// <returns>Collection of QueryInfo objects</returns>
        public static IEnumerable<QueryInfo> ParseText(TextReader reader)
        {
            var result = new List<QueryInfo>();

            QueryInfo current = new QueryInfo();
            StringBuilder sqlBuilder = new StringBuilder();
            List<string> comments = new List<string>();
            bool commentRead = true;

            while (true)
            {
                var line = reader.ReadLine();

                if (line == null)
                {
                    current.Comments = comments.ToArray();
                    current.SQL = sqlBuilder.ToString().Trim();
                    result.Add(current);
                    break;
                }
                else if (line.StartsWith("-- Name:"))
                {
                    if (!String.IsNullOrWhiteSpace(current.Name))
                    {
                        // Add entry
                        current.Comments = comments.ToArray();
                        current.SQL = sqlBuilder.ToString().Trim();
                        result.Add(current);

                        // Reset data for new query
                        current = new QueryInfo();
                        comments.Clear();
                        sqlBuilder.Clear();
                        commentRead = true;
                    }

                    current.Name = line.Substring(9).Trim();
                } 
                else if (line.StartsWith("--") && commentRead)
                {
                    if (line.Length > 2)
                        comments.Add(line.Substring(2).Trim());
                }
                else
                {
                    commentRead = false;

                    if (!String.IsNullOrWhiteSpace(line))
                        sqlBuilder.AppendLine(line.TrimEnd());
                }
            }

            return result;
        }
    }
}