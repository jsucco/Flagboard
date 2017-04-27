using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Collections;

namespace FlagBoard_mvc.Helpers
{
    public class objectMapper<t> where t : class, new()
    {
        private t obj = new t(); 

        public t mapCollection(ICollection<Helpers.InputObject> controls)
        {
            if (controls == null || controls.Count == 0)
                return new t();

            PropertyInfo field = null;
            foreach (var item in controls)
            {
                try
                {
                    field = obj.GetType().GetProperty(item.id);
                    if (field != null)
                        field.SetValue(obj, ConvertType(item.value, field.PropertyType.Name));
                } catch (Exception ex)
                {

                }            
            }

            return obj;
        }

        public Hashtable createPropertyInfoHash()
        {
            t mapObj = new t();
            Type busType = mapObj.GetType();
            PropertyInfo[] props = busType.GetProperties();
            Hashtable hash = new Hashtable(); 
            if (props != null)
            {
                foreach(PropertyInfo item in props)
                {
                    hash[item.Name.ToUpper()] = item; 
                }
            }
            return hash; 
        }

        public object ConvertType(object Obj, string TypeName)
        {
            object returnobj = new object();
            switch (TypeName.ToUpper())
            {
                case "STRING":
                    try
                    {
                        if (Obj == null)
                            throw new Exception("No Nulls");
                        returnobj = Convert.ToString(Obj).Trim();
                    }
                    catch (Exception e)
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
                    }
                    catch (Exception ex)
                    {
                        returnobj = 0;
                    }
                    break;
                case "SHORT":
                    try
                    {
                        if (Obj == null)
                            throw new Exception("No Nulls");
                        returnobj = Convert.ToInt16(Obj);
                    }
                    catch (Exception ex)
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
                    }
                    catch (Exception ex)
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