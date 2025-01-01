using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tomes.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class userHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductVisitList",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductVisitList",
                table: "AspNetUsers");
        }
    }
}
