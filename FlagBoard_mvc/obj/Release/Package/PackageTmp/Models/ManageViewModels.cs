using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FlagBoard_mvc.Models.EF;
using System;


namespace FlagBoard_mvc.Models
{
    public class IndexViewModel
    {
        public List<Machine> machines { get; set; }
        public List<Maintenance_Types> types { get; set; }
        public List<Employee> employees { get; set; }
        public pageInputs[] flagboards { get; set; }
        public string CID { get; set; }
        public Corporate Location { get; set; }
        public List<schedule> schedule { get; set; }
        public int MFB_Id { get; set; }
        public string MFB_label { get; set; }
        public short MS_Maint_Code { get; set; }
        public string errorMessage { get; set; }
        public bool isMobile { get; set; }
        public bool canDelete { get; set; }
        public TimerStatus[] ActiveTimers { get; set; }
        public string UserName { get; set; }
        public string ticketname { get; set; }
    }

    public class MobileEditorModel
    {
        public IList<Helpers.PageInput> inputs { get; set; }
        public IList<Helpers.InputObject> hiddenInputs { get; set; }
        public bool canDelete { get; set; }
        public string CID { get; set; }
        public int MS_Id { get; set; }
        public int MM_Id { get; set; }
        public Machine[] machines { get; set; }
    }

    public class FlagboardsModel
    {
        public IList<Helpers.PageInput> inputs { get; set;}
        public IList<Helpers.InputObject> hiddenInputs { get; set; }
        public string CID { get; set; }
        public int MFB_Id;
        public string MFB_Name; 
    }

    public class FlagboardPostModel
    {
        public int MFB_Id { get; set; }
        public string CID { get; set; }
    }
    public class LocationsModel
    {
        public IList<Helpers.PageInput> inputs { get; set; }
        public IList<Helpers.InputObject> hiddenInputs { get; set; }
        public int MFB_Id { get; set; }
        public string CID { get; set; }
    }

    public class LocationsPostModel
    {
        public int MFB_Id { get; set; }
        public string CID { get; set; }
    }

    public class TimerPostModel
    {
        public string TimerContent { get; set; }
        public string UserContent { get; set; }
    }

    public class schedule
    {
        public int MM_Number { get; set; }
        public string MT_Description { get; set; }
        public int MT_Id { get; set; }
        public string MT_MFB_Code2 { get; set; }
        public string MT_Name { get; set; }
        public string MS_Unscheduled_Reason { get; set; }
        public string MM_Name { get; set; }
        public int MM_Id { get; set; }
        public string MS_Workorder { get; set; }
        public int MFB_Id { get; set; }
        public string MS_WOCreate_Timestamp { get; set; }
        public string MS_WOClosed_Timestamp { get; set; }
        public DateTime? MS_WOClosed_Date { get; set; }
        public DateTime? MS_WOCreate_Date { get; set; }
        public string CompletedBy { get; set; }
        public string MS_Last_Main_Date { get; set; }
        public string MS_Next_Main_Date { get; set; }
        public DateTime? MS_Next_Main_Datetime { get; set; }
        public short MS_Maint_Time_Alotted { get; set; }
        public short MS_Maint_Time_Required { get; set; }
        public string MS_Main_Comp_Date { get; set; }
        public short MS_Frequency { get; set; }
        public int MS_Id { get; set; }
        public string MS_Notes { get; set; }
        public short MS_Maint_Code { get; set; }
        public float MS_Total_Machine_Downtime { get; set; }
        public int MS_Machine_Hours { get; set; }
        public int EMP_ID { get; set; }
        public string actionbtn { get; set; }
    }

    public class pageInputs
    {
        public string key;
        public string value; 
    }
    public class dbschedule
    {
        public int? MM_Number { get; set; }
        public string MT_Description { get; set; }      
        public int? MT_Id { get; set; }
        public string MT_MFB_Code2 { get; set; }
        public string MT_Name { get; set; }
        public string MS_Unscheduled_Reason { get; set; }
        public string MM_Name { get; set; }
        public int? MM_Id { get; set; }
        public string WorkOrder { get; set; }
        public int? MFB_Id { get; set; }
        public DateTime? Date_WOCreate_Timestamp { get; set; }
        public DateTime? Date_WOClosed_Timestamp { get; set; }
        public DateTime Date_Last_Main_Date { get; set; }
        public DateTime Date_Next_Main_Date { get; set; }
        public DateTime? Date_Main_Comp_Date { get; set; }
        public short? MS_Maint_Time_Alotted { get; set; }
        public short? MS_Maint_Time_Required { get; set; }
        public short? MS_Frequency { get; set; }
        public int MS_Id { get; set; }
        public string MS_Notes { get; set; }
        public short MS_Maint_Code { get; set; }
        public float MS_Total_Machine_Downtime { get; set; }
        public int? MS_Machine_Hours { get; set; }
        public int? EMP_ID { get; set; }
        public string EMP_First_Name { get; set; }
        public string EMP_Last_Name { get; set; }
    }

    public class ActiveMachineImgae
    {
        public int MMID { get; set; }
        public string filename1 { get; set; }
        public byte[] filebytes1 { get; set; }

    }

    public class TimerStatus
    {
        public int MS_Id { get; set; }
        public bool value { get; set; }
    }




}