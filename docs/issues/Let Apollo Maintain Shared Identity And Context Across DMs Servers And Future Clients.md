---
issue_number: 208
title: Let Apollo maintain shared identity and context across DMs, servers, and future clients
status: open
labels:
  - enhancement
assignees: []
milestone: backlog-access-identity-and-platform
milestone_doc: "[[Backlog Access Identity and Platform]]"
created_at: 2026-03-22T20:10:06Z
updated_at: 2026-03-22T20:10:06Z
source_url: https://github.com/codeacula/apollo/issues/208
---

# Summary

Apollo should eventually feel like the same persistent assistant no matter where you interact with it: Discord DMs, Discord servers, the web app, a future PWA, or other clients.\n\n## Why\n\n- users should be able to talk to Apollo in channels it is part of\n- Apollo should remember the user across servers and clients when appropriate\n- the assistant should feel continuous rather than fragmented by transport/platform\n\n## Goals\n\n- support channel/server interactions in addition to DMs\n- preserve user identity across multiple Discord servers and future surfaces\n- define what memory/context should be global to the person versus local to a server/channel\n- avoid confusing bleed-through where server-specific context leaks into unrelated spaces\n\n## Questions to answer\n\n- what should be shared globally per person?\n- what should remain scoped to a server, channel, or conversation?\n- how should permissions and visibility work when Apollo is present in a server?\n- how does this interact with future web/mobile-first usage outside Discord?\n\n## Notes\n\nThis is a strategic design issue and likely overlaps future work on multiple PlatformIDs, server installs, and moving beyond Discord as the primary transport.\n

## Why

- 

## Scope

- 

## Acceptance Criteria

- 

## Notes

- Related issues:
- Context:
