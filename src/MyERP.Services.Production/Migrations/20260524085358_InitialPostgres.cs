using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyERP.Services.Production.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BOMs",
                columns: table => new
                {
                    BOMId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    BomCode = table.Column<string>(type: "text", nullable: false),
                    ProductName = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BOMs", x => x.BOMId);
                });

            migrationBuilder.CreateTable(
                name: "PendingRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesOrderNumber = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerName = table.Column<string>(type: "text", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CancellationReason = table.Column<string>(type: "text", nullable: true),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Processes",
                columns: table => new
                {
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessCode = table.Column<string>(type: "text", nullable: false),
                    ProcessName = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    StandardTimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Processes", x => x.ProcessId);
                });

            migrationBuilder.CreateTable(
                name: "WorkCenters",
                columns: table => new
                {
                    WorkCenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    CenterCode = table.Column<string>(type: "text", nullable: false),
                    CenterName = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    CostPerHour = table.Column<decimal>(type: "numeric", nullable: false),
                    CapacityPerHour = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkCenters", x => x.WorkCenterId);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "text", nullable: false),
                    SalesOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalesOrderNumber = table.Column<string>(type: "text", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductCode = table.Column<string>(type: "text", nullable: false),
                    ProductName = table.Column<string>(type: "text", nullable: false),
                    BOMId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkCenterName = table.Column<string>(type: "text", nullable: true),
                    QuantityPlanned = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityGood = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityScrap = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PlannedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BomCode = table.Column<string>(type: "text", nullable: true),
                    BomVersion = table.Column<int>(type: "integer", nullable: true),
                    ReservationStatus = table.Column<string>(type: "text", nullable: true),
                    ReservationFailReason = table.Column<string>(type: "text", nullable: true),
                    ReservationAttempts = table.Column<int>(type: "integer", nullable: false),
                    LastReservationAttempt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReleasedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "text", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaterialsReturned = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_BOMs_BOMId",
                        column: x => x.BOMId,
                        principalTable: "BOMs",
                        principalColumn: "BOMId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PendingRequestItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PendingRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductCode = table.Column<string>(type: "text", nullable: false),
                    ProductName = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingRequestItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingRequestItems_PendingRequests_PendingRequestId",
                        column: x => x.PendingRequestId,
                        principalTable: "PendingRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BOMLines",
                columns: table => new
                {
                    BOMLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    BOMId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    RawMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialCode = table.Column<string>(type: "text", nullable: false),
                    MaterialName = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    ScrapPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcessCode = table.Column<string>(type: "text", nullable: true),
                    ProcessName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BOMLines", x => x.BOMLineId);
                    table.ForeignKey(
                        name: "FK_BOMLines_BOMs_BOMId",
                        column: x => x.BOMId,
                        principalTable: "BOMs",
                        principalColumn: "BOMId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BOMLines_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentCode = table.Column<string>(type: "text", nullable: false),
                    EquipmentName = table.Column<string>(type: "text", nullable: false),
                    WorkCenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Manufacturer = table.Column<string>(type: "text", nullable: true),
                    Model = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CostPerHour = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.EquipmentId);
                    table.ForeignKey(
                        name: "FK_Equipment_WorkCenters_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalTable: "WorkCenters",
                        principalColumn: "WorkCenterId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcessRoutes",
                columns: table => new
                {
                    ProcessRouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteCode = table.Column<string>(type: "text", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkCenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessRoutes", x => x.ProcessRouteId);
                    table.ForeignKey(
                        name: "FK_ProcessRoutes_WorkCenters_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalTable: "WorkCenters",
                        principalColumn: "WorkCenterId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaterialRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductionOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RawMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialCode = table.Column<string>(type: "text", nullable: false),
                    MaterialName = table.Column<string>(type: "text", nullable: false),
                    QuantityRequired = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityReserved = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityConsumed = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialRequirements_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentProcesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentProcesses_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentProcesses_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessRouteSteps",
                columns: table => new
                {
                    ProcessRouteStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessRouteId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepNumber = table.Column<int>(type: "integer", nullable: false),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    SetupTimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    RunTimePerUnitMinutes = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    OutputMultiplier = table.Column<decimal>(type: "numeric", nullable: false),
                    OutputUnit = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessRouteSteps", x => x.ProcessRouteStepId);
                    table.ForeignKey(
                        name: "FK_ProcessRouteSteps_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentId");
                    table.ForeignKey(
                        name: "FK_ProcessRouteSteps_ProcessRoutes_ProcessRouteId",
                        column: x => x.ProcessRouteId,
                        principalTable: "ProcessRoutes",
                        principalColumn: "ProcessRouteId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcessRouteSteps_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "ProcessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessRouteStepMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessRouteStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    BOMLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    RawMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialCode = table.Column<string>(type: "text", nullable: false),
                    MaterialName = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessRouteStepMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessRouteStepMaterials_BOMLines_BOMLineId",
                        column: x => x.BOMLineId,
                        principalTable: "BOMLines",
                        principalColumn: "BOMLineId");
                    table.ForeignKey(
                        name: "FK_ProcessRouteStepMaterials_ProcessRouteSteps_ProcessRouteSte~",
                        column: x => x.ProcessRouteStepId,
                        principalTable: "ProcessRouteSteps",
                        principalColumn: "ProcessRouteStepId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderNumber = table.Column<string>(type: "text", nullable: false),
                    ProductionOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessRouteStepId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcessId = table.Column<Guid>(type: "uuid", nullable: true),
                    StepNumber = table.Column<int>(type: "integer", nullable: false),
                    OperationName = table.Column<string>(type: "text", nullable: false),
                    RouteCode = table.Column<string>(type: "text", nullable: true),
                    RouteVersion = table.Column<int>(type: "integer", nullable: true),
                    ProductCode = table.Column<string>(type: "text", nullable: false),
                    ProductName = table.Column<string>(type: "text", nullable: false),
                    WorkCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuantityPlanned = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ScheduledStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduledEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    QuantityCompleted = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityScrap = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ActualStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReservationStatus = table.Column<string>(type: "text", nullable: true),
                    ReservationFailReason = table.Column<string>(type: "text", nullable: true),
                    ReservationAttempts = table.Column<int>(type: "integer", nullable: false),
                    CancelReason = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.WorkOrderId);
                    table.ForeignKey(
                        name: "FK_WorkOrders_ProcessRouteSteps_ProcessRouteStepId",
                        column: x => x.ProcessRouteStepId,
                        principalTable: "ProcessRouteSteps",
                        principalColumn: "ProcessRouteStepId");
                    table.ForeignKey(
                        name: "FK_WorkOrders_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "ProcessId");
                    table.ForeignKey(
                        name: "FK_WorkOrders_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrders_WorkCenters_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalTable: "WorkCenters",
                        principalColumn: "WorkCenterId");
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderExecutions",
                columns: table => new
                {
                    ExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivatedBy = table.Column<string>(type: "text", nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QuantityProduced = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityScrap = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderExecutions", x => x.ExecutionId);
                    table.ForeignKey(
                        name: "FK_WorkOrderExecutions_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderExecutions_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "WorkOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BOMLines_BOMId",
                table: "BOMLines",
                column: "BOMId");

            migrationBuilder.CreateIndex(
                name: "IX_BOMLines_ProcessId",
                table: "BOMLines",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_BOMs_BomCode",
                table: "BOMs",
                column: "BomCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BOMs_ProductId",
                table: "BOMs",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_BOMs_ProductId_Version",
                table: "BOMs",
                columns: new[] { "ProductId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_EquipmentCode",
                table: "Equipment",
                column: "EquipmentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_WorkCenterId",
                table: "Equipment",
                column: "WorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentProcesses_EquipmentId_ProcessId",
                table: "EquipmentProcesses",
                columns: new[] { "EquipmentId", "ProcessId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentProcesses_ProcessId",
                table: "EquipmentProcesses",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialRequirements_ProductionOrderId",
                table: "MaterialRequirements",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingRequestItems_PendingRequestId",
                table: "PendingRequestItems",
                column: "PendingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingRequests_EventId",
                table: "PendingRequests",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PendingRequests_SalesOrderId",
                table: "PendingRequests",
                column: "SalesOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PendingRequests_Status",
                table: "PendingRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_Category",
                table: "Processes",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_ProcessCode",
                table: "Processes",
                column: "ProcessCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRoutes_ProductId",
                table: "ProcessRoutes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRoutes_RouteCode",
                table: "ProcessRoutes",
                column: "RouteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRoutes_WorkCenterId",
                table: "ProcessRoutes",
                column: "WorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRouteStepMaterials_BOMLineId",
                table: "ProcessRouteStepMaterials",
                column: "BOMLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRouteStepMaterials_ProcessRouteStepId",
                table: "ProcessRouteStepMaterials",
                column: "ProcessRouteStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRouteSteps_EquipmentId",
                table: "ProcessRouteSteps",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRouteSteps_ProcessId",
                table: "ProcessRouteSteps",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRouteSteps_ProcessRouteId",
                table: "ProcessRouteSteps",
                column: "ProcessRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_BOMId",
                table: "ProductionOrders",
                column: "BOMId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_OrderNumber",
                table: "ProductionOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_ReservationStatus",
                table: "ProductionOrders",
                column: "ReservationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_SalesOrderId",
                table: "ProductionOrders",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_Status",
                table: "ProductionOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkCenters_CenterCode",
                table: "WorkCenters",
                column: "CenterCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderExecutions_EquipmentId",
                table: "WorkOrderExecutions",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderExecutions_Status",
                table: "WorkOrderExecutions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderExecutions_WorkOrderId",
                table: "WorkOrderExecutions",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProcessId",
                table: "WorkOrders",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProcessRouteStepId",
                table: "WorkOrders",
                column: "ProcessRouteStepId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProductionOrderId",
                table: "WorkOrders",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_Status",
                table: "WorkOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_WorkCenterId",
                table: "WorkOrders",
                column: "WorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_WorkOrderNumber",
                table: "WorkOrders",
                column: "WorkOrderNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentProcesses");

            migrationBuilder.DropTable(
                name: "MaterialRequirements");

            migrationBuilder.DropTable(
                name: "PendingRequestItems");

            migrationBuilder.DropTable(
                name: "ProcessRouteStepMaterials");

            migrationBuilder.DropTable(
                name: "WorkOrderExecutions");

            migrationBuilder.DropTable(
                name: "PendingRequests");

            migrationBuilder.DropTable(
                name: "BOMLines");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropTable(
                name: "ProcessRouteSteps");

            migrationBuilder.DropTable(
                name: "ProductionOrders");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "ProcessRoutes");

            migrationBuilder.DropTable(
                name: "Processes");

            migrationBuilder.DropTable(
                name: "BOMs");

            migrationBuilder.DropTable(
                name: "WorkCenters");
        }
    }
}
