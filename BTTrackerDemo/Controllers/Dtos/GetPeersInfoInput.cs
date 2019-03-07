namespace BTTrackerDemo.Controllers.Dtos
{
    public class GetPeersInfoInput
    {
        /// <summary>
        /// 种子的唯一 Hash 标识。
        /// </summary>
        public string Info_Hash { get; set; }

        /// <summary>
        /// 客户端的随机 Id，由 BT 客户端生成。
        /// </summary>
        public string Peer_Id { get; set; }

        /// <summary>
        /// 客户端的 IP 地址。
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 客户端监听的端口。
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 已经上传的数据大小。
        /// </summary>
        public long Uploaded { get; set; }

        /// <summary>
        /// 已经下载的数据大小。
        /// </summary>
        public long Downloaded { get; set; }

        /// <summary>
        /// 事件表示，具体可以转换为 <see cref="TorrentEvent"/> 枚举的具体值。
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// 该客户端剩余待下载的数据。
        /// </summary>
        public long Left { get; set; }

        /// <summary>
        /// 是否启用压缩，当该值为 1 的时候，表示当前客户端接受压缩格式的 Peer 列表，即使用
        /// 6 字节表示一个 Peer (前 4 字节表示 IP 地址，后 2 字节表示端口号)。当该值为 0
        /// 的时候则表示客户端不接受。
        /// </summary>
        public int Compact { get; set; }

        /// <summary>
        /// 表示客户端想要获得的 Peer 数量。
        /// </summary>
        public int? NumWant { get; set; }
    }
}