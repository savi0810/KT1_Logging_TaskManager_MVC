using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KT1_Logging_TaskManager_MVC.Migrations
{
    /// <inheritdoc />
    public partial class AditionalFieldsForTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WhenOverdue",
                table: "OverdueTasks",
                newName: "WhenOverdueDate");

            migrationBuilder.RenameColumn(
                name: "TaskDeascription",
                table: "OverdueTasks",
                newName: "TaskDescription");

            migrationBuilder.RenameColumn(
                name: "WhenWasDeleted",
                table: "DeletedTasks",
                newName: "TaskDescription");

            migrationBuilder.RenameColumn(
                name: "TaskDeascription",
                table: "DeletedTasks",
                newName: "DeletedDate");

            migrationBuilder.RenameColumn(
                name: "WhenWasCompleted",
                table: "CompletedTasks",
                newName: "TaskDescription");

            migrationBuilder.RenameColumn(
                name: "TaskDeascription",
                table: "CompletedTasks",
                newName: "CompletedDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WhenOverdueDate",
                table: "OverdueTasks",
                newName: "WhenOverdue");

            migrationBuilder.RenameColumn(
                name: "TaskDescription",
                table: "OverdueTasks",
                newName: "TaskDeascription");

            migrationBuilder.RenameColumn(
                name: "TaskDescription",
                table: "DeletedTasks",
                newName: "WhenWasDeleted");

            migrationBuilder.RenameColumn(
                name: "DeletedDate",
                table: "DeletedTasks",
                newName: "TaskDeascription");

            migrationBuilder.RenameColumn(
                name: "TaskDescription",
                table: "CompletedTasks",
                newName: "WhenWasCompleted");

            migrationBuilder.RenameColumn(
                name: "CompletedDate",
                table: "CompletedTasks",
                newName: "TaskDeascription");
        }
    }
}
