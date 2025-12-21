using System.Text;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ExamSchedulingSystem
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest()
        {
            // Her istekte UTF-8 encoding garantisi
            Response.ContentType = "text/html; charset=utf-8";
            Response.Charset = "utf-8";
            Response.HeaderEncoding = Encoding.UTF8;
        }
    }
}

