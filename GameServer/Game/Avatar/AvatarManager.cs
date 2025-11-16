using NahidaImpact.Data;
using NahidaImpact.Data.Excel;
using NahidaImpact.Database;
using NahidaImpact.Database.Avatar;
using NahidaImpact.GameServer.Game.Entity;
using NahidaImpact.GameServer.Game.Player;
using NahidaImpact.GameServer.Game.Player.Team;
using NahidaImpact.GameServer.Server.Packet.Send.Avatar;

namespace NahidaImpact.GameServer.Game.Avatar;

public class AvatarManager(PlayerInstance player) : BasePlayerManager(player)
{
    public AvatarData AvatarData { get; } = DatabaseHelper.GetInstanceOrCreateNew<AvatarData>(player.Uid);
    public List<GameAvatarTeam>? AvatarTeams { get; set; } = new();
    public uint CurTeamIndex { get; set; }
    public async ValueTask<AvatarDataExcel?> AddAvatar(int avatarId, int level = 90)
    {
        if (AvatarData.Avatars.Any(a => a.AvatarId == avatarId)) return null;
        GameData.AvatarData.TryGetValue(avatarId, out var avatarExcel);
        if (avatarExcel == null) return null;

        uint currentTimestamp = (uint)DateTimeOffset.Now.ToUnixTimeSeconds();
        var avatar = new AvatarDataInfo
        {
            Level = (uint)level,
            SkillDepotId = avatarExcel.SkillDepotId,
            AvatarId = avatarExcel.Id,
            Guid = NextGuid(),
            WeaponId = avatarExcel.InitialWeapon,
            BornTime = currentTimestamp,
            WearingFlycloakId = 340005
        };
        
        AvatarTeams!.Add(new()
        {
            AvatarGuidList = new() { avatar!.Guid },
            Index = 1
        });
        CurTeamIndex = 1;
        
        avatar.InitDefaultProps(avatarExcel);
        AvatarData.Avatars.Add(avatar);

        return avatarExcel;
    }
    
    public GameAvatarTeam GetCurrentTeam()
        => AvatarTeams!.Find(team => team.Index == CurTeamIndex)!;
    
    public EntityAvatar CreateAvatar(PlayerInstance player, AvatarDataInfo avatarInfo)
    {
        return new(player, avatarInfo, ++player.EntityIdSeed);
    }

    public ulong NextGuid()
    {
        return ((ulong)Player.Uid << 32) + (++Player.GuidSeed);
    }

    public AvatarDataInfo? GetAvatar(int avatarId)
    {
        return AvatarData.Avatars.Find(avatar => avatar.AvatarId == avatarId);
    }
}