using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koloqwa.Infrastructure.Persistence.Migrations
{
    public partial class AddTrigramExtension : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_wordentries_headword_trgm ON \"WordEntries\" USING gin (\"Headword\" gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_phraseentries_phrasetext_trgm ON \"PhraseEntries\" USING gin (\"PhraseText\" gin_trgm_ops);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_wordentries_headword_trgm;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_phraseentries_phrasetext_trgm;");
            migrationBuilder.Sql("DROP EXTENSION IF EXISTS pg_trgm;");
        }
    }
}