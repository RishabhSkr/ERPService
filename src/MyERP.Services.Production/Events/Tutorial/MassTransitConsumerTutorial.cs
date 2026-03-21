// /*
//  * =============================================================================
//  * 🎓 MASSTRANSIT EVENT CONSUMPTION TUTORIAL
//  * =============================================================================
//  * 
//  * Avi, is tutorial me hum step-by-step samjhenge:
//  * 1. MassTransit me event consume kaise hota hai
//  * 2. Routing kaise kaam karta hai
//  * 3. Kyu tumhara message consume nahi ho raha tha
//  * 4. Industry-level patterns
//  * 
//  * =============================================================================
//  */

// // ============================================================================
// // 📌 STEP 1: MASSTRANSIT ARCHITECTURE SAMJHO
// // ============================================================================
// /*
//  * MassTransit ek ABSTRACTION hai RabbitMQ ke upar. Ye 3 main components use karta hai:
//  * 
//  *   ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
//  *   │  PUBLISHER  │────▶│  EXCHANGE   │────▶│  CONSUMER   │
//  *   │ (Sales)     │     │ (RabbitMQ)  │     │ (Production)│
//  *   └─────────────┘     └─────────────┘     └─────────────┘
//  * 
//  * 🔹 EXCHANGE: Message ka "router" - decide karta hai ki message kahan jaega
//  * 🔹 QUEUE: Message ka "storage" - consumer yahan se padhta hai
//  * 🔹 BINDING: Exchange aur Queue ke beech ka connection
//  * 
//  * KEY CONCEPT: MassTransit exchange name CLR Type Name se banata hai!
//  * 
//  * Example:
//  *   Event Class: MyERP.Shared.Events.SalesOrderCreatedEvent
//  *   Exchange Name: MyERP.Shared.Events:SalesOrderCreatedEvent
//  *                  ^^^^^^^^^^^^^^^^^ ^^^^^^^^^^^^^^^^^^^^^^
//  *                   Namespace (: se)    Class Name
//  */


// // ============================================================================
// // 📌 STEP 2: MESSAGE ROUTING KA FLOW
// // ============================================================================
// /*
//  * Jab Sales Service event PUBLISH karti hai:
//  * 
//  * STEP A: Publisher side (Sales Service)
//  * ─────────────────────────────────────────
//  * await _publishEndpoint.Publish(new SalesOrderCreatedEvent { ... });
//  *                                    ╲
//  *                                     ╲
//  * MassTransit internally:              ▼
//  *   1. Event ka Type Name liya: "MyERP.Shared.Events.SalesOrderCreatedEvent"
//  *   2. Exchange create kiya: "MyERP.Shared.Events:SalesOrderCreatedEvent"
//  *   3. Message bhej diya exchange pe
//  * 
//  * 
//  * STEP B: Consumer side (Production Service)
//  * ─────────────────────────────────────────
//  * public class SalesOrderCreatedConsumer : IConsumer<SalesOrderCreatedEvent>
//  *                                                    ^^^^^^^^^^^^^^^^^^^^^^
//  *                                                    SAME TYPE ZARURI HAI! ⚠️
//  * 
//  * MassTransit internally:
//  *   1. Consumer ka generic type dekha: SalesOrderCreatedEvent
//  *   2. Queue create ki: "sales-order-created" (ya auto-generated name)
//  *   3. Queue ko Exchange se BIND kiya
//  * 
//  * 
//  * VISUAL FLOW:
//  * ═══════════════════════════════════════════════════════════════════════════
//  * 
//  *   SALES SERVICE                    RABBITMQ                    PRODUCTION SERVICE
//  *   ═════════════                    ════════                    ══════════════════
//  *   
//  *   Publish<SalesOrderCreatedEvent>
//  *         │
//  *         │  1. Serialize to JSON
//  *         │  2. Add metadata headers
//  *         ▼
//  *   ┌─────────────────────────────────────────┐
//  *   │ Exchange: MyERP.Shared.Events:          │
//  *   │           SalesOrderCreatedEvent        │
//  *   │ Type: Fanout                            │
//  *   └─────────────────────────────────────────┘
//  *         │
//  *         │  BINDING (auto-created by MassTransit)
//  *         ▼
//  *   ┌─────────────────────────────────────────┐
//  *   │ Queue: sales-order-created              │────▶ SalesOrderCreatedConsumer
//  *   └─────────────────────────────────────────┘            │
//  *                                                          ▼
//  *                                                    Consume(context) called
//  *                                                          │
//  *                                                          ▼
//  *                                                    Business Logic Execute
//  */


