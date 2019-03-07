using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BencodeNET.Objects;
using BTTrackerDemo.Controllers.Dtos;
using BTTrackerDemo.Tracker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
 
 namespace BTTrackerDemo.Controllers
 {
     public class AnnounceController : Controller
     {
         private readonly IHttpContextAccessor _httpContextAccessor;
         private readonly IBitTorrentManager _bitTorrentManager;

         public AnnounceController(IHttpContextAccessor httpContextAccessor, IBitTorrentManager bitTorrentManager)
         {
             _httpContextAccessor = httpContextAccessor;
             _bitTorrentManager = bitTorrentManager;
         }

         [HttpGet]
         [Route("/Announce/GetPeersInfo")]
         public async Task GetPeersInfo(GetPeersInfoInput input)
         {
             // 如果 BT 客户端没有传递 IP，则通过 Context 获得。
             if (string.IsNullOrEmpty(input.Ip)) input.Ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

             // 本机测试用。
             input.Ip = "127.0.0.1";

             AnnounceInputParameters inputPara = input;
             var resultDict = new BDictionary();

             // 如果产生了错误，则不执行其他操作，直接返回结果。
             if (inputPara.Error.Count == 0)
             {
                 _bitTorrentManager.UpdatePeer(input.Info_Hash,inputPara);
                 _bitTorrentManager.ClearZombiePeers(input.Info_Hash,TimeSpan.FromMinutes(10));
                 var peers = _bitTorrentManager.GetPeers(input.Info_Hash);
             
                 HandlePeersData(resultDict,peers,inputPara);
             
                 // 构建剩余字段信息
                 // 客户端等待时间
                 resultDict.Add(TrackerServerConsts.IntervalKey,new BNumber((int)TimeSpan.FromSeconds(30).TotalSeconds));
                 // 最小等待间隔
                 resultDict.Add(TrackerServerConsts.MinIntervalKey,new BNumber((int)TimeSpan.FromSeconds(30).TotalSeconds));
                 // Tracker 服务器的 Id
                 resultDict.Add(TrackerServerConsts.TrackerIdKey,new BString("Tracker-DEMO"));
                 // 已完成的 Peer 数量
                 resultDict.Add(TrackerServerConsts.CompleteKey,new BNumber(_bitTorrentManager.GetComplete(input.Info_Hash)));
                 // 非做种状态的 Peer 数量
                 resultDict.Add(TrackerServerConsts.IncompleteKey,new BNumber(_bitTorrentManager.GetInComplete(input.Info_Hash)));
             }
             else
             {
                 resultDict = inputPara.Error;
             }
                          
             // 写入响应结果。
             var resultDictBytes = resultDict.EncodeAsBytes();
             var response = _httpContextAccessor.HttpContext.Response;
             response.ContentType = "text/plain";
             response.StatusCode = 200;
             response.ContentLength = resultDictBytes.Length;
             await response.Body.WriteAsync(resultDictBytes);
         }

         /// <summary>
         /// 将 Peer 集合的数据转换为 BT 协议规定的格式
         /// </summary>
         private void HandlePeersData(BDictionary resultDict, IReadOnlyList<Peer> peers, AnnounceInputParameters inputParameters)
         {
             var total = Math.Min(peers.Count, inputParameters.PeerWantCount);
             //var startIndex = new Random().Next(total);
             
             // 判断当前 BT 客户端是否需要紧凑模式的数据。
             if (inputParameters.IsEnableCompact)
             {
                 var compactResponse = new byte[total * 6];
                 for (int index =0; index<total; index++)
                 {
                     var peer = peers[index];
                     Buffer.BlockCopy(peer.ToBytes(),0,compactResponse,(total -1) *6,6);
                 }
                 
                 resultDict.Add(TrackerServerConsts.PeersKey,new BString(compactResponse));
             }
             else
             {
                 var nonCompactResponse = new BList();
                 for (int index =0; index<total; index++)
                 {
                     var peer = peers[index];
                     nonCompactResponse.Add(peer.ToEncodedDictionary());
                 }
                 
                 resultDict.Add(TrackerServerConsts.PeersKey,nonCompactResponse);
             }
         }
     }
 }