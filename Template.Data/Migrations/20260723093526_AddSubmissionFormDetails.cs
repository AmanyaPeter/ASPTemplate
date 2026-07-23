using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionFormDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExpectedBenefits",
                table: "InnovationIdeas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImpactIndicators",
                table: "InnovationIdeas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImplementationApproach",
                table: "InnovationIdeas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeyEnablers",
                table: "InnovationIdeas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StrategicObjective",
                table: "InnovationIdeas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamCompositionJson",
                table: "InnovationIdeas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamMemberNames",
                table: "InnovationIdeas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedBenefits",
                table: "InnovationIdeas");

            migrationBuilder.DropColumn(
                name: "ImpactIndicators",
                table: "InnovationIdeas");

            migrationBuilder.DropColumn(
                name: "ImplementationApproach",
                table: "InnovationIdeas");

            migrationBuilder.DropColumn(
                name: "KeyEnablers",
                table: "InnovationIdeas");

            migrationBuilder.DropColumn(
                name: "StrategicObjective",
                table: "InnovationIdeas");

            migrationBuilder.DropColumn(
                name: "TeamCompositionJson",
                table: "InnovationIdeas");

            migrationBuilder.DropColumn(
                name: "TeamMemberNames",
                table: "InnovationIdeas");
        }
    }
}