// // ============================================================================
// // 📌 STEP 3: ❌ KYU MESSAGE CONSUME NAHI HO RAHA THA
// // ============================================================================
// /*
//  * COMMON MISTAKE #1: Different Namespaces
//  * ─────────────────────────────────────────
//  * 
//  * ❌ PROBLEM (NOOB): Alag-alag event classes
//  * 
//  *   Sales Service:
//  *   namespace MyERP.SalesService.Events
//  *   {
//  *       public class SalesOrderCreatedEvent { ... }
//  *   }
//  *   Exchange created: "MyERP.SalesService.Events:SalesOrderCreatedEvent"
//  *   
//  *   Production Service:
//  *   namespace MyERP.ProductionService.Events  
//  *   {
//  *       public class SalesOrderCreatedEvent { ... }  // Same name, DIFFERENT namespace!
//  *   }
//  *   Listens on: "MyERP.ProductionService.Events:SalesOrderCreatedEvent"
//  *   
//  *   RESULT: ❌ No binding! Publisher aur Consumer ALAG exchanges pe hain!
//  * 
//  * 
//  * ✅ SOLUTION (INDUSTRY): Shared Event Library
//  * 
//  *   MyERP.Shared project me:
//  *   namespace MyERP.Shared.Events
//  *   {
//  *       public class SalesOrderCreatedEvent { ... }
//  *   }
//  *   
//  *   Sales Service: using MyERP.Shared.Events; (publish)
//  *   Production Service: using MyERP.Shared.Events; (consume)
//  *   
//  *   BOTH use SAME fully qualified type = SAME exchange = ✅ WORKS!
//  * 
//  * 
//  * COMMON MISTAKE #2: ConfigureEndpoints missing
//  * ─────────────────────────────────────────
//  * 
//  * ❌ PROBLEM:
//  *   x.AddConsumer<SalesOrderCreatedConsumer>();  // Consumer registered
//  *   x.UsingRabbitMq((context, cfg) => {          
//  *       cfg.Host(...);
//  *       // ConfigureEndpoints() NOT called! 
//  *       // Queue create nahi hui, binding nahi hua
//  *   });
//  * 
//  * ✅ SOLUTION:
//  *   cfg.ConfigureEndpoints(context);  // Ye zaruri hai!
//  *   
//  *   Ye AUTO-create karta hai:
//  *   - Queue: "sales-order-created-consumer" (consumer name se)
//  *   - Exchange binding to message type exchange
//  * 
//  * 
//  * COMMON MISTAKE #3: RabbitMQ connection different
//  * ─────────────────────────────────────────
//  * 
//  * ❌ PROBLEM:
//  *   Sales appsettings: RabbitMQ:HostName = "localhost"
//  *   Production appsettings: RabbitMQ:HostName = "rabbitmq"  // Docker container name
//  *   
//  *   RESULT: Different RabbitMQ instances! Message alag server pe gaya!
//  * 
//  * ✅ SOLUTION:
//  *   Check both services connect to SAME RabbitMQ:
//  *   - Locally: "localhost" 
//  *   - Docker: "rabbitmq" (container name)
//  */


// // ============================================================================
// // 📌 STEP 4: CORRECT PROGRAM.CS SETUP
// // ============================================================================
// /*
//  * Production Service me properly configure kaise karna hai:
//  */

// // In Program.cs:
// /*
// using MassTransit;
// using MyERP.Services.Production.Events.Consumers;

// builder.Services.AddMassTransit(x =>
// {
//     // ═══════════════════════════════════════════════════════════════════
//     // STEP 1: REGISTER CONSUMERS
//     // ═══════════════════════════════════════════════════════════════════
//     // Ye MassTransit ko batata hai ki kaunse consumer classes hai
    
//     x.AddConsumer<SalesOrderCreatedConsumer>();  // Our consumer
//     x.AddConsumer<StockReservedConsumer>();      // Another consumer
    
//     // ═══════════════════════════════════════════════════════════════════
//     // STEP 2: CONFIGURE RABBITMQ
//     // ═══════════════════════════════════════════════════════════════════
    
//     var rabbitHost = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
    
//     x.UsingRabbitMq((context, cfg) =>
//     {
//         // Connect to RabbitMQ
//         cfg.Host(rabbitHost, "/", h =>
//         {
//             h.Username(builder.Configuration["RabbitMQ:UserName"] ?? "guest");
//             h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
//         });
        
//         // ═══════════════════════════════════════════════════════════════
//         // STEP 3: EXPLICIT ENDPOINT WITH BINDING (RECOMMENDED)
//         // ═══════════════════════════════════════════════════════════════
//         // Explicit binding ensures queue connects to correct exchange
        
//         cfg.ReceiveEndpoint("sales-order-created", e =>
//         {
//             // Configure consumer for this queue
//             e.ConfigureConsumer<SalesOrderCreatedConsumer>(context);
            
//             // 🔑 KEY: Bind to the message type exchange
//             // This creates the binding between queue and exchange
//             e.Bind<MyERP.Shared.Events.SalesOrderCreatedEvent>();
//         });
        
