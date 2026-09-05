# AI-assisted development files

This fork was developed with AI assistance under a methodology called **rhizome**
(governance and session-memory tooling shared across several of the author's
projects). It runs rhizome's **two-branch model**
(`rhizome-protocol:protocol/docs/rhiz-child-repo-convention.md#5.5`):
**`rhiz-working-bracken`** carries the full tooling and is where all development
happens; **`master`** is generated from it by mechanically stripping every
rhizome/AI file and carries none of it.

**If you just want clean code: you're probably already looking at it.** `master`
is this repo's default branch, and it never carries any of the files below —
nothing to run, nothing to download differently. This file only matters if you're
on `rhiz-working-bracken` (or a branch forked from it) and want a temporarily
clean local copy without switching to `master`.

## What's on `rhiz-working-bracken` and not on `master`

| Path | What it is |
|---|---|
| `.rhiz-binding.json` | Pins to the shared `rhizome-protocol` tooling repo and this project's own `WebPageScreensaver-memory` notes repo. |
| `.rhiz-artifacts.json` | The registry — declares exactly this list, with a reason per row. Read by `rhiz promote` when generating `master`. |
| `tools/rhiz.py` | A bootstrap/dispatcher script that fetches and forwards to the real tooling in `rhizome-protocol`. Also the entry point for Claude Code session hooks, since `.claude/` is present. |
| `.claude/` | Claude Code configuration (hooks, custom commands). |
| This file | — |

## Why `master` doesn't need its own configs

`rhiz promote` guarantees that everything on `master` outside the list above is
**byte-identical** to `rhiz-working-bracken` — not just similar, the exact same
bytes. So anything already checked against `rhiz-working-bracken` (review, the
existing `.NET` build) is transitively still true of `master`'s copy.
`.github/workflows/dotnet.yml` builds both branches, so a promoted `master` that
fails to build is a CI failure, not a silent possibility.
