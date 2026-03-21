/*
 * =============================================================================
 * 🎓 PRODUCTION ORDER RELEASE — LOW LEVEL DESIGN (LLD) v2.0
 * =============================================================================
 * 
 * NEW DESIGN: Sub-States + Saga Pattern + Outbox
 * 
 * Purana LLD (v1.0) me sirf basic async release tha.
 * Ab hum 3 industry patterns use kar rahe hain:
 * 
 *   1. SUB-STATE PATTERN  → ReservationStatus (Pending/Reserved/Failed)
 *   2. SAGA PATTERN       → Cancel hone pe materials wapas return
 *   3. OUTBOX PATTERN     → Guaranteed event delivery via MassTransit
 * 
 * =============================================================================
 */


// ============================================================================
// 📌 PART 1: OLD vs NEW DESIGN — KYA BADLA?
// ============================================================================
/*
 * 
 * 🔷 OLD DESIGN (v1.0) — Problems:
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   ┌──────────┐    ┌──────────┐    ┌────────────┐
 *   │  Create  │───▶│ Released │───▶│ InProgress │
 *   └──────────┘    └──────────┘    └────────────┘
 *                        │
 *                   Materials reserved? Unknown! 💀
 *                   Reservation failed? PO stuck! 💀
 *                   Cancel kiya? Materials lost! 💀
 * 
 * 
 * 🔷 NEW DESIGN (v2.0) — Solutions:
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   ┌──────────┐    ┌─────────────────────────────────────┐    ┌────────────┐
 *   │  Create  │───▶│  Released                           │───▶│ InProgress │
 *   └──────────┘    │  ├── PendingReservation (waiting)   │    └────────────┘
 *                   │  ├── Reserved ✅ (can Start)        │
 *                   │  └── ReservationFailed ❌ (Retry)   │
 *                   └─────────────────────────────────────┘
 *                        │
 *                        ▼ Cancel? → Saga returns materials
 *                   ┌──────────┐
 *                   │ Cancelled│
 *                   └──────────┘
 * 
 * 
 * 🔷 COMPARISON TABLE:
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   │ Aspect               │ Old Design         │ New Design ✅          │
 *   ├──────────────────────┼────────────────────┼────────────────────────┤
 *   │ After Release        │ Status=Released    │ Released.Pending       │
 *   │ Inventory call       │ Async (no track)   │ Async + sub-state      │
 *   │ If reservation fails │ PO stuck Released  │ ReservationFailed      │
 *   │ Can retry?           │ No                 │ Yes → RetryReservation │
 *   │ Cancel + return mat? │ Not designed       │ Saga → return event    │
 *   │ BOM locked?          │ No                 │ Yes (BomCode+Version)  │
 *   │ Event guaranteed?    │ No (fire & forget) │ Outbox pattern         │
 * 
 */


// ============================================================================
// 📌 PART 2: STATE MACHINE — COMPLETE LIFECYCLE
// ============================================================================
/*
 * 
 * 🔷 STATES:
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   ProductionOrderStatus          ReservationStatus (Sub-State)
 *   ─────────────────────          ─────────────────────────────
 *   "Create"                       null (not relevant yet)
 *   "Released"                     "Pending"  → waiting for Inventory
 *   "Released"                     "Reserved" → materials confirmed ✅
 *   "Released"                     "Failed"   → reservation failed ❌
 *   "InProgress"                   "Reserved" → production started
 *   "Completed"                    "Reserved"
 *   "Cancelled"                    any
 * 
 * 
 * 🔷 TRANSITIONS:
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   Create ──[ReleaseAsync]──▶ Released + Pending
 *                                    │
 *                    ┌───────────────┼───────────────┐
 *                    ▼               ▼               ▼
 *              [Inventory OK]  [Inventory Fail]  [CancelAsync]
 *                    │               │               │
 *                    ▼               ▼               ▼
 *           Released+Reserved  Released+Failed    Cancelled
 *                    │               │           (+ MaterialReturnEvent
 *                    │               │            if was Reserved)
 *            [StartAsync]     [RetryAsync]
 *                    │               │
 *                    ▼               ▼
 *             InProgress      Released+Pending (retry)
 *                    │
 *            [CompleteAsync]
 *                    │
 *                    ▼
 *              Completed
 * 
 */


