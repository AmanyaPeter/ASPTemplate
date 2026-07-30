using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedFormFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalComments",
                table: "InnovationIdeas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DeclarationAccepted",
                table: "InnovationIdeas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EstimatedBudgetRange",
                table: "InnovationIdeas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpectedTimeline",
                table: "InnovationIdeas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InnovationPriorityArea",
                table: "InnovationIdeas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "InnovationIdeas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StrategicAlignment",
                table: "InnovationIdeas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeOfInnovation",
                table: "InnovationIdeas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalComments",
                table: "InnovationIdeas");

            migrationBuilder.DropColumn(
                name: "DeclarationAccepted",
                table: "InnovationIdeas");

            migrationBuilder.DropColumn(
                name: "EstimatedBudgetRange",
                table: "InnovationIdeas");

            migrationBuilder.DropColumn(
                name: "ExpectedTimeline",
                table: "InnovationIdeas");

            migrationBuilder.DropColumn(
                name: "InnovationPriorityArea",
                table: "InnovationIdeas");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "InnovationIdeas");

            migrationBuilder.DropColumn(
                name: "StrategicAlignment",
                table: "InnovationIdeas");

            migrationBuilder.DropColumn(
                name: "TypeOfInnovation",
                table: "InnovationIdeas");
        }
    }
}
