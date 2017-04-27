﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FlagBoard_mvc.Models.EF;

namespace FlagBoard_mvc.Helpers
{
    public class AprService
    {
        private AprManagerContext context { get; set; }
        public AprService(AprManagerContext _context = null)
        {
            context = (_context != null) ? _context : new AprManagerContext(); 
        }

        public List<Corporate> filterInActiveLocations(List<Corporate> ctxlocations)
        {
            if (ctxlocations == null || ctxlocations.Count == 0)
                return ctxlocations;

            Dictionary<string, LocationMaster> activeLocations = getActiveLocations();
            List<Corporate> activeCtxLocations = new List<Corporate>(); 
            foreach (var item in ctxlocations)
            {
                if (activeLocations.ContainsKey(item.CID.Trim()))
                    activeCtxLocations.Add(item); 
            }
            return activeCtxLocations; 
        }

        public Dictionary<string, LocationMaster> getActiveLocations()
        {
            Dictionary<string, LocationMaster> list = new Dictionary<string, LocationMaster>();
            try
            {
                var activeList = (from x in context.LocationMasters select x).ToList();

                if (activeList != null)
                {
                    foreach (var item in activeList)
                    {
                        if (item.CID != null && item.CID.Trim().Length > 5)
                            list.Add(item.CID.Trim(), item);
                    }
                        
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex); 
            }
            finally
            {
                context.Dispose();
            }
            return list; 
        }
    }
}