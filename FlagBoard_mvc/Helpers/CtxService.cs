using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FlagBoard_mvc.Models.EF;
using FlagBoard_mvc.Models;
using System.Data.SqlClient;

namespace FlagBoard_mvc.Helpers
{
    public class CtxService : CtxCache
    {
        public bool error = false;
        public string errorMessage = ""; 

        public CtxService(CtxContext _context = null, string CID = "")
        {
            CID = CID.Trim();
            if (CID.Length < 6)
                throw new Exception("CID incorrect length");

            CID_ = CID;

            context = (_context != null) ? _context : getContext(CID); 
        }

        private CtxContext context;

        private ManagerService manager = new ManagerService();

        private string CID_; 

        private CtxContext getContext(string CID)
        {
            return new CtxContext(manager.BuildConnectionString(CID));
        }

        private void validateScheduleInputs(schedule record)
        {
            if (record == null)
                throw new Exception("Object to add cannot be null");
            if (record.MFB_Id == 0)
                throw new Exception("FlagBoard must be selected.");
            if (record.MM_Id == 0)
                throw new Exception("Machine must be selected.");
            if (record.EMP_ID == 0)
                throw new Exception("Employee must be selected.");
            if (record.MT_Id == 0)
                throw new Exception("Maintenance type must be selected.");
            if (record.MS_Maint_Code == 0)
                throw new Exception("Maintenance Code must be selected."); 
        }

        public int Update(schedule record)
        {
            var MS_ID = record.MS_Id;

            validateScheduleInputs(record); 
            if (MS_ID < 1)
                throw new Exception("ScheduleId cannot be zero");

            var scheduleRecord = (from x in context.Maintenance_Schedule where x.MS_Id == MS_ID select x).FirstOrDefault();

            if (scheduleRecord == null)
                throw new Exception("Schedule record not found in database.");

            scheduleRecord.MM_Id = record.MM_Id;
            scheduleRecord.MT_Id = record.MT_Id;
            scheduleRecord.MS_Next_Main_Date = parseDate(record.MS_Next_Main_Date);
            scheduleRecord.MS_Workorder = record.MS_Workorder;
            scheduleRecord.MS_Frequency = ConvertToShort(record.MS_Frequency);
            scheduleRecord.MS_Last_Main_Date = parseDate(record.MS_Last_Main_Date);
            if (record.MS_Main_Comp_Date.ToUpper() == "TRUE")
            {
                scheduleRecord.MS_Main_Comp_Date = System.DateTime.Now.Date;
                scheduleRecord.MS_WOClosed_Timestamp = DateTime.Now;
            } else if (record.MS_Main_Comp_Date.ToUpper() == "FALSE")
            {
                scheduleRecord.MS_Main_Comp_Date = null;
                scheduleRecord.MS_WOClosed_Timestamp = null; 
            }          
            scheduleRecord.EMP_ID = record.EMP_ID;
            scheduleRecord.MS_Maint_Code = record.MS_Maint_Code;
            scheduleRecord.MS_Maint_Time_Alotted = ConvertToShort(record.MS_Maint_Time_Alotted);
            scheduleRecord.MS_Main_Time_Required = ConvertToShort(record.MS_Maint_Time_Required);
            scheduleRecord.MS_Machine_Hours = record.MS_Machine_Hours;
            scheduleRecord.MS_Unscheduled_Reason = record.MS_Unscheduled_Reason;
            scheduleRecord.MS_Notes = record.MS_Notes;
            //scheduleRecord.MS_Total_Machine_Downtime = record.MS_Total_Machine_Downtime / 60;
            scheduleRecord.MFB_Id = record.MFB_Id;
            
            return context.SaveChanges(); 
        }

        public void updateMachineDowntime(int ms_id, int downtime)
        {
            if (ms_id == 0)
                throw new Exception("primary index cannot be 0.");

            var mrec = (from x in context.Maintenance_Schedule where x.MS_Id == ms_id select x).FirstOrDefault(); 

            if (mrec != null)
            {
                mrec.MS_Total_Machine_Downtime = downtime;
                context.SaveChanges(); 
            }
        }

