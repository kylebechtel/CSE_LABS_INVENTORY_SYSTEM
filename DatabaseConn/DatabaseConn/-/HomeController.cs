using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DatabaseConn.Controllers
{
    public class HomeController : Controller
    {




        /********************************************************
         * Disposable Items
         * - Remove
         * - Add
         * - View
         ********************************************************/

        public ActionResult removeDisposable()
        {
            string all = Request.Form["All"];
            string itemsUsed = Request.Form["quantity"];
            string UPC = Request.Form["UPC"];

            if (all != null)
            {
                query("Delete from [Inventory].[dbo].[Disposable] where UPC = '" + UPC + "';");
                ViewBag.message = "true:RemovedAllDisposable";
                return View();
            }

            String quantityCheck = query("SELECT quantity from [Inventory].[dbo].[Disposable] where upc = '"+UPC+"';");
            if (quantityCheck == null || quantityCheck.Equals(""))
            {
                ViewBag.message = "false:RemoveNoSuchItem";
                return View();
            }
            int decrement = 0, currentQuantity = 0;
            try
            {
                currentQuantity = Int32.Parse(quantityCheck);
                decrement = Int32.Parse(itemsUsed);
            }
            catch(Exception e)
            {
                ViewBag.message = "False:InvalidQuantity:PraseError";
                return View();
            }
            if (decrement < currentQuantity)
            {
                string ret = query("Update [Inventory].[dbo].[Disposable] set quantity = " + (currentQuantity - decrement) + 
                    " WHERE UPC = '" + UPC + "';");
                if (ret == null || ret.Equals(""))
                    ViewBag.messaage = "False:RemovedItem";
                else
                    ViewBag.message = "true:Removed" + decrement + "Items";
            }
            else
            {
                string ret = query("Delete from [Inventory].[dbo].[Disposable] where UPC = '" + UPC + "';");
                if (ret != null || ret.Equals(""))
                    ViewBag.message = "False:removeDisposableFailed";
                else
                    ViewBag.message = "true:RemovedRemainingDisposable";
            }

            return View();
        }

        public ActionResult viewDisposable()
        {

            string UPC = Request.Form["UPC"];
            string existance = query("SELECT UPC,quantity,description FROM [Inventory].[dbo].[Disposable] WHERE UPC = '" + UPC + "';");
  
            if (existance.Equals(""))
            {

                    ViewBag.serial = "Your Item: Does not exist.";
                    return View();

            }

            String[] parts = existance.Split('|');
            ViewBag.UPC = "UPC: " + parts[0];
            ViewBag.description = "Description: " + parts[2];
            ViewBag.Quantity = "Quantity: " + parts[1];


            return View();
        }

        public ActionResult addDisposable()
        {

            string UPC = Request.Form["UPC"];
            string ammount = Request.Form["quantity"];
            string description = Request.Form["description"]; 
            String existanceCheck = query("Select Quantity from [inventory].[dbo].[Disposable] where UPC = " + UPC+";");
            if (existanceCheck.Equals(""))
            {
                query("Insert into [inventory].[dbo].[Disposable] (quantity, UPC, description) values(" + UPC + "," + ammount + ","+description+");");
                ViewBag.message = "true:AddedNewDisposable";
            }
            else
            {
                int quantity = Int32.Parse(existanceCheck) + Int32.Parse(ammount);
                if (quantity < 0)
                {
                    ViewBag.message = "false:addItem:InvalidQuantity";
                }
                string retUp = query("Update [inventory].[dbo].[Disposable] set Quantity = " + quantity + "where UPC = "+UPC+";");
                if (retUp.Contains("false"))
                    ViewBag.message = "false:UpdateFailed";
                else
                    ViewBag.message = "true:UpdatedQuantity";
            }
            return View();
        }
        
        


        /**************************************************************
         * Item Methods
         * - Remove
         * - Add
         * - View
         **************************************************************/
        public ActionResult removeItem()
        {
            string serial = Request.Form["serial_number"];
            string existanceCheck = query("SELECT * FROM [Inventory].[dbo].[Item] WHERE serial_number = '" + serial + "';");

            if (!existanceCheck.Equals(""))
            {
                string ret = query("DELETE FROM [Inventory].[dbo].[Item]  WHERE serial_number = '" + serial + "';");
                ViewBag.message = "true:Remove";
            }
            else
                ViewBag.message = "false:Remove";
            return View();
        }

        public ActionResult addItem()
        {
            string UPC = Request.Form["UPC"];
            string serial = Request.Form["serial_number"];
            string model = Request.Form["model_number"];
            string manufacture = Request.Form["manufacturer"];
            string bId = Request.Form["building_ID"];
            string descript = Request.Form["description"];
            string PON = Request.Form["PO_number"];
            string checkedO = Request.Form["checked_out"];
            string checkedI = Request.Form["checked_in"];
            string PID = Request.Form["person_ID"];
            string vend = Request.Form["vendor"];
            string decal = Request.Form["decal"];
            ViewBag.message = "Results: ";
            ViewBag.param = ""+UPC +" | "+serial+" | "+model+" | "+manufacture+
                " | "+bId+" | "+descript+" | "+PON+" | "+checkedO+" | "+checkedI+" | "+PID+" | "+vend+" | "+decal+" | ";

            string existance = query("SELECT serial_number FROM [Inventory].[dbo].[Item] WHERE serial_number = " + serial + ";");
            ViewBag.exist = existance;
            if (existance.Equals("") == false)
            {
                ViewBag.message = "" + existance + " False:addItem:AlreadyExists"; 
                return View();
            }
            else if (!UPC.Equals("") && !serial.Equals("") && !descript.Equals("") && !model.Equals("") && !manufacture.Equals(""))
            {
                
                String ret = query("INSERT INTO [Inventory].[dbo].[Item]" +
                    "(UPC,serial_number,description,building_ID,received_date,model_number,manufacturer,surplus_date,PO_number,checked_out" +
                    " ,checked_in,person_ID) values" +
                    "(" + UPC + ",'" + serial + "','" + descript + "','J','" + getDate() + "','" + model + "','" + manufacture + "',NULL,NULL,NULL,NULL,NULL);");

                if (ret.Contains("False") || ret.Equals(""))
                    ViewBag.message += "False:addItemFull";
                else
                    ViewBag.message += "True:addItemFull";
            }
            else if (!UPC.Equals("") && !serial.Equals("") && !descript.Equals(""))
            {
                String ret = "";
                ret +=query("INSERT INTO [Inventory].[dbo].[Item]" +
                     "(UPC,serial_number,description,building_ID,received_date,model_number,manufacturer,surplus_date,PO_number,checked_out" +
                     ",checked_in,person_ID) values" +
                     "(" + UPC + ",'" + serial + "','" + descript + "','J','" + getDate() + "',NULL,NULL,NULL,NULL,NULL,NULL,NULL);"
                     );

                if (ret.Contains("False") || ret.Equals(""))
                    ViewBag.message += "False:addItem";
                else
                    ViewBag.message += "True:addItem";
            }
            else
                ViewBag.message += "UnableToAdd";

            if (PON.Equals(""))
                return View();

            string poExist = query("SELECT * FROM [Inventory].[dbo].[PO] WHERE PO_number = "+PON+";");
           if(!poExist.Equals(""))
            {
                string ret = query("UPDATE [Inventory].[dbo].[Item] SET PO_number = " + PON + " WHERE serial_number = '" + serial + "';");
            }
            else
            {
                string poRet = query("INSERT INTO [Inventory].[dbo].[PO] VALUES (" + PON + ",'" + vend + "','" + decal + "');");
                if (poRet.Contains("False:"))
                    ViewBag.message += ":False:addPOtoItem";
                else
                    ViewBag.message += ":True:addPOtoItem";
            }
            return View();
        }

        public ActionResult viewItem()
        {
            string serial = Request.Form["serial_number"];
            string decal = Request.Form["decal"];
            string existance = query("SELECT serial_number,description,person_ID,decal FROM [Inventory].[dbo].[Item] WHERE serial_number = '" + serial + "';");
            string dExist = query("SELECT serial_number,description,person_ID,decal FROM [Inventory].[dbo].[Item] WHERE decal = '" + decal + "';");
           
            if (existance.Equals(""))
            {
                if (dExist.Equals(""))
                {
                    ViewBag.serial = "Your Item: Does not exist.";
                    return View();
                }
                else
                    existance = dExist;
            }

            String[] parts = existance.Split('|');
            ViewBag.serial = "Serial Number: " + parts[0];
            ViewBag.descript = "Description: " + parts[1];
            ViewBag.decal = "Decal ID: " + parts[3];
            if (parts[2].Trim().Equals(""))
            {
                ViewBag.personName= "Person: Not checked out.";
            }
            else
            {
                ViewBag.personId = "Person ID: " + parts[2];
                string peopleFind = query("SELECT name FROM [Inventory].[dbo].[Person] WHERE person_ID = '" + parts[2] + "';");
                parts = peopleFind.Split('|');
                ViewBag.personName = "Person Name: " + parts[0];
            }
            return View();
        }


        /*************************************************************
         * Person Methods
         * - View Items by Person ID method (Bug Kyle about format)
         * - Check In (not finished)
         * - Check Out (not finished)
         *************************************************************/
        public ActionResult viewPerson()
        {
            string pid = Request.Form["person_ID"];
            string existance = query("SELECT serial_number,description,decal FROM [Inventory].[dbo].[Item] WHERE person_ID = '" + pid + "';");

            if (existance.Equals(""))
            {
                    ViewBag.serial = "This person has: 0 Items checked out.";
                    return View();

            }
            ViewBag.param = existance;
            String[] parts = existance.Split('|');
            for (int i = 0; i < parts.Length-2; i+=3)
            {
                ViewBag.upc += "" + parts[i];
                ViewBag.descript += "," + parts[i+1];
                ViewBag.decal += ", " + parts[i+2];
            }
         
            /*    ViewBag.personId = "Person ID: " + parts[2];
                string peopleFind = query("SELECT name FROM [Inventory].[dbo].[Person] WHERE person_ID = '" + parts[2] + "';");
                parts = peopleFind.Split('|');
                ViewBag.personName = "Person Name: " + parts[0];
            */
            return View();
        }

        public ActionResult checkIn()
        {
            string serial = Request.Form["serial_number"];
            string decal = Request.Form["decal"];

            string existance = query("SELECT serial_number,person_ID,decal FROM [Inventory].[dbo].[Item] WHERE serial_number = " + serial + ";");
            string dExist = query("SELECT serial_number,person_ID,decal FROM [Inventory].[dbo].[Item] WHERE decal = '" + decal + "';");
            if (existance.Equals("") && dExist.Equals(""))
            {
                ViewBag.error = "Sorry, that item does not exist in the database.";
                return View();
            }

            string[] parts = existance.Split('|');

            string pExist = parts[1];
            if(pExist == null || pExist.Equals("") || pExist.ToUpper().Equals("NULL"))
            {
                ViewBag.error = "This Item has not been checked out.";
                return View();
            }

            if (!existance.Equals(""))
            {
                string retUp = query("Update [inventory].[dbo].[Item] set person_ID =  NULL where serial_number = " + serial + ";");
                ViewBag.message = "Item with serial number " + serial + " was checked in.";
            }
            else if(!dExist.Equals(""))
            {
                string retUp = query("Update [inventory].[dbo].[Item] set person_ID =  NULL where decal = " + decal + ";");
                ViewBag.message = "Item with decal " + decal + " was checked in.";
            }

            return View();
        }
        public ActionResult checkOut()
        {
            string serial = Request.Form["serial_number"];
            string decal = Request.Form["decal"];
            string personId = Request.Form["person_ID"];
            string personName = Request.Form["person_name"]; // It is name in the database, but that seems like it could cuase problems elsewhere
            // get building id

            string existance = query("SELECT serial_number,person_ID,decal FROM [Inventory].[dbo].[Item] WHERE serial_number = " + serial + ";");
            string dExist = query("SELECT serial_number,person_ID,decal FROM [Inventory].[dbo].[Item] WHERE decal = '" + decal + "';");
            ViewBag.param = existance; // DEBUG

            if (existance.Equals("") && dExist.Equals(""))
            {
                ViewBag.error = "Sorry, that item does not exist in the database.";
                return View();
            }
            if(personId.Equals(""))
            {
                ViewBag.error = "Sorry, no person was specified for the item to be checked out to.";
                return View();
            }
            string pidExist = query("SELECT person_ID,name FROM [Inventory].[dbo].[Person] WHERE person_ID = '" + personId + "';");
            if(pidExist.Equals(""))
            {
                // If the ID was not in the database, check to see if the name was as well   
                string pinExist = query("SELECT person_ID,name FROM [Inventory].[dbo].[Person] WHERE name = '" + personName + "';");
                if (pinExist.Equals("")) // If the name was not in the database either, 
                {
                    // Insert into databse that person
                    string insertResult = query("INSERT INTO [Inventory].[dbo].[Person] (person_ID,name) VALUES (" + personId + ", '" + personName + "');");
                }
                else
                {
                    // Gets the correct PID from the database based on the name you entered
                    string[] parts = pidExist.Split('|');
                    personId = parts[0];
                }

            }
            if(personName.Equals(""))
            {
                string[] parts = pidExist.Split('|');
                personName = parts[1];
            }
            // A valid Serial Number or Decal is present
            // a Valid PID is also present at this point.
            string result = "false";
            // Check that person into the system
            if(!existance.Equals(""))
                result = query("UPDATE [Inventory].[dbo].[Item] SET person_ID = " + personId + " WHERE serial_number = '" + serial + "';");
            else if (!dExist.Equals(""))
                result = query("UPDATE [Inventory].[dbo].[Item] SET person_ID = " + personId + " WHERE decal = '" + decal + "';");
            if (!result.Contains("false"))
                ViewBag.message = "The Item " + serial + " was checked out to " + personName + " sucessfully.";
            return View();
        }

        /*************************************************************
         * Other Methods (Default Page Methods)
         * - Index
         * - Contact
         * - About
         *************************************************************/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {

            ViewBag.message = "This is our about page!";
            

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Our contact page.";

            return View();
        }



        /****************************************************
         * Refference Methods
         * - Query
         * - GetDate
         ****************************************************/

        //generalize so it returns a list of strings, and can select things from the database based on whats passed
        public String query(String query, List<String> keys)
        {
            String data = "";
            //taken from http://stackoverflow.com/questions/14171794/retrieve-data-from-a-sql-server-database-in-c-sharp
            try
            {
                
                //stores the connection config stuff
                var con = ConfigurationManager.ConnectionStrings["InventoryConnection"].ToString();

                //opens connection to db
                using (SqlConnection myConnection = new SqlConnection(con))
                {

                    //sets up the query to execute on the given database connection
                    SqlCommand cmd = new SqlCommand(query, myConnection);
                    myConnection.Open();

                    //executes the command and returns an sqldatareader thing
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (query.Contains("INSERT"))
                        {
                            myConnection.Close();
                            return "True:InsertedCorrectly";
                        }
                        else if (query.Contains("UPDATE"))
                        {
                            myConnection.Close();
                            return "True:UpdatedCorrectly";
                        }
                        else if (query.Contains("DELETE FROM"))
                        {
                            myConnection.Close();
                            return "True:DeletedCorrectly";
                        }
                        for (int i = 0; i < keys.Count; i++)
                        {
                                data += (reader["serial_number"].ToString());
                            // generalize for multiple strings
                            // Pass in a list of things to pull from ths reader (like up)

                        }
                        myConnection.Close();
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                return ""+e.ToString();
            }
            catch(Exception e)
            {
                return "False:failedQuery";
            }
            return data;
        }
        public String query(String query)
        {
            //taken from http://stackoverflow.com/questions/14171794/retrieve-data-from-a-sql-server-database-in-c-sharp
            try
            {
                //stores the connection config stuff
                var con = ConfigurationManager.ConnectionStrings["InventoryConnection"].ToString();

                //opens connection to db
                using (SqlConnection myConnection = new SqlConnection(con))
                {

                    //sets up the query to execute on the given database connection
                    SqlCommand cmd = new SqlCommand(query, myConnection);
                    myConnection.Open();

                    //executes the command and returns an sqldatareader thing
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (query.Contains("INSERT"))
                        {
                            myConnection.Close();
                            return "True:InsertedCorrectly";
                        }
                        else if (query.Contains("UPDATE"))
                        {
                            myConnection.Close();
                            return "True:UpdatedCorrectly";
                        }
                        else if(query.Contains("DELETE FROM"))
                        {
                            myConnection.Close();
                            return "True:DeletedCorrectly";
                        }
                         String ret = "";
                        // Grab reader at a key?
                        while (reader.Read())
                        {
                            Object[] v = new Object[reader.FieldCount];
                            reader.GetValues(v);
                            for (int i = 0; i < v.Length; i++)
                                ret += v[i]+" | ";
                            ret += "\n";
                        }
                            // generalize for multiple strings
                            // Pass in a list of things to pull from ths reader (like up)

                      
                        myConnection.Close();
                        return ret;
                    }
                }
            }
            catch (Exception e)
            {
                return e.ToString()+"false:queryfunction";
            }
        }

        public string getDate()
        {
            DateTime myDateTime = DateTime.Now;
            string sqlFormattedDate = myDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            return sqlFormattedDate;
        }

    }
}

/* OLD CONNECTION METHODS

 *         public String readSingleRow(IDataRecord d)
        {
            String outS = "";
            for(int i = 0; i < d.FieldCount; i++)
                outS+= d[i].ToString();
            return outS;
        }
            var con = ConfigurationManager.ConnectionStrings["InventoryConnection"].ToString();


            using (SqlConnection myConnection = new SqlConnection(con))
            {
                string oString = "Select * from Item";
                SqlCommand oCmd = new SqlCommand(oString, myConnection);
                //oCmd.Parameters.AddWithValue("@Fname", fName);
                myConnection.Open();
                using (SqlDataReader oReader = oCmd.ExecuteReader())
                {
                    while (oReader.Read())
                    {
                        ViewBag.Message = oReader["UPC"].ToString();
                        //matchingPerson.firstName = oReader["FirstName"].ToString();
                        //matchingPerson.lastName = oReader["LastName"].ToString();
                    }
                    if (ViewBag.Message == null)
                    {
                        ViewBag.Message = "nothing there";
                    }
                    myConnection.Close();
                }
            }
*/