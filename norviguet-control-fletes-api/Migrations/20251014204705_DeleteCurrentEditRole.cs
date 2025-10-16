using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace norviguet_control_fletes_api.Migrations
{
    /// <inheritdoc />
    public partial class DeleteCurrentEditRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentEditRole",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentEditRole",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