// ============================================================================
// 📌 PART 3: DATABASE MODEL — NEW FIELDS
// ============================================================================
/*
 * 
 * 🔷 ProductionOrder Model (New fields):
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   File: Models/ProductionOrder.cs
 * 
 *   // ── BOM Locking ──
 *   string? BomCode                     → "BOM-CHAIR-001"
 *   int?    BomVersion                  → 2
 * 
 *   // ── Reservation Sub-State ──
 *   string? ReservationStatus           → "Pending" / "Reserved" / "Failed"
 *   string? ReservationFailReason       → "Insufficient stock for BOLT-001"
 *   int     ReservationAttempts         → 1, 2, 3...
 *   DateTime? LastReservationAttempt    → when last tried
 * 
 *   // ── Release Audit ──
 *   DateTime? ReleasedAt               → when released
 *   Guid?     ReleasedBy               → who released
 * 
 *   // ── Cancel Tracking ──
 *   string?   CancelReason             → "Customer cancelled order"
 *   DateTime? CancelledAt              → when cancelled
 *   bool      MaterialsReturned        → false (Saga tracking)
 * 
 * 
 * 🔷 ProductionOrderStatus Constants:
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   public static class ProductionOrderStatus
 *   {
 *       public const string Create     = "Create";
 *       public const string Released   = "Released";
 *       public const string InProgress = "InProgress";
 *       public const string Completed  = "Completed";
 *       public const string Cancelled  = "Cancelled";
 *   }
 * 
 *   public static class ReservationStatus
 *   {
 *       public const string Pending  = "Pending";
 *       public const string Reserved = "Reserved";
 *       public const string Failed   = "Failed";
 *   }
 * 
 */


// ============================================================================
// 📌 PART 4: EVENTS — KYA BHEJTE HAIN, KYA AATA HAI
// ============================================================================
/*
 * 
 * 🔷 OUTGOING EVENTS (Production → Inventory):
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   EVENT 1: MaterialReservationRequestedEvent
 *   ──────────────────────────────────────────
 *   Kab: ReleaseAsync() aur RetryReservationAsync()
 *   Kisko: Inventory Service
 * 
 *   {
 *     EventId, OccurredAt,
 *     ProductionOrderId, ProductionOrderNumber,
 *     BomCode, BomVersion,        ← NEW: BOM traceability
 *     Materials: [
 *       { RawMaterialId, MaterialCode, Quantity, Unit }
 *     ]
 *   }
 * 
 * 
 *   EVENT 2: MaterialReturnRequestedEvent (SAGA)
 *   ─────────────────────────────────────────────
 *   Kab: CancelAsync() — jab materials reserved the
 *   Kisko: Inventory Service
 * 
 *   {
 *     EventId, OccurredAt,
 *     ProductionOrderId, ProductionOrderNumber,
 *     MaterialsConsumed: [
 *       { RawMaterialId, MaterialCode, QuantityConsumed=0, QuantityReturned=ALL }
 *     ]
 *   }
 * 
 * 
 * 🔷 INCOMING EVENT (Inventory → Production):
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   EVENT: StockReservedEvent
 *   ──────────────────────────
 *   Consumer: StockReservedConsumer.cs
 * 
 *   {
 *     EventId, OccurredAt,
 *     ProductionOrderId,
 *     Success: true/false,
 *     FailureReason: "Insufficient stock...",
 *     ReservedMaterials: [
 *       { RawMaterialId, QuantityReserved }
 *     ]
 *   }
 * 
 *   Consumer Logic:
 *     if Success → ReservationStatus = "Reserved", update quantities
 *     if Fail    → ReservationStatus = "Failed", store FailReason
 * 
 */


