# Business Logic & System Documentation - Onyx.Oms

## 1. Introduction
This document outlines the business logic, domain entities, workflows, and rules governing the Onyx Order Management System (OMS). It serves as a reference for the development team to ensure a shared understanding of the system's behavior and intent.

**Target Audience:** Developers, Product Owners, QA, Stakeholders.

## 2. System Overview
Onyx.Oms is an Order Management System tailored for a clothing business that receives orders primarily through social media channels (Facebook, WhatsApp). The system handles product catalog management, order processing, dynamic inventory tracking, uncoupled production/procurement task management, and integration with local couriers with manual fallbacks. 

## 3. Domain Entities

### 3.1 Catalog
- **Category**: Hierarchical product categories (e.g., Category -> Subcategory -> Sub-subcategory).
- **Product**: The base product definition.
- **Product Variant**: A specific variation of a product (e.g., Size, Color). Tracks inventory using four key metrics:
  - **StockOnHand**: Physical items currently in the warehouse.
  - **ReservedStock**: Items allocated to confirmed, unshipped orders.
  - **AvailableStock**: (Calculated: `StockOnHand` - `ReservedStock`). What can be sold right now.
  - **InboundStock**: Items currently being produced or procured via active Fulfillment Tasks.

### 3.2 Sales
- **Customer**: The buyer. Can be selected or created on the fly during order placement.
- **Order**: Represents a customer purchase. Associates a customer with multiple order items.
- **Order Item**: A specific product variant within an order, tracking its requested quantity and individual fulfillment status.
- **Payment**: A manual record of money received for a specific order.

### 3.3 Fulfillment
- **Fulfillment Task (Production/Procurement)**: Represents the work to produce or acquire items. *Crucially, these can be decoupled from a specific Order Item (orphaned) if an order is cancelled, allowing the completed items to flow into general `AvailableStock`.*

## 4. Status Definitions

### 4.1 Order Statuses
- **Pending**: Initial state. Saved but not yet confirmed.
- **Confirmed**: Payment criteria met. Stock is allocated or tasks are created for missing items.
- **Processing**: Work has started on fulfillment tasks for missing items.
- **Ready to Pack**: All order items are ready for fulfillment.
- **Packed**: Items are packed.
- **Shipped**: Handed over to the courier; tracking number is assigned.
- **Delivered**: Courier successfully delivered the package.
- **Completed**: Order is delivered AND fully paid.
- **Payment Failed**: Advance payment was rejected or invalid. `ReservedStock` is released.
- **Cancelled**: Order is voided before shipping. `ReservedStock` is released.
- **Returned to Sender (RTO)**: Package bounced back from courier. Goods are physically returned to `StockOnHand`.
- **Delivery Failed**: Package lost, stolen, or destroyed by courier. No stock is returned.

### 4.2 Order Item Statuses
- **Allocated**: Item is in stock and reserved.
- **To Be Produced**: Item is out of stock; production task pending.
- **To Be Procured**: Item is out of stock; procurement task pending.
- **In Production**: Internal work has started.
- **Ordered (Procurement)**: Order placed with a supplier.
- **Ready**: Item is made/acquired and ready to pack.

### 4.3 Payment Statuses
- **Unpaid**: No payments recorded.
- **Partially Paid**: Advance payment recorded; balance remains.
- **Fully Paid**: Total order amount received.

## 5. Key Workflows

### 5.1 Order Placement & Confirmation
1. **Intake**: Customer requests items via social channels.
2. **Creation**: User creates an Order and adds Product Variants. System allows any quantity to be added. UI displays `AvailableStock` and `InboundStock` for informed selling.
3. **Confirmation**: User confirms payment method. 
4. **Allocation**: 
   - Available items increment the variant's `ReservedStock`.
   - Missing items generate Fulfillment Tasks.

### 5.2 Fulfillment & Stock Inflow
1. **Task Visibility**: Missing items populate a Production or Procurement Backlog.
2. **Start Work**: User begins the task. `InboundStock` for the variant is incremented. Parent Order status becomes **Processing**.
3. **Completion**: User marks the task as **Ready**. 
   - `InboundStock` is decremented.
   - `StockOnHand` is incremented.
   - If the task is linked to an Order Item, that item is marked **Ready** and added to the order's `ReservedStock`.
4. **Order Readiness**: When all Order Items are "Ready" or "Allocated", Order status becomes **Ready to Pack**.

### 5.3 Shipping & Delivery (Fault-Tolerant)
1. **Packing**: User marks the order as **Packed**.
2. **Waybill Generation (Optional API)**: User attempts to generate a waybill via courier API. If successful, tracking number and label are fetched.
3. **Manual Fallback**: If the API fails or is skipped, the user manually types the tracking number from a pre-printed sticker.
4. **Dispatch**: Order handed to courier; status changes to **Shipped**.

### 5.4 Exceptions: Splitting, Cancellations, & Returns
- **Order Splitting ("Clone and Remove")**: If an order is stuck waiting on a delayed item, the user can select the delayed items and split them into a new Order. The original order retains the available items and transitions to **Ready to Pack**. Advance payments remain on the original order, cascading any excess balance to the new order.
- **Cancellations & Orphaned Tasks**: If an order is **Cancelled**, its `ReservedStock` is released. If it had active Fulfillment Tasks, the user is prompted to either cancel the tasks or keep them. Kept tasks are unlinked (orphaned) from the order, and upon completion, their yield goes directly into general `StockOnHand`.
- **Post-Shipment Failures**: 
  - If **Returned to Sender**, the items are added back to `StockOnHand` and `ReservedStock` is cleared for those items.
  - If **Delivery Failed**, the items are written off (no inventory adjustment).

## 6. Business Rules
- **Confirmation Validation**: Order cannot be confirmed unless marked COD or has a payment record.
- **Stock Flexibility**: Order confirmation is never blocked by insufficient stock; shortages automatically queue Fulfillment Tasks.
- **Strict Separation of Stock**: `AvailableStock` is strictly a calculated view. All inventory operations must mutate either `StockOnHand`, `ReservedStock`, or `InboundStock`.
- **Readiness Rule**: Order cannot transition to "Ready to Pack" unless all Order Items are "Ready" or "Allocated".

## 7. Integration Points
- **Local Couriers**: Optional REST API integration for automated waybill generation and tracking number retrieval, with full support for manual data entry fallbacks.