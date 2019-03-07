using BencodeNET.Objects;

namespace BTTrackerDemo.Tracker
{
    /// <summary>
    /// 常用的字典 KEY。
    /// </summary>
    public static class TrackerServerConsts
    {
        public static readonly BString PeerIdKey = new BString("peer id");
        public static readonly BString PeersKey = new BString("peers"); 
        public static readonly BString IntervalKey = new BString("interval");
        public static readonly BString MinIntervalKey = new BString("min interval");
        public static readonly BString TrackerIdKey = new BString("tracker id");
        public static readonly BString CompleteKey = new BString("complete");
        public static readonly BString IncompleteKey = new BString("incomplete");
        
        public static readonly BString Port = new BString("port");
        public static readonly BString Ip = new BString("ip");
        
        public static readonly string FailureKey = "failure reason";
    }
}