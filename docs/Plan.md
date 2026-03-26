
# Plan for Apollo

## Short Term

### Milestone 1: Reminder trust and reliability

Reference: [[short-term-reminder-trust-and-reliability]]

- Fix reminder timezone handling so reminder creation and display use the user's timezone consistently
- Fix multi-reminder delivery behavior when several reminders are due together
- Define reminder lifecycle rules clearly enough to support recurrence, follow-ups, and dashboard visibility

### Milestone 2: Nagging reminders

Reference: [[short-term-nagging-reminders]]

- Persist reminder interactions so Apollo can track unresolved reminder conversations over time
- Add follow-up scheduling with default nagging behavior and per-user settings
- Resolve reminder replies against the correct reminder context and support done/started/dismissed outcomes
- Add friendly aliases and direct interaction affordances after the core flow is reliable

### Milestone 3: Daily/repeating reminders and basic management

Reference: [[short-term-recurring-reminders-and-management]]

- Add recurring reminders for long-running tasks
- Add basic reminder management flows for listing, rescheduling, snoozing, and canceling reminders
- Add due dates to to-dos so reminders and planning have stronger time context

### Near-Term Secondary Work

Reference: [[near-term-dashboard-foundation]]

- Dashboard overview MVP and supporting overview data model
- Realtime dashboard/client updates for reminders and to-dos
- Product telemetry and discoverability work that helps evaluate viability testing

## Long Term

- Access, approval, and identity expansion across servers and future clients
- Reduced Discord dependency, including possible mobile/PWA notification paths
- Broader multi-tenant and subscription-oriented platform work

Roadmap index: [[docs/milestones/README]]
