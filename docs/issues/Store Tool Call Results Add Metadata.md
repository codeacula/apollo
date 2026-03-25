---
issue_number: 166
title: "Store Tool Call Results + Add Metadata"
status: "open"
labels: ["enhancement"]
assignees: []
created_at: "2026-02-02T03:06:15Z"
updated_at: "2026-03-22T19:55:58Z"
source_url: "https://github.com/codeacula/apollo/issues/166"
---

# Summary

In order to be able to better associate a conversation and know what to dos, reminders, and such are being referenced, we should start storing the metadata associated with phase 1 and phase 2 tool calls. Update phase 1 so that it also outputs the expected todo ids if it isn't already, then update step 2 to record the cool call results. then, we don't need to arbitrarily load the entire conversation, instead in the system prompt we can load the specifics and then only focus on the immediate reply, helping alleviate hallucinations.

## Why

- 

## Scope

- 

## Acceptance Criteria

- 

## Notes

- Related issues:
- Context:
