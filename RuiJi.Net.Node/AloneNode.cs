﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RuiJi.Net.Core.Utils.Log;

namespace RuiJi.Net.Node
{
    public class AloneNode : INode
    {
        public string BaseUrl { get; protected set; }

        public NodeTypeEnum NodeType { get; protected set; }

        public DateTime StartTime { get; protected set; }

        public bool IsLeader { get; protected set; }

        public AloneNode(string baseUrl)
        {
            this.BaseUrl = baseUrl;
            this.NodeType = NodeTypeEnum.ALONE;

            StartTime = DateTime.Now;
        }

        public NodeData GetData(string path)
        {
            return new NodeData();
        }

        public void SetData(string path, string data, int version = -1)
        {
            
        }

        public void Start()
        {
            Logger.Add(BaseUrl, new List<IAppender> {
                new RollingFileAppender(BaseUrl),
                new MemoryAppender(),
                new ConsoleAppender()
            });

            Logger.GetLogger(BaseUrl).Info("Start WebApiServer At http://" + BaseUrl + " with " + NodeType.ToString() + " node");
        }

        public void Stop()
        {
           
        }
    }
}