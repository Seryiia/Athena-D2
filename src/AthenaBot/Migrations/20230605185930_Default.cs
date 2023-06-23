using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AthenaBot.Migrations
{
    public partial class Default : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "discorduser",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    userid = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    username = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    discriminator = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    avatarid = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dateadded = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_discorduser", x => x.id);
                    table.UniqueConstraint("ak_discorduser_userid", x => x.userid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "expressions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    guildid = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    response = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trigger = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    autodeletetrigger = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    dmresponse = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    containsanywhere = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    allowtarget = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    reactions = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dateadded = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expressions", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "guildconfigs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    guildid = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    prefix = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    deletemessageoncommand = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    filterwords = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    locale = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    timezoneid = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    verboseerrors = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    notifystreamoffline = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    deletestreamonlinemessage = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    disableglobalexpressions = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    dateadded = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_guildconfigs", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "quotes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    guildid = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    keyword = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    authorname = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    authorid = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    text = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dateadded = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quotes", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "reminders",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    when = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    channelid = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    serverid = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    userid = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    message = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    isprivate = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    dateadded = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reminders", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "streamonlinemessages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    channelid = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    messageid = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dateadded = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_streamonlinemessages", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "delmsgoncmdchannel",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    channelid = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    state = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    guildconfigid = table.Column<int>(type: "int", nullable: true),
                    dateadded = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_delmsgoncmdchannel", x => x.id);
                    table.ForeignKey(
                        name: "fk_delmsgoncmdchannel_guildconfigs_guildconfigid",
                        column: x => x.guildconfigid,
                        principalTable: "guildconfigs",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "feedsub",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    guildconfigid = table.Column<int>(type: "int", nullable: false),
                    channelid = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    url = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    message = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dateadded = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feedsub", x => x.id);
                    table.UniqueConstraint("ak_feedsub_guildconfigid_url", x => new { x.guildconfigid, x.url });
                    table.ForeignKey(
                        name: "fk_feedsub_guildconfigs_guildconfigid",
                        column: x => x.guildconfigid,
                        principalTable: "guildconfigs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "filteredword",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    word = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    guildconfigid = table.Column<int>(type: "int", nullable: true),
                    dateadded = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filteredword", x => x.id);
                    table.ForeignKey(
                        name: "fk_filteredword_guildconfigs_guildconfigid",
                        column: x => x.guildconfigid,
                        principalTable: "guildconfigs",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "followedstream",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    guildid = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    channelid = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    username = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type = table.Column<int>(type: "int", nullable: false),
                    message = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    guildconfigid = table.Column<int>(type: "int", nullable: true),
                    dateadded = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_followedstream", x => x.id);
                    table.ForeignKey(
                        name: "fk_followedstream_guildconfigs_guildconfigid",
                        column: x => x.guildconfigid,
                        principalTable: "guildconfigs",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "groupname",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    guildconfigid = table.Column<int>(type: "int", nullable: false),
                    number = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dateadded = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groupname", x => x.id);
                    table.ForeignKey(
                        name: "fk_groupname_guildconfigs_guildconfigid",
                        column: x => x.guildconfigid,
                        principalTable: "guildconfigs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_delmsgoncmdchannel_guildconfigid",
                table: "delmsgoncmdchannel",
                column: "guildconfigid");

            migrationBuilder.CreateIndex(
                name: "ix_discorduser_userid",
                table: "discorduser",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "ix_filteredword_guildconfigid",
                table: "filteredword",
                column: "guildconfigid");

            migrationBuilder.CreateIndex(
                name: "ix_followedstream_guildconfigid",
                table: "followedstream",
                column: "guildconfigid");

            migrationBuilder.CreateIndex(
                name: "ix_groupname_guildconfigid_number",
                table: "groupname",
                columns: new[] { "guildconfigid", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_guildconfigs_guildid",
                table: "guildconfigs",
                column: "guildid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_quotes_guildid",
                table: "quotes",
                column: "guildid");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_keyword",
                table: "quotes",
                column: "keyword");

            migrationBuilder.CreateIndex(
                name: "ix_reminders_when",
                table: "reminders",
                column: "when");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "delmsgoncmdchannel");

            migrationBuilder.DropTable(
                name: "discorduser");

            migrationBuilder.DropTable(
                name: "expressions");

            migrationBuilder.DropTable(
                name: "feedsub");

            migrationBuilder.DropTable(
                name: "filteredword");

            migrationBuilder.DropTable(
                name: "followedstream");

            migrationBuilder.DropTable(
                name: "groupname");

            migrationBuilder.DropTable(
                name: "quotes");

            migrationBuilder.DropTable(
                name: "reminders");

            migrationBuilder.DropTable(
                name: "streamonlinemessages");

            migrationBuilder.DropTable(
                name: "guildconfigs");
        }
    }
}
