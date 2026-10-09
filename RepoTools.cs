using System.ComponentModel;
using System.Text;
using ModelContextProtocol;
using ModelContextProtocol.Server;

[McpServerToolType]
public static class RepoTools
{
    private const int MaxEntries = 500;
    private static readonly HashSet<string> SkippedDirs = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", "bin", "obj", "node_modules", ".vs", "venv", "__pycache__"
    };

    [McpServerTool(Name = "read_repo", ReadOnly = true)]
    [Description("Lists the directory tree of a local repository as indented text. Folders end with '/'. Skips .git, bin, obj, node_modules, .vs, venv and __pycache__.")]
    public static string ReadRepo(
        [Description("Absolute path to the repository root directory.")] string path,
        [Description("How many levels deep to list. 1 means only the root's direct children.")] int maxDepth = 3)
    {
        if (!Directory.Exists(path))
            throw new McpException($"Directory not found: {path}");
        if (maxDepth < 1)
            throw new McpException("maxDepth must be at least 1.");

        var sb = new StringBuilder();
        var count = 0;
        var truncated = false;

        Walk(path, 0);
        if (truncated)
            sb.AppendLine("... (truncated)");
        return sb.ToString();

        void Walk(string dir, int depth)
        {
            var indent = new string(' ', depth * 2);

            foreach (var subDir in Directory.EnumerateDirectories(dir).Order(StringComparer.OrdinalIgnoreCase))
            {
                var name = Path.GetFileName(subDir);
                if (SkippedDirs.Contains(name))
                    continue;
                if (!Add(indent + name + "/"))
                    return;
                if (depth + 1 < maxDepth)
                    Walk(subDir, depth + 1);
                if (truncated)
                    return;
            }

            foreach (var file in Directory.EnumerateFiles(dir).Order(StringComparer.OrdinalIgnoreCase))
            {
                if (!Add(indent + Path.GetFileName(file)))
                    return;
            }
        }

        bool Add(string line) 
        {
            if (count == MaxEntries)
            {
                truncated = true;
                return false;
            }
            sb.AppendLine(line);
            count++;
            return true;
        }
    }
}