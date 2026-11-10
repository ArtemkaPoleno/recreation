using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecreationBase.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261109090001_CreateRoles")]
public class CreateRoles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "NormalizedName",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AspNetRoles");
    }
}
