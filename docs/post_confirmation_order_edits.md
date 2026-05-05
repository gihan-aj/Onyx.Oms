# Post-Confirmation Order Edits Discussion

This document captures the discussion, identified issues, and decided solutions for handling order modifications (adding, removing, updating items) *after* an order has been confirmed.

## 1. Adding New Items Post-Confirmation
**Context:** When an order is confirmed, stock is tracked closely. If a new item is added, we calculate an `allocatingQty` based on what is available.
**Decisions:**
- **Stock Reservation:** We must ensure `variant.ReserveStock(allocatingQty)` is explicitly called when a new item is added. (Currently, the handler calculates the quantity but it is missing the actual reservation call on the variant).
- **Fulfillment Tasks:** Any remaining unallocated quantity becomes `Pending`. Since the user manually creates tasks from the orders page and decides whether it is a production or procurement task, we **do not** need to automatically create a task. The item will remain pending until a user manually assigns a task.

## 2. Removing Items Post-Confirmation
**Context:** Removing an item that was already confirmed.
**Decisions:**
- **Stock:** Release the reserved stock (already implemented).
- **Tasks:** We must **unlink** any `FulfillmentTasks` linked to the removed order item. This orphans the task so that when it finishes, its output goes directly into `StockOnHand`.

## 3. Decreasing Item Quantity Post-Confirmation
**Context:** User reduces the quantity of an existing item.
**Decisions:**
- **Tasks:** If there is a `FulfillmentTask` actively producing more than the new required amount, we will **let it overproduce**. 
- **Allocation:** When the task completes, the `OrderItem.AllocateFromTask()` logic must handle the excess correctly by sending the overflow into general `StockOnHand`.

## 4. Increasing Item Quantity Post-Confirmation
**Context:** User increases the quantity of an existing item.
**Decisions:**
- **Stock:** Try to allocate the increased amount from the variant's `AvailableQuantity` (calling `variant.ReserveStock()`).
- **Tasks:** If there is still a pending amount, check if there are any active tasks assigned to the order item.
  - If a task exists, create a **new** task of the same type (Production or Procurement) for the pending shortfall amount.
  - *Note:* We decided not to blindly increase the quantity of the existing task, particularly for Procurement tasks, as orders may have already been sent to suppliers. Creating a new task is safer.

## 5. Order Status Regressions
**Context:** An order that is `ReadyToPack` or `Packed` drops back to `Processing` or `Confirmed` because a new item is added or quantity increased, breaking readiness.
**Decisions:**
- **Action:** Append a message to the order's `Notes` field documenting that the order regressed in status due to an edit. This provides a paper trail for the warehouse staff who might have already physically interacted with the order.

---

## Technical Note: Domain Events vs. Data Integrity

A key architectural question was raised regarding Domain Events: *If a domain event fails, does it cause data corruption because the main handler already saved changes?*

**Answer:** 
In a standard EF Core + MediatR setup, Domain Events are completely safe for data integrity. 

Domain Events are typically dispatched **within the same database transaction** as the primary action. The flow looks like this:
1. The domain entity (`Order`) records an event internally (e.g., `AddDomainEvent(new OrderItemAddedEvent(...))`).
2. During `_context.SaveChangesAsync()`, the system loops through all entities, extracts the domain events, and publishes them via MediatR.
3. The Domain Event Handlers run. They make their own changes to the tracked entities in the DbContext (like modifying Tasks or Variants).
4. The database transaction finally commits.

If **any** event handler throws an exception or fails, the entire database transaction rolls back. The original order update is completely aborted, and data integrity is perfectly maintained. 

Domain Events are not asynchronous, fire-and-forget background threads unless you explicitly configure them that way (e.g., using an Outbox pattern or external message queue). For in-memory MediatR domain events, they are strongly consistent and transactional.
