using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace norviguet_control_fletes_api.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadePaymentOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentOrders_Orders_OrderId",
                table: "PaymentOrders");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentOrders_Orders_OrderId",
                table: "PaymentOrders",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentOrders_Orders_OrderId",
                table: "PaymentOrders");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentOrders_Orders_OrderId",
                table: "PaymentOrders",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
