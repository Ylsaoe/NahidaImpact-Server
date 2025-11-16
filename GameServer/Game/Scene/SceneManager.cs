using NahidaImpact.Database.Avatar;
using NahidaImpact.GameServer.Game.Entity;
using NahidaImpact.GameServer.Game.Player;
using NahidaImpact.GameServer.Game.Player.Team;
using NahidaImpact.GameServer.Server.Packet.Send.Player;
using NahidaImpact.GameServer.Server.Packet.Send.Scene;

namespace NahidaImpact.GameServer.Game.Scene;

public class SceneManager
{
    private readonly EntityManager EntityManager;
    public readonly List<EntityAvatar> TeamAvatars;
    private readonly PlayerInstance Player;
    public ulong BeginTime { get; set; }
    public uint SceneId { get; set; }
    public uint EnterToken { get; private set; }
    
    public SceneManager(PlayerInstance player)
    {
        Player = player;
        EntityManager = Player.EntityManager!;
        TeamAvatars = new();
    }

    public async ValueTask EnterSceneAsync(uint sceneId)
    {
        if (BeginTime != 0) ResetState();

        BeginTime = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds();
        SceneId = sceneId;
        EnterToken = ++EnterToken;
    }
    
    public async ValueTask ChangeTeamAvatarsAsync(ulong[] guidList)
    {
        TeamAvatars.Clear();

        foreach (ulong guid in guidList)
        {
            AvatarDataInfo avatarInfo = Player.Avatars!.Find(avatar => avatar.Guid == guid)!; // currently only first one

            EntityAvatar entityAvatar = Player.AvatarManager!.CreateAvatar(Player, avatarInfo);
            entityAvatar.SetPosition(2336.789f, 249.98896f, -751.3081f);

            TeamAvatars.Add(entityAvatar);
        }

        await Player.SendPacket(new PacketSceneTeamUpdateNotify(Player));
        await EntityManager.AddEntityAsync(TeamAvatars[0], VisionType.Born);
    }
    
    public async ValueTask OnSceneInitFinished()
    {
        GameAvatarTeam avatarTeam = Player.AvatarManager!.GetCurrentTeam();

        foreach (ulong guid in avatarTeam.AvatarGuidList)
        {
            AvatarDataInfo avatarInfo = Player.Avatars!.Find(avatar => avatar.Guid == guid)!;

            EntityAvatar entityAvatar = Player.AvatarManager!.CreateAvatar(Player, avatarInfo);
            if (Player.EntityAvatar == null)
            {
                Player.EntityAvatar = entityAvatar;
            }
            entityAvatar.SetPosition(2336.789f, 249.98896f, -751.3081f);

            TeamAvatars.Add(entityAvatar);
        }

        await Player.SendPacket(new PacketPlayerEnterSceneInfoNotify(Player));
        await Player.SendPacket(new PacketSceneTeamUpdateNotify(Player));
    }

    private void ResetState()
    {
        TeamAvatars.Clear();
        EntityManager.Reset();
    }

    public string CreateTransaction(uint sceneId, uint playerUid, ulong beginTime)
        => string.Format("{0}-{1}-{2}-13830", sceneId, playerUid, beginTime);
}