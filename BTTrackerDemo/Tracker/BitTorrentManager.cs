using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BTTrackerDemo.Tracker
{
    public class BitTorrentManager : IBitTorrentManager
    {
        private readonly ConcurrentDictionary<string, List<Peer>> _peers;
        private readonly ConcurrentDictionary<string, BitTorrentStatus> _bitTorrentStatus;

        public BitTorrentManager()
        {
            _peers = new ConcurrentDictionary<string, List<Peer>>();
            _bitTorrentStatus = new ConcurrentDictionary<string, BitTorrentStatus>();
        }

        public Peer AddPeer(string infoHash, AnnounceInputParameters inputParameters)
        {
            CheckParameters(infoHash, inputParameters);

            var newPeer = new Peer(inputParameters);
            
            if (!_peers.ContainsKey(infoHash))
            {
                _peers.TryAdd(infoHash, new List<Peer> {newPeer});
            }
            
            _peers[infoHash].Add(newPeer);
            
            UpdateBitTorrentStatus(infoHash);
            
            return newPeer;
        }

        public void DeletePeer(string infoHash, AnnounceInputParameters inputParameters)
        {
            CheckParameters(infoHash, inputParameters);

            if (!_peers.ContainsKey(infoHash)) return;

            _peers[infoHash].RemoveAll(p => p.UniqueId == inputParameters.PeerId);
            
            UpdateBitTorrentStatus(infoHash);
        }

        public void UpdatePeer(string infoHash, AnnounceInputParameters inputParameters)
        {
            CheckParameters(infoHash, inputParameters);
            
            if (!_peers.ContainsKey(inputParameters.InfoHash)) _peers.TryAdd(infoHash, new List<Peer>());
            if (!_bitTorrentStatus.ContainsKey(inputParameters.InfoHash)) _bitTorrentStatus.TryAdd(infoHash, new BitTorrentStatus());

            // 如果 Peer 不存在则添加，否则更新其状态。
            var peers = _peers[infoHash];
            var peer = peers.FirstOrDefault(p => p.UniqueId == inputParameters.PeerId);
            if (peer == null)
            {
                AddPeer(infoHash, inputParameters);
            }
            else
            {
                peer.UpdateStatus(inputParameters);
            }
            
            // 根据事件更新种子状态与 Peer 信息。
            if (inputParameters.Event == TorrentEvent.Stopped) DeletePeer(infoHash,inputParameters);
            if (inputParameters.Event == TorrentEvent.Completed) _bitTorrentStatus[infoHash].Downloaded++;
            
            UpdateBitTorrentStatus(infoHash);
        }

        public IReadOnlyList<Peer> GetPeers(string infoHash)
        {
            if (!_peers.ContainsKey(infoHash)) return null;
            return _peers[infoHash];
        }

        public void ClearZombiePeers(string infoHash, TimeSpan expiry)
        {
            if (!_peers.ContainsKey(infoHash)) return;

            var now = DateTime.Now;

            _peers[infoHash].RemoveAll(p => now - p.LastRequestTrackerTime > expiry);
        }

        public int GetComplete(string infoHash)
        {
            if (_bitTorrentStatus.TryGetValue(infoHash, out BitTorrentStatus status))
            {
                return status.Completed;
            }

            return 0;
        }

        public int GetInComplete(string infoHash)
        {
            if (_bitTorrentStatus.TryGetValue(infoHash, out BitTorrentStatus status))
            {
                return status.InCompleted;
            }

            return 0;
        }

        /// <summary>
        /// 更新种子的统计信息。
        /// </summary>
        private void UpdateBitTorrentStatus(string infoHash)
        {
            if (!_peers.ContainsKey(infoHash)) return;
            if (!_bitTorrentStatus.ContainsKey(infoHash)) return;

            // 遍历种子所有的 Peer 状态，对种子统计信息进行处理。
            int complete = 0, incomplete = 0;
            var peers = _peers[infoHash];
            foreach (var peer in peers)
            {
                if (peer.IsCompleted) complete++;
                else incomplete++;
            }

            _bitTorrentStatus[infoHash].Completed = complete;
            _bitTorrentStatus[infoHash].InCompleted = incomplete;
        }

        /// <summary>
        /// 检测参数与种子唯一标识的状态。
        /// </summary>
        private void CheckParameters(string infoHash,AnnounceInputParameters inputParameters)
        {
            if (string.IsNullOrEmpty(infoHash)) throw new Exception("种子的唯一标识不能为空。");
            if (inputParameters == null) throw new Exception("BT 客户端传入的参数不能为空。");
        }
    }
}