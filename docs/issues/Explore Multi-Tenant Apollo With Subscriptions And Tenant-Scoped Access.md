---
issue_number: 207
title: "Explore multi-tenant Apollo with subscriptions and tenant-scoped access"
status: "open"
labels: ["enhancement"]
assignees: []
created_at: "2026-03-22T20:10:05Z"
updated_at: "2026-03-22T20:10:05Z"
source_url: "https://github.com/codeacula/apollo/issues/207"
---

# Summary

Apollo may eventually need to support multiple tenants/customers with subscription-backed access rather than only being a single privately managed assistant instance.\n\n## Why\n\n- allow people outside the initial test circle to subscribe and use Apollo\n- separate data, configuration, and access cleanly per tenant/workspace\n- create a path toward a hosted product model\n\n## Questions to answer\n\n- what is the tenant boundary: person, household, Discord server, workspace, or account?\n- how should billing/subscription state map to access control?\n- which configuration lives globally vs per tenant vs per person?\n- how should reminders, conversations, telemetry, and AI/provider usage be isolated?\n- what architectural changes are needed for auth, routing, storage, and admin UI?\n\n## Notes\n\nThis is intentionally parked as a future-facing design issue. It can be promoted into Release or a dedicated commercialization milestone later.\n

## Why

- 

## Scope

- 

## Acceptance Criteria

- 

## Notes

- Related issues:
- Context:
