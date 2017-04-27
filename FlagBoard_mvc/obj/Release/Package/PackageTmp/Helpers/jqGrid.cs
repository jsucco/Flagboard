using System;
using System.Collections; 
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using System.Reflection;
using System.Data;
using FlagBoard_mvc.Models; 

namespace FlagBoard_mvc.Helpers
{

    
    public class jqGrid<t> where t : class, new()
    {
        public List<t> loadPageRecords(int pageNum, int recordsPerPage, IList<t> allRecords)
        {
            List<t> pageRecords = new List<t>();
            int FirstRecordRowNumber = (pageNum * recordsPerPage) - recordsPerPage;
            int LastRecordRowNumber = pageNum * recordsPerPage; 

            for (var i = 0; i < allRecords.Count; i++)
            {
                if (i >= FirstRecordRowNumber && i <= LastRecordRowNumber)
                    pageRecords.Add(allRecords[i]);
                if (i == LastRecordRowNumber)
                    break; 
            }

            return pageRecords; 
        }

        public t mapToObjectProperty(pageInputs[] inputs)
        {

            t mapObj = new t();
            Type busType = mapObj.GetType();
            PropertyInfo[] fields = busType.GetProperties(); 
            Hashtable hash = new Hashtable();

            if (fields != null) 
                foreach(PropertyInfo item in fields)
                {
                    hash[item.Name.ToUpper()] = item;
                }
            ICollection keys = hash.Keys; 
            if (hash.Count > 0)
            {
                foreach(var item in inputs)
                {
                    PropertyInfo info = (PropertyInfo)hash[item.key.ToUpper()];
                    if (info != null)
                    {
                        try
                        {
                            object fieldval = ConvertType(item.value, info.PropertyType.Name);
                            info.SetValue(mapObj, fieldval);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            return mapObj; 
        }

        public t getReqParamsAsObject(NameValueCollection RequestParams)
        {
            t mapObj = new t();
            Type busType = mapObj.GetType();
            FieldInfo[] properties = busType.GetFields();
            Hashtable hash = new Hashtable(); 

            if (properties != null)
            {
                foreach(FieldInfo item in properties)
                {
                    hash[item.Name.ToUpper()] = item; 
                }
                ICollection keys = hash.Keys; 
                if (hash.Count > 0)
                {
                    foreach(DictionaryEntry entry in hash)
                    {
                        FieldInfo info = (FieldInfo)hash[entry.Key];
                        if (info != null )
                        {
                            string[] param = RequestParams.GetValues(entry.Key.ToString());
                            if (param != null && param.Length > 0)
                            {
                                object obj = ConvertType(param[0], info.FieldType.Name);
                                info.SetValue(mapObj, obj);
                            }
                        }
                    }
                }
            }

            return mapObj; 
        }

        public List<t> FilterType(string Field, string value, List<t> records)
        {
            List<t> filtered = new List<t>();

            t mapObj = new t();
            Type busType = mapObj.GetType();
            PropertyInfo[] props = busType.GetProperties();
            PropertyInfo FilterProp = null; 

            if (props != null)
            {
                foreach (PropertyInfo item in props)
                {
                    if (item.Name.ToUpper() == Field.ToUpper())
                    {
                        FilterProp = item;
                        break;
                    }             
                }

                if (FilterProp != null)
                {
                    foreach (t item in records)
                    {
                        object result = FilterProp.GetValue(item);
                        if (result.ToString() == value)
                        {
                            filtered.Add(item);
                        }
                    }
                }
            }

            return filtered; 
        }
        private object ConvertType(object Obj, string TypeName)
        {
            object returnobj = new object(); 
            switch(TypeName.ToUpper())
            {
                case "STRING":
                    try
                    {
                    if (Obj == null)
                        throw new Exception("No Nulls");
                        returnobj = Convert.ToString(Obj); 
                    } catch (Exception e)
                    {
                        returnobj = ""; 
                    }
                break;
                case "INT16":
                    try
                    {
                        if (Obj == null)
                            throw new Exception("No Nulls");
                        returnobj = Convert.ToInt16(Obj); 
                    } catch(Exception ex)
                    {
                        returnobj = 0; 
                    }
                    break;
                case "INT32":
                    try
                    {
                    if (Obj == null)
                        throw new Exception("No Nulls");
                        returnobj = Convert.ToInt32(Obj);
                    }
                    catch (Exception e)
                    {
                        returnobj = 0;
                    }
                break;
                case "INTEGER":
                    try
                    {
                    if (Obj == null)
                        throw new Exception("No Nulls");
                        returnobj = Convert.ToInt32(Obj);
                    }
                    catch (Exception e)
                    {
                        returnobj = 0;
                    }
                break;
                case "INT64":
                    try
                    {
                    if (Obj == null)
                        throw new Exception("No Nulls");
                        returnobj = Convert.ToInt64(Obj);
                    }
                    catch (Exception e)
                    {
                        returnobj = 0;
                    }
                    break;
                case "BOOL":
                    try
                    {
                    if (Obj == null)
                        throw new Exception("No Nulls");
                        returnobj = Convert.ToBoolean(Obj); 
                    }
                    catch (Exception e)
                    {
                        returnobj = false;
                    }
                    break;
                case "BOOLEAN":
                    try
                    {
                    if (Obj == null)
                        throw new Exception("No Nulls");
                        returnobj = Convert.ToBoolean(Obj);
                    }
                    catch (Exception e)
                    {
                        returnobj = false;
                    }
                    break;
                case "DECIMAL":
                    try
                    {
                    if (Obj == null)
                        throw new Exception("No Nulls");
                        returnobj = Convert.ToDecimal(Obj);
                    }
                    catch (Exception e)
                    {
                        returnobj = 0;
                    }
                    break;
                case "SINGLE":
                    try
                    {
                        if (Obj == null)
                            throw new Exception("No Nulls");
                        returnobj = Convert.ToSingle(Obj); 
                    } catch (Exception ex)
                    {
                        returnobj = 0; 
                    }
                    break;
                case "DATETIME":
                    try
                    {
                    if (Obj == null)
                        throw new Exception("No Nulls");
                        returnobj = Convert.ToDateTime(Obj);
                    }
                    catch (Exception e)
                    {
                        returnobj = new DateTime(1900, 1, 1);
                    }
                    break;

            }
            return returnobj; 
        }
    }
}