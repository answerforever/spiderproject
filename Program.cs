using System;
using System.Text;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Extension.Model;
using DotnetSpider.Extension.Downloader;

namespace spiderproject
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            #region 掠食龙海运数据爬取 SpiderCompanyCode=1

            //EzcarrySpider oceanFreightSpider = new EzcarrySpider();
            //oceanFreightSpider.Run();

            #endregion

            #region 建航海运数据爬取 SpiderCompanyCode=4

            // EumexSpider eumexSpider = new EumexSpider();
            // eumexSpider.Run();

            #endregion
            
            #region 添加cookie 利丰供应链 SpiderCompanyCode=2

            //美西基港数据爬取
            // FileCookieInject inject = new FileCookieInject("a.cookies");
            // var ccliquoteSpider = new CcliquoteCookieSpider();
            // inject.Inject(ccliquoteSpider.Downloader, ccliquoteSpider, false);
            // ccliquoteSpider.Run();

            #endregion

            #region 宁波世荣 SpiderCompanyCode=3

            var webDriverCookieInject = new WebDriverCookieInjector
            {
               Url = "http://egi-freight.100jit.com/cpmembership/commonLog.ctrl",
               User = "18668535237",
               Password = "ycf5188++",
               UserSelector = new DotnetSpider.Extension.Model.SelectorAttribute("//*[@id='username']"),
               PasswordSelector = new DotnetSpider.Extension.Model.SelectorAttribute("//*[@id='password']"),
               SubmitSelector = new DotnetSpider.Extension.Model.SelectorAttribute("//*[@id='loginForm']/a[2]"),
               AfterLoginUrl = "http://egi-freight.100jit.com/marketing-portal-rest/rest/home"
            };
            var egiFreightSpider = new EgiFreightSpider();
            egiFreightSpider.Downloader = new CustomPostJsonHttpClientDownloader();
            webDriverCookieInject.Inject(egiFreightSpider.Downloader, egiFreightSpider, false);
            egiFreightSpider.Run();

            #endregion
            
            Console.Read();
        }
    }
}
