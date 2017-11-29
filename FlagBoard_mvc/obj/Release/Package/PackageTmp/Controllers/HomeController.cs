using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlagBoard_mvc.Models.EF;
using FlagBoard_mvc.Models;
using System.Configuration;
using System.Collections;
using System.Reflection;
using FlagBoard_mvc.Helpers;
using System.IO;
using System.Diagnostics;
using System.Web.Security;

namespace FlagBoard_mvc.Controllers
{
    public class HomeController : Controller
    {
        private string CIDTest = "000590";
        private string debugMode = ConfigurationManager.AppSettings["debugMode"];
        private IndexViewModel model = new IndexViewModel();
        private Helpers.CtxService service;
        private string ticketname = ""; 
        public ActionResult Index()
        {              
            model.CID = getCID();

            if (model.CID.Trim().Length < 6)
                return RedirectToAction("Locations", "Manage"); 

            service = new Helpers.CtxService(null, model.CID);
            Helpers.ManagerService manageservice = new Helpers.ManagerService(); 
            try
            {
                model.MFB_Id = getSelectedFlagboard();
                model.machines = service.getMachines(model.MFB_Id);
                model.types = service.getMaintenanceTypes();
                model.employees = service.getEmployees();
                model.MS_Maint_Code = getMaintCode();
                model.schedule = getSchedule();
                model.Location = manageservice.getRecord(model.CID.Trim()); 
                model.flagboards = service.getFlagBoards().ToArray();            
                model.MFB_label = getFBLabel(model.MFB_Id);               
                model.UserName = User.Identity.Name; 
                UpdateCachedTimers(model.CID); 

                model.ActiveTimers = GetActiveTimers(model.CID);
                model.isMobile = isMobile();
                model.canDelete = IsAdmin();
                model.ticketname = ticketname; 
            } finally
            {
                service.Dispose();
                model.schedule = filterSchedule(model.schedule);           
            }      

            if (service.error == true)
                model.errorMessage = service.errorMessage;

            if (model.CID.Length < 6)
                return RedirectToAction("Locations", "Manage");

            addLocationCookie(model.CID.Trim());

            return View(model);
        }

        [HttpPost]
        public FileStreamResult MachineFileClick(string CID, string MM_Idp, string number)
        {
            byte[] buffer = new byte[0];
            string filename = "export.pdf";
            string prefix = "pdf";
            Dictionary<string, string> mimetypes = getMimeTypes();
            try
            {
                int MM_Id = 0;
                int fileNumber = Convert.ToInt32(number);   
                Int32.TryParse(MM_Idp, out MM_Id); 

                if (CID.Length < 6)
                    throw new Exception("location variable incorrectly formatted.");

                if (MM_Id == 0)
                    throw new Exception("Machine primary key cannot be zero."); 

                Helpers.CtxService service = new Helpers.CtxService(null, CID);

                List<ActiveMachineImgae> files = service.getmachineFiles(MM_Id);

                for (var i = 1; i <= files.Count; i++)
                {
                    if (i == fileNumber)
                    {
                        buffer = files[i - 1].filebytes1;
                        filename = files[i - 1].filename1;
                        var splitarr = filename.Split('.');
                        if (splitarr.Length > 1)
                            prefix = splitarr[splitarr.Length - 1];

                        break;
                    }
                        
                }

            } catch (Exception ex)
            {

            }
            if (buffer != null && mimetypes.ContainsKey(prefix.ToUpper()))
            {
                MemoryStream stream = new MemoryStream(buffer);
                stream.Flush();
                stream.Position = 0;
                return File(stream, mimetypes[prefix.ToUpper()], filename);
            } else
            {
                MemoryStream stream = new MemoryStream();
                stream.Flush();
                stream.Position = 0;
                return File(stream, "application/pdf", "export.pdf"); 
            }
        }
            
        public ActionResult MobileEditor()
        {
            var CID = getCID();
            int MS_Id = 0, MM_Id = 0, MFB_Id = 0;

            if (CID.Length < 6)
                return RedirectToAction("Locations", "Manage");    
                   
            if (Request.QueryString["MSID"] != null)
                MS_Id = (Int32.TryParse(Request.QueryString["MSID"], out MS_Id) == false) ? 0 : MS_Id;

            if (Request.QueryString["MFBID"] != null)
                MFB_Id = (Int32.TryParse(Request.QueryString["MFBID"], out MFB_Id) == false) ? 0 : MFB_Id;

            if (Request.QueryString["MMID"] != null)
                MM_Id = (Int32.TryParse(Request.QueryString["MMID"], out MM_Id) == false) ? 0 : MM_Id; 

            Models.MobileEditorModel model = getEditorModelObject(CID, MS_Id, MFB_Id, MM_Id);
            model.canDelete = false;
            model.MS_Id = MS_Id;
            model.MM_Id = MM_Id; 

            try
            {
                model.inputs = mapScheduleValues(model.inputs, model.MS_Id, model.CID); 
            } catch (Exception Ex)
            {
                if (model.inputs.Count > 0)
                {
                    model.inputs[0].errorFlag = true;
                    model.inputs[0].errorMessage = Ex.Message; 
                }
            }

            return View(model); 
        }

