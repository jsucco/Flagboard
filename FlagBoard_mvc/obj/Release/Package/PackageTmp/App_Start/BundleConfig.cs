using System.Web;
using System.Web.Optimization;

namespace FlagBoard_mvc
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-1.11.1.min.js",
                        "~/Scripts/jquery-ui.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/fb_index").Include(
                        "~/Scripts/grid.locale-en.js", 
                        "~/Scripts/select2.js", 
                        "~/Scripts/jquery.jqGrid.min.js"
                ));

            bundles.Add(new StyleBundle("~/Content/fb_index_style").Include(
                        "~/Content/select2.css",
                        "~/Content/ui.jqgrid.css",
                        "~/Content/Index.css",
                        "~/Content/Timers.css"
                ));

        }
    }
}