        public void updateCompDates()
        {
            var minDate = DateTime.Now.Date.AddDays(-3);

            var recs = (from x in context.Maintenance_Schedule where x.MS_Main_Comp_Date != null && x.MS_Main_Comp_Date > minDate select x).ToList(); 

            if (recs != null && recs.Count > 0)
            {
                foreach (var item in recs)
                {
                    item.MS_Main_Comp_Date = item.MS_Main_Comp_Date.Value.Date;
                }
                context.SaveChanges(); 
            }
        }
        public int Add(schedule record)
        {          
            validateScheduleInputs(record);

            Maintenance_Schedule newRecord = new Maintenance_Schedule();
            newRecord.MM_Id = record.MM_Id;
            newRecord.MT_Id = record.MT_Id;

            if (record.MS_Next_Main_Date.Trim().Length == 0)
                newRecord.MS_Next_Main_Date = DateTime.Now;
            else 
                newRecord.MS_Next_Main_Date = parseDate(record.MS_Next_Main_Date);

            var woHashed = GetDistinctWorkorders(CID_); 

            if (woHashed.Contains(record.MS_Workorder))
                newRecord.MS_Workorder = getNewWorkoder().ToString(); 
             else 
                newRecord.MS_Workorder = record.MS_Workorder;

            newRecord.MS_Frequency = ConvertToShort(record.MS_Frequency);
            if (record.MS_Last_Main_Date.Trim().Length == 0)
                newRecord.MS_Last_Main_Date = DateTime.Now; 
            else
                newRecord.MS_Last_Main_Date = parseDate(record.MS_Last_Main_Date);
            
            newRecord.MS_Main_Comp_Date = null;
            newRecord.EMP_ID = record.EMP_ID;
            newRecord.MS_Maint_Code = record.MS_Maint_Code;
            newRecord.MS_Maint_Time_Alotted = ConvertToShort(record.MS_Maint_Time_Alotted);
            newRecord.MS_Main_Time_Required = ConvertToShort(record.MS_Maint_Time_Required);
            newRecord.MS_Machine_Hours = record.MS_Machine_Hours;
            newRecord.MS_Unscheduled_Reason = record.MS_Unscheduled_Reason;
            newRecord.MS_Notes = record.MS_Notes;
            newRecord.MS_Total_Machine_Downtime = record.MS_Total_Machine_Downtime /  60;
            newRecord.MFB_Id = record.MFB_Id;
            newRecord.MS_WOCreate_Timestamp = DateTime.Now;
            context.Maintenance_Schedule.Add(newRecord);

            int affected = context.SaveChanges();

            if (affected > 0)
            {
                AddWorkOrderToCache(newRecord.MS_Workorder, CID_); 
            }

            return affected; 
        }

        private DateTime parseDate(string dateStr)
        {
            DateTime returnObj;

            DateTime.TryParse(dateStr, out returnObj);

            return returnObj; 
        }

        private short ConvertToShort(int number)
        {
            try
            {
                return (short)number; 
            } catch (Exception ex)
            {
                
            }
            return 1; 
        }

        public void delete(int MS_ID)
        {
            if (MS_ID < 1)
                throw new Exception("schedule primary key must be greater than zero in order to delete record.");
            deleteAsync(MS_ID);
        }

