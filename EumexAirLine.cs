using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using System.Collections.Generic;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Processor;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension;

namespace spiderproject
{
    public class EumexAirLine : EntitySpider
    {
        public EumexAirLine() : base("EumexAirLine", new Site
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
        { }

        protected override void MyInit(params string[] arguments)
        {
            var token = "b4b147bc522828731f1a016bfa72c073-1504522840550-0-364629a1e95e9f9450ab945ae3adeeb0-35ebd44cfa19c0450152121f332cc4fc-0-44cbce77ea242ed3b5ba50d4a78f31a1-0";
            AddStartUrl(string.Format("http://www.51eumex.com/port/search_start_port.json?token={0}", token));
            Downloader.AddAfterDownloadCompleteHandler(new CutoutHandler("data", "message", 6, 9));
            AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=192.168.10.171 ;User ID=root;Password=sr@12345;Port=3306"));
            AddPageProcessor(new FormProcessor());
        }

        private class FormProcessor : EntityProcessor<AirLineModel>
        {
            public FormProcessor()
            {
                // 定义目标页的筛选
                //TargetUrlsExtractor = new RegionAndPatternTargetUrlsExtractor(".", "^http://www\\.51eumex.com/$", "http://www\\.51eumex.com/port/search_start_port.json?token=b4b147bc522828731f1a016bfa72c073-1504522840550-0-364629a1e95e9f9450ab945ae3adeeb0-35ebd44cfa19c0450152121f332cc4fc-0-44cbce77ea242ed3b5ba50d4a78f31a1-0");
            }

            protected override void Handle(Page page)
            {
                // 利用 Selectable 查询并构造自己想要的数据对象
                var formElements = page.Selectable.SelectList(Selectors.JsonPath("$.[*]")).Nodes();
                List<AirLineModel> results = new List<AirLineModel>();
                foreach (var form in formElements)
                {
                    var info = new AirLineModel();
                    info.name = form.Select(Selectors.JsonPath("$.port_name_cn")).GetValue();
                    info.code = form.Select(Selectors.JsonPath("$.port_code")).GetValue();
                    if (form.Select(Selectors.JsonPath("$.is_default")).GetValue() != null && form.Select(Selectors.JsonPath("$.is_default")).GetValue() != "")
                    {
                        info.type = "1";
                    }
                    else
                    {
                        info.type = "2";
                    }
                    info.bpid = "3";
                    results.Add(info);
                    if (info.type == "1")
                    {
                        var url = $"http://www.51eumex.com/port/search_dest_port.json?startPortCode=" + info.code + "&token=b4b147bc522828731f1a016bfa72c073-1504522840550-0-364629a1e95e9f9450ab945ae3adeeb0-35ebd44cfa19c0450152121f332cc4fc-0-44cbce77ea242ed3b5ba50d4a78f31a1-0";
                        page.AddTargetRequest(url);
                    }
                }
                // 以自定义KEY存入page对象中供Pipeline调用
                page.AddResultItem("spiderproject.EumexAirLine+AirLineModel", results);
                
            }
        }
        [EntityTable("spider", "airline", Uniques = new[] { "code" })]
        class AirLineModel: ISpiderEntity
        {
            /// <summary>
            /// 默认主键, 在插入数据的模式中, __Id 并没有什么作用. 在更新操作中, 需要把__id信息保存到Request的Extras中
            /// </summary>
            [PropertyDefine(Expression = "id", Type = SelectorType.Enviroment)]
            public int id { get; set; }
            [PropertyDefine(Length = 50)]
            public string name { get; set; }
            [PropertyDefine(Length = 50)]
            public string code { get; set; }
            [PropertyDefine(Length = 11)]
            public string type { get; set; }
            [PropertyDefine(Length = 11)]
            public string bpid { get; set; }
        }
    }
}
