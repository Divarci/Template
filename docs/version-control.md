# Version Control

Repository-wide policy for how AI agents (Claude and every subagent it dispatches) interact with version control in this repository. It applies to every platform (`api`, `deployment`, `bruno`) and overrides any default agent behaviour.

---

## Hard rule — no `git` commands without explicit permission

> An agent **must not run any command that starts with `git`** unless the user has explicitly instructed it to in the current conversation.

- **Every `git` command is covered**, read-only or not: `git status`, `git log`, `git diff`, `git show`, `git blame`, `git add`, `git commit`, `git checkout`, `git switch`, `git branch`, `git stash`, `git reset`, `git restore`, `git merge`, `git rebase`, `git pull`, `git fetch`, `git push`, `git tag`, and any other `git` subcommand.
- **Every shell is covered.** The rule applies regardless of whether the command is issued through Bash, PowerShell, a script, or a chained command (e.g. `cd repo && git …`).
- **Subagents are covered.** The orchestrator must state this rule in every delegation brief; a subagent may not run `git` on the strength of its own judgement.

---

## Exceptions

- A `git` command may be run **only** when the user explicitly asks for it (e.g. "commit this", "show me the diff with git", "you may use git for this task").
- The permission is **scoped to what was asked**. Approval for one operation (e.g. a commit) does not extend to other operations (e.g. a push or a branch switch), to later tasks, or to future conversations.
- The policy stays in force until the user explicitly lifts or changes it.

---

## What to do instead

- **Need to know what changed?** Read the files directly with the file tools, or ask the user to share the relevant `git` output.
- **Work is ready to commit, branch, or push?** Report it to the user and state the suggested `git` action. Do not perform it.
- **Unsure whether a command counts?** If it starts with `git`, it counts. Ask the user first.