        public void deleteAsync(int MS_Id)
        {
            try
            {
                context.Maintenance_Schedule.Remove((from x in context.Maintenance_Schedule where x.MS_Id == MS_Id select x).FirstOrDefault());
                context.SaveChanges(); 
            } catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex); 
            }
            
        }

        public int getNewWorkoder2()
        {
            int WorkOrder = 1001;
            
            var wo = (from x in context.Maintenance_Schedule                      
                      orderby Convert.ToDecimal(x.MS_Workorder) descending
                      select x.MS_Workorder).FirstOrDefault();

            bool result = Int32.TryParse(wo, out WorkOrder);
            return (result == true) ? WorkOrder + 1 : 1001;
        }

        public int getNewWorkoder()
        {
            int results = 0;
            string woStr = ""; 
            using (SqlConnection conn = new SqlConnection(manager.BuildConnectionString(CID_)))
            {
                string sqlstatement = "select top(1) MS_Workorder from Maintenance_Schedule " +
                                        "WHERE isnumeric(MS_Workorder) = 1 " +
                                        "order by CAST(MS_Workorder AS INT) desc";
                conn.Open();
                SqlCommand cmd = new SqlCommand(sqlstatement, conn);
                try
                {
                    
                    cmd.CommandType = System.Data.CommandType.Text;
                    SqlDataReader dr = cmd.ExecuteReader(); 
                    while(dr.Read())
                    {
                        woStr = dr[0].ToString(); 
                    }
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
                finally
                {
                    conn.Close();
                }
            }

            if (int.TryParse(woStr, out results))
                return results + 1;
            else
                return 0;
        }

        public List<Machine> getMachines(int flgId = 0)
        {
            List<Machine> machines = new List<Machine>(); 
            try
            {
                context.Configuration.LazyLoadingEnabled = false;
                var machenums = (from x in context.Machines
                                 where x.MM_Active == true && x.MM_Name.Length > 0 && x.MM_MFB_Code1 != null
                                 select x).AsEnumerable();
                
                machines = machenums.Select(x => new Machine {MM_Id= x.MM_Id, MM_Name = x.MM_Name, Mach_Filename1 = x.Mach_Filename1, Mach_Filename2 = x.Mach_Filename2, Mach_Filename3 = x.Mach_Filename3, Mach_Filename4 = x.Mach_Filename4, MM_MFB_Code1 = x.MM_MFB_Code1}).ToList();

                if (flgId > 0)
                    machines = (from x in machines orderby x.MM_Name ascending where x.MM_MFB_Code1.Trim() == "FB" + flgId.ToString() select x).ToList();
            } catch (Exception ex)
            {
                error = true;
                errorMessage = ex.Message; 
            }

            return machines; 
        }

        public List<ActiveMachineImgae> getmachineFiles(int MM_Id)
        {
            List<ActiveMachineImgae> files = new List<ActiveMachineImgae>(); 
            try
            {
                var filedata = (from x in context.Machines
                                where x.MM_Id == MM_Id
                                select new { x.Mach_Filename1, x.Mach_Image1, x.Mach_Filename2, x.Mach_Image2, x.Mach_Filename3, x.Mach_Filename4, x.Mach_Image3, x.Mach_Image4 }).FirstOrDefault(); 
                if (filedata != null)
                {
                    
                    files.Add(new ActiveMachineImgae { filename1 = filedata.Mach_Filename1, filebytes1 = filedata.Mach_Image1 });
                    files.Add(new ActiveMachineImgae { filename1 = filedata.Mach_Filename2, filebytes1 = filedata.Mach_Image2 });
                    files.Add(new ActiveMachineImgae { filename1 = filedata.Mach_Filename3, filebytes1 = filedata.Mach_Image3 });
                    files.Add(new ActiveMachineImgae { filename1 = filedata.Mach_Filename4, filebytes1 = filedata.Mach_Image4 });
                }

            } catch (Exception ex)
            {

            } finally
            {
                context.Dispose(); 
            }
            return files;
        }

        public int getDefaultEMPID()
        {
            int EMPID = 0;

            var found = (from x in context.Employees where x.EMP_First_Name == "." select x.EMP_ID).FirstOrDefault();

            if (found > 0)
                EMPID = found; 

            return EMPID; 
        }

        public List<Maintenance_Types> getMaintenanceTypes()
        {
            List<Maintenance_Types> types = new List<Maintenance_Types>(); 

            try
            {
                context.Configuration.LazyLoadingEnabled = false;
                types = (from x in context.Maintenance_Types
                         where x.MT_MFB_Code1 == "FB"
                         orderby x.MT_Number ascending
                         select x).ToList();
            } catch (Exception ex)
            {
                error = true;
                errorMessage = ex.Message; 
            }
            
            return types; 

        }

        public List<Employee> getEmployees()
        {
            List<Employee> employees = new List<Employee>(); 
            try
            {
                context.Configuration.LazyLoadingEnabled = false;
                employees = (from x in context.Employees
                             where x.EMP_Active == true
                             orderby x.EMP_First_Name ascending
                             select x).ToList(); 
            } catch (Exception ex)
            {
                error = true;
                errorMessage = ex.Message; 
            }
            return employees;
        }

        public List<Models.pageInputs> getFlagBoards()
        {
            List<pageInputs> fbs = new List<pageInputs>(); 
            try
            {
                context.Configuration.LazyLoadingEnabled = false; 
                fbs = (from x in context.Maintenance_Flagboard
                       select new pageInputs { key = x.MFB_Id.ToString(), value = x.Loc_Description }).ToList();
            } catch (Exception e)
            {
                error = true;
                errorMessage = e.Message; 
            }
            return fbs; 
        }

        public List<pageInputs> getFlagBoardsInput()
        {
            List<pageInputs> fbs = new List<pageInputs>(); 

            try
            {
                context.Configuration.LazyLoadingEnabled = false;
                fbs = (from x in context.Maintenance_Flagboard
                       select new pageInputs { key = x.MFB_Id.ToString(), value = x.Loc_Description }).ToList(); 
            } catch (Exception e)
            {
                error = true;
                errorMessage = e.Message; 
            } finally
            {
                context.Dispose(); 
            }
            return fbs; 
        }

        public Models.EF.Maintenance_Schedule getScheduleRecord(int MS_Id)
        {
            if (MS_Id == 0)
                throw new Exception("schedule primary key required and cannot be zero.");
            Models.EF.Maintenance_Schedule record = null; 
            try
            {
                context.Configuration.LazyLoadingEnabled = false;
                record = (from x in context.Maintenance_Schedule where x.MS_Id == MS_Id select x).FirstOrDefault(); 
            } finally
            {
                context.Dispose(); 
            }
            return record; 
        }

        public string getFBName(int id)
        {
            if (id == 0)
                throw new Exception("primary key required"); 

            try
            {
                context.Configuration.LazyLoadingEnabled = false;
                var name = (from x in context.Maintenance_Flagboard
                            where x.MFB_Id == id
                            select x.Loc_Description).FirstOrDefault();

                if (name != null)
                    return name; 

            } catch(Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex); 
            } 
            return ""; 
        }

        public List<schedule> getSchedule(int MS_Maint_Code = 2)
        {
            List<schedule> records = new List<schedule>();
            try
            {
                var DateMin = DateTime.Now.AddDays(-2);
                var recs = (from x in context.Maintenance_Schedule
                            join mt in context.Maintenance_Types on x.MT_Id equals mt.MT_Id
                            join mm in context.Machines on x.MM_Id equals mm.MM_Id
                            join em in context.Employees on x.EMP_ID equals em.EMP_ID
                            where x.EMP_ID != null && x.MS_Workorder.Length > 0 && x.MFB_Id != null && (x.MS_Main_Comp_Date == null || x.MS_Main_Comp_Date >= DateMin)
                            select new dbschedule
                            {
                                MS_Unscheduled_Reason = x.MS_Unscheduled_Reason,
                                MT_Id = x.MT_Id,
                                MT_MFB_Code2 = mt.MT_MFB_Code2,
                                MT_Name = mt.MT_Name,
                                MT_Description = mt.MT_Description,
                                MM_Name = mm.MM_Name,
                                MM_Id = mm.MM_Id,
                                MS_Id = x.MS_Id,
                                MFB_Id = x.MFB_Id,
                                MM_Number = mm.MM_Number,
                                MS_Notes = x.MS_Notes,
                                Date_WOClosed_Timestamp = x.MS_WOClosed_Timestamp,
                                Date_WOCreate_Timestamp = x.MS_WOCreate_Timestamp,
                                Date_Last_Main_Date = x.MS_Last_Main_Date,
                                Date_Next_Main_Date = x.MS_Next_Main_Date,
                                Date_Main_Comp_Date = x.MS_Main_Comp_Date, 
                                MS_Maint_Time_Alotted = x.MS_Maint_Time_Alotted, 
                                MS_Maint_Time_Required = x.MS_Main_Time_Required, 
                                MS_Frequency = x.MS_Frequency,
                                WorkOrder = x.MS_Workorder,
                                MS_Maint_Code = x.MS_Maint_Code, 
                                MS_Total_Machine_Downtime = x.MS_Total_Machine_Downtime,
                                MS_Machine_Hours = x.MS_Machine_Hours, 
                                EMP_ID = x.EMP_ID,
                                EMP_First_Name = em.EMP_First_Name, 
                                EMP_Last_Name = em.EMP_Last_Name
                            }).AsEnumerable();
                records = recs.Select(r => new schedule
                {
                    actionbtn = "",
                    MS_Unscheduled_Reason = (r == null || r.MS_Unscheduled_Reason == null) ? "" : r.MS_Unscheduled_Reason,
                    MT_Id = (int)r.MT_Id,
                    MT_MFB_Code2 = r.MT_MFB_Code2,
                    MT_Name = r.MT_Name,
                    MM_Id = (int)r.MM_Id,
                    MT_Description = (r == null || r.MT_Description == null) ? "" : r.MT_Description,
                    MM_Name = (r == null) ? "" : getDefault(r.MM_Name),
                    MS_Id = r.MS_Id,
                    MFB_Id = (r == null || r.MFB_Id == null) ? 0 : (int)r.MFB_Id,
                    MM_Number = (r == null || r.MM_Number == null) ? 0 : (int)r.MM_Number,
                    MS_Notes = (r == null || r.MS_Notes == null) ? "" : r.MS_Notes,
                    MS_WOClosed_Timestamp = (r == null || r.Date_WOClosed_Timestamp == null) ? "" : Formatdate(r.Date_WOClosed_Timestamp),
                    MS_WOClosed_Date = r.Date_WOClosed_Timestamp,
                    MS_WOCreate_Date = r.Date_WOCreate_Timestamp,
                    MS_WOCreate_Timestamp = (r == null || r.Date_WOCreate_Timestamp == null) ? "" : Formatdate(r.Date_WOCreate_Timestamp),
                    MS_Last_Main_Date = (r == null || r.Date_Last_Main_Date == null) ? "" : Formatdate(r.Date_Last_Main_Date),
                    MS_Next_Main_Date = (r == null || r.Date_Next_Main_Date == null) ? "" : Formatdate(r.Date_Next_Main_Date),
                    MS_Next_Main_Datetime = r.Date_Next_Main_Date,
                    MS_Maint_Time_Alotted = (r.MS_Maint_Time_Alotted == null) ? (short)0 : (short)r.MS_Maint_Time_Alotted,
                    MS_Maint_Time_Required = (r.MS_Maint_Time_Required == null) ? (short)0 : (short)r.MS_Maint_Time_Required,
                    MS_Main_Comp_Date = (r.Date_Main_Comp_Date == null) ? "" : Formatdate(r.Date_Main_Comp_Date),
                    MS_Frequency = (r.MS_Frequency == null) ? (short)0 : (short)r.MS_Frequency,
                    MS_Workorder = (r == null || r.WorkOrder == null) ? "" : r.WorkOrder,
                    MS_Maint_Code = r.MS_Maint_Code,
                    MS_Total_Machine_Downtime = r.MS_Total_Machine_Downtime * 60,
                    MS_Machine_Hours = (r.MS_Machine_Hours == null) ? 0 : (int)r.MS_Machine_Hours,
                    EMP_ID = (r.EMP_ID == null) ? 0 : (int)r.EMP_ID,
                    CompletedBy = r.EMP_First_Name + " " + r.EMP_Last_Name
                           }).OrderBy(x => x.MS_WOCreate_Date).ToList();

                if (MS_Maint_Code == 1)
                    records = (from x in records orderby x.MS_Next_Main_Datetime ascending select x).ToList(); 

            } catch (Exception ex)
            {
                error = true;
                errorMessage = ex.Message; 
            }         

            return records; 
        }

        public HashSet<string> GetDBDistinctWorkOrders(string CID)
        {
            try
            {
                var wos = (from x in context.Maintenance_Schedule
                           select x.MS_Workorder).Distinct();

                HashSet<string> woh = new HashSet<string>(); 

                foreach(var item in wos)
                {
                    if (item.Trim().Length > 0)
                        woh.Add(item); 
                }

                if (woh.Count > 0)
                    AddCacheItem("uniq-wos-" + CID, woh); 

                return woh;
            } catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex); 
            } finally
            {
                //context.Dispose(); 
            }

            return new HashSet<string>(); 
        }

        public HashSet<string> GetDistinctWorkorders(string CID)
        {
            var cachedWOs = getCacheitem("uniq-wos-" + CID);

            if (cachedWOs == null)
                return GetDBDistinctWorkOrders(CID);

            return (HashSet<string>)cachedWOs; 
        }

        public void AddWorkOrderToCache(string wo, string CID)
        {
            if (wo.Trim().Length == 0)
                return; 

            HashSet<string> existing = GetDistinctWorkorders(CID);

            existing.Add(wo);

            AddCacheItem("uniq-wos-" + CID, existing);
        }

        private string getDefault(string obj)
        {
            return (obj != null) ? obj : ""; 
        }

        public void cache(List<schedule> records, string cid)
        {
            if (records == null && records.Count == 0)
                return;

            if (cid.ToString().Trim().Length != 6)
                return; 

            HttpContext.Current.Cache.Insert("MaintenanceSchedule_" + cid.ToString().Trim(), records, null, DateTime.Now.AddDays(2), System.Web.Caching.Cache.NoSlidingExpiration);

            HttpContext.Current.Cache.Insert("MaintenanceScheduled_cacheEntry_Timestamp_" + cid.ToString().Trim(), DateTime.Now, null, DateTime.Now.AddDays(2), System.Web.Caching.Cache.NoSlidingExpiration); 

        }

        public void clearCache()
        {
            try
            {
                HttpContext.Current.Cache.Remove("MaintenanceSchdule_" + CID_);

                HttpContext.Current.Cache.Remove("MaintenanceScheduled_cacheEntry_Timestamp_" + CID_);
            } catch(Exception ex)
            {

            }
            
        }

        private string Formatdate(DateTime? date)
        {
            DateTime dd = (DateTime)date; 

            return (date != null) ? dd.ToString("yyyy-MM-dd hh:mm tt") : "";
        }

        public void Dispose()
        {
            context.Dispose(); 
        }
    }
}