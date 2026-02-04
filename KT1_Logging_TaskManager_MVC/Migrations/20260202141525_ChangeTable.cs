using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KT1_Logging_TaskManager_MVC.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaskDeascription",
                table: "CurrentTasks",
                newName: "TaskDescription");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "CurrentTasks",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "CurrentTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "CurrentTasks",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "CurrentTasks");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "CurrentTasks");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "CurrentTasks");

            migrationBuilder.RenameColumn(
                name: "TaskDescription",
                table: "CurrentTasks",
                newName: "TaskDeascription");
        }
    }
}
