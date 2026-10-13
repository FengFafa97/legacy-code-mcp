using System.ComponentModel;
using System.Text;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.Diagnostics;

[McpServerToolType]
public static class GitTools
{

    [McpServerTool(Name = "query_git_history", ReadOnly = true)]
    [Description("Query recent Git commit history in a local Git repository. Returns commit hash, author, date, and commit message. Use it to see what changed recently and who changed it. Only works on a local clone, not a remote URL. Returns at most 50 commits and does not include diffs.")]
    public static async Task<string> QueryGitHistory(
        [Description("Absolute path to the repository root directory.")] string path,
        [Description("The maximum number of commits currently returned for the repository, from 1 to 50.")] int maxCount = 50)
    {

        if (maxCount < 1 || maxCount > 50)
        {
            throw new McpException(
                "maxCount must be between 1 and 50.");
        }
        if (!Directory.Exists(path))
        {
            throw new McpException(
                $"Directory does not exist: {path}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = path,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("log");
        startInfo.ArgumentList.Add("-n");
        startInfo.ArgumentList.Add(maxCount.ToString());
        startInfo.ArgumentList.Add("--date=short");
        startInfo.ArgumentList.Add(
            "--pretty=format:%h | %an | %ad | %s");


        using var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();
        process.StandardInput.Close();
        Task<string> outputTask =
            process.StandardOutput.ReadToEndAsync();

        Task<string> errorTask =
            process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        string output = await outputTask;
        string error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new McpException(
                $"Git command failed: {error.Trim()}");
        }
        return output;
    }
}