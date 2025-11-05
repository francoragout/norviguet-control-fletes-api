using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace norviguet_control_fletes_api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEntitiesRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNotes_Carriers_CarrierId",
                table: "DeliveryNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Carriers_CarrierId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentOrders_Invoices_InvoiceId",
                table: "PaymentOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentOrders_Orders_OrderId",
                table: "PaymentOrders");

            migrationBuilder.DropIndex(
                name: "IX_PaymentOrders_InvoiceId",
                table: "PaymentOrders");

            migrationBuilder.DropIndex(
                name: "IX_PaymentOrders_OrderId",
                table: "PaymentOrders");

            migrationBuilder.RenameColumn(
                name: "InvoiceId",
                table: "PaymentOrders",
                newName: "CarrierId");

            migrationBuilder.AlterColumn<int>(
                name: "SellerId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_CarrierId",
                table: "PaymentOrders",
                column: "CarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_OrderId_CarrierId",
                table: "PaymentOrders",
                columns: new[] { "OrderId", "CarrierId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNotes_Carriers_CarrierId",
                table: "DeliveryNotes",
                column: "CarrierId",
                principalTable: "Carriers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Carriers_CarrierId",
                table: "Invoices",
                column: "CarrierId",
                principalTable: "Carriers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentOrders_Carriers_CarrierId",
                table: "PaymentOrders",
                column: "CarrierId",
                principalTable: "Carriers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentOrders_Orders_OrderId",
                table: "PaymentOrders",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNotes_Carriers_CarrierId",
                table: "DeliveryNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Carriers_CarrierId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentOrders_Carriers_CarrierId",
                table: "PaymentOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentOrders_Orders_OrderId",
                table: "PaymentOrders");

            migrationBuilder.DropIndex(
                name: "IX_PaymentOrders_CarrierId",
                table: "PaymentOrders");

            migrationBuilder.DropIndex(
                name: "IX_PaymentOrders_OrderId_CarrierId",
                table: "PaymentOrders");

            migrationBuilder.RenameColumn(
                name: "CarrierId",
                table: "PaymentOrders",
                newName: "InvoiceId");

            migrationBuilder.AlterColumn<int>(
                name: "SellerId",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_InvoiceId",
                table: "PaymentOrders",
                column: "InvoiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_OrderId",
                table: "PaymentOrders",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNotes_Carriers_CarrierId",
                table: "DeliveryNotes",
                column: "CarrierId",
                principalTable: "Carriers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Carriers_CarrierId",
                table: "Invoices",
                column: "CarrierId",
                principalTable: "Carriers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentOrders_Invoices_InvoiceId",
                table: "PaymentOrders",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentOrders_Orders_OrderId",
                table: "PaymentOrders",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
