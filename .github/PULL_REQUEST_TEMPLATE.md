## Summary

Describe the user problem and the smallest change that solves it.

## Validation

- [ ] Managed build and tests pass.
- [ ] Native build/tests pass, or this change does not affect native code.
- [ ] Resolver behavior has synthetic fixture coverage, or is unaffected.
- [ ] Visible UI changes include screenshots.
- [ ] New controls are keyboard accessible and have accessible names.

## Architecture and scope

- [ ] Application identity remains the primary user abstraction.
- [ ] No raw PipeWire/process details leak into normal UI.
- [ ] Dependency direction remains acyclic.
- [ ] Any material departure from `TECH_SPEC.md` has an ADR.

## Privacy, security, and packaging

Describe effects on process inspection, logs, permissions, persistence, native ownership, dependencies, and packaging. Write “None” where appropriate.
