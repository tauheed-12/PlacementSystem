using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Migrations
{
    /// <inheritdoc />
    public partial class ConvertTokenTypeStringToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Map known string values to enum int values, then coerce any unknown/non-numeric values to a safe default (EmailVerification = 0).
            migrationBuilder.Sql("UPDATE [UserTokens] SET [TokenType] = '0' WHERE [TokenType] = 'EmailVerification';");
            migrationBuilder.Sql("UPDATE [UserTokens] SET [TokenType] = '1' WHERE [TokenType] = 'PasswordReset';");
            // For any remaining non-numeric values (including NULL/empty) set a safe default
            migrationBuilder.Sql("UPDATE [UserTokens] SET [TokenType] = '0' WHERE TRY_CAST([TokenType] AS int) IS NULL;");

            migrationBuilder.AlterColumn<int>(
                name: "TokenType",
                table: "UserTokens",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert column to string first so we can write textual enum names
            migrationBuilder.AlterColumn<string>(
                name: "TokenType",
                table: "UserTokens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // Map numeric enum values back to their string names
            migrationBuilder.Sql("UPDATE [UserTokens] SET [TokenType] = 'EmailVerification' WHERE TRY_CAST([TokenType] AS int) = 0;");
            migrationBuilder.Sql("UPDATE [UserTokens] SET [TokenType] = 'PasswordReset' WHERE TRY_CAST([TokenType] AS int) = 1;");
            // Any unexpected numeric values map to EmailVerification as a safe default
            migrationBuilder.Sql("UPDATE [UserTokens] SET [TokenType] = 'EmailVerification' WHERE TRY_CAST([TokenType] AS int) IS NULL;");
        }
    }
}
