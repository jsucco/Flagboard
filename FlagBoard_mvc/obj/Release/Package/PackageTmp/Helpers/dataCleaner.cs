using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlagBoard_mvc.Helpers
{
	public class dataCleaner
	{
        public static void repairCompData()
        {
            var service = new Helpers.ManagerService();

            var cids = service.getLocations();

            if (cids.Count == 0)
                return;
            AprService aprservice = new AprService();

            cids = aprservice.filterInActiveLocations(cids); 
            foreach (var item in cids)
            {
                try
                {
                    CtxService CtxService = new CtxService(null, item.CID.Trim());

                    CtxService.updateCompDates();
                } catch (Exception ex)
                {

                }
             
            }
            

        }
	}
}