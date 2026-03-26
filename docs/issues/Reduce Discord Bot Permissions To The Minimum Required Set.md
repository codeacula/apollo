---
issue_number: 205
title: Reduce Discord bot permissions to the minimum required set
status: open
labels:
  - enhancement
assignees: []
milestone: backlog-access-identity-and-platform
milestone_doc: "[[Backlog Access Identity and Platform]]"
created_at: 2026-03-22T20:06:05Z
updated_at: 2026-03-22T20:06:05Z
source_url: https://github.com/codeacula/apollo/issues/205
---

# Summary

Apollo should request only the Discord permissions it actually needs. This lowers friction for server installs, reduces security concerns, and makes the bot easier to trust.\n\n## Goals\n\n- audit the permissions Apollo currently requests\n- determine the minimum viable permission set for supported features\n- document which features require which permissions\n\n## Scope\n\n- Discord OAuth/invite permission audit\n- feature-to-permission mapping\n- update invite/install guidance to use the least-privilege set\n- confirm Apollo still functions correctly with the reduced scope\n\n## Acceptance Criteria\n\n- Apollo's requested Discord permissions are reviewed and reduced where possible\n- the required permission set is documented\n- invite/install flows reflect the least-privilege permissions Apollo needs\n- any features that require elevated permissions are clearly identified\n

## Why

- 

## Scope

- 

## Acceptance Criteria

- 

## Notes

- Related issues:
- Context:
