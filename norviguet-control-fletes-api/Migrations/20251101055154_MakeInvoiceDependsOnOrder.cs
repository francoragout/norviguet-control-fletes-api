using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace norviguet_control_fletes_api.Migrations
{
    /// <inheritdoc />
    public partial class MakeInvoiceDependsOnOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentOrders_InvoiceId",
                table: "PaymentOrders");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_InvoiceId",
                table: "PaymentOrders",
                column: "InvoiceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentOrders_InvoiceId",
                table: "PaymentOrders");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_InvoiceId",
                table: "PaymentOrders",
                column: "InvoiceId");
        }
    }
}
