using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Apollo.Database.Migrations
{
  /// <inheritdoc />
  public partial class RemoveSettingsTimestamps : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "CreatedAt",
          table: "Settings");

      migrationBuilder.DropColumn(
          name: "UpdatedAt",
          table: "Settings");

      migrationBuilder.AlterColumn<string>(
          name: "Value",
          table: "Settings",
          type: "text",
          nullable: false,
          oldClrType: typeof(string),
          oldType: "character varying(500)",
          oldMaxLength: 500);

      migrationBuilder.AlterColumn<string>(
          name: "Key",
          table: "Settings",
          type: "text",
          nullable: false,
          oldClrType: typeof(string),
          oldType: "character varying(100)",
          oldMaxLength: 100);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<string>(
          name: "Value",
          table: "Settings",
          type: "character varying(500)",
          maxLength: 500,
          nullable: false,
          oldClrType: typeof(string),
          oldType: "text");

      migrationBuilder.AlterColumn<string>(
          name: "Key",
          table: "Settings",
          type: "character varying(100)",
          maxLength: 100,
          nullable: false,
          oldClrType: typeof(string),
          oldType: "text");

      migrationBuilder.AddColumn<DateTime>(
          name: "CreatedAt",
          table: "Settings",
          type: "timestamp with time zone",
          nullable: false,
          defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

      migrationBuilder.AddColumn<DateTime>(
          name: "UpdatedAt",
          table: "Settings",
          type: "timestamp with time zone",
          nullable: false,
          defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
    }
  }
}
