using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Project_MASM__
{
    class Program
    {
        static string Str { get; set; }
        static int linkCounter = 0;
        static int varStringCounter = 0;
        static string GetComparer(string comparer) => comparer switch
        {
            "==" => "je",
            "!=" => "jne",
            "<" => "jl",
            ">" => "jg",
            "<=" => "jle",
            ">=" => "jge",
        };

        static string GetInverseComparer(string comparer) => comparer switch
        {
            "==" => "jne",
            "!=" => "je",
            "<" => "jge",
            ">" => "jle",
            "<=" => "jg",
            ">=" => "jl",
        };

        static string ParseBody(string body, string space)
        {
            return string
                .Join('\n', body
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x));
        }

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("[source file name] [result file name]");
                return;
            }

            DirectoryInfo dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            string path = dir.FullName + "\\" + args[0];
            Console.WriteLine(path);
            Str = File.ReadAllText(path);

            var parsers = new List<Parser>();

            parsers.Add(new Parser(new Regex(@"(?<Space>^[^\w;\n]+)if\s*\(\s*(?<Arg1>[^=!><]+?)\s*(?<Comparer>==|!=|<=|>=|<|>)\s*(?<Arg2>[^\(\)]+?)\s*\)\s*\{\s*(?<Body>[^\{\}]*?)\s*\}(?:\s*else\s*\{\s*(?<Body2>[^\{\}]*?)\s*\})?", RegexOptions.Multiline), IfElseParser));
            parsers.Add(new Parser(new Regex(@"(?<Space>^[^\w;\n]+)do\s*\{\s*(?<Body>[^\{\}]*?)\s*\}\s*while\s*\(\s*(?<Arg1>[^=!><]+?)\s*(?<Comparer>(==|!=|<|>|<=|>=))\s*(?<Arg2>[^\(\)]+?)\s*\)(?:;)?", RegexOptions.Multiline), DoWhileParser));
            parsers.Add(new Parser(new Regex(@"(?<Space>^[^\w;\n]+)for\s*\(\s*(?<Field1>[^;]*?)\s*;\s*(?<Arg1>[^=!><]+?)\s*(?<Comparer>(==|!=|<|>|<=|>=))\s*(?<Arg2>[^\(\)]+?)\s*;\s*(?<Field2>[^\)]*?)\s*\)\s*\{\s*(?<Body>[^\{\}]*?)\s*\}", RegexOptions.Multiline), ForParser));
            parsers.Add(new Parser(new Regex(@"(?<Space>^[^\w;\n]+)(?<Unsafe>u)?print\s*\(\s*(?<Var1>[^\)]+?)\s*\)", RegexOptions.Multiline), PrintParser));   
            
            bool isFullyParsed = false;
            while (!isFullyParsed)
            {
                isFullyParsed = true;
                foreach (var parser in parsers)
                {
                    while (parser.Reg.IsMatch(Str))
                    {
                        Str = parser.Reg.Replace(Str, parser.Method);
                        isFullyParsed = false;
                    }
                }
            }
            
            
            Console.WriteLine(Str);
            File.WriteAllText(args[1], Str);
        }

        static string IfElseParser(Match match)
        {
            string space = match.Groups["Space"].Value;
            string body1 = ParseBody(match.Groups["Body"].Value, space);
            string body2 = ParseBody(match.Groups["Body2"].Value, space);
            string comment1 = $"\t;if ({match.Groups["Arg1"].Value} {match.Groups["Comparer"].Value} {match.Groups["Arg2"].Value})";
            string comment2 = $"\t;else";

            linkCounter++;
            if (body2 == null || body2 == "" || body2.Trim() == "")
            {
                return
                    $"{space}cmp {match.Groups["Arg1"].Value}, {match.Groups["Arg2"].Value}\n" +
                    $"{space}{GetInverseComparer(match.Groups["Comparer"].Value)} __end{linkCounter}{comment1}\n" +
                    $"{space}    {body1}\n" +
                    $"{space}__end{linkCounter}:\n";
            }
            return
                $"{space}cmp {match.Groups["Arg1"].Value}, {match.Groups["Arg2"].Value}\n" +
                $"{space}{GetInverseComparer(match.Groups["Comparer"].Value)} __else{linkCounter}{comment1}\n" +
                $"{space}    {body1}\n" +
                $"{space}jmp __end{linkCounter}\n" +
                $"{space}__else{linkCounter}:{comment2}\n" +
                $"{space}    {body2}\n" +
                $"{space}__end{linkCounter}:\n";
        }

        static string DoWhileParser(Match match)
        {
            string space = match.Groups["Space"].Value;
            string body1 = ParseBody(match.Groups["Body"].Value, space);
            string comment1 = $"\t\t;do";
            string comment2 = $"\t;while ({match.Groups["Arg1"].Value} {match.Groups["Comparer"].Value} {match.Groups["Arg2"].Value})";
            linkCounter++;
            return
                $"{space}__while{linkCounter}:{comment1}\n" +
                $"{space}    {body1}\n" +
                $"{space}cmp {match.Groups["Arg1"].Value}, {match.Groups["Arg2"].Value}\n" +
                $"{space}{GetComparer(match.Groups["Comparer"].Value)} __while{linkCounter}{comment2}\n";
        }

        static string ForParser(Match match)
        {
            string space = match.Groups["Space"].Value;
            string body1 = ParseBody(match.Groups["Body"].Value, space);
            string comment1 = $"\t;for ({match.Groups["Field1"].Value}; {match.Groups["Arg1"].Value} {match.Groups["Comparer"].Value} {match.Groups["Arg2"].Value}; {match.Groups["Field2"].Value})";
            linkCounter++;
            return
                $"{space}{match.Groups["Field1"].Value}\n" +
                $"{space}jmp __enter{linkCounter}\n" +
                $"{space}__for{linkCounter}:{comment1}\n" +
                $"{space}    {body1}\n" +
                $"{space}    {match.Groups["Field2"].Value}\n" +
                $"{space}__enter{linkCounter}:\n" +
                $"{space}cmp {match.Groups["Arg1"].Value}, {match.Groups["Arg2"].Value}\n" +
                $"{space}{GetComparer(match.Groups["Comparer"].Value)} __for{linkCounter}\n";
        }

        static string PrintParser(Match match)
        {
            string space = match.Groups["Space"].Value;
            char endChar = match.Groups["Var1"].Value[match.Groups["Var1"].Value.Length - 1];
            string push = match.Groups["Unsafe"].Value == "u" ? "" : $"{space}push ax\n{space}push dx\n";
            string pop = match.Groups["Unsafe"].Value == "u" ? "" : $"{space}pop dx\n{space}pop ax\n";

            if (match.Groups["Var1"].Value.Length == 2 && (endChar == 'l' || endChar == 'h'))
            {
                return
                    push +
                    $"{space}mov al, {match.Groups["Var1"].Value}\n" +
                    $"{space}mov ah, 02h\n" +
                    $"{space}mov dl, al\n" +
                    $"{space}int 21h;\t{match.Value}\n" +
                    pop;
            }
            return
                push +
                $"{space}mov ah, 09h\n" +
                $"{space}mov dx, offset {match.Groups["Var1"].Value}\n" +
                $"{space}int 21h;\t{match.Value}\n" +
                pop;
        }
    }
}
