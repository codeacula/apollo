# Issue Tracking Checklist

**Based on:** ARCHITECTURAL_REVIEW.md  
**Created:** 2026-01-31

Use this checklist to track progress on fixing the identified architectural issues.

---

## 🚨 P0 - Blocking Production (36 hours)

**Must be fixed before production deployment**

### Security Issues

- [ ] **Issue #1: Missing gRPC Authentication** (4h)
  - Location: `Apollo.GRPC/Service/ApolloGrpcService.cs`
  - Task: Implement server-side authentication interceptor
  - Owner: _____________
  - Status: ⬜ Not Started
  - PR: _______________

- [ ] **Issue #2: Missing Authorization Checks** (8h)
  - Location: All gRPC endpoints (7 endpoints)
  - Task: Add access validation to all user data endpoints
  - Owner: _____________
  - Status: ⬜ Not Started
  - PR: _______________

- [ ] **Issue #3: Prompt Injection Vulnerability** (16h)
  - Location: `Apollo.AI/Prompts/PromptTemplateProcessor.cs`
  - Task: Implement input sanitization and structured prompting
  - Owner: _____________
  - Status: ⬜ Not Started
  - PR: _______________

### Reliability Issues

- [ ] **Issue #7: Job Transaction Boundary** (8h)
  - Location: `Apollo.Service/Jobs/ToDoReminderJob.cs`
  - Task: Make notification + state update atomic
  - Owner: _____________
  - Status: ⬜ Not Started
  - PR: _______________

**P0 Progress:** 0/4 (0%)

---

## ⚠️ P1 - Critical Reliability (30 hours)

**Fix before scale/load testing**

### Data Consistency

- [ ] **Issue #4: Missing Event Transactions** (12h)
  - Location: `ToDoStore.cs`, `ReminderStore.cs`, `PersonStore.cs`
  - Task: Add explicit transaction boundaries to all stores
  - Owner: _____________
  - Status: ⬜ Not Started
  - PR: _______________

- [ ] **Issue #5: Silent Projection Failures** (8h)
  - Location: `Apollo.Database/ServiceCollectionExtension.cs`
  - Task: Switch from Inline to Async projections
  - Owner: _____________
  - Status: ⬜ Not Started
  - PR: _______________

- [ ] **Issue #6: Stream Creation Race** (4h)
  - Location: All stores
  - Task: Add duplicate detection before StartStream
  - Owner: _____________
  - Status: ⬜ Not Started
  - PR: _______________

### Reliability

- [ ] **Issue #8: No Job Retry Logic** (6h)
  - Location: `Apollo.Service/Jobs/ToDoReminderJob.cs`
  - Task: Implement exponential backoff + dead-letter queue
  - Owner: _____________
  - Status: ⬜ Not Started
  - PR: _______________

**P1 Progress:** 0/4 (0%)

---

## 📋 P2 - High Severity (19 hours)

**Fix before scale to production load**

### Resource Management

- [ ] **Issue #9: Resource Leak (RestClient)** (4h)
  - Location: `Apollo.Service/ServiceCollectionExtensions.cs`
  - Task: Implement proper disposal pattern
  - Owner: _____________
  - Status: ⬜ Not Started
  - PR: _______________

### Security

- [ ] **Issue #10: Insufficient Input Validation** (8h)
  - Location: All gRPC endpoints
  - Task: Add validation middleware for all inputs
  - Owner: _____________
  - Status: ⬜ Not Started
  - PR: _______________

- [ ] **Issue #11: Identity Spoofing** (6h)
  - Location: All gRPC endpoints
  - Task: Verify identity from auth context, not client input
  - Owner: _____________
  - Status: ⬜ Not Started
  - PR: _______________

### Bug Fixes

- [ ] **Issue #12: Index Bounds Risk** (1h)
  - Location: `Apollo.Application/ToDos/ToDoPlugin.cs:544`
  - Task: Fix array slicing condition
  - Owner: _____________
  - Status: ⬜ Not Started
  - PR: _______________

**P2 Progress:** 0/4 (0%)

---

## 🧪 Testing (40 hours)

**Add critical missing test coverage**

### Access Control Tests
- [ ] Test `GrantAccessAsync` (2h)
- [ ] Test `RevokeAccessAsync` (2h)
- [ ] Test unauthorized access attempts (2h)

### Background Job Tests
- [ ] Test `ToDoReminderJob` execution (4h)
- [ ] Test notification sending (2h)
- [ ] Test reminder state updates (2h)
- [ ] Test failure scenarios (2h)

### Event Sourcing Tests
- [ ] Test transaction boundaries (4h)
- [ ] Test projection failures (4h)
- [ ] Test concurrent stream creation (2h)
- [ ] Test `WrongExpectedVersionException` handling (2h)

### AI Security Tests
- [ ] Test prompt injection defenses (4h)
- [ ] Test input sanitization (2h)
- [ ] Test tool plan validation (2h)

