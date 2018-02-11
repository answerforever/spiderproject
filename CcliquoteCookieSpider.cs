using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model.Formatter;
using DotnetSpider.Extension.Pipeline;
using DotnetSpider.Extension.Processor;
using System;
using System.Collections.Generic;
using System.Text;

namespace spiderproject
{
    public class CcliquoteCookieSpider : EntitySpider
    {
        public CcliquoteCookieSpider() : base("CcliquoteCookieSpider", new Site
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
            //美西基港
            var portCode =new List<string>{ "Honolulu,HI", "Long Beach,CA", "LOS angeles,CA", "Oakland,CA", "Portland,OR", "Seattle,wa", "Tacoma,WA" };
            foreach (var item in portCode)
            {
                AddStartUrl($"https://ccliquote.lflogistics.net/yjcx.asp?gk={item}&gk1=Ningbo");
            }

            //var cookieDictionary = new Dictionary<string, string>
            //{
            //    { "ASPSESSIONIDSURABTAR"  ,"HIGPPBKAFPKCPHMEFABEOCKD" },
            //    { "UM_distinctid"  ,"1614c01aef2b54-05a0dbb3a3d167-4323461-1fa400-1614c01aef3108f" },
            //    { "username","NBMARKET4%40SOUTH%2DLOGISTICS%2ECOM" },
            //    { "ASP.NET_SessionId" , "gt42movchb3v3lzma301mxlt" },
            //    { "ASPSESSIONIDQWRCATAR" , "NLHNBECBBIDILKPPAAMNFDJP" },
            //    { "ASPSESSIONIDQUSCATBQ","EANDAEMDGIAFBIOGMEEMENHF"},
            //    { "ASPSESSIONIDSUSACSBQ","EGILBEHABCPONCCNJCBGLOIN"},
            //    { "CNZZDATA1254109862","35965116-1517396995-%7C1517883080"}
            //};

            //Downloader.AddCookies(cookieDictionary, "ccliquote.lflogistics.net");
            AddPageProcessor(new CustomPageprocessor());
            AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=192.168.10.171 ;User ID=root;Password=sr@12345;Port=3306"));
        }

        class CustomPageprocessor : EntityProcessor<OceanFreightInfo>
        {
            public CustomPageprocessor()
            {                
            }

            protected override void Handle(Page page)
            {
                // 利用 Selectable 查询并构造自己想要的数据对象
                var formElements = page.Selectable.SelectList(Selectors.XPath(".//*[@id='KH_table']/tr[position()>2]")).Nodes();
                List<OceanFreightInfo> results = new List<OceanFreightInfo>();
                var currency = "CNY";
                var companyCode = "2";
                foreach (var form in formElements)
                {
                    var info = new OceanFreightInfo();
                    var link = form.Select(Selectors.XPath(".//td[15]/a/@href")).GetValue();
                    if (!string.IsNullOrEmpty(link) && link.IndexOf('=') > 0)
                    {
                        info.oceanfreightid = System.Web.HttpUtility.UrlDecode(link.Substring(link.IndexOf('=') + 1));
                    }
                    else
                    {
                        info.oceanfreightid = Guid.NewGuid().ToString();
                    }
                    info.currency = currency;
                    info.fromterminalname = form.Select(Selectors.XPath(".//td[2]")).GetValue();
                    info.toterminalname = form.Select(Selectors.XPath(".//td[4]")).GetValue();
                    info.oceancarriername = form.Select(Selectors.XPath(".//td[13]")).GetValue();
                    info.sailingstr = form.Select(Selectors.XPath(".//td[6]")).GetValue();
                    info.ctn20 = form.Select(Selectors.XPath(".//td[9]")).GetValue().Replace("\r\n", "").Replace("\t", "").Replace(" ", "");
                    info.ctn40 = form.Select(Selectors.XPath(".//td[10]")).GetValue().Replace("\r\n", "").Replace("\t", "").Replace(" ", "");
                    info.ctn40hq = form.Select(Selectors.XPath(".//td[11]")).GetValue().Replace("\r\n", "").Replace("\t", "").Replace(" ", "");
                    info.ctn45hq = form.Select(Selectors.XPath(".//td[12]")).GetValue().Replace("\r\n", "").Replace("\t", "").Replace(" ", "");
                    info.SpiderCompanyCode = companyCode;
                    info.CDate = DateTime.Now;
                    var dateContent = form.Select(Selectors.XPath(".//td[14]")).GetValue();
                    if (!string.IsNullOrEmpty(dateContent))
                    {
                        var dateArr = dateContent.Split("<br>");
                        if (dateArr.Length > 1)
                        {
                            info.expireddate = dateArr[1].Replace("\r\n", "").Replace("\t", "").Replace(" ", "");
                        }
                        info.begindate = dateArr[0].Replace("\r\n", "").Replace("\t", "").Replace(" ", "");

                    }
                    results.Add(info);
                }
                // 以自定义KEY存入page对象中供Pipeline调用
                page.AddResultItem("spiderproject.CcliquoteCookieSpider+OceanFreightInfo", results);

            }
        }

        [EntityTable("spider", "itemfclinfo", Uniques = new[] { "oceanfreightid" })]
        class OceanFreightInfo : SpiderEntity
        {
            /// <summary>
            /// 航线编号
            /// </summary>
            [PropertyDefine(Length = 35)]
            public string oceanfreightid { get; set; }

            /// <summary>
            /// 币种
            /// </summary>
            [PropertyDefine(Length = 35)]
            public string currency { get; set; }

            /// <summary>
            /// 起运港
            /// </summary>
            [PropertyDefine(Length = 35)]
            public string fromterminalname { get; set; }

            /// <summary>
            /// 目的港
            /// </summary>
            [PropertyDefine(Length = 35)]
            public string toterminalname { get; set; }

            /// <summary>
            /// 船公司
            /// </summary>
            [PropertyDefine(Length = 35)]
            public string oceancarriername { get; set; }            

            /// <summary>
            /// 路经
            /// </summary>
            //[PropertyDefine(Expression = ".//td[5]", Type = SelectorType.XPath, Length = 35)]
            //public string routing { get; set; }

            /// <summary>
            /// 船期
            /// </summary>
            [PropertyDefine(Length = 35)]
            public string sailingstr { get; set; }

            /// <summary>
            /// 20GP价格
            /// </summary>
            [PropertyDefine(Length = 25)]
            public string ctn20 { get; set; }

            /// <summary>
            /// 40GP价格
            /// </summary>
            [PropertyDefine(Length = 25)]
            public string ctn40 { get; set; }

            /// <summary>
            /// 40HQ价格
            /// </summary>
            [PropertyDefine(Length = 25)]
            public string ctn40hq { get; set; }

            /// <summary>
            /// 45HQ价格
            /// </summary>
            [PropertyDefine( Length = 25)]
            public string ctn45hq { get; set; }

            [PropertyDefine(Length = 50)]
            public string begindate { get; set; }

            [PropertyDefine(Length = 50)]
            public string expireddate { get; set; }

            [PropertyDefine(Length = 20)]
            public string SpiderCompanyCode { get; set; }
        }

    }
}
