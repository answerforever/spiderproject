using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.Pipeline;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace spiderproject
{
    /// <summary>
    /// 建航国际货运
    /// </summary>
    public class EumexSpider : EntitySpider
    {
        public EumexSpider() : base("EumexSpider", new Site
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36",
            Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
            Headers = new Dictionary<string, string>
                {
                    { "Accept-Encoding"  ,"gzip, deflate, sdch, br" },
                    { "Upgrade-Insecure-Requests"  ,"1" },
                    { "Accept-Language"  ,"en,en-US;q=0.8" },
                    { "Cache-Control" , "ax-age=0" },
                }
        })
        {
        }

        protected override void MyInit(params string[] arguments)
        {
            //var request = new Request("http://www.51eumex.com/freight_search.json");
            //request.Method = HttpMethod.Post;
            //request.PostBody = "nowPage=1&pageSize=30&startPortCode=SHANGHAI&destPortCode=CHICAGOIL&sailTime=&orderName=&orderRule=&boatCompany=&shortPorts=0&cycle=0&sales_num=0&available_ratio=0&company_id=3006&token=26fb6d951556a2e9cc2db63750753fb2-1517918395224-3464-0ec4861c3b9f44ff1e0ee40cf480aceb-35ebd44cfa19c0450152121f332cc4fc-3006-5b699d3460713d71c59a411610097b44-0";
            //AddStartRequest(request);
            var startPortCode = "SHANGHAI";
            var destPortCode = "CHICAGOIL";
            AddStartUrl($"http://www.51eumex.com/freight_search.json?nowPage=1&pageSize=30&startPortCode={startPortCode}&destPortCode={destPortCode}&sailTime=&orderName=&orderRule=&boatCompany=&shortPorts=0&cycle=0&sales_num=0&available_ratio=0&company_id=3006&token=26fb6d951556a2e9cc2db63750753fb2-1517918395224-3464-0ec4861c3b9f44ff1e0ee40cf480aceb-35ebd44cfa19c0450152121f332cc4fc-3006-5b699d3460713d71c59a411610097b44-0"
            ,new Dictionary<string, dynamic> { { "SpiderCompanyCode", "4" } });

            Downloader.AddAfterDownloadCompleteHandler(new CutoutHandler("data", "count", 6, 7));

            AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=192.168.10.171 ;User ID=root;Password=sr@12345;Port=3306"));
            AddEntityType<OceanFreightInfo>();
        }
        
        [EntityTable("spider", "itemfclinfo")]
        [EntitySelector(Expression = "$.[*]", Type = SelectorType.JsonPath)]
        class OceanFreightInfo : SpiderEntity
        {
            [PropertyDefine(Expression = "$.id", Type = SelectorType.JsonPath, Length = 35)]
            public string oceanfreightid { get; set; }

            [PropertyDefine(Expression = "$.price_20gp", Type = SelectorType.JsonPath, Length = 25)]
            public string ctn20 { get; set; }

            [PropertyDefine(Expression = "$.price_40gp", Type = SelectorType.JsonPath, Length = 25)]
            public string ctn40 { get; set; }

            [PropertyDefine(Expression = "$.price_40hq", Type = SelectorType.JsonPath, Length = 25)]
            public string ctn40hq { get; set; }

            [PropertyDefine(Expression = "$.price_45hq", Type = SelectorType.JsonPath, Length = 25)]
            public string ctn45hq { get; set; }

            [PropertyDefine(Expression = "$.currency_code", Type = SelectorType.JsonPath, Length = 25)]
            public string currency { get; set; }

            [PropertyDefine(Expression = "$.CompanyName", Type = SelectorType.JsonPath, Length = 100)]
            public string companyname { get; set; }

            [PropertyDefine(Expression = "$.start_time", Type = SelectorType.JsonPath, Length = 30)]
            public string begindate { get; set; }

            [PropertyDefine(Expression = "$.end_time", Type = SelectorType.JsonPath, Length = 30)]
            public string expireddate { get; set; }

            [PropertyDefine(Expression = "$.boat_cycle", Type = SelectorType.JsonPath, Length = 25)]
            public string sailingstr { get; set; }

            [PropertyDefine(Expression = "$.start_port_name_en", Type = SelectorType.JsonPath, Length = 50)]
            public string fromterminalname { get; set; }

            [PropertyDefine(Expression = "$.dest_port_name_en", Type = SelectorType.JsonPath, Length = 50)]
            public string toterminalname { get; set; }

            [PropertyDefine(Expression = "$.carrier_code", Type = SelectorType.JsonPath, Length = 50)]
            public string oceancarriercode { get; set; }

            [PropertyDefine(Expression = "$.carrier_name_cn", Type = SelectorType.JsonPath, Length = 100)]
            public string oceancarriername { get; set; }

            [PropertyDefine(Expression = "SpiderCompanyCode", Type = SelectorType.Enviroment, Length = 20)]
            public string SpiderCompanyCode { get; set; }
        }

    }
}
