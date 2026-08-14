# Workflow

## Language
- Always answer the user in Vietnamese.

## Before Work
- Before code edits, project analysis, or implementation, read this whole file.
- Use the `ponyman` skill for coding work.
- Prefer minimal, local changes. Do not add dependencies unless required.
- Preserve existing app flows and settings compatibility.

## UI Standards
- Keep settings grouped by existing tabs and controls.
- Prefer direct, familiar controls: checkbox for boolean, textbox for app names, buttons for commands.
- Avoid broad redesign when the requested behavior fits existing UI.
- New UI text must be short and clear.

## Build And Git
- After code changes, build Release until there are 0 errors and 0 warnings.
- Stage only scoped files.
- Commit and push automatically.
- Report the real commit hash and build result.
