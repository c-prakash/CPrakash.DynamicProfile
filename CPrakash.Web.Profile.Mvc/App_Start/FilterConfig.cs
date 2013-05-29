using System.Web;
using System.Web.Mvc;

namespace CPrakash.Web.Profile.Mvc
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}