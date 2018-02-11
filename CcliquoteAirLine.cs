using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using System;
using System.Collections.Generic;
using System.Text;

namespace spiderproject
{
    public class CcliquoteAirLine : EntitySpider
    {
        public CcliquoteAirLine() : base("EumexAirLine", new Site
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
            AddStartUrl(string.Format("https://ccliquote.lflogistics.net/ajaxHandler.asp?act=pol"));
            Downloader.AddAfterDownloadCompleteHandler(new CutoutHandler("table", "]}", 7, 1));
            AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=192.168.10.171 ;User ID=root;Password=sr@12345;Port=3306"));
            AddPageProcessor(new FormProcessor());
        }

        private class FormProcessor : EntityProcessor<AirLineModel>
        {
            public FormProcessor()
            {
                // 定义目标页的筛选
            }
            protected override void Handle(Page page)
            {
                // 利用 Selectable 查询并构造自己想要的数据对象
                var formElements = page.Selectable.SelectList(Selectors.JsonPath("$.[*]")).Nodes();
                List<AirLineModel> results = new List<AirLineModel>();
                foreach (var form in formElements)
                {
                    var info = new AirLineModel();
                    info.name = "";
                    if(form.Select(Selectors.JsonPath("$.pol")).GetValue() != null && form.Select(Selectors.JsonPath("$.pol")).GetValue() != "")
                    {
                        info.code = form.Select(Selectors.JsonPath("$.pol")).GetValue();
                        info.type = "1";
                    }
                    else if(form.Select(Selectors.JsonPath("$.gk")).GetValue() != null && form.Select(Selectors.JsonPath("$.gk")).GetValue() != "")
                    {
                        info.code = form.Select(Selectors.JsonPath("$.gk")).GetValue();
                        info.type = "2";
                    }
                    info.bpid = "2";
                    results.Add(info);
                    if (info.type == "1")
                    {
                        page.AddTargetRequest(string.Format("https://ccliquote.lflogistics.net/ajaxHandler.asp?act=gk&pol={0}", info.code));
                    }
                }
                // 以自定义KEY存入page对象中供Pipeline调用
                page.AddResultItem("spiderproject.CcliquoteAirLine+AirLineModel", results);

            }
        }

        [EntityTable("spider", "airline", Uniques = new[] { "code" })]
        class AirLineModel : ISpiderEntity
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
