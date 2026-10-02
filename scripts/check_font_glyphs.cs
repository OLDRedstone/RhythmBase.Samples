#:package SkiaSharp@3.119.2
#:property Nullable=enable

// 检查目标文件夹下的字体文件是否包含“所需文字”的字形。
// .NET 10 单文件脚本，运行方式：
//   dotnet run Scripts/check-font-glyphs.cs -- <文件夹> [--file <文本文件> ...] [--text "文字"] [--ext ttf,otf,ttc] [--all] [--top N]
//
// 退出码：0 = 至少有一个字体/字面满足全部字形；1 = 没有；2 = 参数错误。

using System.Text;
using SkiaSharp;

try { Console.OutputEncoding = Encoding.UTF8; } catch { /* 输出被重定向时忽略 */ }

// ---------- 参数解析 ----------
string folder = ".";
string? inlineText = null;
List<string> textFiles = [];
HashSet<string> exts = new(StringComparer.OrdinalIgnoreCase) { ".ttf", ".otf", ".ttc" };
int top = 30;
bool listAll = false;

for (int i = 0; i < args.Length; i++)
{
    string a = args[i];
    switch (a)
    {
        case "--text":
            inlineText = (inlineText ?? "") + (i + 1 < args.Length ? args[++i] : "");
            break;
        case "--file":
            if (i + 1 < args.Length) textFiles.Add(args[++i]);
            break;
        case "--ext":
            if (i + 1 < args.Length)
            {
                exts = new(StringComparer.OrdinalIgnoreCase);
                foreach (string e in args[++i].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    exts.Add(e.StartsWith('.') ? e : "." + e);
                }
            }
            break;
        case "--top":
            if (i + 1 < args.Length && int.TryParse(args[++i], out int t)) top = Math.Max(0, t);
            break;
        case "--all":
            listAll = true;
            break;
        case "-h":
        case "--help":
            PrintUsage();
            return 0;
        default:
            if (!a.StartsWith('-'))
            {
                folder = a;
            }
            else
            {
                Console.Error.WriteLine($"未知参数: {a}");
                PrintUsage();
                return 2;
            }
            break;
    }
}

// ---------- 收集所需字符（按码点去重） ----------
var required = new SortedSet<int>();
void AddText(string s)
{
    foreach (var rune in s.EnumerateRunes())
    {
        int cp = rune.Value;
        if (cp == 0xFEFF || Rune.IsWhiteSpace(rune) || Rune.IsControl(rune))
        {
            continue;
        }

        required.Add(cp);
    }
}

if (inlineText is not null)
{
    AddText(inlineText);
}

foreach (string f in textFiles)
{
    if (!File.Exists(f))
    {
        Console.Error.WriteLine($"找不到文本文件: {f}");
        return 2;
    }

    AddText(File.ReadAllText(f, Encoding.UTF8));
}

if (required.Count == 0)
{
    Console.Error.WriteLine("未提供所需文字：用 --text \"...\" 或 --file <文件> 指定。");
    PrintUsage();
    return 2;
}

if (!Directory.Exists(folder))
{
    Console.Error.WriteLine($"文件夹不存在: {folder}");
    return 2;
}

// ---------- 扫描字体 ----------
List<string> files = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
    .Where(p => exts.Contains(Path.GetExtension(p)))
    .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
    .ToList();

var results = new List<(string Path, int Face, string Family, List<int> Missing)>();
foreach (string path in files)
{
    bool isCollection = path.EndsWith(".ttc", StringComparison.OrdinalIgnoreCase);
    for (int face = 0; ; face++)
    {
        SKTypeface? typeface;
        try
        {
            typeface = SKTypeface.FromFile(path, face);
        }
        catch
        {
            typeface = null;
        }

        if (typeface is null)
        {
            break;
        }

        using (typeface)
        {
            List<int> missing = required.Where(cp => !typeface.ContainsGlyph(cp)).ToList();
            results.Add((path, face, typeface.FamilyName ?? "", missing));
        }

        if (!isCollection)
        {
            break; // 非 ttc 只取第一个字面
        }
    }
}

// ---------- 汇总输出 ----------
List<(string Path, int Face, string Family, List<int> Missing)> full = results.Where(r => r.Missing.Count == 0).ToList();
List<(string Path, int Face, string Family, List<int> Missing)> partial = results
    .Where(r => r.Missing.Count > 0)
    .OrderBy(r => r.Missing.Count)
    .ThenBy(r => r.Path, StringComparer.OrdinalIgnoreCase)
    .ToList();

string FaceTag((string Path, int Face, string Family, List<int> Missing) r) => r.Face > 0 ? $" (face {r.Face})" : "";

Console.WriteLine($"目标文件夹 : {Path.GetFullPath(folder)}");
Console.WriteLine($"所需字符   : {required.Count} 个（去重后）");
Console.WriteLine($"扫描到的字体: {results.Count} 个字面 / {files.Count} 个文件");
Console.WriteLine();

Console.WriteLine($"== 满足全部字形（{full.Count}） ==");
if (full.Count == 0)
{
    Console.WriteLine("  （无）");
}
else
{
    foreach (var r in full)
    {
        Console.WriteLine($"  [OK] {r.Path}{FaceTag(r)}  ·  {r.Family}");
    }
}

Console.WriteLine();
Console.WriteLine("== 部分满足（按缺少数量升序） ==");
int shown = listAll ? partial.Count : Math.Min(top, partial.Count);
foreach (var r in partial.Take(shown))
{
    string miss = string.Concat(r.Missing.Take(24).Select(char.ConvertFromUtf32));
    string more = r.Missing.Count > 24 ? $"…(+{r.Missing.Count - 24})" : "";
    Console.WriteLine($"  [{required.Count - r.Missing.Count}/{required.Count}] {r.Path}{FaceTag(r)}  ·  {r.Family}  缺 {r.Missing.Count}: {miss}{more}");
}

if (!listAll && partial.Count > shown)
{
    Console.WriteLine($"  …其余 {partial.Count - shown} 个（用 --all 查看）");
}

Console.WriteLine();
Console.WriteLine(full.Count > 0
    ? $"结论: 有 {full.Count} 个字体/字面满足全部所需字形。"
    : "结论: 没有字体满足全部所需字形。");
return full.Count > 0 ? 0 : 1;

static void PrintUsage()
{
    Console.WriteLine("""
用法:
  dotnet run Scripts/check-font-glyphs.cs -- <文件夹> [选项]

选项:
  --text "文字"        直接给出所需文字
  --file <路径>        从文本文件读取所需文字（可重复）
  --ext ttf,otf,ttc    要扫描的扩展名（默认 ttf,otf,ttc）
  --top N              部分满足列表最多显示 N 个（默认 30）
  --all                部分满足列表全部显示
  -h, --help           显示帮助

示例:
  dotnet run Scripts/check-font-glyphs.cs -- Teto/Level --file Teto/Level/lyric_1.brc
  dotnet run Scripts/check-font-glyphs.cs -- C:\\Fonts --text "歌词测试" --all
""");
}
