using DotnetSpider.Core;
using DotnetSpider.Core.Selector;
using DotnetSpider.Extension;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace spiderproject
{
    public class EzcarryShipowner : EntitySpider
    {
        public EzcarryShipowner() : base("EzcarryShipowner")
        {
        }

        protected override void MyInit(params string[] arguments)
        {
            AddStartUrl(string.Format("https://www.ezcarry.com/OceanFreight/OceanFreight"), new Dictionary<string, dynamic> { { "bpid", "1" },{ "name",""} });
            AddPipeline(new MySqlEntityPipeline("Database='mysql';Data Source=192.168.10.171 ;User ID=root;Password=sr@12345;Port=3306"));
            AddEntityType<ShipownerEntry>();
        }
        [EntityTable("spider", "shipowner")]
        [EntitySelector(Expression = ".//ul[@class='clearfix']/li/input[@class='OceanCarrierName']", Type = SelectorType.XPath)]
        class ShipownerEntry : ISpiderEntity
        {
            /// <summary>
            /// 默认主键, 在插入数据的模式中, __Id 并没有什么作用. 在更新操作中, 需要把__id信息保存到Request的Extras中
            /// </summary>
            [PropertyDefine(Expression = "id", Type = SelectorType.Enviroment)]
            public int id { get; set; }
            [PropertyDefine(Expression = "./@value")]
            public string code { get; set; }
            [PropertyDefine(Expression = "name", Type = SelectorType.Enviroment, Length = 50)]
            public string name { get; set; }
            [PropertyDefine(Expression = "bpid", Type = SelectorType.Enviroment, Length = 50)]
            public string bpid { get; set; }
        }
    }
}
