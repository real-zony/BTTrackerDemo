using BencodeNET.Objects;

namespace BTTrackerDemo.Tracker
{
    /// <summary>
    /// 用于表示某个种子的状态与统计信息。
    /// </summary>
    public class BitTorrentStatus
    {
        /// <summary>
        /// 下载完成的 Peer 数量。
        /// </summary>
        public BNumber Downloaded { get; set; }

        /// <summary>
        /// 已经完成种子下载的 Peer 数量。
        /// </summary>
        public BNumber Completed { get; set; }

        /// <summary>
        /// 正在下载种子的 Peer 数量。
        /// </summary>
        public BNumber InCompleted { get; set; }

        public BitTorrentStatus()
        {
            Downloaded = new BNumber(0);
            Completed = new BNumber(0);
            InCompleted = new BNumber(0);
        }
    }
}