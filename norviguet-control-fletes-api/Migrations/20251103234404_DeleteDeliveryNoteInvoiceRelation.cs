using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace norviguet_control_fletes_api.Migrations
{
    /// <inheritdoc />
    public partial class DeleteDeliveryNoteInvoiceRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryNoteInvoice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryNoteInvoice",
                columns: table => new
                {
                    DeliveryNotesId = table.Column<int>(type: "int", nullable: false),
                    InvoicesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryNoteInvoice", x => new { x.DeliveryNotesId, x.InvoicesId });
                    table.ForeignKey(
                        name: "FK_DeliveryNoteInvoice_DeliveryNotes_DeliveryNotesId",
                        column: x => x.DeliveryNotesId,
                        principalTable: "DeliveryNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryNoteInvoice_Invoices_InvoicesId",
                        column: x => x.InvoicesId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteInvoice_InvoicesId",
                table: "DeliveryNoteInvoice",
                column: "InvoicesId");
        }
    }
}