        [HttpPost]
        public ActionResult MobileEditor(ICollection<InputObject> pageInputs, string method, string CID, string MSID, string MMID)
        {
            int MS_Id = 0, MM_Id = 0;
            int MFBID = 1;
            Int32.TryParse(MSID, out MS_Id);
            Int32.TryParse(MMID, out MM_Id); 
            try
            {              
                if (CID.Length < 6)
                    throw new Exception("Location variable in wrong format");

                Helpers.objectMapper<schedule> mapper = new Helpers.objectMapper<schedule>();
                schedule record = mapper.mapCollection(pageInputs);
                MFBID = record.MFB_Id;
                record.MS_Id = MS_Id;

                if (record == null)
                    throw new Exception("ERROR: could not map page inputs.  Contact helper desk to log bug.");
                switch (method.ToUpper())
                {
                    case "SUBMIT":                                   
                        Save(CID, record);
                        break;
                    case "DELETE":
                        delete(CID, record);
                        break;
                    default:
                        throw new Exception("ERROR: NO METHOD RECIEVED");
                }
            } catch (Exception ex)
            {
                Models.MobileEditorModel model = getEditorModelObject(CID, MS_Id, MFBID, MM_Id);
                model.CID = CID;
                model.MS_Id = MS_Id;
                model.MM_Id = MM_Id; 

                if (model.inputs.Count > 0)
                {
                    model.inputs[0].errorFlag = true;
                    model.inputs[0].errorMessage = ex.Message; 
                }
                model.inputs = mapScheduleValues(model.inputs, model.MS_Id, model.CID);
                return View(model); 
            }

            return RedirectToAction("Index", "Home", new { CID = CID });
        }

        private void Save(string CID, schedule record)
        {
            Helpers.CtxService service = new Helpers.CtxService(null, CID); 
            switch(record.MS_Id)
            {
                case 0:
                    service.Add(record);
                    break;
                default:
                    service.Update(record);
                    if (record.MS_Maint_Code == 1 && record.MS_Main_Comp_Date.ToUpper() == "TRUE")
                    {
                        if (record.MS_Frequency < 1)
                            throw new Exception("Interval must be greater than 0");
                        System.Threading.Tasks.Task.Run(() => AutoAddScheduledOrder(record, service));
                    }                 
                    break; 
            }
            service.clearCache(); 
        }
        private void delete(string CID, schedule record)
        {
            Helpers.CtxService service = new Helpers.CtxService(null, CID);
            service.deleteAsync(record.MS_Id);
            service.clearCache(); 
        }

        [HttpGet]
        public JsonResult getNewWorkorder(string CID)
        {
            Helpers.CtxService service = new Helpers.CtxService(null, CID);
            int workorder = 0; 
            try
            {
                workorder = service.getNewWorkoder(); 
            } finally
            {
                service.Dispose(); 
            }
            return Json(workorder, JsonRequestBehavior.AllowGet); 
        }

        [HttpPost]
        public JsonResult Refresh(short MS_Maint_Code, string hidecompleted, int MFB_Id, string CID, bool Forced)
        {
            //int cpuUsage = cpuPercentage(); 
            //if (cpuUsage > 80 && Forced == false)
            //    return Json("CPUERR", JsonRequestBehavior.AllowGet); 

            Helpers.CtxService service = new Helpers.CtxService(null, CID);
            System.Web.Script.Serialization.JavaScriptSerializer jser = new System.Web.Script.Serialization.JavaScriptSerializer();
            string objSerialized = "";
            try
            {
                List<schedule> updated = new List<schedule>();
                updated = service.getSchedule();

                model.MS_Maint_Code = MS_Maint_Code;
                model.MFB_Id = MFB_Id;
                updated = filterSchedule(updated);
                if (hidecompleted == "true")
                {
                    List<schedule> uncompleted = new List<schedule>(); 
                    foreach(var item in updated)
                    {
                        if (item.MS_WOClosed_Timestamp.Trim().Length == 0)
                            uncompleted.Add(item); 
                    }
                    objSerialized = jser.Serialize(uncompleted);
                    return Json(objSerialized, JsonRequestBehavior.AllowGet); 
                }
                
                objSerialized = jser.Serialize(updated);
            }
            catch (Exception ex)
            {
                objSerialized = "ERROR"; 
            }
            finally
            {
                service.Dispose();
            }
            return Json(objSerialized, JsonRequestBehavior.AllowGet); 
        }

