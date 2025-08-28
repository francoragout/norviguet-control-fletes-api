using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace norviguet_control_fletes_api.Migrations
{
    /// <inheritdoc />
    public partial class AddPOSEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sellers_Name",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PointOfSale",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PointOfSale",
                table: "Invoices");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Sellers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Payments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "POS",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountRate",
                table: "Orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Invoices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "POS",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "POS",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "POS",
                table: "Invoices");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Sellers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PointOfSale",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<float>(
                name: "DiscountRate",
                table: "Orders",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PointOfSale",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sellers_Name",
                table: "Sellers",
                column: "Name",
                unique: true);
        }
    }
}
