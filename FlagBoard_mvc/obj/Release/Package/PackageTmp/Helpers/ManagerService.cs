using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FlagBoard_mvc.Models.EF; 

namespace FlagBoard_mvc.Helpers
{
    public class ManagerService
    {
        private CtxManagerContext context; 

        public ManagerService(CtxManagerContext _context = null)
        {
            context = (_context != null) ? _context : getcontext(); 
        }

        private CtxManagerContext getcontext()
        {
            return new CtxManagerContext(); 
        }

        public string BuildConnectionString(string CID)
        {
            Corporate CorporateRecord = null;

            if (CID.Length != 6)
                throw new Exception("CID Parameter formatted incorrectly.  Must be 6 characters long"); 
                try
                {
                    CorporateRecord = (Corporate)HttpContext.Current.Cache["FlagBoardManagerService_CID_" + CID];
                } catch
                {

                }
                if (CorporateRecord == null || CorporateRecord.DBName.Length == 0)
                    CorporateRecord = getRecord(CID); 

                if (CorporateRecord == null)
                    throw new Exception("Corporate Connection String not found based on CID");

                if (CorporateRecord.DBName.Length == 0)
                    throw new Exception("No Database Found in CtxManager");
                
            return "Data Source=10.5.4.10;Initial Catalog=" + CorporateRecord.DBName + ";Persist Security Info=True;User ID=sa;Password=textyler";
        }

        public Corporate getRecord(string CID)
        {
            Corporate CorporateRecord = null;
            try
            {
                CorporateRecord = (from x in context.Corporates where x.CID == CID select x).FirstOrDefault();

                if (CorporateRecord != null && CorporateRecord.DBName.Length > 0)
                    HttpContext.Current.Cache.Insert("FlagBoardManagerService_CID_" + CID, CorporateRecord, null, DateTime.Now.AddDays(14), System.Web.Caching.Cache.NoSlidingExpiration);
            } finally
            {
                context.Dispose(); 
            }

            return CorporateRecord; 
        }

        public List<Corporate> getLocations()
        {
            return (from x in context.Corporates select x).ToList();
        }
    }
}