// ============================================================================
// 📌 PART 5: SEQUENCE DIAGRAMS — 4 FLOWS
// ============================================================================
/*
 * 
 * 🔷 FLOW 1: RELEASE (Happy Path)
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   USER          CONTROLLER          SERVICE             REPOSITORY          RABBITMQ           INVENTORY
 *   ────          ──────────          ───────             ──────────          ────────           ─────────
 *     │                │                │                      │                  │                   │
 *     │ PATCH /release │                │                      │                  │                   │
 *     ├───────────────▶│                │                      │                  │                   │
 *     │                │ ReleaseAsync() │                      │                  │                   │
 *     │                ├───────────────▶│                      │                  │                   │
 *     │                │                │ GetWithDetails()     │                  │                   │
 *     │                │                ├─────────────────────▶│                  │                   │
 *     │                │                │◀─────────────────────┤                  │                   │
 *     │                │                │                      │                  │                   │
 *     │                │                │ Validate:                               │                   │
 *     │                │                │  Status==Create? ✅                     │                   │
 *     │                │                │  Materials exist? ✅                    │                   │
 *     │                │                │  BOM active? ✅                         │                   │
 *     │                │                │                      │                  │                   │
 *     │                │                │ Lock BOM version                        │                   │
 *     │                │                │ Status=Released                         │                   │
 *     │                │                │ ReservationStatus=Pending               │                   │
 *     │                │                │                      │                  │                   │
 *     │                │                │ UpdateAsync()        │                  │                   │
 *     │                │                ├─────────────────────▶│                  │                   │
 *     │                │                │                      │                  │                   │
 *     │                │                │ PublishReservationEvent                 │                   │
 *     │                │                ├────────────────────────────────────────▶│                   │
 *     │                │                │                      │                  │                   │
 *     │                │ 200 OK         │                      │                  │                   │
 *     │◀───────────────┤                │                      │                  │                   │
 *     │                │                │                      │                  │   (Async later)   │
 *     │                │                │                      │                  ├──────────────────▶│
 *     │                │                │                      │                  │  Reserve stock    │
 *     │                │                │                      │                  │◀──────────────────┤
 *     │                │                │                      │                  │ StockReservedEvent│
 * 
 * 
 * 🔷 FLOW 2: INVENTORY RESPONDS (Success/Failure)
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   INVENTORY         RABBITMQ         StockReservedConsumer        DB
 *   ─────────         ────────         ─────────────────────        ──
 *       │                 │                     │                    │
 *       │ StockReservedEvent                    │                    │
 *       ├────────────────▶│                     │                    │
 *       │                 ├────────────────────▶│                    │
 *       │                 │                     │ Find order + mat   │
 *       │                 │                     ├───────────────────▶│
 *       │                 │                     │◀───────────────────┤
 *       │                 │                     │                    │
 *       │                 │                     │ Check: Released+Pending?
 *       │                 │                     │                    │
 *       │                 │                     │ if Success:        │
 *       │                 │                     │   ReservationStatus│
 *       │                 │                     │    = "Reserved"    │
 *       │                 │                     │   Update quantities│
 *       │                 │                     │                    │
 *       │                 │                     │ if Failure:        │
 *       │                 │                     │   ReservationStatus│
 *       │                 │                     │    = "Failed"      │
 *       │                 │                     │   Store FailReason │
 *       │                 │                     │                    │
 *       │                 │                     │ SaveChanges()      │
 *       │                 │                     ├───────────────────▶│
 * 
 * 
 * 🔷 FLOW 3: RETRY RESERVATION (After failure)
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   USER          CONTROLLER          SERVICE               RABBITMQ
 *   ────          ──────────          ───────               ────────
 *     │                │                │                       │
 *     │ POST /retry    │                │                       │
 *     ├───────────────▶│                │                       │
 *     │                │ RetryAsync()   │                       │
 *     │                ├───────────────▶│                       │
 *     │                │                │ Validate:            │
 *     │                │                │  Status==Released? ✅ │
 *     │                │                │  ReservationStatus    │
 *     │                │                │    ==Failed? ✅       │
 *     │                │                │                       │
 *     │                │                │ ReservationStatus     │
 *     │                │                │   = Pending           │
 *     │                │                │ Attempts += 1         │
 *     │                │                │ ClearFailReason       │
 *     │                │                │                       │
 *     │                │                │ PublishReservationEvent│
 *     │                │                ├──────────────────────▶│
 *     │                │                │                       │
 *     │◀───────────────┤ 200 OK         │                       │
 * 
 * 
 * 🔷 FLOW 4: CANCEL WITH SAGA (Material return)
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   USER          CONTROLLER          SERVICE               RABBITMQ        INVENTORY
 *   ────          ──────────          ───────               ────────        ─────────
 *     │                │                │                       │               │
 *     │ DELETE /cancel │                │                       │               │
 *     ├───────────────▶│                │                       │               │
 *     │                │ CancelAsync()  │                       │               │
 *     │                ├───────────────▶│                       │               │
 *     │                │                │ Check: wasReserved?   │               │
 *     │                │                │                       │               │
 *     │                │                │ Status=Cancelled      │               │
 *     │                │                │ CancelReason=reason   │               │
 *     │                │                │ Save to DB            │               │
 *     │                │                │                       │               │
 *     │                │                │ if wasReserved:       │               │
 *     │                │                │  MaterialReturnEvent  │               │
 *     │                │                ├──────────────────────▶│               │
 *     │                │                │                       ├──────────────▶│
 *     │                │                │                       │  Return stock │
 *     │                │                │                       │               │
 *     │◀───────────────┤ 200 OK         │                       │               │
 * 
 */


