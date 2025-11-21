using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebGioiThieuAmThuc.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    region_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    region_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Regions__01146BAE6E3C8104", x => x.region_id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    fullname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "member"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    status = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__B9BE370F0BC353E0", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "Specialties",
                columns: table => new
                {
                    specialty_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    region_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    short_description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    full_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    image_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "approved")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Specialt__B90D8D12B226448A", x => x.specialty_id);
                    table.ForeignKey(
                        name: "FK_Specialties_Regions",
                        column: x => x.region_id,
                        principalTable: "Regions",
                        principalColumn: "region_id");
                    table.ForeignKey(
                        name: "FK_Specialties_Users",
                        column: x => x.created_by,
                        principalTable: "Users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    favorite_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    specialty_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Favorite__46ACF4CBC5790764", x => x.favorite_id);
                    table.ForeignKey(
                        name: "FK_Favorites_Specialties",
                        column: x => x.specialty_id,
                        principalTable: "Specialties",
                        principalColumn: "specialty_id");
                    table.ForeignKey(
                        name: "FK_Favorites_Users",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    rating_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    specialty_id = table.Column<int>(type: "int", nullable: false),
                    rating = table.Column<int>(type: "int", nullable: true),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Ratings__D35B278B0D6589A6", x => x.rating_id);
                    table.ForeignKey(
                        name: "FK_Ratings_Specialties",
                        column: x => x.specialty_id,
                        principalTable: "Specialties",
                        principalColumn: "specialty_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ratings_Users",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_specialty_id",
                table: "Favorites",
                column: "specialty_id");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_User",
                table: "Favorites",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_Specialty",
                table: "Ratings",
                column: "specialty_id");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_user_id",
                table: "Ratings",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Specialties_created_by",
                table: "Specialties",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_Specialties_Region",
                table: "Specialties",
                column: "region_id");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__AB6E6164E0CEB30B",
                table: "Users",
                column: "email",
                unique: true,
                filter: "[email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__F3DBC572A191AE1E",
                table: "Users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "Specialties");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
