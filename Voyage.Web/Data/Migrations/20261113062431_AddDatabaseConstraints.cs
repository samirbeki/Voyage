using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voyage.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabaseConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Destinations_Categories_CategoryId",
                table: "Destinations");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Trips_Budget",
                table: "Trips",
                sql: "[Budget] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Trips_EndDate",
                table: "Trips",
                sql: "[EndDate] >= [StartDate]");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Destinations_Categories_CategoryId",
                table: "Destinations",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Destinations_Categories_CategoryId",
                table: "Destinations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Trips_Budget",
                table: "Trips");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Trips_EndDate",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

            migrationBuilder.AddForeignKey(
                name: "FK_Destinations_Categories_CategoryId",
                table: "Destinations",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
