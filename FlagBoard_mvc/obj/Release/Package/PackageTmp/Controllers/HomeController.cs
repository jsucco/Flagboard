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

namespace FlagBoard_mvc.Controllers
{
    public class HomeController : Controller
    {
        private string CIDTest = "000578";
        private string debugMode = ConfigurationManager.AppSettings["debugMode"];
        private IndexViewModel model = new IndexViewModel();

        public ActionResult Index()
        {              
            model.CID = getCID();

            if (model.CID.Trim().Length < 6)
                return RedirectToAction("Locations", "Manage"); 

            Helpers.CtxService service = new Helpers.CtxService(null, model.CID);
            Helpers.ManagerService manageservice = new Helpers.ManagerService(); 
            try
            {
                model.MFB_Id = getSelectedFlagboard();
                model.machines = service.getMachines(model.MFB_Id);
                model.types = service.getMaintenanceTypes();
                model.employees = service.getEmployees(); 
                model.schedule = getSchedule();
                model.Location = manageservice.getRecord(model.CID.Trim()); 
                model.flagboards = service.getFlagBoards();            
                model.MFB_label = (model.MFB_Id < 1) ? "ALL" : model.MFB_Id.ToString(); 
                model.MS_Maint_Code = getMaintCode();
                model.isMobile = isMobile();
                model.canDelete = false; 
            } finally
            {
                service.Dispose();
                model.schedule = filterSchedule(model.schedule);           
            }      

            if (service.error == true)
                model.errorMessage = service.errorMessage;

            if (model.CID.Length < 6)
                return RedirectToAction("Locations", "Manage"); 
            
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
            int MS_Id = 0;
            int MFB_Id = 0; 

            if (CID.Length < 6)
                return RedirectToAction("Locations", "Manage");    
                   
            if (Request.QueryString["MSID"] != null)
                MS_Id = (Int32.TryParse(Request.QueryString["MSID"], out MS_Id) == false) ? 0 : MS_Id;

            if (Request.QueryString["MFBID"] != null)
                MFB_Id = (Int32.TryParse(Request.QueryString["MFBID"], out MFB_Id) == false) ? 0 : MFB_Id; 

            Models.MobileEditorModel model = getEditorModelObject(CID, MS_Id, MFB_Id);
            model.canDelete = false;
            model.MS_Id = MS_Id; 
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
        public ActionResult MobileEditor(ICollection<InputObject> pageInputs, string method, string CID, string MSID)
        {
            int MS_Id = 0;
            int MFBID = 1;
            Int32.TryParse(MSID, out MS_Id);
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
                Models.MobileEditorModel model = getEditorModelObject(CID, MS_Id, MFBID);
                model.CID = CID;
                model.MS_Id = MS_Id;
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
                    if (record.MS_Maint_Code == 1 && record.MS_Main_Comp_Date == "True")
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
                if (updated != null && updated.Count > 0)
                    service.cache(updated); 
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
                           // System.Threading.Tasks.Task.Run(() => AutoAddScheduledOrder(record, service));
                            AutoAddScheduledOrder(record, service);
                            //if (record.MS_Frequency < 1)
                            //    throw new Exception("Interval must be greater than 0");
                            //int defaultEMPID = service.getDefaultEMPID(); 
                            //record.MS_Next_Main_Date = DateTime.Now.AddDays(record.MS_Frequency).ToShortDateString() + " " + DateTime.Now.AddDays(record.MS_Frequency).ToShortTimeString();
                            //record.MS_Main_Comp_Date = ""; 
                            //record.MS_WOCreate_Timestamp = DateTime.Now.AddDays(record.MS_Frequency).ToShortDateString() + " " + DateTime.Now.AddDays(record.MS_Frequency).ToShortTimeString();
                            //record.MS_WOClosed_Timestamp = "";
                            //record.EMP_ID = (defaultEMPID > 0) ? defaultEMPID : record.EMP_ID; 

                            //int result = service.Add(record);
                            //if (result == 0)
                            //    throw new Exception("Error editing scheduled maintenance record."); 
                        }
                        userMessage = "true"; 
                        break;
                    case "add":
                        if (record.MS_Unscheduled_Reason.Length > 50)
                            throw new Exception("Unscheduled Reason cannot be greater than 50 characters long.");

                        int rowsaff = service.Add(record);
                        if (rowsaff == 0)
                            throw new Exception("unknown error adding rows. ");
                        userMessage = "true"; 
                        break;
                    case "delete":
                        service.delete(record.MS_Id);
                        userMessage = "true"; 
                        break; 
                }
                service.clearCache(); 
            } catch (Exception ex)
            {
                userMessage = "ERROR: " + ex.Message; 
            } finally
            {
                service.Dispose();
            }

            return Json(userMessage, JsonRequestBehavior.AllowGet); 
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

        private List<schedule> getSchedule()
        {
            if (HttpContext.Cache["MaintenanceSchedule"] != null)
            {
                try
                {
                    List<schedule> cachedrecs = (List<schedule>)HttpContext.Cache["MaintenanceSchdule"];
                    if (cachedrecs.Count > 0)
                        return cachedrecs;

                } catch (Exception ex)
                {

                }
                
            }                
            Helpers.CtxService service = new Helpers.CtxService(null, model.CID);

            return service.getSchedule();
        }

        private Models.MobileEditorModel getEditorModelObject(string CID, int MS_Id, int MFB_Id)
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
            return model;
        }

        private List<Helpers.PageInput> getEditorInputs(string CID, int MFB_Id, int MS_Id = 0)
        {
            List<Helpers.PageInput> inputs = new List<Helpers.PageInput>();

            inputs.Add(getFlagboardDropdown(CID, MFB_Id));

            inputs.Add(getMachineDropdown(CID, MFB_Id));

            inputs.Add(getMTypesDropdown(CID));

            inputs.Add(getEmployeesDropdown(CID));

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

            Helpers.PageInput downtime = new Helpers.PageInput("default", "Total Machine Downtime", "MS_Total_Machine_Downtime", "MS_Total_Machine_Downtime", "0");
            downtime.function = "number";
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
                List<int> fbs = service.getFlagBoards();

                if (fbs == null || fbs.Count == 0)
                {
                    fbInput.errorFlag = true;
                    fbInput.errorMessage = "ERROR: No FlagBoards found in database.";
                    return fbInput;
                }

                foreach (var item in fbs)
                {
                    fbInput.input.options.Add(new Helpers.InputObject.option { text = "Flagboard # " + item, value = item.ToString() });
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
                record.EMP_ID = (defaultEMPID > 0) ? defaultEMPID : record.EMP_ID;

                int result = service.Add(record);
                if (result == 0)
                    throw new Exception("Error editing scheduled maintenance record.");
            } finally
            {
                service.Dispose(); 
            }
            
        }

        #endregion
    }


}