// ============================================================================
// 📌 PART 6: ACTUAL CODE — METHOD BY METHOD
// ============================================================================
/*
 * 
 * 🔷 METHOD 1: ReleaseAsync (ProductionOrderService.cs)
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   File: Services/ProductionOrders/ProductionOrderService.cs
 * 
 *   Step 1: GetByIdWithDetailsAsync(id) → order + materials
 *   Step 2: Validate Status == "Create"
 *   Step 3: Validate MaterialRequirements.Any()
 *   Step 4: Validate BOM is active
 *   Step 5: Lock BOM version → order.BomCode, order.BomVersion
 *   Step 6: Set Status = Released, ReservationStatus = Pending
 *           Set ReleasedAt, ReleasedBy, ReservationAttempts = 1
 *   Step 7: UpdateAsync(order) → save first, then publish
 *   Step 8: PublishReservationEvent(order, bom) → RabbitMQ
 * 
 *   ⚠️ KEY POINT: Save to DB BEFORE publishing event!
 *      If publish fails, order is still Released+Pending (Outbox handles retry)
 *      If save fails, event never published (consistent)
 * 
 * 
 * 🔷 METHOD 2: RetryReservationAsync (ProductionOrderService.cs)
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   Step 1: GetByIdWithDetailsAsync(id)
 *   Step 2: Validate Status == Released AND ReservationStatus == Failed
 *   Step 3: Reset → ReservationStatus = Pending, clear FailReason
 *   Step 4: Increment ReservationAttempts
 *   Step 5: UpdateAsync + PublishReservationEvent
 * 
 *   ⚠️ REUSE: Both ReleaseAsync and RetryReservationAsync use
 *      the same helper method PublishReservationEvent()
 * 
 * 
 * 🔷 METHOD 3: StartAsync (ProductionOrderService.cs)
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   Step 1: Validate Status == Released
 *   Step 2: Validate ReservationStatus == Reserved  ← NEW CHECK!
 *           Without this, production could start without materials!
 *   Step 3: Status = InProgress, set ActualStartDate
 * 
 * 
 * 🔷 METHOD 4: CancelAsync — SAGA PATTERN (ProductionOrderService.cs)
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   Step 1: Get order + materials
 *   Step 2: Block cancel from Completed & InProgress
 *   Step 3: Check wasReserved = (ReservationStatus == Reserved)
 *   Step 4: Status = Cancelled, save CancelReason, CancelledAt
 *   Step 5: Save to DB
 *   Step 6: if wasReserved → publish MaterialReturnRequestedEvent
 *           (SAGA: compensating action to undo reservation)
 * 
 *   ⚠️ SAGA PATTERN:
 *      Normal flow: Reserve materials → Start → Complete
 *      Compensating: Cancel → Return materials (undo reservation)
 *      This ensures Inventory state stays consistent even after cancel.
 * 
 * 
 * 🔷 METHOD 5: PublishReservationEvent — HELPER (private)
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   Used by: ReleaseAsync, RetryReservationAsync
 *   Creates: MaterialReservationRequestedEvent with BOM + materials
 *   Publishes via: _eventPublisher.PublishAsync()
 *   Avoids: Code duplication between Release and Retry
 * 
 */


