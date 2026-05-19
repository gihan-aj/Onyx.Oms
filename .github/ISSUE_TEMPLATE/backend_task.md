---
name: Backend Feature / Endpoint
about: Create a new backend task using Vertical Slice Architecture
title: '[Backend] '
labels: 'backend, enhancement'
assignees: ''
---
**Description:**
We need to expose endpoints to manage the `[Domain Entity]`. Please follow our established Vertical Slice Architecture patterns. You can reference `[Insert Link to Existing Feature Slice]` as a blueprint.

**Acceptance Criteria:**
- [ ] Implement `GET` endpoint(s) to retrieve data (with standard pagination if applicable).
- [ ] Implement `POST`/`PUT` endpoints for creation or updates.
- [ ] Implement FluentValidation for the commands (e.g., validate exact numeric ranges, string lengths).
- [ ] Implement specific action endpoints (e.g., Activate/Deactivate) without heavy validation if not required.
- [ ] Ensure all operations update the database correctly and return appropriate HTTP status codes.
- [ ] Logic is placed strictly within the correct layer/slice.
