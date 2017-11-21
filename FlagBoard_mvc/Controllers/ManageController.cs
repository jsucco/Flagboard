using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlagBoard_mvc.Helpers; 

namespace FlagBoard_mvc.Controllers
{
    public class ManageController : Controller
    {

        private string CIDTest = "000485";
        private string debugMode = ConfigurationManager.AppSettings["debugMode"];
        private Models.FlagboardsModel fbmodel;
        private Dictionary<string, string> fbdict = new Dictionary<string, string>(); 

        // GET: Manage
        public ActionResult Flagboards()
        {
            var CID = getCID();
           
            if (CID.Length < 6)
                RedirectToAction("Locations");

            fbmodel = getFlagboardModelObjects(CID);
            fbmodel.CID = CID;
            fbmodel.MFB_Id = getSelectedFlagboard(); 

            return View(fbmodel);
        }

        [HttpPost]
        public ActionResult Flagboards(ICollection<InputObject> pageInputs)
        {
            Models.FlagboardsModel errorModel = new Models.FlagboardsModel();
            Models.FlagboardPostModel posted = new Models.FlagboardPostModel(); 
            try
            {
                objectMapper<Models.FlagboardPostModel> mapper = new objectMapper<Models.FlagboardPostModel>();

                posted = mapper.mapCollection(pageInputs);

                if (posted.MFB_Id > 0)
                {
                    insertCookie("MFBInfo", "MFBID", posted.MFB_Id.ToString());
                    setFBName(posted.MFB_Id); 
                }

                if (posted.CID.Length < 6)
                    throw new Exception("ERROR: incorrect formatting of Location variable.  Please reset location.");

                return RedirectToAction("Index", "Home", new { CID = posted.CID, MFBID = posted.MFB_Id.ToString() });
            } catch (Exception Ex)
            {
                errorModel = getFlagboardModelObjects(posted.CID);
                errorModel.inputs[0].errorFlag = true;
                errorModel.inputs[0].errorMessage = "ERROR: " + Ex.Message;
            }
            return View(errorModel); 
        }

        public ActionResult Locations()
        {
            Models.LocationsModel model = getLocationsModelObjects();

            int MFBINT = 0;
            Int32.TryParse(Request.QueryString["MFBID"], out MFBINT);
            model.MFB_Id = (Request.QueryString["MFBID"] == null) ? 0 : MFBINT;
            model.CID = getCID(); 

            return View(model); 
        }

        [HttpPost]
        public ActionResult Locations(ICollection<InputObject> inputs)
        {
            Models.LocationsModel errorModel = new Models.LocationsModel();
            Models.LocationsPostModel posted = new Models.LocationsPostModel(); 
            try
            {
                objectMapper<Models.LocationsPostModel> mapper = new objectMapper<Models.LocationsPostModel>();

                posted = mapper.mapCollection(inputs);

                if (posted.CID.Trim().Length < 6)
                    throw new Exception("ERROR: location variable format incorrect.  Contact developer.");

                return RedirectToAction("Index", "Home", new { CID = posted.CID.Trim(), MFBID = posted.MFB_Id.ToString() });

            } catch (Exception ex)
            {
                errorModel = getLocationsModelObjects();
                errorModel.inputs[0].errorFlag = true;
                errorModel.inputs[0].errorMessage = ex.Message;
            }
            return View(errorModel); 
        }

        #region Helpers
        private Models.FlagboardsModel getFlagboardModelObjects(string CID)
        {
            Models.FlagboardsModel model = new Models.FlagboardsModel();
            try
            {
                List<PageInput> controls = new List<PageInput>();
                controls.Add(getFlagboardDropdown(CID));
                model.inputs = controls as IList<PageInput>;

                List<InputObject> hiddenControls = new List<InputObject>();
                InputObject hidden1 = new InputObject();
                hidden1.id = "CID";
                hidden1.value = CID;
                hiddenControls.Add(hidden1);
                model.hiddenInputs = hiddenControls as IList<InputObject>;
            } catch (Exception ex)
            {

            }
            
            return model;
        }