// ============================================================================
// 📌 PART 7: CONSUMER — StockReservedConsumer
// ============================================================================
/*
 * 
 * 🔷 StockReservedConsumer (Events/Consumers/StockReservedConsumer.cs)
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   Trigger: Inventory publishes StockReservedEvent
 *   
 *   Step 1: Find order (Include MaterialRequirements)
 *   Step 2: Guard — only process if Released + Pending
 *           (ignore if already Reserved/Failed/Cancelled)
 *   Step 3: if Success:
 *             → Update each MaterialRequirement.QuantityReserved
 *             → Set ReservationStatus = "Reserved"
 *   Step 4: if Failure:
 *             → Set ReservationStatus = "Failed"
 *             → Store FailureReason in ReservationFailReason
 *   Step 5: SaveChangesAsync()
 * 
 *   ⚠️ IDEMPOTENCY: Guard at Step 2 ensures duplicate messages
 *      don't corrupt state (important for async systems!)
 * 
 */


// ============================================================================
// 📌 PART 8: API ENDPOINTS
// ============================================================================
/*
 * 
 * 🔷 RELEASE ENDPOINT:
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   [HttpPatch("{id:guid}/release")]
 *   
 *   Request:  PATCH /api/production/orders/{id}/release
 *   Auth:     userId extracted from JWT claims
 *   Body:     none
 *   
 *   Response (200):
 *   {
 *     "success": true,
 *     "message": "Production order released. Material reservation requested."
 *   }
 *   
 *   Errors:
 *   - 404: Order not found
 *   - 400: Status not "Create"
 *   - 400: No material requirements
 *   - 400: BOM inactive
 * 
 * 
 * 🔷 RETRY RESERVATION ENDPOINT:
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   [HttpPost("{id:guid}/retry-reservation")]
 *   
 *   Request:  POST /api/production/orders/{id}/retry-reservation
 *   Body:     none
 *   
 *   Response (200):
 *   {
 *     "success": true,
 *     "message": "Reservation retry initiated."
 *   }
 *   
 *   Errors:
 *   - 404: Order not found
 *   - 400: Status not "Released"
 *   - 400: ReservationStatus not "Failed"
 * 
 */


// ============================================================================
// 📌 PART 9: 3 INDUSTRY PATTERNS — KYU USE KIYE?
// ============================================================================
/*
 * 
 * 🔷 PATTERN 1: SUB-STATE PATTERN
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   ❌ WITHOUT: Status=Released → Materials confirmed? Unknown!
 *   ✅ WITH:    Status=Released + ReservationStatus=Pending/Reserved/Failed
 * 
 *   Benefit: UI can show:
 *   - "Released — Waiting for materials..." (Pending)
 *   - "Released — Materials ready ✅" (Reserved)
 *   - "Released — Reservation failed, Retry? ❌" (Failed)
 * 
 * 
 * 🔷 PATTERN 2: SAGA PATTERN (Compensating Transaction)
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   Problem:  Materials reserved in Inventory. User cancels PO.
 *             Materials are stuck as "reserved" forever! 💀
 * 
 *   Solution: SAGA = Forward action + Compensating action
 * 
 *   Forward:     ReleaseAsync     → MaterialReservationRequestedEvent
 *   Compensating:CancelAsync      → MaterialReturnRequestedEvent
 *                                   (return all reserved materials)
 * 
 *   Real-world analogy:
 *     Forward:     Book hotel   → Pay deposit
 *     Compensating:Cancel trip  → Refund deposit
 * 
 * 
 * 🔷 PATTERN 3: OUTBOX PATTERN (via MassTransit)
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 *   Problem:  DB save + Event publish = 2 separate operations
 *             What if DB saves but event fails to publish? 💀
 *             Or event publishes but DB save fails? 💀
 * 
 *   Solution: Outbox — Save event to DB first, then deliver later
 * 
 *   MassTransit handles this automatically:
 *     1. Event saved to outbox table with DB transaction
 *     2. Background job picks up and publishes to RabbitMQ
 *     3. If publish fails, retries automatically
 * 
 *   Our code:
 *     await _repository.UpdateAsync(order);    ← Save to DB
 *     await PublishReservationEvent(order);     ← MassTransit Outbox
 *     // Both succeed or both fail (transactional)
 * 
 */