//         // ═══════════════════════════════════════════════════════════════
//         // STEP 4: AUTO-CONFIGURE REMAINING CONSUMERS
//         // ═══════════════════════════════════════════════════════════════
//         // For consumers not explicitly configured above
        
//         cfg.ConfigureEndpoints(context);
//     });
// });
// */


// // ============================================================================
// // 📌 STEP 5: CONSUMER CLASS ANATOMY
// // ============================================================================

// using MassTransit;
// using Microsoft.EntityFrameworkCore;
// using MyERP.Services.Production.Data;
// using MyERP.Shared.Events;  // ⚠️ CRITICAL: Shared Events use karo!
// using MyERP.Services.Production.Models;

// namespace MyERP.Services.Production.Events.Tutorial
// {
//     /*
//      * CONSUMER CLASS STRUCTURE:
//      * 
//      * 1. IConsumer<TEvent> interface implement karo
//      *    - TEvent = jo event consume karna hai (from Shared library)
//      * 
//      * 2. Consume(ConsumeContext<TEvent>) method implement karo
//      *    - context.Message = actual event data
//      *    - context.MessageId = unique message ID
//      *    - context.Headers = metadata headers
//      * 
//      * 3. Dependencies inject karo constructor me
//      *    - DbContext, Logger, Services etc.
//      */
    
//     public class TutorialConsumer : IConsumer<SalesOrderCreatedEvent>
//     {
//         private readonly ProductionDbContext _context;
//         private readonly ILogger<TutorialConsumer> _logger;

//         // ═══════════════════════════════════════════════════════════════
//         // CONSTRUCTOR INJECTION
//         // ═══════════════════════════════════════════════════════════════
//         // MassTransit uses DI container to create consumer instances
//         // So you can inject any registered service
        
//         public TutorialConsumer(
//             ProductionDbContext context,
//             ILogger<TutorialConsumer> logger)
//         {
//             _context = context;
//             _logger = logger;
//         }

//         // ═══════════════════════════════════════════════════════════════
//         // CONSUME METHOD - MAIN LOGIC
//         // ═══════════════════════════════════════════════════════════════
        
//         public async Task Consume(ConsumeContext<SalesOrderCreatedEvent> context)
//         {
//             // 🔹 Get the event data
//             var @event = context.Message;
            
//             _logger.LogInformation(
//                 "📦 Received SalesOrderCreatedEvent: " +
//                 "OrderNumber={OrderNumber}, EventId={EventId}",
//                 @event.OrderNumber,
//                 @event.EventId);

//             // ═══════════════════════════════════════════════════════════
//             // PATTERN 1: IDEMPOTENCY CHECK (Industry Practice)
//             // ═══════════════════════════════════════════════════════════
//             /*
//              * 🔑 WHY: Same message can arrive multiple times because:
//              *    - Network retry
//              *    - Consumer crash before ACK
//              *    - RabbitMQ redelivery
//              * 
//              * 🔑 HOW: Check if we already processed this EventId
//              */
            
//             var alreadyProcessed = await _context.PendingRequests
//                 .AnyAsync(r => r.EventId == @event.EventId);
                
//             if (alreadyProcessed)
//             {
//                 _logger.LogWarning(
//                     "⚠️ Duplicate event detected: EventId={EventId}, skipping",
//                     @event.EventId);
//                 return;  // Already processed, don't process again
//             }

//             // ═══════════════════════════════════════════════════════════
//             // PATTERN 2: INBOX PATTERN (Industry Practice)
//             // ═══════════════════════════════════════════════════════════
//             /*
//              * 🔑 WHY: Don't process immediately, save to DB first
//              *    - If processing fails, message is not lost
//              *    - Can retry later from DB
//              *    - Audit trail maintained
//              * 
//              * 🔑 HOW: Create a "PendingRequest" record
//              */
            
//             var pendingRequest = new PendingRequest
//             {
//                 Id = Guid.NewGuid(),
//                 EventId = @event.EventId,  // For idempotency
//                 SalesOrderId = @event.SalesOrderId,
//                 SalesOrderNumber = @event.OrderNumber,
//                 CustomerId = @event.CustomerId,
//                 CustomerName = @event.CustomerName,
//                 OrderDate = @event.OrderDate,
//                 Status = PendingRequestStatus.Pending,  // Will be processed later
//                 ReceivedAt = DateTime.UtcNow,
//                 Items = @event.Items.Select(i => new PendingRequestItem
//                 {
//                     Id = Guid.NewGuid(),
//                     ProductId = i.ProductId,
//                     ProductCode = i.ProductCode,
//                     ProductName = i.ProductName,
//                     Quantity = i.Quantity,
//                     UnitPrice = i.UnitPrice
//                 }).ToList()
//             };

