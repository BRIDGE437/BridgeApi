using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatchingApi.Migrations
{
    /// <inheritdoc />
    public partial class AddHnswIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // HNSW approximate nearest-neighbour index for cosine distance queries.
            // Speeds up: ORDER BY "Embedding" <=> $vector LIMIT N
            // m=16 (connections per layer), ef_construction=64 (build quality)
            migrationBuilder.Sql(
                @"CREATE INDEX IF NOT EXISTS ""IX_Startups_Embedding_Hnsw""
                  ON ""Startups"" USING hnsw (""Embedding"" vector_cosine_ops)
                  WITH (m = 16, ef_construction = 64);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Startups_Embedding_Hnsw"";");
        }
    }
}
