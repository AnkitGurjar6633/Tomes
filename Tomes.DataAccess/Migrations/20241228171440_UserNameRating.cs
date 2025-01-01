using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tomes.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UserNameRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "RatingAndReviews",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                table: "RatingAndReviews");
        }
    }
}
