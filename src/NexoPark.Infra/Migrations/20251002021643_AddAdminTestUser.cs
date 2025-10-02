using Microsoft.EntityFrameworkCore.Migrations;
using BCrypt.Net; // GARANTIR O USING
using NexoPark.Core.Constants; // GARANTIR O USING

#nullable disable

namespace NexoPark.Infra.Migrations
{
    public partial class AddAdminTestUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Senha: 'test456'
            var senhaHash = BCrypt.Net.BCrypt.HashPassword("test456");
            var adminId = Guid.NewGuid();

            migrationBuilder.InsertData(
                table: "Administradores",
                columns: new[] { "Id", "Nome", "Email", "SenhaHash", "Role" },
                values: new object[] 
                { 
                    adminId,
                    "Usuario Teste",
                    "test@nexopark.com", 
                    senhaHash, 
                    AppRoles.Editor // Usamos a Role Editor
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Administradores",
                keyColumn: "Email",
                keyValue: "test@nexopark.com");
        }
    }
}