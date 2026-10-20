using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MechanicShop.infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppointmentRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CarId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RequestedStartAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RequestedEndAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentRequests", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_AppointmentRequests_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppointmentRequests_Vehicles_CarId",
                        column: x => x.CarId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppointmentRequests_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentRequestRepairTasks",
                columns: table => new
                {
                    AppointmentRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepairTasksId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentRequestRepairTasks", x => new { x.AppointmentRequestId, x.RepairTasksId });
                    table.ForeignKey(
                        name: "FK_AppointmentRequestRepairTasks_AppointmentRequests_AppointmentRequestId",
                        column: x => x.AppointmentRequestId,
                        principalTable: "AppointmentRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppointmentRequestRepairTasks_RepairTasks_RepairTasksId",
                        column: x => x.RepairTasksId,
                        principalTable: "RepairTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentRequestRepairTasks_RepairTasksId",
                table: "AppointmentRequestRepairTasks",
                column: "RepairTasksId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentRequests_CarId",
                table: "AppointmentRequests",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentRequests_CustomerId",
                table: "AppointmentRequests",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentRequests_Status_RequestedStartAtUtc",
                table: "AppointmentRequests",
                columns: new[] { "Status", "RequestedStartAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentRequests_WorkOrderId",
                table: "AppointmentRequests",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentRequestRepairTasks");

            migrationBuilder.DropTable(
                name: "AppointmentRequests");
        }
    }
}