// ============================================================================
// 📌 PART 10: TESTING CHECKLIST
// ============================================================================
/*
 * 
 * ☑ TEST 1: Basic Release
 *    1. Create PO (Status=Create)
 *    2. PATCH /release
 *    3. Verify: Status=Released, ReservationStatus=Pending
 *    4. Check RabbitMQ: MaterialReservationRequestedEvent published
 * 
 * ☑ TEST 2: Release Validation Errors
 *    - Try releasing Status=InProgress → 400 error
 *    - Try releasing without materials → 400 error
 *    - Try releasing with inactive BOM → 400 error
 * 
 * ☑ TEST 3: StockReserved (Success)
 *    - Simulate StockReservedEvent (Success=true)
 *    - Verify: ReservationStatus=Reserved
 *    - Verify: MaterialRequirements quantities updated
 * 
 * ☑ TEST 4: StockReserved (Failure)
 *    - Simulate StockReservedEvent (Success=false)
 *    - Verify: ReservationStatus=Failed, FailReason stored
 * 
 * ☑ TEST 5: Retry Reservation
 *    - After failure, POST /retry-reservation
 *    - Verify: ReservationStatus=Pending, Attempts incremented
 *    - Verify: New MaterialReservationRequestedEvent published
 * 
 * ☑ TEST 6: Start After Reserved
 *    - ReservationStatus=Reserved → StartAsync works ✅
 *    - ReservationStatus=Pending  → StartAsync blocked ❌
 *    - ReservationStatus=Failed   → StartAsync blocked ❌
 * 
 * ☑ TEST 7: Cancel with Saga
 *    - Cancel when Reserved → MaterialReturnRequestedEvent published
 *    - Cancel when Pending → No return event (nothing to return)
 *    - Cancel when Create → Simple cancel, no events
 * 
 * ☑ TEST 8: BOM Version Lock
 *    - After release, verify BomCode and BomVersion stored on order
 *    - Even if BOM updated later, order has original version
 * 
 */


// ============================================================================
// 📌 PART 11: FILES CHANGED — QUICK REFERENCE
// ============================================================================
/*
 * 
 * ┌────────────────────────────────────────────────────────────────────────┐
 * │ File                          │ What Changed                          │
 * ├────────────────────────────────────────────────────────────────────────┤
 * │ Models/ProductionOrder.cs     │ + BomCode, BomVersion                 │
 * │                               │ + ReservationStatus, FailReason       │
 * │                               │ + ReservationAttempts, LastAttempt    │
 * │                               │ + ReleasedAt, ReleasedBy              │
 * │                               │ + CancelReason, CancelledAt           │
 * │                               │ + ReservationStatus constants         │
 * ├────────────────────────────────────────────────────────────────────────┤
 * │ DTOs/ProductionOrderDtos.cs   │ + All new fields in DTO               │
 * ├────────────────────────────────────────────────────────────────────────┤
 * │ Data/ProductionDbContext.cs   │ + ReservationStatus index             │
 * ├────────────────────────────────────────────────────────────────────────┤
 * │ Events/ProductionEvents.cs    │ + BomCode/BomVersion in Reservation   │
 * │                               │ + MaterialReturnRequestedEvent        │
 * ├────────────────────────────────────────────────────────────────────────┤
 * │ Services/ProductionOrder      │ ~ ReleaseAsync (full rewrite)         │
 * │   Service.cs                  │ + RetryReservationAsync               │
 * │                               │ ~ StartAsync (sub-state check)        │
 * │                               │ ~ CancelAsync (Saga pattern)          │
 * │                               │ + PublishReservationEvent (helper)    │
 * │                               │ ~ MapToDto (new fields)               │
 * ├────────────────────────────────────────────────────────────────────────┤
 * │ IProductionOrderService.cs    │ + RetryReservationAsync signature     │
 * ├────────────────────────────────────────────────────────────────────────┤
 * │ Consumers/StockReserved       │ + Sub-state handling                  │
 * │   Consumer.cs                 │ + Guard: Released+Pending only        │
 * ├────────────────────────────────────────────────────────────────────────┤
 * │ Controllers/ProductionOrders  │ + PATCH /release                      │
 * │   Controller.cs               │ + POST /retry-reservation             │
 * └────────────────────────────────────────────────────────────────────────┘
 * 
 * 
 * 📝 KEY TAKEAWAYS:
 * ═════════════════
 * 
 * 1. Sub-states give VISIBILITY into async processes
 * 2. Saga ensures CONSISTENCY when cancelling
 * 3. Outbox ensures RELIABLE event delivery
 * 4. BOM locking ensures TRACEABILITY
 * 5. Retry mechanism handles FAILURES gracefully
 * 
 */
