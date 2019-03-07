using System;
using System.Collections.Generic;

namespace BTTrackerDemo.Tracker
{
    /// <summary>
    /// 用于管理 BT 种子与其关联的 Peer 集合。
    /// </summary>
    public interface IBitTorrentManager
    {
        /// <summary>
        /// 添加一个新的 Peer 到指定种子关联的集合当中。
        /// </summary>
        /// <param name="infoHash">种子的唯一标识。</param>
        /// <param name="inputParameters">BT 客户端传入的参数信息。</param>
        Peer AddPeer(string infoHash,AnnounceInputParameters inputParameters);
        
        /// <summary>
        /// 根据参数删除指定种子的 Peer 信息。
        /// </summary>
        /// <param name="infoHash">种子的唯一标识。</param>
        /// <param name="inputParameters">BT 客户端传入的参数信息。</param>
        void DeletePeer(string infoHash,AnnounceInputParameters inputParameters);

        /// <summary>
        /// 更新指定种子的某个 Peer 状态。
        /// </summary>
        /// <param name="infoHash">种子的唯一标识。</param>
        /// <param name="inputParameters">BT 客户端传入的参数信息。</param>
        void UpdatePeer(string infoHash, AnnounceInputParameters inputParameters);

        /// <summary>
        /// 获得指定种子的可用 Peer 集合。
        /// </summary>
        /// <param name="infoHash">种子的唯一标识。</param>
        /// <returns>当前种子关联的 Peer 列表。</returns>
        IReadOnlyList<Peer> GetPeers(string infoHash);

        /// <summary>
        /// 清理指定种子内部不活跃的 Peer 。 
        /// </summary>
        /// <param name="infoHash">种子的唯一标识。</param>
        /// <param name="expiry">超时周期，超过这个时间的 Peer 将会被清理掉。</param>
        void ClearZombiePeers(string infoHash,TimeSpan expiry);

        /// <summary>
        /// 获得指定种子已经完成下载的 Peer 数量。
        /// </summary>
        /// <param name="infoHash">种子的唯一标识。</param>
        int GetComplete(string infoHash);

        /// <summary>
        /// 获得指定种子正在下载的 Peer 数量。
        /// </summary>
        /// <param name="infoHash">种子的唯一标识。</param>
        int GetInComplete(string infoHash);
    }
}