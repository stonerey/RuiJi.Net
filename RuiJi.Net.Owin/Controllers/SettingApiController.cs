﻿using Newtonsoft.Json;
using RestSharp;
using RuiJi.Net.Core.Crawler;
using RuiJi.Net.Core.Utils;
using RuiJi.Net.Core.Utils.Page;
using RuiJi.Net.Node.Db;
using RuiJi.Net.NodeVisitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace RuiJi.Net.Owin.Controllers
{
    public class SettingApiController : ApiController
    {
        #region 节点函数
        [HttpGet]
        [NodeRoute(Target = NodeProxyTypeEnum.Feed)]
        public object Funcs(int offset, int limit)
        {
            var node = ServerManager.Get(Request.RequestUri.Authority);

            var paging = new Paging();
            paging.CurrentPage = (offset / limit) + 1;
            paging.PageSize = limit;

            var list = FuncLiteDb.GetModels(paging);

            return new
            {
                rows = list,
                total = list.Count
            };
        }

        [HttpGet]
        [NodeRoute(Target = NodeProxyTypeEnum.Feed)]
        public object GetFunc(int id)
        {
            return FuncLiteDb.Get(id);
        }

        [HttpPost]
        public object FuncTest(FuncModel func)
        {
            var code = "{# " + func.Sample + " #}";
            var test = new ComplieFuncTest(func.Code);
            return test.Compile(code);
        }

        [HttpPost]
        [NodeRoute(Target = NodeProxyTypeEnum.Feed)]
        public object UpdateFunc(FuncModel func)
        {
            if (func.Name == "now" || func.Name == "ticks")
                return false;

            var f = FuncLiteDb.Get(func.Name);
            if (f != null && f.Id == 0)
                return false;

            FuncLiteDb.AddOrUpdate(func);
            return true;
        }

        [HttpGet]
        public bool RemoveFunc(string ids)
        {
            var removes = ids.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(m => Convert.ToInt32(m)).ToArray();

            return FuncLiteDb.Remove(removes);
        }
        #endregion

        #region Proxys
        [HttpGet]
        public object Proxys(int offset, int limit)
        {
            var node = ServerManager.Get(Request.RequestUri.Authority);

            if (node.NodeType == Node.NodeTypeEnum.CRAWLERPROXY)
            {
                var paging = new Paging();
                paging.CurrentPage = (offset / limit) + 1;
                paging.PageSize = limit;

                return new
                {
                    rows = ProxyLiteDb.GetModels(paging),
                    total = paging.Count
                };
            }
            else
            {
                var baseUrl = ProxyManager.Instance.Elect(NodeVisitor.NodeProxyTypeEnum.Crawler);

                var client = new RestClient("http://" + baseUrl);
                var restRequest = new RestRequest("api/proxys");
                restRequest.Method = Method.GET;

                var restResponse = client.Execute(restRequest);

                var response = JsonConvert.DeserializeObject<object>(restResponse.Content);

                return response;
            }
        }

        [HttpPost]
        public object UpdateProxy(ProxyModel proxy)
        {
            ProxyLiteDb.AddOrUpdate(proxy);

            return true;
        }

        [HttpGet]
        public object GetProxy(int id)
        {
            var feed = ProxyLiteDb.Get(id);

            return feed;
        }

        [HttpGet]
        public bool RemoveProxy(string ids)
        {
            var removes = ids.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(m => Convert.ToInt32(m)).ToArray();

            return ProxyLiteDb.Remove(removes);
        }

        [HttpGet]
        public int ProxyPing(int id)
        {
            try
            {
                var watch = new Stopwatch();
                watch.Start();

                var crawler = new RuiJiCrawler();
                var request = new Request("http://2017.ip138.com/ic.asp");

                var proxy = ProxyLiteDb.Get(id);
                var host = (proxy.Type == Node.Db.ProxyTypeEnum.HTTP ? "http" : "https") + proxy.Ip;
                request.Proxy = new RequestProxy(host, proxy.Port, proxy.UserName, proxy.Password);

                var response = crawler.Request(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    watch.Stop();
                    return watch.Elapsed.Milliseconds;
                }
            }
            catch
            {
                return -2;
            }

            return -1;
        }
        #endregion
    }

    public class ComplieFuncTest : CompileUrl
    {
        private string code;

        public ComplieFuncTest(string code)
        {
            this.code = code;
        }

        public override string FormatCode(CompileExtract extract)
        {
            var formatCode = string.Format(code, extract.Args);

            return formatCode;
        }
    }
}
