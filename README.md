# legacy-code-mcp

An MCP server that helps AI coding agents understand unfamiliar legacy codebases.

## Why

Before you can safely change a large legacy codebase, you first have to figure out how it is laid out,
which parts matter, and how it got that way. That ramp-up is slow and mostly manual.
This server exposes that kind of information as MCP tools, so an agent such as Claude Code
can explore a repository for you and answer those questions.

## Tools

### `read_repo` (read-only)

Lists the directory tree of a local repository as indented text. Folders end with `/`.

| Parameter | Type | Default | Description |
|---|---|---|---|
| `path` | string | (required) | Absolute path to the repository root. |
| `maxDepth` | int | `3` | How many levels deep to list. `1` means only the root's direct children. |

- Skips `.git`, `bin`, `obj`, `node_modules`, `.vs`, `venv` and `__pycache__`.
- Stops after 500 entries and ends with `... (truncated)`.
- Returns an error if the directory does not exist or `maxDepth` is less than 1.

## Requirements

- .NET 10 SDK
- Node.js (only for MCP Inspector)

## Build

```bash
dotnet build
```

The server uses the stdio transport: stdout carries MCP messages, and all logs go to stderr.

## Debug with MCP Inspector

```bash
npx @modelcontextprotocol/inspector dotnet bin/Debug/net10.0/LegacyCodeMcp.dll
```

Run the built DLL instead of `dotnet run`, so build output can never end up on stdout.

## Use with Claude Code

```bash
claude mcp add legacy-code -- dotnet /absolute/path/to/LegacyCodeMcp.dll
```

## Roadmap

- [x] `read_repo`: directory tree
- [ ] `query_git_history`: recent commits for a repository
- [ ] `parse_ast`: classes and methods in a source file
- [ ] Multi-agent workflow (Planner / Reader / Reviewer) built on these tools
- [ ] Agent-level evaluation: tool-call success rate, task completion rate, steps per task, token cost per task
