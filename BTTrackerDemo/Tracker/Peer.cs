using System;
using System.Net;
using BencodeNET.Objects;

namespace BTTrackerDemo.Tracker
{
    /// <summary>
    /// 每个 BT 下载客户端的定义。
    /// </summary>
    public class Peer
    {
        /// <summary>
        /// 客户端 IP 端点信息。
        /// </summary>
        public IPEndPoint ClientAddress { get; private set; }

        /// <summary>
        /// 客户端的随机 Id，由 BT 客户端生成。
        /// </summary>
        public string PeerId { get; private set; }

        /// <summary>
        /// 客户端唯一标识。
        /// </summary>
        public string UniqueId { get; private set; }
        
        /// <summary>
        /// 客户端在本次会话过程中下载的数据量。(以 Byte 为单位)
        /// </summary>
        public long DownLoaded { get; private set; }

        /// <summary>
        /// 客户端在本次会话过程当中上传的数据量。(以 Byte 为单位)
        /// </summary>
        public long Uploaded { get; private set; }

        /// <summary>
        /// 客户端的下载速度。(以 Byte/秒 为单位)
        /// </summary>
        public long DownloadSpeed { get; private set; }

        /// <summary>
        /// 客户端的上传速度。(以 Byte/秒 为单位)
        /// </summary>
        public long UploadSpeed { get; private set; }

        /// <summary>
        /// 客户端是否完成了当前种子，True 为已经完成，False 为还未完成。
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// 最后一次请求 Tracker 服务器的时间。
        /// </summary>
        public DateTime LastRequestTrackerTime { get; private set; }

        /// <summary>
        /// Peer 还需要下载的数量。
        /// </summary>
        public long Left { get; private set; }

        public Peer() { }

        public Peer(AnnounceInputParameters inputParameters)
        {
            UniqueId = inputParameters.PeerId;
            
            // 根据输入参数更新 Peer 的状态。
            UpdateStatus(inputParameters);
        }

        /// <summary>
        /// 根据输入参数更新 Peer 的状态。
        /// </summary>
        /// <param name="inputParameters">BT 客户端请求 Tracker 服务器时传递的参数。</param>
        public void UpdateStatus(AnnounceInputParameters inputParameters)
        {
            var now = DateTime.Now;

            var elapsedTime = (now - LastRequestTrackerTime).TotalSeconds;
            if (elapsedTime < 1) elapsedTime = 1;

            ClientAddress = inputParameters.ClientAddress;
            // 通过差值除以消耗的时间，得到每秒的大概下载速度。
            DownloadSpeed = (int) ((inputParameters.Downloaded - DownLoaded) / elapsedTime);
            DownLoaded = inputParameters.Downloaded;
            UploadSpeed = (int) ((inputParameters.Uploaded) / elapsedTime);
            Uploaded = inputParameters.Uploaded;
            Left = inputParameters.Left;
            PeerId = inputParameters.PeerId;
            LastRequestTrackerTime = now;
            
            // 如果没有剩余数据，则表示 Peer 已经完成下载。
            if (Left == 0) IsCompleted = true;
        }

        /// <summary>
        /// 将 Peer 信息进行 B 编码，按照协议处理为字典。
        /// </summary>
        public BDictionary ToEncodedDictionary()
        {
            return new BDictionary
            {
                {TrackerServerConsts.PeerIdKey,new BString(PeerId)},
                {TrackerServerConsts.Ip,new BString(ClientAddress.Address.ToString())},
                {TrackerServerConsts.Port,new BNumber(ClientAddress.Port)}
            };
        }

        /// <summary>
        /// 将 Peer 信息进行紧凑编码成字节组。
        /// </summary>
        public byte[] ToBytes()
        {
            var portBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short) ClientAddress.Port));
            var addressBytes = ClientAddress.Address.GetAddressBytes();

            var resultBytes = new byte[portBytes.Length + addressBytes.Length];
            
            // 根据协议规定，首部的 4 字节为 IP 地址，尾部的 2 自己为端口信息
            Array.Copy(addressBytes,resultBytes,addressBytes.Length);
            Array.Copy(portBytes,0,resultBytes,addressBytes.Length,portBytes.Length);

            return resultBytes;
        }
    }
}