        private void insertCookie(string name, string key, string value)
        {
            if (name.Trim().Length == 0 || key.Trim().Length == 0 || value.Trim().Length == 0)
                return; 
            
            HttpCookie inserted = new HttpCookie(name);
            inserted[key] = value;
            inserted.Expires = DateTime.Now.AddDays(60);
            Response.Cookies.Remove(name); 
            Response.Cookies.Add(inserted); 
        }
        private Models.LocationsModel getLocationsModelObjects()
        {
            Models.LocationsModel model = new Models.LocationsModel();
            try
            {
                List<PageInput> controls = new List<PageInput>();

                controls.Add(getLocationsDropdown());
                model.inputs = controls as IList<PageInput>;

                List<InputObject> hiddenControls = new List<InputObject>();
                InputObject hidden1 = new InputObject();
                hidden1.id = "MFB_Id";
                hidden1.value = getSelectedFlagboard().ToString();
                model.hiddenInputs = hiddenControls as IList<InputObject>;
            } catch (Exception ex)
            {

            }
            
            return model; 
        }

        private PageInput getFlagboardDropdown(string CID)
        {
            PageInput fbInput = new PageInput("select", "Flagboards", "MFB_Id", "MFB_Id", "");
            try
            {
                CtxService service = new CtxService(null, CID);
                List<Models.pageInputs> fbs = service.getFlagBoardsInput();

                if (fbs == null || fbs.Count == 0)
                {
                    fbInput.errorFlag = true;
                    fbInput.errorMessage = "ERROR: No FlagBoards found in database.";
                    return fbInput;
                }

                foreach (var item in fbs)
                {
                    fbInput.input.options.Add(new InputObject.option { text = "FB " + item.value, value = item.key });
                    fbdict.Add(item.key, item.value); 
                }

                fbInput.input.options.Add(new InputObject.option { text = "ALL", value = "0" });

                Session["MFB_Array"] = fbdict; 
            }
            catch (Exception ex)
            {
                fbInput.errorFlag = true;
                fbInput.errorMessage = "ERROR CREATING FB SELECT LIST: " + ex.Message;
            }

            return fbInput;
        }

        private void setFBName(int MFB_Id)
        {
            fbdict = (Dictionary<string, string>)Session["MFB_Array"]; 

            if (MFB_Id != 0 && fbdict.ContainsKey(MFB_Id.ToString()))
            {
                Session["MFB_Name"] = fbdict[MFB_Id.ToString()];
            } 
        }

        private PageInput getLocationsDropdown()
        {
            PageInput LocationInput = new PageInput("select", "SELECT A Location", "CID", "CID", "");
            try
            {
                ManagerService service = new ManagerService();
                
                List<Models.EF.Corporate> locations = service.getLocations();
                
                if (locations == null || locations.Count == 0)
                {
                    LocationInput.errorFlag = true;
                    LocationInput.errorMessage = "ERROR: No Locations found in database.";
                    return LocationInput;
                }

                AprService aprservice = new AprService();

                locations = aprservice.filterInActiveLocations(locations); 

                foreach (var item in locations)
                {
                    LocationInput.input.options.Add(new InputObject.option { text = item.CorporateName, value = item.CID.ToString() });
                }
            }
            catch (Exception ex)
            {
                LocationInput.errorFlag = true;
                LocationInput.errorMessage = "ERROR CREATING FB SELECT LIST: " + ex.Message;
            }

            return LocationInput;
        }

        private string getCID()
        {

            if (debugMode == "true")
                return CIDTest;
            return (Request.QueryString["CID"] != null && Request.QueryString["CID"].Trim().Length > 0) ? Request.QueryString["CID"] : checkCookie("APRKeepMeIn", "CID_Print");
        }

        private int getSelectedFlagboard()
        {
            int flag = 0;
            try
            {
                var test = Request.QueryString["MFBID"];
                flag = (Request.QueryString["MFBID"] != null) ? Convert.ToInt32(Request.QueryString["MFBID"]) : Convert.ToInt32(checkCookie("MFBInfo", "MFBID"));
            }
            catch (Exception ex)
            {

            }
            return flag;
        }

        private string checkCookie(string CookieName, string key = "")
        {
            if (Request.Cookies[CookieName] != null && Request.Cookies[CookieName].HasKeys)
                if (Request.Cookies[CookieName][key] != null)
                    return Request.Cookies[CookieName][key];
            return "0";
        }

        #endregion
    }
}