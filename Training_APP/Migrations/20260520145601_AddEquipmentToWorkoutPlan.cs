using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Training_APP.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentToWorkoutPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Equipment",
                table: "WorkoutPlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Equipment",
                table: "WorkoutPlans");
        }
    }
}
