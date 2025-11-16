using NahidaImpact.Data.Models.Dispatch;
using NahidaImpact.Util;
using NahidaImpact.Util.Security;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace NahidaImpact.SdkServer.Handlers.Dispatch;

[ApiController]
public class RegionHandler : ControllerBase
{
    private static readonly Dictionary<string, RegionData> regions;
    private const string DEFAULT_REGION_DATA = "CAESGE5vdCBGb3VuZCB2ZXJzaW9uIGNvbmZpZw==";
    private static readonly int[] SERVER_VERSION_PARTS = GameConstants.GAME_VERSION.Split('.').Select(int.Parse).ToArray();
    
    // 添加CN和OS的区域列表响应
    private static string regionListResponseOS;
    private static string regionListResponseCN;

    static RegionHandler()
    {
        regions = new Dictionary<string, RegionData>
        {
            { "os_usa", new RegionData(BuildDefaultRegionResponse()) },
        };
        
        // 初始化区域列表
        InitializeRegionList();
    }

    /// <summary>
    /// 初始化区域列表响应
    /// </summary>
    private static void InitializeRegionList()
    {
        var dispatchDomain = ConfigManager.Config.HttpServer.GetDisplayAddress();

        // 创建服务器列表
        var servers = new List<RegionSimpleInfo>();
        
        // 添加OS服务器
        servers.Add(new RegionSimpleInfo
        {
            Type = "DEV_PUBLIC",
            DispatchUrl = $"{dispatchDomain}/query_cur_region/",
            Name = "os_usa",
            Title = ConfigManager.Config.GameServer.GameServer
        });

        // OS配置
        var osConfig = new
        {
            sdkenv = "2",
            checkdevice = "false",
            loadPatch = "false",
            showexception = "false",
            regionConfig = "pm|fk|add",
            downloadMode = "0",
            codeSwitch = "4334", // 4.6及以上版本
            coverSwitch = "40"
        };

        string osConfigJson = System.Text.Json.JsonSerializer.Serialize(osConfig);
        byte[] osConfigEncrypted = Crypto.Xor(osConfigJson, Crypto.DISPATCH_KEY);

        QueryRegionListHttpRsp osRsp = new()
        {
            ClientCustomConfigEncrypted = ByteString.CopyFrom(osConfigEncrypted),
            ClientSecretKey = ByteString.CopyFrom(Crypto.DISPATCH_SEED),
            EnableLoginPc = true,
            Retcode = (int)Retcode.RetSucc
        };
        
        osRsp.RegionList.AddRange(servers);
        regionListResponseOS = Convert.ToBase64String(osRsp.ToByteArray());
        
        var cnConfig = new
        {
            sdkenv = "0", 
            checkdevice = "false",
            loadPatch = "false", 
            showexception = "false",
            regionConfig = "pm|fk|add",
            downloadMode = "0",
            codeSwitch = "4334", // 4.6及以上版本
            coverSwitch = "40"
        };

        string cnConfigJson = System.Text.Json.JsonSerializer.Serialize(cnConfig);
        byte[] cnConfigEncrypted = Crypto.Xor(cnConfigJson, Crypto.DISPATCH_KEY);

        QueryRegionListHttpRsp cnRsp = new()
        {
            ClientCustomConfigEncrypted = ByteString.CopyFrom(cnConfigEncrypted),
            ClientSecretKey = ByteString.CopyFrom(Crypto.DISPATCH_SEED),
            EnableLoginPc = true,
            Retcode = (int)Retcode.RetSucc
        };
        
        cnRsp.RegionList.AddRange(servers);
        regionListResponseCN = Convert.ToBase64String(cnRsp.ToByteArray());
    }