        [HttpPost]
        public JsonResult dispatch(string inputs, string type, string CID)
        {
            System.Web.Script.Serialization.JavaScriptSerializer jser = new System.Web.Script.Serialization.JavaScriptSerializer();
            Helpers.CtxService service = new Helpers.CtxService(null, CID); 
            string userMessage = "";
            int msid = 0; 

            if (inputs.Length == 0)
                return Json("Input Box Objects cannot be empty.", JsonRequestBehavior.AllowGet); 
            
            try
            {
                FlagBoard_mvc.Models.pageInputs[] inputObjects = jser.Deserialize<FlagBoard_mvc.Models.pageInputs[]>(inputs);
                Helpers.jqGrid<FlagBoard_mvc.Models.schedule> jq = new Helpers.jqGrid<FlagBoard_mvc.Models.schedule>();
                FlagBoard_mvc.Models.schedule record = jq.mapToObjectProperty(inputObjects); 
                switch (type)
                {
                    case "edit":
                        if (record.MS_Unscheduled_Reason.Trim().Length > 50)
                            throw new Exception("Unscheduled Reason cannot be greater than 50 characters long."); 

                        int rows = service.Update(record);
                        if (rows > 0 && record.MS_Maint_Code == 1 && record.MS_Main_Comp_Date == "True")
                        {
                            if (record.MS_Frequency < 1)
                                throw new Exception("Interval must be greater than 0");
                            AutoAddScheduledOrder(record, service);
                        }
                        userMessage = "true"; 
                        break;
                    case "add":
                        if (record.MS_Unscheduled_Reason.Length > 50)
                            throw new Exception("Unscheduled Reason cannot be greater than 50 characters long.");

                        int rowsaff = service.Add(record);
                        service.Dispose(); 
                        if (rowsaff == 0)
                            throw new Exception("unknown error adding rows. ");
                        userMessage = "true";
                        msid = service.rowKey; 
                        break;
                    case "delete":
                        service.delete(record.MS_Id);
                        userMessage = "true"; 
                        break; 
                }
                
            } catch (Exception ex)
            {
                userMessage = "ERROR: " + ex.Message; 
            } finally
            {
                service.Dispose();
                service.clearCache();
            }

            return Json(new { message = userMessage, id = msid }, JsonRequestBehavior.AllowGet); 
        }

        [HttpPost]
        public JsonResult timer(int ms_id, string cid, string action)
        {
            var result = "";
            var usercontent = ""; 
            var cache_key = "ms_timer_" + cid + "_" + ms_id;
            switch (action)
            {
                case "GET":                                
                    try
                    {
                        var started = "false"; 
                        TimeSpan prior_ts = new TimeSpan();
                        var test_cache_str = "ms_timer_" + cid + "_" + ms_id + "_prior_timespan";
                        var c_obj = HttpContext.Cache[test_cache_str];

                        if (c_obj != null)
                        {
                            prior_ts = (TimeSpan)HttpContext.Cache[cache_key + "_prior_timespan"]; 
                        }
                        if (HttpContext.Cache[cache_key] != null)
                        {
                            DateTime startime = (DateTime)HttpContext.Cache[cache_key];
                            TimeSpan nws = (DateTime.Now - startime);
                            prior_ts =  + prior_ts.Add(nws);
                            started = "true"; 
                        }
                        if (prior_ts.Hours + prior_ts.Minutes > 0)
                        {
                            //result = prior_ts.Hours.ToString() + ":" + prior_ts.Minutes.ToString() + "/" + started;
                            result = prior_ts.ToString(@"hh\:mm") + "/" + started; 
                        }                           
                        else
                        {
                            result = "0:00/" + started;
                        }
                        if (HttpContext.Cache[cache_key + "_owner"] != null)
                        {
                            usercontent = (string)HttpContext.Cache[cache_key + "_owner"]; 
                        }
                             
                    } catch (Exception ex)
                    {
                        result = "ERR";
                        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    }
                    
                    break;
                case "START":                  
                    result = "STARTED";
                    HttpContext.Cache.Insert(cache_key, DateTime.Now, null, DateTime.Now.AddMonths(6), System.Web.Caching.Cache.NoSlidingExpiration);
                    HttpContext.Cache.Insert(cache_key + "_owner", User.Identity.Name + ": " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(), null, DateTime.Now.AddMonths(6), System.Web.Caching.Cache.NoSlidingExpiration);
                    usercontent = User.Identity.Name + ": " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                    UpdateTimerList(ms_id, true, cid); 

                    break;
                case "STOP":
                    if (HttpContext.Cache[cache_key] != null)
                    {
                        try
                        {
                            TimeSpan prior_ts = new TimeSpan();
                            if (HttpContext.Cache[cache_key + "_prior_timespan"] != null)
                            {
                                prior_ts = (TimeSpan)HttpContext.Cache[cache_key + "_prior_timespan"];
                                HttpContext.Cache.Remove(cache_key + "_prior_timespan"); 
                            }
                            DateTime startime = (DateTime)HttpContext.Cache[cache_key];
                            TimeSpan ts = (DateTime.Now - startime) + prior_ts;
                            HttpContext.Cache.Insert(cache_key + "_prior_timespan", ts, null, DateTime.Now.AddMonths(6), System.Web.Caching.Cache.NoSlidingExpiration);
                            HttpContext.Cache[cache_key + "_owner"] = User.Identity.Name + ": " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                            usercontent = User.Identity.Name + ": " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                            HttpContext.Cache.Remove(cache_key);
                            UpdateMachineDowntime(ms_id, cid, (int)ts.TotalMinutes);
                            if (ts.Hours + ts.Minutes > 0)
                            {
                                //result = ts.Hours.ToString() + ":" + ts.Minutes.ToString() + "/STOPPED";
                                var stp_str = "/STOPPED"; 
                                result = ts.ToString(@"hh\:mm") + stp_str;
                            }
                            else
                            {
                                result = "0:00/STOPPED";
                            }

                            UpdateTimerList(ms_id, false, cid);
                        } catch (Exception ex)
                        {
                            result = "ERR"; 
                        }
                    }

                    break;
            }

            if (usercontent.Length > 0)
                usercontent = "owner " + usercontent; 

            return Json(new Models.TimerPostModel() { TimerContent = result, UserContent = usercontent });
        }

