﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AthenaBot.Migrations;

public static class MigrationQueries
{
    public static void MigrateRero(MigrationBuilder migrationBuilder)
    {
        if (migrationBuilder.IsMySql())
        {
            migrationBuilder.Sql(
                @"INSERT IGNORE into reactionroles(guildid, channelid, messageid, emote, roleid, `group`, levelreq, dateadded)
select guildid, channelid, messageid, emotename, roleid, exclusive, 0, reactionrolemessage.dateadded
from reactionrole
left join reactionrolemessage on reactionrolemessage.id = reactionrole.reactionrolemessageid
left join guildconfigs on reactionrolemessage.guildconfigid = guildconfigs.id;");
        }
        else
        {
            throw new NotSupportedException("This database provider doesn't have an implementation for MigrateRero");
        }
    }
}