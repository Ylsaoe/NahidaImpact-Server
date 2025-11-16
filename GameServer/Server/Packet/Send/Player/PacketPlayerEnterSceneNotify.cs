using NahidaImpact.GameServer.Game.Player;
using NahidaImpact.KcpSharp;
using NahidaImpact.Proto;

namespace NahidaImpact.GameServer.Server.Packet.Send.Player;

public class PacketPlayerEnterSceneNotify : BasePacket
{
    public PacketPlayerEnterSceneNotify(PlayerInstance player) : base(CmdIds.PlayerEnterSceneNotify)
    {
        var proto = new PlayerEnterSceneNotify
        {
            SceneBeginTime = player.SceneManager!.BeginTime,
            Type = EnterType.Self,
            SceneId =  player.SceneManager.SceneId,
            SceneTransaction =  player.SceneManager.CreateTransaction(player.SceneManager.SceneId, (uint)player.Uid,  player.SceneManager.BeginTime),
            Pos = new()
            {
                X = 2191.16357421875f,
                Y = 214.65115356445312f,
                Z = -1120.633056640625f
            },
            TargetUid = (uint)player.Uid,
            EnterSceneToken =  player.SceneManager.EnterToken
        };
        
        SetData(proto);
    }
}