        #region Helpers
        private string getCID()
        {

            if (debugMode == "true")
                return CIDTest;
            return (Request.QueryString["CID"] != null && Request.QueryString["CID"].Trim().Length > 0) ? Request.QueryString["CID"] : checkCookie("APRKeepMeIn", "CID_Print");
        }

        private List<schedule> filterSchedule(List<schedule> records, int MS_Maint_Code = 2)
        {
            if (records == null || records.Count == 0)
                return records;
            try
            {
                if (model.MFB_Id > 0)
                    records = (from x in records where x.MFB_Id == model.MFB_Id select x).ToList();

                records = (from x in records where x.MS_Maint_Code == model.MS_Maint_Code orderby x.MS_WOCreate_Date ascending select x).ToList();

                if (model.MS_Maint_Code == 1)
                    records = (from x in records orderby x.MS_Next_Main_Datetime ascending select x).ToList(); 

            } catch (Exception ex)
            {
                model.errorMessage = ex.Message; 
            }
            return records; 
        }

        private int getSelectedFlagboard()
        {
            int flag = 0; 
            try
            { 
                flag = (Request.QueryString["MFBID"] != null) ? Convert.ToInt32(Request.QueryString["MFBID"]) : Convert.ToInt32(checkCookie("MFBInfo", "MFBID"));
            } catch (Exception ex)
            {
                
            }
            return flag;  
        }

        private short getMaintCode()
        {
            var scheduledQryStr = Request.QueryString["code"];
            short MS_Maint_Code = 2; 
            try
            {
                MS_Maint_Code = (scheduledQryStr == null) ? (short)2 : Convert.ToInt16(scheduledQryStr);
            }
            catch (Exception e)
            {
            }
            return MS_Maint_Code; 
        }
        private string checkCookie(string CookieName, string key = "")
        {
            if (Request.Cookies[CookieName] != null && Request.Cookies[CookieName].HasKeys)
                if (Request.Cookies[CookieName][key] != null)
                    return Request.Cookies[CookieName][key];
            return "0"; 
        }

        private void addLocationCookie(string CID)
        {
            if (CID.Trim().Length != 6)
                return;

            HttpCookie cookie = new HttpCookie("maintenance_location");
            cookie["CID"] = CID;
            cookie.Expires = DateTime.Now.AddDays(21); 

            Response.Cookies.Add(cookie); 
        }

        private List<schedule> getSchedule(int maint_code = 1)
        {
            //if (HttpContext.Cache["MaintenanceSchedule_" + model.CID] != null)
            //{
            //    try
            //    {
            //        List<schedule> cachedrecs = (List<schedule>)HttpContext.Cache["MaintenanceSchedule_" + model.CID];
            //        if (cachedrecs.Count > 0)
            //        {
            //            if (maint_code == 1)
            //            {
            //                return (from x in cachedrecs orderby x.MS_Next_Main_Datetime ascending select x).ToList();
            //            }
            //            else
            //            {
            //                return (from x in cachedrecs select x).OrderBy(x => x.MS_WOCreate_Date).ToList();
            //            }

            //        }


            //    }
            //    catch (Exception ex)
            //    {
            //        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            //    }

            //}
            Helpers.CtxService service = new Helpers.CtxService(null, model.CID);

            var recs = service.getSchedule();

            //if (recs.Count > 0)
            //    service.cache(recs, model.CID); 

            return recs;
        }

