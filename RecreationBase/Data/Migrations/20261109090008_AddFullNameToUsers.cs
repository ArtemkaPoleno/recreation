using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecreationBase.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261109090008_AddFullNameToUsers")]
public class AddFullNameToUsers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "FullName",
            table: "AspNetUsers",
            type: "TEXT",
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FullName",
            table: "AspNetUsers");
    }
}