//             _context.PendingRequests.Add(pendingRequest);
//             await _context.SaveChangesAsync();

//             _logger.LogInformation(
//                 "✅ Created PendingRequest: Id={Id}, OrderNumber={OrderNumber}",
//                 pendingRequest.Id,
//                 pendingRequest.SalesOrderNumber);
            
//             // ═══════════════════════════════════════════════════════════
//             // AUTOMATIC ACKNOWLEDGEMENT
//             // ═══════════════════════════════════════════════════════════
//             /*
//              * 🔑 NOTE: MassTransit auto-ACK karta hai jab Consume() complete hota hai
//              *    - If exception thrown → Message goes to _error queue (DLQ)
//              *    - If successful return → Message acknowledged and removed
//              */
//         }
//     }
// }


// // ============================================================================
// // 📌 STEP 6: DEBUGGING CHECKLIST
// // ============================================================================
// /*
//  * Agar message consume nahi ho raha, ye check karo:
//  * 
//  * ☐ 1. RabbitMQ Management UI check karo (http://localhost:15672)
//  *      - Exchange exists: MyERP.Shared.Events:SalesOrderCreatedEvent ?
//  *      - Queue exists: sales-order-created ?
//  *      - Binding exists between them?
//  *      - Messages in queue? (Ready count > 0?)
//  * 
//  * ☐ 2. Both services SAME event type use kar rahe?
//  *      - Check: using MyERP.Shared.Events; in BOTH services
//  *      - Check: SalesOrderCreatedEvent class is FROM Shared project
//  * 
//  * ☐ 3. Production service running hai?
//  *      - dotnet run check karo
//  *      - Logs me "Connected to RabbitMQ" dikha?
//  * 
//  * ☐ 4. Consumer registered hai Program.cs me?
//  *      - x.AddConsumer<SalesOrderCreatedConsumer>();
//  *      - cfg.ConfigureEndpoints(context); OR explicit ReceiveEndpoint
//  * 
//  * ☐ 5. appsettings.json me RabbitMQ config same hai?
//  *      - HostName: "localhost" (local) ya "rabbitmq" (docker)
//  *      - UserName/Password match karte hai?
//  * 
//  * ☐ 6. Check service logs:
//  *      - "Received SalesOrderCreatedEvent" log dikha?
//  *      - Koi exception/error hai?
//  * 
//  * ☐ 7. Network/Firewall:
//  *      - Port 5672 accessible hai?
//  *      - Docker network configuration sahi hai?
//  */


// // ============================================================================
// // 📌 STEP 7: RABBITMQ MANAGEMENT UI SE VERIFY KARO
// // ============================================================================
// /*
//  * Open: http://localhost:15672 (guest/guest)
//  * 
//  * EXCHANGES TAB:
//  * ─────────────
//  * Dekho ye exchanges exist karte hai:
//  * - MyERP.Shared.Events:SalesOrderCreatedEvent (Type: fanout)
//  * 
//  * QUEUES TAB:
//  * ─────────────
//  * Dekho ye queues exist karti hai:
//  * - sales-order-created
//  *   - Ready: 0 (messages waiting)
//  *   - Unacked: 0 (being processed)
//  *   - Total: 0
//  * 
//  * QUEUE BINDINGS (click on queue):
//  * ──────────────────────────────────
//  * Dekho binding hai:
//  *   From Exchange: MyERP.Shared.Events:SalesOrderCreatedEvent
//  *   Routing Key: (empty for fanout)
//  * 
//  * 
//  * AGAR BINDING NAHI HAI:
//  * ─────────────────────
//  * 1. Production service restart karo
//  * 2. Check Program.cs has ConfigureEndpoints() or explicit Bind<>()
//  * 3. Delete queue and let MassTransit recreate it
//  */