        private Models.MobileEditorModel getEditorModelObject(string CID, int MS_Id, int MFB_Id, int MM_Id)
        {
            Models.MobileEditorModel model = new Models.MobileEditorModel();
            model.CID = CID; 
            model.inputs = getEditorInputs(CID, MFB_Id, MS_Id);
            model.hiddenInputs = new List<Helpers.InputObject>();

            Helpers.InputObject hidden1 = new Helpers.InputObject();
            hidden1.id = "CID";
            hidden1.value = CID;
            model.hiddenInputs.Add(hidden1);

            Helpers.InputObject hidden2 = new Helpers.InputObject();
            hidden2.id = "MS_Id";
            hidden2.value = MS_Id.ToString();
            model.hiddenInputs.Add(hidden2);

            Helpers.InputObject hidden3 = new Helpers.InputObject();
            hidden3.id = "MM_Id";
            hidden3.value = MM_Id.ToString();
            model.hiddenInputs.Add(hidden3); 

            service = new Helpers.CtxService(null, model.CID);
            model.machines = service.getMachines(MFB_Id).ToArray(); 

            return model;
        }

        private List<Helpers.PageInput> getEditorInputs(string CID, int MFB_Id, int MS_Id = 0)
        {
            List<Helpers.PageInput> inputs = new List<Helpers.PageInput>();

            inputs.Add(getFlagboardDropdown(CID, MFB_Id));

            inputs.Add(getMachineDropdown(CID, MFB_Id));

            inputs.Add(getMTypesDropdown(CID));

            inputs.Add(getEmployeesDropdown(CID));

            if (MS_Id == 0)
            {
                foreach (var item in inputs[inputs.Count - 1].input.options)
                {
                    if (item.text == ". .")
                    {
                        inputs[inputs.Count - 1].input.value = item.value;
                        break;
                    }
                        
                }
            }

            inputs.Add(new Helpers.PageInput("default", "Unscheduled Reason", "MS_Unscheduled_Reason", "MS_Unscheduled_Reason", ""));

            if (MS_Id == 0)
            {
                Helpers.CtxService service = new Helpers.CtxService(null, CID);
                try
                {
                    inputs.Add(new Helpers.PageInput("default", "WorkOrder Number", "MS_Workorder", "MS_Workorder", service.getNewWorkoder().ToString()));
                }
                finally
                {
                    service.Dispose();
                }               
            } else
            {
                inputs.Add(new Helpers.PageInput("default", "WorkOrder Number", "MS_Workorder", "MS_Workorder", ""));
            }

            Helpers.PageInput lastDate = new Helpers.PageInput("default", "Last Maintenance Date", "MS_Last_Main_Date", "MS_Last_Main_Date", "");
            lastDate.function = "date";
            inputs.Add(lastDate);

            Helpers.PageInput nextDate = new Helpers.PageInput("default", "Next Maintenance Date", "MS_Next_Main_Date", "MS_Next_Main_Date", "");
            nextDate.function = "date";
            inputs.Add(nextDate);

            Helpers.PageInput completed = new Helpers.PageInput("default", "Completed by", "MS_Main_Comp_Date", "MS_Main_Comp_Date", "false");
            completed.function = "checkbox";
            inputs.Add(completed);

            inputs.Add(getMaintCodeDropdown()); 

            Helpers.PageInput timeAlotted = new Helpers.PageInput("default", "Time Alloted", "MS_Maint_Time_Alotted", "MS_Maint_Time_Alotted", "0");
            timeAlotted.function = "number";
            inputs.Add(timeAlotted);

            Helpers.PageInput timeRequired = new Helpers.PageInput("default", "Time Required", "MS_Maint_Time_Required", "MS_Maint_Time_Required", "0");
            timeRequired.function = "number";
            inputs.Add(timeRequired);

            Helpers.PageInput timeInterval = new Helpers.PageInput("default", "Time Interval", "MS_Frequency", "MS_Frequency", "0");
            timeInterval.function = "number";
            inputs.Add(timeInterval);

            Helpers.PageInput downtime = new Helpers.PageInput("timer", "Total Machine Downtime", "MS_Total_Machine_Downtime", "MS_Total_Machine_Downtime", "0");
            downtime.function = "timer";
            inputs.Add(downtime);

            Helpers.PageInput machineHours = new Helpers.PageInput("default", "Machine Hours", "MS_Machine_Hours", "MS_Machine_Hours", "0");
            machineHours.function = "number";
            inputs.Add(machineHours);

            inputs.Add(new Helpers.PageInput("default", "Notes", "MS_Notes", "MS_Notes", "")); 

            return inputs; 
        }