### Plugin Tests
- [ ] Test `ToDoPlugin` execution (2h)
- [ ] Test `RemindersPlugin` execution (2h)
- [ ] Test `PersonPlugin` execution (2h)

**Testing Progress:** 0/20 (0%)

---

## 📊 Overall Progress

| Phase | Tasks | Complete | Effort | Status |
|-------|-------|----------|--------|--------|
| P0 - Blocking | 4 | 0 | 36h | ⬜ 0% |
| P1 - Critical | 4 | 0 | 30h | ⬜ 0% |
| P2 - High | 4 | 0 | 19h | ⬜ 0% |
| Testing | 20 | 0 | 40h | ⬜ 0% |
| **Total** | **32** | **0** | **125h** | **⬜ 0%** |

---

## 🎯 Milestones

### Milestone 1: Minimum Viable Production (P0)
- **Effort:** 36 hours
- **Target:** End of Sprint 1
- **Criteria:**
  - ✅ gRPC authentication enabled
  - ✅ Authorization checks on all endpoints
  - ✅ Prompt injection defenses deployed
  - ✅ Job transaction boundaries fixed
  - ✅ All P0 tests passing
  - ✅ Security review completed

### Milestone 2: Production Ready (P0 + P1)
- **Effort:** 66 hours
- **Target:** End of Sprint 2
- **Criteria:**
  - ✅ All P0 issues resolved
  - ✅ Event sourcing transactions added
  - ✅ Async projections enabled
  - ✅ Retry logic implemented
  - ✅ All P0 + P1 tests passing
  - ✅ Load testing completed

### Milestone 3: Production Hardened (P0 + P1 + P2)
- **Effort:** 85 hours
- **Target:** End of Sprint 3
- **Criteria:**
  - ✅ All P0 + P1 issues resolved
  - ✅ Resource leaks fixed
  - ✅ Input validation comprehensive
  - ✅ Identity verification in place
  - ✅ 80%+ test coverage
  - ✅ Penetration testing completed

---

## 📅 Sprint Planning Template

### Sprint 1 (P0 Focus)
**Goal:** Fix blocking production issues

**Week 1:**
- [ ] Issue #1: gRPC Authentication (4h)
- [ ] Issue #2: Authorization Checks (8h)
- [ ] Write access control tests (6h)

**Week 2:**
- [ ] Issue #3: Prompt Injection (16h)
- [ ] Issue #7: Job Transactions (8h)
- [ ] Write AI security tests (6h)
- [ ] Security review

**Deliverable:** System ready for monitored production deployment

---

### Sprint 2 (P1 Focus)
**Goal:** Fix critical reliability issues

**Week 1:**
- [ ] Issue #4: Event Transactions (12h)
- [ ] Issue #5: Async Projections (8h)
- [ ] Write event sourcing tests (8h)

**Week 2:**
- [ ] Issue #6: Race Conditions (4h)
- [ ] Issue #8: Retry Logic (6h)
- [ ] Write background job tests (8h)
- [ ] Load testing

**Deliverable:** System ready for production load

---

### Sprint 3 (P2 + Testing)
**Goal:** Harden for production scale

**Week 1:**
- [ ] Issue #9: Resource Disposal (4h)
- [ ] Issue #10: Input Validation (8h)
- [ ] Issue #11: Identity Verification (6h)
- [ ] Issue #12: Index Bounds (1h)

**Week 2:**
- [ ] Complete missing plugin tests (6h)
- [ ] Integration testing
- [ ] Penetration testing
- [ ] Performance optimization

**Deliverable:** Production-hardened system

---

## 📝 Issue Template

Use this template when creating tickets for each issue:

```markdown
## [Issue #X]: [Issue Title]

**Severity:** [CRITICAL/HIGH/MEDIUM]
**Priority:** [P0/P1/P2]
**Effort:** [Xh]

### Problem
[Description from ARCHITECTURAL_REVIEW.md]

### Location
- File: `[path/to/file.cs]`
- Lines: [X-Y]

### Impact
- [Security/Reliability/Quality impact]
- [User-facing impact]
- [Business impact]

### Tasks
- [ ] Task 1
- [ ] Task 2
- [ ] Write tests
- [ ] Update documentation

### Acceptance Criteria
- [ ] Criterion 1
- [ ] Criterion 2
- [ ] Tests passing
- [ ] Code reviewed
- [ ] Documentation updated

### Resources
- Full details: [ARCHITECTURAL_REVIEW.md#issue-X]
- Example code: [See review document]
```

---

## ✅ Status Legend

- ⬜ Not Started
- 🔄 In Progress
- ✅ Complete
- ❌ Blocked
- ⏸️ On Hold

---

## 📞 Review Cadence

- **Daily Standup:** Update progress on assigned issues
- **Weekly Review:** Update this checklist, review blockers
- **Sprint Review:** Demo completed fixes, plan next sprint
- **Security Review:** After each P0/P1 milestone

---

**Last Updated:** 2026-01-31  
**Next Review:** [Date]  
**Updated By:** [Name]