// // ============================================================================
// // 📌 STEP 8: COMPLETE FLOW EXAMPLE
// // ============================================================================
// /*
//  * SCENARIO: Sales me order create hua, Production me consume hona chahiye
//  * 
//  * 
//  * SALES SERVICE (Publisher):
//  * ─────────────────────────────
//  * 
//  * File: SalesOrderService.cs
//  * 
//  *   public async Task<SalesOrder> CreateOrderAsync(CreateOrderDto dto)
//  *   {
//  *       // 1. Create order in DB
//  *       var order = new SalesOrder { ... };
//  *       await _context.SaveChangesAsync();
//  *       
//  *       // 2. Publish event
//  *       var @event = new SalesOrderCreatedEvent
//  *       {
//  *           EventId = Guid.NewGuid(),
//  *           SalesOrderId = order.Id,
//  *           OrderNumber = order.OrderNumber,
//  *           CustomerId = order.CustomerId,
//  *           CustomerName = order.CustomerName,
//  *           OrderDate = order.OrderDate,
//  *           Items = order.Items.Select(i => new SalesOrderItemEvent
//  *           {
//  *               ProductId = i.ProductId,
//  *               ProductCode = i.ProductCode,
//  *               ProductName = i.ProductName,
//  *               Quantity = i.Quantity,
//  *               UnitPrice = i.UnitPrice
//  *           }).ToList()
//  *       };
//  *       
//  *       await _publishEndpoint.Publish(@event);
//  *       
//  *       _logger.LogInformation("Published SalesOrderCreatedEvent: {OrderNumber}", order.OrderNumber);
//  *       
//  *       return order;
//  *   }
//  * 
//  * 
//  * PRODUCTION SERVICE (Consumer):
//  * ─────────────────────────────────
//  * 
//  * File: SalesOrderCreatedConsumer.cs (already shown above)
//  *   
//  *   - Receive event
//  *   - Check idempotency
//  *   - Create PendingRequest
//  *   - Save to DB
//  *   - Log success
//  * 
//  * 
//  * RESULT:
//  * ─────────────────────────────────
//  * 
//  * Sales logs:
//  *   ✅ Published SalesOrderCreatedEvent: SO-2026-0001
//  * 
//  * Production logs:
//  *   📦 Received SalesOrderCreatedEvent: OrderNumber=SO-2026-0001, EventId=abc-123
//  *   ✅ Created PendingRequest: Id=xyz-456, OrderNumber=SO-2026-0001
//  * 
//  * Database:
//  *   Production.PendingRequests table me naya record
//  */


// // ============================================================================
// // 📌 SUMMARY - KEY POINTS
// // ============================================================================
// /*
//  * ✅ CRITICAL RULES:
//  * 
//  * 1. SAME EVENT TYPE: Publisher aur Consumer SAME class use karein
//  *    → MyERP.Shared.Events.SalesOrderCreatedEvent (from Shared library)
//  * 
//  * 2. REGISTER CONSUMER: Program.cs me consumer register karo
//  *    → x.AddConsumer<SalesOrderCreatedConsumer>();
//  * 
//  * 3. CONFIGURE ENDPOINTS: Queue create hone dena hai
//  *    → cfg.ConfigureEndpoints(context); 
//  *    → OR explicit: cfg.ReceiveEndpoint("queue-name", e => { ... });
//  * 
//  * 4. SAME RABBITMQ: Both services same RabbitMQ server se connect hona chahiye
//  *    → Check appsettings.json: RabbitMQ:HostName
//  * 
//  * 5. BINDING CHECK: RabbitMQ UI me verify karo queue is bound to exchange
//  *    → http://localhost:15672 → Queues → Click queue → Check Bindings
//  * 
//  * 
//  * 🔧 DEBUGGING QUICK TIPS:
//  * 
//  * - Message queue me hai but consume nahi ho raha?
//  *   → Consumer service restart karo
//  *   → Check consumer is registered in Program.cs
//  *   
//  * - Message publish hua but queue me nahi?
//  *   → Check binding exists in RabbitMQ UI
//  *   → Event types match karte hai ensure karo
//  *   
//  * - Error queue (_error) me ja rha hai?
//  *   → Consumer me exception ho raha hai
//  *   → Check logs for error details
//  */