        private Helpers.PageInput getMachineDropdown(string CID, int flgId = 0)
        {
            Helpers.PageInput machineInput = new Helpers.PageInput("select", "Machine Name", "MM_Id", "MM_Id", "");
            try
            {
                Helpers.CtxService service = new Helpers.CtxService(null, CID);
                List<Machine> machines = service.getMachines(flgId);

                if (machines == null || machines.Count == 0)
                {
                    machineInput.errorFlag = true;
                    machineInput.errorMessage = "ERROR: No machines found in database.";
                    return machineInput;
                }

                foreach (var item in machines)
                {
                    machineInput.input.options.Add(new Helpers.InputObject.option { text = item.MM_Name, value = item.MM_Id.ToString() });
                }
            } catch (Exception ex)
            {
                machineInput.errorFlag = true;
                machineInput.errorMessage = "ERROR CREATING MACHINE SELECT LIST: " + ex.Message; 
            }
                   
            return machineInput; 
        }

        private Helpers.PageInput getMTypesDropdown(string CID)
        {
            Helpers.PageInput machineInput = new Helpers.PageInput("select", "Maintenance Types", "MT_Id", "MT_Id", "");
            try
            {
                Helpers.CtxService service = new Helpers.CtxService(null, CID);
                List<Maintenance_Types> mtypes = service.getMaintenanceTypes();

                if (mtypes == null || mtypes.Count == 0)
                {
                    machineInput.errorFlag = true;
                    machineInput.errorMessage = "ERROR: No maintenance types found in database.";
                    return machineInput;
                }

                foreach (var item in mtypes)
                {
                    machineInput.input.options.Add(new Helpers.InputObject.option { text = item.MT_Name, value = item.MT_Id.ToString() });
                }
            }
            catch (Exception ex)
            {
                machineInput.errorFlag = true;
                machineInput.errorMessage = "ERROR CREATING MAINTENANCE SELECT LIST: " + ex.Message;
            }

            return machineInput;
        }

        private Helpers.PageInput getEmployeesDropdown(string CID)
        {
            Helpers.PageInput machineInput = new Helpers.PageInput("select", "Employees", "EMP_ID", "EMP_ID", "");
            try
            {
                Helpers.CtxService service = new Helpers.CtxService(null, CID);
                List<Employee> employees = service.getEmployees();

                if (employees == null || employees.Count == 0)
                {
                    machineInput.errorFlag = true;
                    machineInput.errorMessage = "ERROR: No empoyes found in database.";
                    return machineInput;
                }

                foreach (var item in employees)
                {
                    machineInput.input.options.Add(new Helpers.InputObject.option { text = item.EMP_First_Name + " " + item.EMP_Last_Name, value = item.EMP_ID.ToString() });
                }
            }
            catch (Exception ex)
            {
                machineInput.errorFlag = true;
                machineInput.errorMessage = "ERROR CREATING EMPLOYEE SELECT LIST: " + ex.Message;
            }

            return machineInput;
        }

        private Helpers.PageInput getFlagboardDropdown(string CID, int MFB_Id = 0)
        {
            string selVal = (MFB_Id == 0) ? "" : MFB_Id.ToString();
            Helpers.PageInput fbInput = new Helpers.PageInput("select", "Flagboards", "MFB_Id", "MFB_Id", selVal);
            try
            {
                Helpers.CtxService service = new Helpers.CtxService(null, CID);
                List<Models.pageInputs> fbs = service.getFlagBoardsInput();

                if (fbs == null || fbs.Count == 0)
                {
                    fbInput.errorFlag = true;
                    fbInput.errorMessage = "ERROR: No FlagBoards found in database.";
                    return fbInput;
                }

                foreach (var item in fbs)
                {
                    fbInput.input.options.Add(new Helpers.InputObject.option { text = "FB " + item.value, value = item.key });
                }
            }
            catch (Exception ex)
            {
                fbInput.errorFlag = true;
                fbInput.errorMessage = "ERROR CREATING FB SELECT LIST: " + ex.Message;
            }

            return fbInput;
        }

        private Helpers.PageInput getMaintCodeDropdown()
        {
            Helpers.PageInput fbInput = new Helpers.PageInput("select", "Maintenance Code", "MS_Maint_Code", "MS_Maint_Code", "2");
            try
            {
                fbInput.input.options.Add(new Helpers.InputObject.option { text = "SCHEDULED", value = "1" });
                fbInput.input.options.Add(new Helpers.InputObject.option { text = "UNSCHEDULED", value = "2" });
            }
            catch (Exception ex)
            {
                fbInput.errorFlag = true;
                fbInput.errorMessage = "ERROR CREATING MTCODE SELECT LIST: " + ex.Message;
            }

            return fbInput;
        }

