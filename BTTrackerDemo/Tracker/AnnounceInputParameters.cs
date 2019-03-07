using System;
using System.Net;
using System.Web;
using BencodeNET.Objects;
using BTTrackerDemo.Controllers.Dtos;

namespace BTTrackerDemo.Tracker
{
    public class AnnounceInputParameters
    {
        /// <summary>
        /// 客户端 IP 端点信息。
        /// </summary>
        public IPEndPoint ClientAddress { get; }

        /// <summary>
        /// 种子的唯一 Hash 标识。
        /// </summary>
        public string InfoHash { get; }

        /// <summary>
        /// 客户端的随机 Id，由 BT 客户端生成。
        /// </summary>
        public string PeerId { get; }

        /// <summary>
        /// 已经上传的数据大小。
        /// </summary>
        public long Uploaded { get; }

        /// <summary>
        /// 已经下载的数据大小。
        /// </summary>
        public long Downloaded { get; }

        /// <summary>
        /// 事件表示，具体可以转换为 <see cref="TorrentEvent"/> 枚举的具体值。
        /// </summary>
        public TorrentEvent Event { get; }

        /// <summary>
        /// 该客户端剩余待下载的数据。
        /// </summary>
        public long Left { get; }

        /// <summary>
        /// Peer 是否允许启用压缩。
        /// </summary>
        public bool IsEnableCompact { get; }

        /// <summary>
        /// Peer 想要获得的可用的 Peer 数量。
        /// </summary>
        public int PeerWantCount { get; }
        
        /// <summary>
        /// 如果在请求过程当中出现了异常，则本字典包含了异常信息。
        /// </summary>
        public BDictionary Error { get; }

        public AnnounceInputParameters(GetPeersInfoInput apiInput)
        {
            Error = new BDictionary();

            ClientAddress = ConvertClientAddress(apiInput);
            InfoHash = ConvertInfoHash(apiInput);
            Event = ConvertTorrentEvent(apiInput);
            PeerId = apiInput.Peer_Id;
            Uploaded = apiInput.Uploaded;
            Downloaded = apiInput.Downloaded;
            Left = apiInput.Left;
            IsEnableCompact = apiInput.Compact == 1;
            PeerWantCount = apiInput.NumWant ?? 30;
        }

        /// <summary>
        /// <see cref="GetPeersInfoInput"/> 到当前类型的隐式转换定义。
        /// </summary>
        public static implicit operator AnnounceInputParameters(GetPeersInfoInput input)
        {
            return new AnnounceInputParameters(input);
        }

        /// <summary>
        /// 将客户端传递的 IP 地址与端口转换为 <see cref="IPEndPoint"/> 类型。
        /// </summary>
        private IPEndPoint ConvertClientAddress(GetPeersInfoInput apiInput)
        {
            if (IPAddress.TryParse(apiInput.Ip, out IPAddress ipAddress))
            {
                return new IPEndPoint(ipAddress,apiInput.Port);
            }

            return null;
        }

        /// <summary>
        /// 将客户端传递的字符串 Event 转换为 <see cref="TorrentEvent"/> 枚举。
        /// </summary>
        private TorrentEvent ConvertTorrentEvent(GetPeersInfoInput apiInput)
        {
            switch (apiInput.Event)
            {
                case "started":
                    return TorrentEvent.Started;
                case "stopped":
                    return TorrentEvent.Stopped;
                case "completed":
                    return TorrentEvent.Completed;
                default:
                    return TorrentEvent.None;
            }
        }

        /// <summary>
        /// 将 info_hash 参数从 URL 编码转换为标准的字符串。
        /// </summary>
        private string ConvertInfoHash(GetPeersInfoInput apiInput)
        {
            var infoHashBytes = HttpUtility.UrlDecodeToBytes(apiInput.Info_Hash);
            if (infoHashBytes == null)
            {
                Error.Add(TrackerServerConsts.FailureKey,new BString("info_hash 参数不能为空."));
                return null;
            }

            if (infoHashBytes.Length != 20)
            {
                Error.Add(TrackerServerConsts.FailureKey,new BString($"info_hash 参数的长度 {{{infoHashBytes.Length}}} 不符合 BT 协议规范."));
            }

            return BitConverter.ToString(infoHashBytes);
        }
    }
}