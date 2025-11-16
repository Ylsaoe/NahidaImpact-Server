using NahidaImpact.GameServer.Game.Player;
using NahidaImpact.KcpSharp;
using NahidaImpact.Proto;

namespace NahidaImpact.GameServer.Server.Packet.Send.Scene;

public class PacketEnterSceneDoneRsp : BasePacket
{
    public PacketEnterSceneDoneRsp(PlayerInstance player) : base(CmdIds.EnterSceneDoneRsp)
    {
        var proto = new EnterSceneDoneRsp()
        {
            EnterSceneToken = player.SceneManager!.EnterToken
        };
        
        SetData(proto);
    }
}