        private IList<Helpers.PageInput> mapScheduleValues(IList<Helpers.PageInput> mobileInputs, int MS_Id, string CID)
        {
            if (MS_Id == 0)
                return mobileInputs; 

            Helpers.CtxService service = new Helpers.CtxService(null, CID); 
        
            Models.EF.Maintenance_Schedule schedule = service.getScheduleRecord(MS_Id);

            if (schedule == null)
                return mobileInputs; 

            Helpers.objectMapper<Maintenance_Schedule> mapper = new Helpers.objectMapper<Maintenance_Schedule>();

            Hashtable hash = mapper.createPropertyInfoHash();

            ICollection keys = hash.Keys; 
            if (keys.Count > 0)
            {
                foreach (var item in mobileInputs)
                {
                    try
                    {
                        if (hash[item.input.id.ToUpper()] != null)
                        {
                            PropertyInfo info = (PropertyInfo)hash[item.input.id.ToUpper()];
                            if (info != null && item.input != null)
                            {
                                var val = info.GetValue(schedule);
                                if (val == null)
                                    val = "";
                                if (info.PropertyType.Name.ToUpper() == "DATETIME")
                                {
                                    DateTime realdate = Convert.ToDateTime(val); 
                                    item.input.value = realdate.ToString("yyyy-MM-dd");
                                }                                  
                                else
                                    item.input.value = mapper.ConvertType(val.ToString(), "STRING").ToString();
                            }
                                
                        }
                    } catch (Exception ex)
                    {
                        if (item.input != null)
                        {
                            item.errorFlag = true;
                            item.errorMessage = ex.Message;
                        }
                    }
                              
                }
            }
           
            return mobileInputs; 
        }

        private bool isMobile()
        {
            var UserAgent = Request.UserAgent;

            char[] splitchar = { '(', ')' };
            char[] syssplit = { ';', ' ' };
            try
            {
                string[] splitArr = UserAgent.Split(splitchar);
                if (splitArr.Length > 0)
                {
                    foreach (var item in splitArr)
                    {
                        var subkeyArr = item.Split(';'); 
                        if (subkeyArr.Length > 1)
                        {
                            foreach (var subitem in subkeyArr)
                            {
                                var subitemfmt = subitem.Trim().ToUpper();
                                if (subitemfmt.StartsWith("ANDROID") == true)
                                    return true;
                            }
                                
                        } else
                        {
                            if (item.Trim().ToUpper() == "ANDROID" || item.ToUpper() == "MAC")
                                return true;
                        }
                        
                    }
                }
            } catch (Exception ex)
            {

            }
           
            return false; 
        }

        private Dictionary<string, string> getMimeTypes()
        {
            Dictionary<string, string> types = new Dictionary<string, string>();

            types.Add("PDF", "application/pdf");

            types.Add("DOC", "application/msword");

            types.Add("JPG", "image/jpeg");

            types.Add("XLS", "application/vnd.ms-excel");

            types.Add("PNG", "image/png"); 

            return types;  
        }
        
        private int cpuPercentage()
        {

            PerformanceCounter pc = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            float usage = pc.NextValue();
           // System.Threading.Thread.Sleep(100);
            usage = pc.NextValue();
            return (int)usage; 
        }

        private void AutoAddScheduledOrder(schedule record, CtxService service)
        {
            if (service == null)
                return; 
            try
            {
                int defaultEMPID = service.getDefaultEMPID();
                record.MS_Next_Main_Date = DateTime.Now.AddDays(record.MS_Frequency).ToShortDateString() + " " + DateTime.Now.AddDays(record.MS_Frequency).ToShortTimeString();
                record.MS_Main_Comp_Date = "";
                record.MS_WOCreate_Timestamp = DateTime.Now.AddDays(record.MS_Frequency).ToShortDateString() + " " + DateTime.Now.AddDays(record.MS_Frequency).ToShortTimeString();
                record.MS_WOClosed_Timestamp = "";
                record.EMP_ID = (record.EMP_ID != 0) ? record.EMP_ID : defaultEMPID;

                int result = service.Add(record);
                if (result == 0)
                    throw new Exception("Error editing scheduled maintenance record.");
                else
                    service.clearCache(); 
            } finally
            {
                service.Dispose(); 
            }
            
        }

        private void UpdateMachineDowntime(int ms_id, string cid, int downtime)
        {
            if (ms_id == 0)
                return;

            if (cid.Trim().Length != 6)
                return;

            if (downtime == 0)
                return; 

            CtxService service = new CtxService(null, cid); 

            try
            {
                service.updateMachineDowntime(ms_id, downtime); 
            } finally
            {
                service.Dispose(); 
            }           
        }

