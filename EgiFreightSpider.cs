using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace spiderproject
{
    public class EgiFreightSpider : EntitySpider
    {
        public EgiFreightSpider() : base("EgiFreightSpider", new Site
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36",
            Accept = "application/json, text/javascript,*/*;q=0.01",
            Headers = new Dictionary<string, string>
                {
                    { "Accept-Encoding"  ,"gzip, deflate, sdch" },
                    { "Upgrade-Insecure-Requests"  ,"1" },
                    { "Accept-Language"  ,"en,en-US;q=0.8" },
                    { "Cache-Control" , "ax-age=0" },
                    //{ "Content-Type","application/json"},
                    { "Host","egi-freight.100jit.com"}
                }
        })
        {
        }

        protected override void MyInit(params string[] arguments)
        {
            var request = new Request("http://egi-freight.100jit.com/marketing-portal-rest/rest/efcl/queryFclList", new Dictionary<string, dynamic> { { "SpiderCompanyCode", "3" },{ "Currency","CNY" } });
            request.Method = HttpMethod.Post;
            
            var queryFreight = new QueryFreight
            {
                queryConditions = JsonConvert.SerializeObject(new QueryCondition
                {
                    dischargeport = "AALBORG",
                    loadport = "NINGBO"
                }),
                pageSize = 30,
                pageNum = 1,
                sortColumn = "internetsellprice2",
                sortBy = "asc",
                biKey = "4842191518059353663",
                isPage = false
            };
            request.PostBody = JsonConvert.SerializeObject(queryFreight);

            AddStartRequest(request);

            //Downloader = new CustomPostJsonHttpClientDownloader();

            Downloader.AddAfterDownloadCompleteHandler(new CustomCutoutHandler("list", "firstPage", queryFreight, 6,11));

            AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=192.168.10.171 ;User ID=root;Password=sr@12345;Port=3306"));
            AddEntityType<OceanFreightInfo>();
        }

        [EntityTable("spider", "itemfclinfo", Uniques = new[] { "oceanfreightid" })]
        [EntitySelector(Expression = "$.[*]", Type = SelectorType.JsonPath)]
        class OceanFreightInfo : SpiderEntity
        {
            [PropertyDefine(Expression = "$.casenumber", Type = SelectorType.JsonPath, Length = 35)]
            public string oceanfreightid { get; set; }

            [PropertyDefine(Expression = "$.price1", Type = SelectorType.JsonPath, Length = 25)]
            public string ctn20 { get; set; }

            [PropertyDefine(Expression = "$.price2", Type = SelectorType.JsonPath, Length = 25)]
            public string ctn40 { get; set; }

            [PropertyDefine(Expression = "$.price3", Type = SelectorType.JsonPath, Length = 25)]
            public string ctn40hq { get; set; }

            [PropertyDefine(Expression = "$.price4", Type = SelectorType.JsonPath, Length = 25)]
            public string ctn45hq { get; set; }

            [PropertyDefine(Expression = "Currency", Type = SelectorType.Enviroment, Length = 25)]
            public string currency { get; set; }

            [PropertyDefine(Expression = "$.CompanyName",IgnoreStore =true, Type = SelectorType.JsonPath, Length = 100)]
            public string companyname { get; set; }

            [PropertyDefine(Expression = "$.begindate", Type = SelectorType.JsonPath, Length = 30)]
            public string begindate { get; set; }

            [PropertyDefine(Expression = "$.validdate", Type = SelectorType.JsonPath, Length = 30)]
            public string expireddate { get; set; }

            [PropertyDefine(Expression = "$.sailtime", Type = SelectorType.JsonPath, Length = 25)]
            public string sailingstr { get; set; }

            [PropertyDefine(Expression = "$.loadport", Type = SelectorType.JsonPath, Length = 50)]
            public string fromterminalname { get; set; }

            [PropertyDefine(Expression = "$.dischargeport", Type = SelectorType.JsonPath, Length = 50)]
            public string toterminalname { get; set; }

            [PropertyDefine(Expression = "$.carrierId", Type = SelectorType.JsonPath, Length = 50)]
            public string oceancarriercode { get; set; }

            [PropertyDefine(Expression = "$.carrier", Type = SelectorType.JsonPath, Length = 100)]
            public string oceancarriername { get; set; }

            [PropertyDefine(Expression = "SpiderCompanyCode", Type = SelectorType.Enviroment, Length = 20)]
            public string SpiderCompanyCode { get; set; }
        }

        /// <summary>
        /// 集合分页数据实体
        /// </summary>
        public class Pagination
        {
            /// <summary>
            /// 当前页
            /// </summary>
            public int PageIndex { get; set; }

            /// <summary>
            /// 页面大小
            /// </summary>
            public int PageSize { get; set; }

            /// <summary>
            /// 总记录数
            /// </summary>
            public int RecordCount { get; set; }

            /// <summary>
            /// 总页数
            /// </summary>
            public int PageCount { get; set; }

            /// <summary>
            /// 总页数
            /// </summary>
            public int TotalPage
            {
                get
                {
                    if (PageSize == 0) return 0;
                    var totalPage = Math.Ceiling((double)RecordCount / PageSize);
                    return (int)totalPage;
                }
            }
        }

        public class QueryFreight
        {
            public string queryConditions { get; set; }

            public int pageNum { get; set; }

            public int pageSize { get; set; }

            public string sortColumn { get; set; }

            public string sortBy { get; set; }

            public string biKey { get; set; }

            public bool isPage { get; set; }
        }

        public class QueryCondition
        {
            public string dischargeport { get; set; }

            public string loadport { get; set; }            
        }

        /// <summary>
        /// Handler that cutout <see cref="Page.Content"/>.
        /// </summary>
        /// <summary xml:lang="zh-CN">
        /// 截取下载内容的处理器
        /// </summary>
        public class CustomCutoutHandler : AfterDownloadCompleteHandler
        {
            private readonly string _startPart;
            private readonly string _endPart;
            private readonly int _startOffset;
            private readonly int _endOffset;

            private readonly QueryFreight _queryFreight;


            /// <summary>
            /// Construct a CutoutHandler instance, it will cutout <see cref="Page.Content"/> from index of <paramref name="startPart"/> 
            /// with <paramref name="startOffset"/> to index of <paramref name="endPart"/> with <paramref name="endOffset"/>.
            /// </summary>
            /// <summary xml:lang="zh-CN">
            /// 构造方法
            /// </summary>
            /// <param name="startPart">起始部分的内容</param>
            /// <param name="endPart">结束部分的内容</param>
            /// <param name="startOffset">开始截取的偏移</param>
            /// <param name="endOffset">结束截取的偏移</param>
            public CustomCutoutHandler(string startPart, string endPart, QueryFreight queryFreight , int startOffset = 0, int endOffset = 0)
            {
                _startPart = startPart;
                _endOffset = endOffset;
                _endPart = endPart;
                _startOffset = startOffset;
                _queryFreight = queryFreight;
            }

            /// <summary>
            /// Cutout <see cref="Page.Content"/>.
            /// </summary>
            /// <summary>
            /// 截取下载内容
            /// </summary>
            /// <param name="page">页面数据</param>
            /// <param name="downloader">下载器</param>
            /// <param name="spider">爬虫</param>
            /// <exception cref="SpiderException"></exception>
            public override void Handle(ref Page page, IDownloader downloader, ISpider spider)
            {
                if (page == null || string.IsNullOrWhiteSpace(page.Content) || page.Skip)
                {
                    return;
                }

                string rawText = page.Content;

                var pagerecordReg = "\"nextPage\":\\d+";
                Regex pagerecordMatch = new Regex(pagerecordReg, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                Match m = pagerecordMatch.Match(page.Content);
                if (m.Success)
                {
                    var pageStr = m.Groups[0];
                    var nextPageStr = pageStr.Value.Split(":")[1];
                    var nextPage = 0;
                    int.TryParse(nextPageStr,out nextPage);
                    if (nextPage > 0)
                    {
                        _queryFreight.pageNum = nextPage;
                        var postData = JsonConvert.SerializeObject(_queryFreight);
                        var url = page.Url;
                        var request = new Request(url, new Dictionary<string, dynamic> { { "SpiderCompanyCode", "3" }, { "Currency", "CNY" } });
                        request.Method = HttpMethod.Post;
                        request.PostBody = postData;
                        page.AddTargetRequest(request);
                    }
                }

                int begin = rawText.IndexOf(_startPart, StringComparison.Ordinal);

                if (begin < 0)
                {
                    throw new SpiderException($"Cutout failed, can not find begin string: {_startPart}.");
                }

                int end = rawText.IndexOf(_endPart, begin, StringComparison.Ordinal);
                int length = end - begin;

                begin += _startOffset;
                length -= _startOffset;
                length -= _endOffset;
                length += _endPart.Length;

                if (begin < 0 || length < 0)
                {
                    throw new SpiderException("Cutout failed. Please check your settings.");
                }

                string newRawText = rawText.Substring(begin, length).Trim();
                page.Content = newRawText;
            }
        }

        

    }
}
