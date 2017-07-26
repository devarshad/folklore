using System.Web;
using System.Web.Optimization;

namespace Folklore
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/base/js").Include(
                "~/Scripts/jquery-ui-{version}.min.js",
                "~/Scripts/jquery.validate.min.js",
                "~/Scripts/jquery.validate.unobtrusive.min.js",
                "~/Scripts/Date.min.js",
                "~/Scripts/jquery-custom.js",
                "~/Scripts/plugins.js"
                ));

            bundles.Add(new StyleBundle("~/bundles/Content/css").Include(
                      "~/Content/style.css"));
        }
    }
}