        private void UpdateTimerList(int ms_id, bool value, string cid)
        {
            Dictionary<int, bool> to_cache = null;
            try
            {
                if (HttpContext.Cache["active_timer_list_" + cid.ToString()] != null)
                {
                    Dictionary<int, bool> t_list = (Dictionary<int, bool>)HttpContext.Cache["active_timer_list_" + cid.ToString()];
                    if (t_list.Count > 0)
                    {
                        if (t_list.ContainsKey(ms_id))
                        {
                            t_list[ms_id] = value;
                        }
                        else
                        {
                            t_list.Add(ms_id, value);
                        }
                    }
                }
                else
                {
                    to_cache = new Dictionary<int, bool>
                        {
                            { ms_id, value }
                        };
                }
                
                if (to_cache != null)
                {
                    if (HttpContext.Cache["active_timer_list_" + cid.ToString()] == null)
                    {
                        HttpContext.Cache.Insert("active_timer_list_" + cid.ToString(), to_cache, null, DateTime.Now.AddMonths(6), System.Web.Caching.Cache.NoSlidingExpiration);
                    } else
                    {
                        HttpContext.Cache["active_timer_list_" + cid.ToString()] = to_cache; 
                    }
                    
                }
                    
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        public TimerStatus[] GetActiveTimers(string cid)
        {
            TimerStatus[] tlist = { }; 
            try
            {
                if (HttpContext.Cache["active_timer_list_" + cid.ToString()] != null)
                {
                    Dictionary<int, bool> tdic = (Dictionary<int, bool>)HttpContext.Cache["active_timer_list_" + cid.ToString()];
                    tlist = new TimerStatus[tdic.Count];
                    int i = 0;
                    foreach (var item in tdic)
                    {
                        tlist[i] = new TimerStatus { MS_Id = item.Key, value = item.Value };
                        i++;
                    }
                }

            } catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex); 
            }
            
            return tlist; 
        }

        private void UpdateCachedTimers(string cid)
        {
            if (HttpContext.Cache["MaintenanceSchedule_" + cid] != null)
            {
                try
                {
                    var cache_recs = (List<schedule>)HttpContext.Cache["MaintenanceSchedule_" + cid];

                    foreach(schedule item in cache_recs)
                    {
                        var cache_key = "ms_timer_" + cid + "_" + item.MS_Id + "_prior_timespan";

                        if (HttpContext.Cache[cache_key] != null)
                        {
                            var ts = (TimeSpan)HttpContext.Cache[cache_key];
                            if (ts.TotalMinutes < item.MS_Total_Machine_Downtime)
                            {
                                //int hours = (int)item.MS_Total_Machine_Downtime / 60;
                                int minutes = (int)item.MS_Total_Machine_Downtime / 60;
                                TimeSpan nws = new TimeSpan(0, minutes, 0);
                                HttpContext.Cache[cache_key] = nws; 
                            }                          
                        } else if (item.MS_Total_Machine_Downtime > 0)
                        {
                            int mins = (int)item.MS_Total_Machine_Downtime / 60;
                            //int minutes = (int)item.MS_Total_Machine_Downtime % 60;
                            var newts = new TimeSpan(0, mins, 0); 
                            HttpContext.Cache.Insert(cache_key, newts, null, DateTime.Now.AddMonths(6), System.Web.Caching.Cache.NoSlidingExpiration);
                            
                        }

                        if (HttpContext.Cache["ms_timer_" + cid + "_" + item.MS_Id] != null)
                        {
                            UpdateTimerList(item.MS_Id, true, cid);
                        } else if (HttpContext.Cache[cache_key] != null)
                        {
                            UpdateTimerList(item.MS_Id, false, cid);
                        }
                        

                    }
                }  catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex); 
                }             
            }
        }

        private string getFBLabel(int id)
        {
            if (id == 0)
                return "ALL"; 

            string name = id.ToString();

            if (Session["MFB_Name"] != null && Session["MFB_Name"].ToString().Length > 0)
                name = Session["MFB_Name"].ToString();
            else
            {
                var dbname = service.getFBName(id);

                if (dbname.Length > 0)
                {
                    name = dbname;
                    Session["MFB_Name"] = dbname; 
                }
            }      

            return name;
        }

        private bool IsPrincipal()
        {
            var principal = ConfigurationManager.AppSettings["PRINCIPAL"];

            if (principal == HttpContext.User.Identity.Name)
                return true;

            return false; 
        }

        private bool IsAdmin()
        {
            var authcookie = HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName]; 
            
            if (authcookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authcookie.Value); 
                if (ticket.Name.Length > 0)
                {
                    ticketname = ticket.Name; 
                    string[] au = getAuthorizedUsers();

                    foreach (var item in au)
                    {
                        var userid = item.Split('@');

                        if (userid[0] == parseAuthTicketName(ticket.Name))
                            return true; 
                    }
                }
            }
            return false; 
        }

        private string[] getAuthorizedUsers()
        {

            AprService srv = new AprService();

            var cached_users = srv.getCachedAdminUserNames();

            if (cached_users != null)
                return cached_users;

            var db_sers = srv.getAdminUserNames();

            if (db_sers.Length > 0)
                srv.CacheAdminUsernames(db_sers);

            return db_sers; 
        }

        private string parseAuthTicketName(string tn)
        {
            if (tn.Length > 0)
            {
                var id = tn.Split('\\');
                if (id.Length == 1)
                    return id[0];
                else if (id.Length > 1)
                    return id[1]; 
            }
            return "";
        }
        #endregion
    }


}