// // ============================================================================
// // 🔴 REAL CASE STUDY: HAMARA ACTUAL PROBLEM (Feb 2026)
// // ============================================================================
// /*
//  * PROBLEM: Sales order publish ho raha tha but Production consume nahi kar raha tha.
//  *          Messages "_skipped" queue me ja rahe the!
//  * 
//  * 
//  * 📌 STEP 1: RABBITMQ UI ME DEKHA - EXCHANGES
//  * ═══════════════════════════════════════════════════════════════════════════
//  * 
//  * http://localhost:15672 → Exchanges tab me ye exchanges the:
//  * 
//  *   ┌──────────────────────────────────────────────────────────────────────┐
//  *   │ Exchange Name                                           │ Type     │
//  *   ├──────────────────────────────────────────────────────────┼──────────┤
//  *   │ MyERP.Shared.Events:SalesOrderCreatedEvent              │ fanout   │ ← ✅ CORRECT (Sales publishes here)
//  *   │ MyERP.Services.Production.Events:SalesOrderCreatedEvent │ fanout   │ ← ❌ PROBLEM! Why 2 exchanges?
//  *   └──────────────────────────────────────────────────────────┴──────────┘
//  * 
//  * 🔴 CLUE: Do alag exchanges the same event ke liye!
//  *          Matlab do different SalesOrderCreatedEvent class thi!
//  * 
//  * 
//  * 📌 STEP 2: GREP SEARCH - DUPLICATE CLASSES DHUNDHE
//  * ═══════════════════════════════════════════════════════════════════════════
//  * 
//  * Command: grep -r "class SalesOrderCreatedEvent" --include="*.cs"
//  * 
//  * Results:
//  *   ✅ src/MyERP.Shared/Events/SharedEvents.cs:25        → namespace MyERP.Shared.Events
//  *   ❌ src/MyERP.Services.Production/Events/ProductionEvents.cs:134  → namespace MyERP.Services.Production.Events
//  *   
//  * 🔴 FOUND THE BUG! Production me apni alag SalesOrderCreatedEvent class thi!
//  * 
//  * 
//  * 📌 STEP 3: KYA HO RAHA THA (Root Cause)
//  * ═══════════════════════════════════════════════════════════════════════════
//  * 
//  *   SALES SERVICE:
//  *   ──────────────
//  *   using MyERP.Shared.Events;  // Uses shared event
//  *   
//  *   _publisher.Publish(new SalesOrderCreatedEvent());
//  *          │
//  *          │  MassTransit creates exchange based on TYPE:
//  *          │  "MyERP.Shared.Events:SalesOrderCreatedEvent"
//  *          ▼
//  *   ┌────────────────────────────────────────────────────┐
//  *   │ Exchange: MyERP.Shared.Events:SalesOrderCreated   │
//  *   │           Event                                    │
//  *   │                              │                     │
//  *   │              NO BINDING! ❌  │                     │
//  *   │                              ▼                     │
//  *   └────────────────────────────────────────────────────┘
//  *   
//  *   
//  *   PRODUCTION SERVICE:
//  *   ───────────────────
//  *   // ProductionEvents.cs me DUPLICATE class thi:
//  *   namespace MyERP.Services.Production.Events
//  *   {
//  *       public class SalesOrderCreatedEvent { ... }  // ❌ WRONG!
//  *   }
//  *   
//  *   // Consumer is class ko use kar raha tha (accidentally):
//  *   public class SalesOrderCreatedConsumer : IConsumer<SalesOrderCreatedEvent>
//  *          │
//  *          │  MassTransit binds to DIFFERENT exchange:
//  *          │  "MyERP.Services.Production.Events:SalesOrderCreatedEvent"
//  *          ▼
//  *   ┌────────────────────────────────────────────────────┐
//  *   │ Exchange: MyERP.Services.Production.Events:       │
//  *   │           SalesOrderCreatedEvent                  │
//  *   │                              │                     │
//  *   │              BINDING ✅      │                     │
//  *   │                              ▼                     │
//  *   │ Queue: sales-order-created                        │ ← Consumer listens here
//  *   └────────────────────────────────────────────────────┘
//  *   
//  *   
//  *   RESULT:
//  *   ═══════
//  *   - Sales publishes to Exchange A
//  *   - Production listens on Exchange B
//  *   - NO CONNECTION! Messages go to _skipped queue!
//  * 
//  * 
//  * 📌 STEP 4: FIX KIYA
//  * ═══════════════════════════════════════════════════════════════════════════
//  * 
//  *   1. DELETE kiya duplicate class from ProductionEvents.cs:
//  *      
//  *      // ❌ REMOVED this entire class
//  *      // namespace MyERP.Services.Production.Events
//  *      // {
//  *      //     public class SalesOrderCreatedEvent { ... }
//  *      // }
//  *      
//  *   2. ENSURE kiya consumer uses Shared event:
//  *      
//  *      // SalesOrderCreatedConsumer.cs
//  *      using MyERP.Shared.Events;  // ✅ Uses shared event
//  *      
//  *      public class SalesOrderCreatedConsumer : IConsumer<SalesOrderCreatedEvent>
//  *      //                                                  ^^^^^^^^^^^^^^^^^
//  *      //                                     Now points to MyERP.Shared.Events.SalesOrderCreatedEvent
//  *      
//  *   3. DELETE kiya old queues from RabbitMQ (fresh start)
//  *   
//  *   4. RESTART kiya Production service
//  *   
//  *   
//  *   AFTER FIX:
//  *   ══════════
//  *   
//  *   SALES SERVICE:
//  *   ──────────────
//  *   using MyERP.Shared.Events;
//  *         │
//  *         ▼
//  *   ┌────────────────────────────────────────────────────┐
//  *   │ Exchange: MyERP.Shared.Events:SalesOrderCreated   │
//  *   │           Event                                    │
//  *   │                              │                     │
//  *   │              BINDING ✅      │                     │  ← NOW CONNECTED!
//  *   │                              ▼                     │
//  *   │ Queue: sales-order-created                        │
//  *   └────────────────────────────────────────────────────┘
//  *         │
//  *         ▼
//  *   PRODUCTION SERVICE:
//  *   ───────────────────
//  *   using MyERP.Shared.Events;  // SAME namespace!
//  *         │
//  *         ▼
//  *   SalesOrderCreatedConsumer receives message ✅
//  * 
//  * 
//  * 📌 STEP 5: SUCCESS LOGS
//  * ═══════════════════════════════════════════════════════════════════════════
//  * 
//  *   Sales Service:
//  *   ─────────────
//  *   🔵 [DEBUG 1] Sales Service - Creating SalesOrderCreatedEvent...
//  *      Order ID: 7e087c64-be50-49ce-9601-bfa8ffcaa3cb
//  *      Order Number: SO-2026-0016
//  *   🔵 [DEBUG 2] Event created with EventId: f7c7ff37-4068-4110-9b6f-8388d7f5cdbc
//  *   🔵 [DEBUG 3] Calling _eventPublisher.PublishAsync()...
//  *   🟡 [DEBUG 5] MassTransitEventPublisher.PublishAsync() called
//  *      Event Type: SalesOrderCreatedEvent
//  *      Event Full Name: MyERP.Shared.Events.SalesOrderCreatedEvent  ← ✅ CORRECT!
//  *   🟢 [DEBUG 7] _publishEndpoint.Publish() completed!
//  *   
//  *   
//  *   Production Service:
//  *   ──────────────────
//  *   🟣 [DEBUG 8] Production Consumer - Consume() method called!
//  *      Message ID: dc780000-2286-5811-9025-08de66f6fa3d
//  *   🟣 [DEBUG 9] Event received:
//  *      EventId: 2d41a889-2f96-44d5-a959-f739a0519aad
//  *      OrderNumber: SO-2026-0019
//  *      CustomerName: Test Company Ltd
//  *   🟣 [DEBUG 10] Checking idempotency...
//  *   🟣 [DEBUG 11] Creating PendingRequest...
//  *   🟣 [DEBUG 12] Saving to database...
//  *   🟢 [DEBUG 13] PendingRequest saved successfully!
//  *      PendingRequest ID: f0373d3b-5bae-4df3-8a69-a9fe67baae40
//  */


