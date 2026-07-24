using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Data.Migrations
{
    /// <inheritdoc />
    public partial class CompleteAndHardenImts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Roles_RoleId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_RoleId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Resources",
                newName: "StorageName");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "IdeaAttachments",
                newName: "StorageName");

            // Convert the legacy text values to stable enum ordinals before changing column types.
            migrationBuilder.Sql("""
                UPDATE InnovationIdeas SET CurrentStage = CASE REPLACE(CurrentStage, ' ', '')
                    WHEN 'Submitted' THEN '0' WHEN 'ConceptDevelopment' THEN '1'
                    WHEN 'Experimentation' THEN '2' WHEN 'Experimentation/Research' THEN '2'
                    WHEN 'Deployment' THEN '3' WHEN 'Closed' THEN '4' ELSE '0' END;
                UPDATE InnovationIdeas SET CurrentStatus = CASE REPLACE(CurrentStatus, ' ', '')
                    WHEN 'PendingInformation' THEN '0' WHEN 'UnderReview' THEN '1'
                    WHEN 'Approved' THEN '2' WHEN 'Declined' THEN '3' ELSE '1' END;
                UPDATE StageHistories SET
                    PreviousStage = CASE REPLACE(PreviousStage, ' ', '') WHEN 'Submitted' THEN '0' WHEN 'ConceptDevelopment' THEN '1' WHEN 'Experimentation' THEN '2' WHEN 'Experimentation/Research' THEN '2' WHEN 'Deployment' THEN '3' WHEN 'Closed' THEN '4' ELSE '0' END,
                    NewStage = CASE REPLACE(NewStage, ' ', '') WHEN 'Submitted' THEN '0' WHEN 'ConceptDevelopment' THEN '1' WHEN 'Experimentation' THEN '2' WHEN 'Experimentation/Research' THEN '2' WHEN 'Deployment' THEN '3' WHEN 'Closed' THEN '4' ELSE '0' END,
                    PreviousStatus = CASE REPLACE(PreviousStatus, ' ', '') WHEN 'PendingInformation' THEN '0' WHEN 'UnderReview' THEN '1' WHEN 'Approved' THEN '2' WHEN 'Declined' THEN '3' ELSE '1' END,
                    NewStatus = CASE REPLACE(NewStatus, ' ', '') WHEN 'PendingInformation' THEN '0' WHEN 'UnderReview' THEN '1' WHEN 'Approved' THEN '2' WHEN 'Declined' THEN '3' ELSE '1' END;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "PreviousStatus",
                table: "StageHistories",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "PreviousStage",
                table: "StageHistories",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "NewStatus",
                table: "StageHistories",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "NewStage",
                table: "StageHistories",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "Resources",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Resources",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "Sha256",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "InnovationIdeas",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentStatus",
                table: "InnovationIdeas",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentStage",
                table: "InnovationIdeas",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<byte[]>(
                name: "Content",
                table: "IdeaAttachments",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "Sha256",
                table: "IdeaAttachments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsBreakGlassAccount",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AspNetUsers",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "EmailOutbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Recipient = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextAttemptAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailOutbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdeaTeamMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdeaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessUnit = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdeaTeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdeaTeamMembers_InnovationIdeas_IdeaId",
                        column: x => x.IdeaId,
                        principalTable: "InnovationIdeas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReminderExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdeaTimelineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReminderKind = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExecutedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderExecutions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailOutbox_IdempotencyKey",
                table: "EmailOutbox",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdeaTeamMembers_IdeaId",
                table: "IdeaTeamMembers",
                column: "IdeaId");

            migrationBuilder.CreateIndex(
                name: "IX_ReminderExecutions_IdeaTimelineId_ReminderKind_DueDate",
                table: "ReminderExecutions",
                columns: new[] { "IdeaTimelineId", "ReminderKind", "DueDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailOutbox");

            migrationBuilder.DropTable(
                name: "IdeaTeamMembers");

            migrationBuilder.DropTable(
                name: "ReminderExecutions");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Sha256",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "IdeaAttachments");

            migrationBuilder.DropColumn(
                name: "Sha256",
                table: "IdeaAttachments");

            migrationBuilder.DropColumn(
                name: "IsBreakGlassAccount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "StorageName",
                table: "Resources",
                newName: "FilePath");

            migrationBuilder.RenameColumn(
                name: "StorageName",
                table: "IdeaAttachments",
                newName: "FilePath");

            migrationBuilder.AlterColumn<string>(
                name: "PreviousStatus",
                table: "StageHistories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "PreviousStage",
                table: "StageHistories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "NewStatus",
                table: "StageHistories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "NewStage",
                table: "StageHistories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "InnovationIdeas",
                type: "varbinary(max)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "rowversion",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<string>(
                name: "CurrentStatus",
                table: "InnovationIdeas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentStage",
                table: "InnovationIdeas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_RoleId",
                table: "AspNetUsers",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Roles_RoleId",
                table: "AspNetUsers",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