    [HttpGet("/query_cur_region")]
    public IActionResult QueryCurRegion([FromQuery] DispatchQuery query, Logger logger)
    {
        var version = query.Version;
        string? key_id = query.Key_Id;
        string? dispatchSeed = query.DispatchSeed;
        string region = "os_usa";

        var hotfix_version = query.Version!;

        // 获取区域数据
        string regionData = DEFAULT_REGION_DATA;
        if (regions.TryGetValue(region, out RegionData? regionObj))
        {
            regionData = regionObj.Base64;
        }
        
        try
        {
            // 清理并解析版本号
            string clientVersionClean = Regex.Replace(version!, "[a-zA-Z]", "");
            string[] versionCode = clientVersionClean.Split('.');
            if (versionCode.Length < 3)
            {
                return Ok(regionData);
            }

            if (ConfigManager.Config.ServerOption.IsServerStop)
            {
                var stopServer = new StopServerInfo
                {
                    StopBeginTime = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    StopEndTime = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 18000,
                };

                var rsp = new QueryCurrRegionHttpRsp
                {
                    Retcode = (int)Retcode.RetStopServer,
                    Msg = "服务器维护",
                    RegionInfo = new RegionInfo(),
                    StopServer = stopServer
                };
                    
                var encryptedResponse = Crypto.EncryptAndSignRegionData(rsp.ToByteArray(), key_id);
                return Ok(encryptedResponse);
            }

            int versionMajor = int.Parse(versionCode[0]);
            int versionMinor = int.Parse(versionCode[1]);
            int versionFix = int.Parse(versionCode[2]);

            // 新客户端处理逻辑
            if (versionMajor >= 3 || 
                (versionMajor == 2 && versionMinor == 7 && versionFix >= 50) || 
                (versionMajor == 2 && versionMinor == 8))
            {
                // 版本不匹配检查
                if (versionMajor != SERVER_VERSION_PARTS[0] || versionMinor != SERVER_VERSION_PARTS[1])
                {
                    bool updateClient = string.Compare(GameConstants.GAME_VERSION, clientVersionClean) > 0;
                    
                    var stopServer = new StopServerInfo
                    {
                        Url = "https://www.bilibili.com/video/BV1GJ411x7h7/",
                        StopBeginTime = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        StopEndTime = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1000,
                        ContentMsg = updateClient 
                            ? $"\n版本不匹配 过时的客户端! \n\nServer version: {GameConstants.GAME_VERSION}\nClient version: {clientVersionClean}" 
                            : $"\n版本不匹配 过时的服务器! \n\nServer version: {GameConstants.GAME_VERSION}\nClient version: {clientVersionClean}"
                    };

                    var rsp = new QueryCurrRegionHttpRsp
                    {
                        Retcode = (int)Retcode.RetStopServer,
                        Msg = "Connection Failed!",
                        RegionInfo = new RegionInfo(),
                        StopServer = stopServer
                    };
                    
                    var encryptedResponse = Crypto.EncryptAndSignRegionData(rsp.ToByteArray(), key_id);
                    return Ok(encryptedResponse);
                }

                // UA Patch
                if (dispatchSeed == null)
                {
                    return Ok(new 
                    { 
                        content = regionData,
                        sign = "TW9yZSBsb3ZlIGZvciBVQSBQYXRjaCBwbGF5ZXJz" 
                    });
                }

                // Encryption and Signature
                byte[] regionInfo = Convert.FromBase64String(regionData);
                var encrypted = Crypto.EncryptAndSignRegionData(regionInfo, key_id);
                return Ok(encrypted);
            }
            else
            {
                // 旧版本客户端处理
                return Ok(regionData);
            }
        }
        catch (Exception e)
        {
            logger.Error($"Error handling query_cur_region: {e}");
            return Ok(regionData);
        }
    }

    [HttpGet("/query_region_list")] [HttpHead("/query_region_list")]
    public IActionResult QueryRegionList([FromQuery] DispatchQuery query)
    {
        string versionName = query.Version;
        
        // 根据版本号判断使用CN还是OS的区域列表
        if (!string.IsNullOrEmpty(versionName))
        {
            string versionCode;
            try
            {
                versionCode = versionName.Length >= 8 ? versionName.Substring(0, 8) : versionName;
            }
            catch
            {
                versionCode = versionName;
            }

            // CN客户端
            if ("CNRELiOS".Equals(versionCode) || "CNRELWin".Equals(versionCode) || "CNRELAnd".Equals(versionCode))
            {
                return Ok(regionListResponseCN);
            }
            // OS客户端  
            else if ("OSRELiOS".Equals(versionCode) || "OSRELWin".Equals(versionCode) || "OSRELAnd".Equals(versionCode))
            {
                return Ok(regionListResponseOS);
            }
        }

        // 默认返回OS区域列表
        return Ok(regionListResponseOS);
    }

    private static QueryCurrRegionHttpRsp BuildDefaultRegionResponse()
    {
        return new QueryCurrRegionHttpRsp
        {
            Retcode = (int)Retcode.RetSucc,
            ClientSecretKey = ByteString.CopyFrom(Crypto.DISPATCH_SEED),
            RegionInfo = new RegionInfo
            {
                GateserverIp = ConfigManager.Config.GameServer.PublicAddress,
                GateserverPort = (uint)ConfigManager.Config.GameServer.Port
            }
        };
    }
}