// // ============================================================================
// // 🔧 DEBUGGING GUIDE: STEP-BY-STEP
// // ============================================================================
// /*
//  * Jab bhi MassTransit issue ho, ye steps follow karo:
//  * 
//  * 
//  * 📌 STEP 1: ADD DEBUG LOGGING
//  * ═══════════════════════════════════════════════════════════════════════════
//  * 
//  * Publisher (Sales Service) me:
//  * 
//  *   public async Task PublishAsync<T>(T @event) where T : class
//  *   {
//  *       Console.WriteLine($"🟡 Publishing {typeof(T).Name}");
//  *       Console.WriteLine($"   Full Type: {typeof(T).FullName}");  // ← KEY!
//  *       
//  *       await _publishEndpoint.Publish(@event);
//  *       
//  *       Console.WriteLine($"🟢 Published successfully!");
//  *   }
//  * 
//  * Consumer (Production Service) me:
//  * 
//  *   public async Task Consume(ConsumeContext<SalesOrderCreatedEvent> context)
//  *   {
//  *       Console.WriteLine($"🟣 Consumer called!");
//  *       Console.WriteLine($"   Message ID: {context.MessageId}");
//  *       Console.WriteLine($"   Event: {context.Message.OrderNumber}");
//  *       
//  *       // ... rest of code
//  *   }
//  * 
//  * 
//  * 📌 STEP 2: CHECK RABBITMQ MANAGEMENT UI
//  * ═══════════════════════════════════════════════════════════════════════════
//  * 
//  * URL: http://localhost:15672 (guest/guest)
//  * 
//  * 2A. EXCHANGES TAB:
//  *     Dekho ye exchanges exist karte hai:
//  *     
//  *     ✅ GOOD: Sirf ek exchange with your event namespace
//  *        "MyERP.Shared.Events:SalesOrderCreatedEvent"
//  *     
//  *     ❌ BAD: Multiple exchanges with same event name but different namespaces!
//  *        "MyERP.Shared.Events:SalesOrderCreatedEvent" 
//  *        "MyERP.Services.Production.Events:SalesOrderCreatedEvent"  ← DUPLICATE!
//  *     
//  * 
//  * 2B. QUEUES TAB:
//  *     Dekho ye queues exist karti hai:
//  *     
//  *     ✅ GOOD: Your queue with 0 messages (being consumed)
//  *        "sales-order-created" - Ready: 0, Unacked: 0
//  *     
//  *     ❌ BAD: Messages in _skipped queue!
//  *        "sales-order-created_skipped" - Ready: 5  ← PROBLEM!
//  *     
//  * 
//  * 2C. QUEUE BINDINGS:
//  *     Click on your queue → Bindings section
//  *     
//  *     ✅ GOOD: Bound to correct exchange
//  *        From: MyERP.Shared.Events:SalesOrderCreatedEvent
//  *        
//  *     ❌ BAD: No binding or bound to wrong exchange
//  * 
//  * 
//  * 📌 STEP 3: GREP FOR DUPLICATE CLASSES
//  * ═══════════════════════════════════════════════════════════════════════════
//  * 
//  * Run in terminal:
//  * 
//  *   grep -r "class SalesOrderCreatedEvent" --include="*.cs"
//  *   
//  * Expected: Only ONE result in Shared project
//  *   src/MyERP.Shared/Events/SharedEvents.cs
//  *   
//  * Problem: Multiple results in different projects
//  *   src/MyERP.Shared/Events/SharedEvents.cs
//  *   src/MyERP.Services.Production/Events/ProductionEvents.cs  ← DELETE!
//  *   src/MyERP.Services.Sales/Events/SalesEvents.cs  ← DELETE!
//  * 
//  * 
//  * 📌 STEP 4: VERIFY USING STATEMENTS
//  * ═══════════════════════════════════════════════════════════════════════════
//  * 
//  * In ALL files that use the event, check:
//  * 
//  *   using MyERP.Shared.Events;  // ✅ CORRECT
//  *   
//  * NOT:
//  *   using MyERP.Services.Production.Events;  // ❌ WRONG
//  *   using MyERP.Services.Sales.Events;  // ❌ WRONG
//  * 
//  * 
//  * 📌 STEP 5: DELETE OLD QUEUES AND RESTART
//  * ═══════════════════════════════════════════════════════════════════════════
//  * 
//  * After fixing code:
//  * 
//  *   1. Go to RabbitMQ UI → Queues tab
//  *   2. Delete these queues (click queue → Delete Queue):
//  *      - sales-order-created
//  *      - sales-order-created_skipped
//  *      - sales-order-created_error
//  *      - Any other related queues
//  *      
//  *   3. Restart BOTH services:
//  *      
//  *      # Terminal 1: Sales
//  *      cd src/MyERP.Services.Sales
//  *      dotnet run
//  *      
//  *      # Terminal 2: Production
//  *      cd src/MyERP.Services.Production
//  *      dotnet run
//  *      
//  *   4. Test again - create a sales order and check if consumed
//  * 
//  * 
//  * 📌 STEP 6: CHECK PROGRAM.CS CONFIGURATION
//  * ═══════════════════════════════════════════════════════════════════════════
//  * 
//  * Production Service Program.cs:
//  * 
//  *   builder.Services.AddMassTransit(x =>
//  *   {
//  *       // ✅ Register consumer with explicit queue name
//  *       x.AddConsumer<SalesOrderCreatedConsumer>()
//  *           .Endpoint(e => e.Name = "sales-order-created");
//  *       
//  *       x.UsingRabbitMq((context, cfg) =>
//  *       {
//  *           cfg.Host("localhost", ...);
//  *           
//  *           // ✅ This creates queues and bindings
//  *           cfg.ConfigureEndpoints(context);
//  *       });
//  *   });
//  */


