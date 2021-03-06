﻿using Newtonsoft.Json;
using RuiJi.Net.Node.Feed.LTS;
using ZooKeeperNet;

namespace RuiJi.Net.Node.Feed
{
    public class FeedNode : NodeBase
    {
        private FeedScheduler scheduler;

        public FeedNode(string baseUrl, string zkServer, string proxyUrl) : base(baseUrl, zkServer, proxyUrl)
        {
        }

        ~FeedNode()
        {
            FeedScheduler.GetSecheduler(BaseUrl).Stop();
        }

        protected override void OnStartup()
        {
            base.CreateLiveNode("/live_nodes/feed/" + BaseUrl, null);

            var stat = zooKeeper.Exists("/config/feed/" + BaseUrl, false);
            if (stat == null)
            {
                var d = new NodeConfig()
                {
                    Name = BaseUrl,
                    baseUrl = BaseUrl,
                    Proxy = ProxyUrl
                };
                zooKeeper.Create("/config/feed/" + BaseUrl, JsonConvert.SerializeObject(d).GetBytes(), Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent);
            }

            scheduler = new FeedScheduler();
            scheduler.Start(BaseUrl, this);
        }

        protected override NodeTypeEnum SetNodeType()
        {
            return NodeTypeEnum.FEED;
        }

        public override void Stop()
        {
            if (scheduler != null)
            {
                scheduler.Stop();
                scheduler = null;
            }

            base.Stop();
        }
    }
}