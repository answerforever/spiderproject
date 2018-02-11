using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace spiderproject
{
    /// <summary>
    /// 掠食龙
    /// </summary>
    public class EzcarrySpider : EntitySpider
    {
        public EzcarrySpider() : base("EzcarrySpider", new Site
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36",
            Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
            Headers = new Dictionary<string, string>
                        {
                            { "Accept-Encoding"  ,"gzip, deflate, sdch" },
                            { "Upgrade-Insecure-Requests"  ,"1" },
                            { "Accept-Language"  ,"en,en-US;q=0.8" },
                            { "Cache-Control" , "ax-age=0" },
                        }
        })
        {
        }

        protected override void MyInit(params string[] arguments)
        {
            //船公司编号集合
            // var shippingCompanyCodeArr = new string[] { "ANL","APL","ASL","BENLINE","BOHAI","CCL","CKS","CMA","CNC","CO-HEUNG","COSCO","CSC","CSCL","CUL","DJS","DYS","EAS","EMC","ESL",
            //    "HAM-SUD","HANSUNG","HASCO","HDS","HEUNG-A","HMM","HPL","IAL","JIAODONG","JJ","K-LINE","KMTC","MCC","MEL","MOL","MSC","MSK","MSL","NAMSUNG","NDS","NOS","NYK","NZL",
            //    "ONTO","OOCL","PAN CON","PIL","QMNS","RCL","RPS","RZF","SAF","SAMUDERA","SCI","SIMATECH","SINOKOR","SINOTRANS","SITC","SML","STAROCEAN","STX","SWIRE",
            //    "TCLC","TSLINE","UCL","WANHAI","WEIDONG","WFL","YANGZIJIANG","YML","ZIM" };

            var shippingCompanyCodeArr=new string[]{"MSK","COSCO"};

            foreach (var item in shippingCompanyCodeArr)
            {
                //var startUrl = $"https://www.ezcarry.com/OceanFreight/OceanFreightSedAjax?load=&discharge=&country=&ShippingLineName=&shippingLineId=&companyCode=&seqStr=&oecanCarrierName=MSK%2C&loadingTerminal=&dischargeTerminal=&transhipPort=&etd=&eta=&isVip=&isPrise=&page=1&sortName=&sortType=&queryType=";
                var startUrl = $"https://www.ezcarry.com/OceanFreight/OceanFreightSedAjax?country=&ShippingLineName=&shippingLineId=&companyCode=&oecanCarrierName={item}&etd=&eta=&page=1";
                AddStartUrl(startUrl,new Dictionary<string, dynamic> { { "SpiderCompanyCode", "1" } });
            }
            // var startUrl = $"https://www.ezcarry.com/OceanFreight/OceanFreightSedAjax?load=&discharge=&country=&ShippingLineName=&shippingLineId=&companyCode=&seqStr=&oecanCarrierName=MSK%2C&loadingTerminal=&dischargeTerminal=&transhipPort=&etd=&eta=&isVip=&isPrise=&page=1&sortName=&sortType=&queryType=";
            // AddStartUrl(startUrl);
            //var endStr= ","OceanFreightHistory":null,"PageListEntity":{"PageIndex":1,"PageSize":15,"RecordCount":2324,"PageCount":0},"OceanFreightInfo":null,"ServiceTypeAdjuestList":null,"SurchargeCurrencyList":null,"SurchargeList":null,"SurchargeType3List":null,"OceanScheduleList":null,"DiscussInteraction":null,"MaxPage":155},"Code":""}";

            Downloader.AddAfterDownloadCompleteHandler(new CustomCutoutHandler("OceanFreightList","OceanFreightHistory", 18,21));
              
            AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=192.168.10.171 ;User ID=root;Password=sr@12345;Port=3306"));
            AddEntityType<OceanFreightInfo>();
        }


        [EntityTable("spider", "itemfclinfo", Uniques = new[] { "oceanfreightid" })]
        [EntitySelector(Expression = "$.[*]", Type = SelectorType.JsonPath)]
        class OceanFreightInfo : SpiderEntity
        {
            [PropertyDefine(Expression = "$.OceanFreightId", Type = SelectorType.JsonPath, Length = 35)]
            public string oceanfreightid { get; set; }

            [PropertyDefine(Expression = "$.Ctn20", Type = SelectorType.JsonPath, Length = 25)]
            public string ctn20 { get; set; }

            [PropertyDefine(Expression = "$.Ctn40", Type = SelectorType.JsonPath, Length = 25)]
            public string ctn40 { get; set; }

            [PropertyDefine(Expression = "$.Ctn40Hq", Type = SelectorType.JsonPath, Length = 25)]
            public string ctn40hq { get; set; }

            [PropertyDefine(Expression = "$.Ctn45Hq", Type = SelectorType.JsonPath, Length = 25)]
            public string ctn45hq { get; set; }

            [PropertyDefine(Expression = "$.Currency", Type = SelectorType.JsonPath, Length = 25)]
            public string currency { get; set; }

            [PropertyDefine(Expression = "$.CompanyName", Type = SelectorType.JsonPath, Length = 100)]
            public string companyname { get; set; }

            [PropertyDefine(Expression = "$.BeginDate", Type = SelectorType.JsonPath, Length = 30)]
            public string begindate { get; set; }

            [PropertyDefine(Expression = "$.ExpiredDate", Type = SelectorType.JsonPath, Length = 30)]
            public string expireddate { get; set; }

            [PropertyDefine(Expression = "$.SailingStr", Type = SelectorType.JsonPath, Length = 25)]
            public string sailingstr { get; set; }

            [PropertyDefine(Expression = "$.LoadingPortName", Type = SelectorType.JsonPath, Length = 50)]
            public string fromterminalname { get; set; }

            [PropertyDefine(Expression = "$.DischargePortName", Type = SelectorType.JsonPath, Length = 50)]
            public string toterminalname { get; set; }

            [PropertyDefine(Expression = "$.OecanCarrierCode", Type = SelectorType.JsonPath, Length = 50)]
            public string oceancarriercode { get; set; }

            [PropertyDefine(Expression = "$.OecanCarrierName", Type = SelectorType.JsonPath, Length = 100)]
            public string oceancarriername { get; set; }

            [PropertyDefine(Expression = "SpiderCompanyCode", Type = SelectorType.Enviroment, Length = 20)]
            public string SpiderCompanyCode{ get;set; }
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
            public CustomCutoutHandler(string startPart, string endPart, int startOffset = 0, int endOffset = 0)
            {
                _startPart = startPart;
                _endOffset = endOffset;
                _endPart = endPart;
                _startOffset = startOffset;
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

                var pagerecordReg = "{\"pageindex\":.*\"pagecount\":0}";
                Regex pagerecordMatch = new Regex(pagerecordReg, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                Match m = pagerecordMatch.Match(page.Content);
                if (m.Success)
                {
                    var pageStr = m.Groups[0];
                    var paginationObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Pagination>(pageStr.Value);
                    if (paginationObj != null && paginationObj.TotalPage > 1)
                    {
                        var url = page.Url;
                        for (int i = 2; i <= paginationObj.TotalPage; i++)
                        {
                            string pattern = "page=\\d+";
                            string replacement = $"page={i}";
                            var targetUrl = Regex.Replace(url, pattern, replacement);
                            if (!string.IsNullOrEmpty(targetUrl))
                            {
                                page.AddTargetRequest(targetUrl);
                            }
                        }
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