// // ============================================================================
// // 📌 QUICK REFERENCE: COMMON ISSUES & FIXES
// // ============================================================================
// /*
//  * ┌────────────────────────────────────────────────────────────────────────────┐
//  * │ SYMPTOM                          │ CAUSE                    │ FIX         │
//  * ├────────────────────────────────────────────────────────────────────────────┤
//  * │ Messages in _skipped queue       │ Duplicate event class    │ Use Shared  │
//  * │                                  │ (different namespaces)   │ event only  │
//  * ├────────────────────────────────────────────────────────────────────────────┤
//  * │ Messages in _error queue         │ Consumer throws          │ Fix         │
//  * │                                  │ exception                │ exception   │
//  * ├────────────────────────────────────────────────────────────────────────────┤
//  * │ No messages anywhere             │ Wrong RabbitMQ host      │ Check       │
//  * │                                  │ or not connected         │ appsettings │
//  * ├────────────────────────────────────────────────────────────────────────────┤
//  * │ Queue exists but no binding      │ Missing                  │ Add cfg.    │
//  * │                                  │ ConfigureEndpoints       │ Configure   │
//  * │                                  │                          │ Endpoints() │
//  * ├────────────────────────────────────────────────────────────────────────────┤
//  * │ Multiple exchanges same event    │ Different namespaces     │ Use only    │
//  * │                                  │                          │ Shared      │
//  * └────────────────────────────────────────────────────────────────────────────┘
//  * 
//  * 
//  * 🎯 GOLDEN RULE:
//  * ══════════════
//  * 
//  *   Event classes MUST be in a SHARED library
//  *   Both Publisher and Consumer MUST reference the SAME class
//  *   
//  *   typeof(T).FullName on Publisher == typeof(T).FullName on Consumer
//  *   
//  *   If these don't match, messages will NEVER be delivered!
//  */

