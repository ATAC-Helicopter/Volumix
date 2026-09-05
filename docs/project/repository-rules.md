# Repository rules

`VMX-0017` owns GitHub-side enforcement of this policy. These rules follow the same protected-history intent as VaultSync while matching Volumix's current Linux-only CI and single `main` release spine.

## Intended `main` ruleset

- Prevent branch deletion.
- Prevent non-fast-forward updates and force-pushes.
- Require changes to arrive through a pull request.
- Dismiss stale approvals when a pull request changes.
- Require all review conversations to be resolved.
- Require the `linux` status check from `build-and-test` against the latest `main` before merge.
- Require the `native-sanitizers` status check added during M0.2 qualification.
- Allow merge commits so the owning work ID and pull-request boundary remain visible in history.
- Permit repository administrators to bypass only for recovery or an explicitly documented emergency.

Pull-request review count and CODEOWNERS approval become required when a second maintainer can provide independent review. Until then, CI and the pull-request record are mandatory even when the repository owner performs the merge.

## Current enforcement

As of 2026-09-04, Volumix is private and GitHub rejects repository rulesets for the current account plan with HTTP 403. Enabling this ruleset therefore requires either a GitHub plan that supports rulesets for private repositories or an intentional change to public visibility. The repository remains private until its owner makes that separate visibility decision.

Until GitHub-side enforcement is available, maintainers follow the intended policy manually: use an ID-bearing branch and pull request, wait for a green `linux` check, merge without rewriting published history, and never delete or force-push `main`.

The reviewable API payload is [main-ruleset.json](main-ruleset.json). After the account/visibility prerequisite is resolved, apply it with `gh api --method POST repos/ATAC-Helicopter/Volumix/rulesets --input docs/project/main-ruleset.json`, then read back the active ruleset and verify the target, bypass policy, pull-request requirements, and both strict status checks before closing `VMX-0017`.
