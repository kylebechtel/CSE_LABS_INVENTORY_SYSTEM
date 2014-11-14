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
            // Post Requests
            string all = Request.Form["All"];               // Determines if all of the item should be removed.
            string itemsUsed = Request.Form["quantity"];    // How much to remove
            string UPC = Request.Form["UPC"];

            if (all != null)    // If anything was put here, it will do this.
            {
                String res = query("Delete from [Inventory].[dbo].[Disposable] where UPC = '" + UPC + "';"); // Will remove all of an Item from the DB
                if (String.IsNullOrEmpty(res))
                    ViewBag.message = "True:removeDisposable:RemovedAllDisposable";      // Says if it worked
                else
                    ViewBag.message = res;                              // Assigns 
                return View();                                          // Returns the View and ends the method
            }

            // If the user did not specify the remove all flag, then that means only some need to be removed.
            // Check to make sure that this item does exist in the database.
            String quantityCheck = query("SELECT quantity from [Inventory].[dbo].[Disposable] where upc = '"+UPC+"';");
            if (String.IsNullOrEmpty(quantityCheck))    // If the itme is not in the database
            {
                ViewBag.message = "False:removeDisposable:NoSuchItem";
                return View();
            }
            int decrement = 0, currentQuantity = 0;
            try                                         // Tries to parse the numbers to update quantiy
            {
                currentQuantity = Int32.Parse(quantityCheck);       // Number of items that are in the database currently
                decrement = Int32.Parse(itemsUsed);                 // Number of items that you are removing
            }
            catch(Exception e)
            {
                ViewBag.message = "False:removeDisposable:InvalidQuantity:PraseError";   // Error indicating parsing failed.
                return View();
            }

            if (decrement < currentQuantity)            // If you are removing less than the total items (IE Items will remain in DB)
            {
                string ret = query("Update [Inventory].[dbo].[Disposable] set quantity = " + (currentQuantity - decrement) + 
                    " WHERE UPC = '" + UPC + "';");     // Update the qunatity to reflect the difference
                if (!String.IsNullOrEmpty(ret))
                    ViewBag.messaage = "False:removeDisposable:UpdatePath";
                else
                    ViewBag.message = "True:removeDisposable:Removed" + decrement + "Items";
            }
            else                                        // If the Quantity is greater than number in database, remove item from DB
            {
                string ret = query("Delete from [Inventory].[dbo].[Disposable] where UPC = '" + UPC + "';");    // Deletes Item
                if (String.IsNullOrEmpty(ret))
                    ViewBag.message = "False:removeDisposable:DeletePath";
                else
                    ViewBag.message = "True:removeDisposable:RemovedRemainingDisposable";
            }

            return View();
        }

        public ActionResult viewDisposable()
        {
            string UPC = Request.Form["UPC"];           // Post Request

            // Makes sure that the Item you wish to view is in the database
            string existance = query("SELECT UPC,quantity,description FROM [Inventory].[dbo].[Disposable] WHERE UPC = '" + UPC + "';");
  
            if (String.IsNullOrEmpty(existance))        // Handles if the Item was not found
            {
                    ViewBag.message = "False:viewDisposable:itemNotFound";
                    return View();
            }

            String[] parts = existance.Split('|');

            // Returned Data in JSON format.
            ViewBag.message = "UPC:" + parts[0].Trim() + ",Description:" + parts[2].Trim() + ",Quantity:"+parts[1].Trim();

            return View();
        }

        public ActionResult addDisposable()
        {

            // Post Requests
            string UPC = Request.Form["UPC"];
            string ammount = Request.Form["quantity"];
            string description = Request.Form["description"]; 

            if(String.IsNullOrEmpty(UPC))               // Makes sure a UPC was specified
            {
                ViewBag.message = "False:addDisposable:noUPCGiven";
                return View();
            }

            // Checks if UPC is already in DB
            String existanceCheck = query("Select Quantity from [inventory].[dbo].[Disposable] where UPC = " + UPC+";");

            // If so, it updates the DB by adding that many itmes, instead of adding a new instance of it
            // By updating quantity, it keeps the database more consistent
            if (String.IsNullOrEmpty(existanceCheck))
            {
                string res = query("Insert into [inventory].[dbo].[Disposable] (UPC,quantity, description) values(" + UPC + "," + ammount + ",'" + description + "');");
                ViewBag.message = "True:addDisposable:AddedNewDisposable";
            }
            else
            {
                int quantity = 0;
                try
                {
                    string []parts = existanceCheck.Split('|'); // Removes the | from the end of the Query Return
                    existanceCheck = parts[0];                  // Grabs just the qunatity from the return

                    quantity = Int32.Parse(existanceCheck) + Int32.Parse(ammount);      // Finds the Updated Quantity
                    if(Int32.Parse(ammount) < 0)                                        // If that Quantity is negative
                    {
                        ViewBag.message = "False:addDisposable:ammountNegative";                      // Show Error Message
                        return View();
                    }
                }
                catch(Exception e)                                                  // Exception HIT while parsing/splitting
                {
                    quantity = 0;                                                   // Zero out quantity
                    ViewBag.message = "False:addDisposable:failedParseQuantity:"+e; // Message to reflect parse failed
                    return View();
                }
                if (quantity < 0)                                               // Does same negative check as for ammount
                {
                    ViewBag.message = "False:addDisposable:InvalidQuantity";          // Might not be needed in addition to Checks above
                    return View();
                }
                // Updates the Qunatity in the DB
                string retUp = query("Update [inventory].[dbo].[Disposable] set Quantity = " + quantity + " where UPC = "+UPC+";");
                if (retUp.Contains("False")) // If the return was false, then it will throw an error
                    ViewBag.message = "False:addDisposable:UpdateFailed";
                else
                    ViewBag.message = "True:addDisposable:UpdatedQuantity";
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
            // Post Request
            string serial = Request.Form["serial_number"];
            string decal = Request.Form["decal"];

            if(String.IsNullOrEmpty(serial) && String.IsNullOrEmpty(decal))
            {
                ViewBag.message = "False:removeItem:noSerialNorDecalGiven";
                return View();
            }

            string existanceCheck = "";
            if (!String.IsNullOrEmpty(serial))      // If the Serial Number is Valid
            {   
                // Check to see if the item is in the database via the serial number
                existanceCheck = query("SELECT * FROM [Inventory].[dbo].[Item] WHERE serial_number = '" + serial + "';");
                if (!String.IsNullOrEmpty(existanceCheck))              // If the query that checked the DB returned something...
                {   
                    // Delete the item (by serial number) from the database
                    string ret = query("DELETE FROM [Inventory].[dbo].[Item]  WHERE serial_number = '" + serial + "';");
                    if (String.IsNullOrEmpty(ret))
                        ViewBag.message = "True:removeItem:serialPath";                // Return if the Query did not have an error returned
                    else
                        ViewBag.messsage = "False:removeItem:FailedRemove:serialPath";  // If the Delete Query returned an error.
                }
                else                                                    // If the query that checked the DB did not return anything
                    ViewBag.message = "False:removeItem:NoItemInDatabase:serialPath";   // Print this error message
                return View();
            }
            else if (!String.IsNullOrEmpty(decal))      // Same as above, but using Decal in the WHERE clause
            {
                existanceCheck = query("SELECT * FROM [Inventory].[dbo].[Item] WHERE decal = '" + decal + "';");
                if (!String.IsNullOrEmpty(existanceCheck))
                {
                    string ret = query("DELETE FROM [Inventory].[dbo].[Item]  WHERE decal = '" + decal + "';");
                    if (String.IsNullOrEmpty(ret))
                        ViewBag.message = "True:removeItem:decalPath";
                    else
                        ViewBag.messsage = "False:removeItem:FailedRemove:decalPath";
                }
                else
                    ViewBag.message = "False:removeItem:NoItemInDatabase:decalPath";
                return View();
            }

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

            ViewBag.param = ""+UPC +" | "+serial+" | "+model+" | "+manufacture+
                " | "+bId+" | "+descript+" | "+PON+" | "+checkedO+" | "+checkedI+" | "+PID+" | "+vend+" | "+decal+" | ";

            string existance = query("SELECT serial_number FROM [Inventory].[dbo].[Item] WHERE serial_number = " + serial + ";");
            ViewBag.exist = existance;
            if (existance.Equals("") == false)
            {
                ViewBag.message = "False:addItem:AlreadyExists"; 
                return View();
            }
            else if (!UPC.Equals("") && !serial.Equals("") && !descript.Equals("") && !model.Equals("") && !manufacture.Equals(""))
            {
                
                String ret = query("INSERT INTO [Inventory].[dbo].[Item]" +
                    "(UPC,serial_number,description,building_ID,received_date,model_number,manufacturer,surplus_date,PO_number,checked_out" +
                    " ,checked_in,person_ID) values" +
                    "(" + UPC + ",'" + serial + "','" + descript + "','J','" + getDate() + "','" + model + "','" + manufacture + "',NULL,NULL,NULL,NULL,NULL);");

                if (ret.Contains("False") || ret.Equals(""))
                    ViewBag.message = "False:addItemFull";
                else
                    ViewBag.message = "True:addItemFull";
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
                    ViewBag.message = "False:addItem";
                else
                    ViewBag.message = "True:addItem";
            }
            else
                ViewBag.message = "False:UnableToAdd";

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
                    ViewBag.message = "False:addPOtoItem";
                else
                    ViewBag.message = "True:addPOtoItem";
            }
            return View();
        }

        public ActionResult viewItem()
        {
            // Post Requests
            string serial = Request.Form["serial_number"];
            string decal = Request.Form["decal"];

            // Makes sure that at decal XOR serial number are vaild.  (one or the other, not  both)
            if(String.IsNullOrEmpty(decal) && String.IsNullOrEmpty(serial))
            {
                ViewBag.message = "False:viewItem:noSerialOrDecalGiven";
                return View();
            }
            else if(!String.IsNullOrEmpty(decal) && !String.IsNullOrEmpty(serial))
            {
                ViewBag.message = "False:viewItem:bothSerialAndDecalGiven";
                return View();
            }

            // Hit the database with a select using first the Serial and then the Decal.
            // One or the other method should hopefully return something to the method that can then be used
            string existance = query("SELECT serial_number,description,person_ID,decal FROM [Inventory].[dbo].[Item] WHERE serial_number = '" + serial + "';");
            string dExist = query("SELECT serial_number,description,person_ID,decal FROM [Inventory].[dbo].[Item] WHERE decal = '" + decal + "';");
           
            if (existance.Equals(""))
            {
                if (dExist.Equals(""))
                {
                    ViewBag.message = "False:viewItem:ItemDoesntExist";
                    return View();
                }
                else
                    existance = dExist;
            }

            // Will construct a JSON Return string
            String[] parts = existance.Split('|');
            ViewBag.message = "serial_number:" + parts[0];      // Grabs the Serial Number
            ViewBag.message += ",description:" + parts[1];      // Grabs the Description(name of) the item
            ViewBag.message += ",decal_ID:" + parts[3];          // Grabs the Decal Number
            if (!String.IsNullOrEmpty(parts[2].Trim()))         // Checks if the Item was checked out, 
            {
                ViewBag.message += ",person_ID:" + parts[2];    // Adds the PID to the JSON return
                string peopleFind = query("SELECT name FROM [Inventory].[dbo].[Person] WHERE person_ID = '" + parts[2] + "';");
                parts = peopleFind.Split('|');                  // Above query gets the name associated with the person id
                ViewBag.message += ",person_name:" + parts[0];   // this adds the name to the JSON return
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
            // Post Request
            string pid = Request.Form["person_ID"];

            // Checks the Database to pull all items that have the given Person Id
            string existance = query("SELECT serial_number,description,decal FROM [Inventory].[dbo].[Item] WHERE person_ID = '" + pid + "';");

            if (String.IsNullOrEmpty(existance))            // If the Query returned nothing, thus no items were checked out to that person
            {
                ViewBag.message = "False:viewPerson:personIdDoesNotExist"; // Or the PID Doesnt exist
                return View();
            }
            String[] parts = existance.Split('|');          // Splits the results based on the | Delimeter
            string data = "";
            for (int i = 0; i < parts.Length - 2; i += 3)       // Runs through all sets of data returned
            {
                if (!String.IsNullOrEmpty(data))
                    data += ",";
                data += "serial_number:" + parts[i];
                data += ",description:" + parts[i + 1];
                data += ",decal:" + parts[i + 2];
            }

            ViewBag.message = data;
            // Message is in JSON in the form of serial_number:value,description:value,decal:value, and so on
            return View();
        }

        public ActionResult checkIn()
        {
            // Post Requests
            string serial = Request.Form["serial_number"];
            string decal = Request.Form["decal"];
            string PID = "";

            if (!String.IsNullOrEmpty(decal) && !String.IsNullOrEmpty(serial))      // If both serial and decal are given
            {  
                ViewBag.message = "False:checkIn:BothDecalAndSerialGiven";      // Error, do not take time to figure out if the decal
                return View();                                          // and serial are for the samee item.
            }
            else if (String.IsNullOrEmpty(decal) && String.IsNullOrEmpty(serial))   // If both data items are null/empty
            {
                ViewBag.message = "False:checkIn:NoDecalorSerialGiven";         // Give errors to the user telling them they 
                return View();                                          // need one or the other
            }


            if (!String.IsNullOrEmpty(decal))       // If the Decal Is not null or Empty
            {
                // Query the database to see an item with the given decal exists in the DB
                PID = query("select person_ID from [Inventory].[dbo].[Item] where decal = " + decal + ";");
                if (String.IsNullOrEmpty(PID))  // if the Person ID for the given decal was not found in the database, 
                {
                    ViewBag.message = "False:checkIn:NotFoundInDB";     // Return an error stating false not found in DB 
                    return View();
                }
                // Null out the person ID that was attached to the given item.  It also time stamps the checked in date.
                string updates = query("Update [inventory].[dbo].[Item] set person_ID =  NULL,checked_in = '" 
                    + getDate() + "' where decal = " + decal + ";");
                if ((updates).Contains("False"))                        // If the query returned false (an error)
                    ViewBag.message = "False:checkIn:ItemNotCheckedOut";    
                else
                    ViewBag.message = "True:checkIn:ItemCheckedIn:" + getDate();// Return a message that the check in was successful
            }
            else        // Does the exact same as above, but for serial numbers
            {
                PID = query("select person_ID from [Inventory].[dbo].[Item] where serial_number = " + serial + ";");
                if (String.IsNullOrEmpty(PID))
                {
                    ViewBag.message = "False:checkIn:NotFoundInDB";
                    return View();
                }
                string updates = query("Update [inventory].[dbo].[Item] set person_ID =  NULL,checked_in = '" + getDate() + "' where serial_number = " + serial + ";");

                if ((updates).Contains("False"))
                    ViewBag.message = "False:checkIn:ItemNotCheckedOut";
                else
                    ViewBag.message = "True:checkIn:ItemCheckedIn:" + getDate();
            }
            return View();
        }
        public ActionResult checkOut()
        {
            // Post Requests
            string serial = Request.Form["serial_number"];
            string decal = Request.Form["decal"];
            string personId = Request.Form["person_ID"];    
            string personName = Request.Form["person_name"];// It is name in the database, but that seems like it could cuase problems elsewhere
            string buildingId = Request.Form["building_ID"];// Building/room where the item is going to now be located

            if(!String.IsNullOrEmpty(serial) && !String.IsNullOrEmpty(decal))     // Make sure that serial XOR decal is defined.  Not both
            {
                ViewBag.message = "False:checkOut:serialAndDecalDefined";
                return View();
            }


            
            string existance ="", dExist = "";
            if(!String.IsNullOrEmpty(serial))   // If the Serial Number is valid, select serial, person_id, decal, and building ID from the item table
                existance = query("SELECT serial_number,person_ID,decal,building_ID FROM [Inventory].[dbo].[Item] WHERE serial_number = " + serial + ";");
            else                                // Do the same, but Decal is now the where value
                dExist = query("SELECT serial_number,person_ID,decal,building_ID FROM [Inventory].[dbo].[Item] WHERE decal = '" + decal + "';");

            // This block checks if the Item was not found with either the Decal or Serial number
            if (existance.Equals("") && dExist.Equals(""))  // If it was not found, the following error is returned
            {
                ViewBag.message = "False:checkOut:NotFoundInDB";
                return View();
            }


            // It can also retrieve an ID based on names.
            if(personId.Equals(""))
            {  
                string pinExist = query("SELECT person_ID,name FROM [Inventory].[dbo].[Person] WHERE name = '" + personName + "';");
                if (pinExist.Equals(""))
                {

                    ViewBag.message = "False:checkOut:NoPersonSpeciified";
                    return View();
                }
                else
                {
                    // Gets the correct PID from the database based on the name you entered
                    string[] parts = pinExist.Split('|');
                    personId = parts[0];
                }
            }

            /**********************************************************************
             * At this point the person ID should have a value.  Whether it be 
             * from searching a name in the previous block, or if it is from user input.
             * If it is from the above block, it should be valid and thus
             * Skip the next block.   
             **********************************************************************/
            string pidExist = query("SELECT person_ID,name FROM [Inventory].[dbo].[Person] WHERE person_ID = '" + personId + "';");

            // If the PID did not exist in the database for People(person)
            // Then essentailly repeat the code block above.
            if(pidExist.Equals(""))
            {
                // If the ID was not in the database, check to see if the name was as well   
                string pinExist = query("SELECT person_ID,name FROM [Inventory].[dbo].[Person] WHERE name = '" + personName + "';");

                if (pinExist.Equals("")) // If the name was not in the database either, 
                {
                    // Insert into databse that person
                    string insertResult = query("INSERT INTO [Inventory].[dbo].[Person] (person_ID,name) VALUES (" + personId + ", '" + personName + "');");
                }
                else // I feel this is redundant code but I cant prove it
                {
                    // Gets the correct PID from the database based on the name you entered
                    string[] parts = pidExist.Split('|');
                    personId = parts[0];
                }

            }

            // If the person name is still not valid (was not retrieved durring an ID lookup
            // Nor was it user defined
            if(personName.Equals(""))
            {
                string[] parts = pidExist.Split('|');       // Splits up the PID return
                personName = parts[1];                      // Uses the name section of it to assign Person Name
            }

            // this block checks if the Building ID was not specified in the post request.
            if(buildingId.Equals(""))
            {
                if (!existance.Equals(""))                  // If serial exists in DB
                {
                    string[] parts = existance.Split('|');  // Split apart the original Item existance check
                    buildingId = parts[3];                  // Set the new Building ID to be the old Building ID
                }
                else if (!dExist.Equals(""))                // If decal existed in DB
                {
                    string[] parts = dExist.Split('|');
                    buildingId = parts[3];                  // Set the new Building ID to be the old Building ID
                }
            }


            // A valid Serial Number or Decal is present.
            // A Valid PID is also present at this point.
            string result = "False:checkOut:UnknowError";


            /*********************************************************************
             * This block is designed to check out an Item to the specified person
             * It will use Decal or Serial depending to sset the Person Id, 
             * Building Id, and date checked out
             *********************************************************************/
            if (!existance.Equals(""))
                result = query("UPDATE [Inventory].[dbo].[Item] SET person_ID = '"+personId+"',building_ID = '"+buildingId+"',[checked_out] = '"+getDate()+"' WHERE serial_number = '" + serial + "';");
            else if (!dExist.Equals(""))
                result = query("UPDATE [Inventory].[dbo].[Item] SET person_ID = '"+personId+"',building_ID = '"+buildingId+"',[checked_out] = '"+getDate()+"' WHERE decal = '"+decal+"';");

            if (!result.Contains("False"))  // If the Result does not have False in it (IE Query did not return a false)
                ViewBag.message = "True:checkOut:ItemCheckedOutSuccess:" + getDate();
            else
                ViewBag.messsaage = "False:checkOut:CheckOutFailed";
            return View();
        }

        /*************************************************************
         * Other Methods (Default Page Methods)
         * - Index
         * - Contact - Deleted
         * - About - Deleted
         *************************************************************/
        public ActionResult Index()
        {
            return View();
        }



        /****************************************************
         * Refference Methods
         * - Query
         * - GetDate
         ****************************************************/

        /************************************************************************************
         * String Query
         * =============
         * This is the Most Core feature of the Controllers above.  
         * This is the method that takes a line of SQL And will executue it on the 
         * database, and return the result.
         * ----------------------------------------------------------------------------------
         * @Param: Query, the string containing a line of SQL to run on the Database
         * @return: If Passed, The result from the database will be returned.  
         *              If nothing is returned from the Database, One of two things happened:
         *                  1.) That Command should not return a value:
         *                     It will return True:COMMANDCorrectly
         *                     Where COMMAND is a place holder for the command type:
         *                         such as Updated, Inserted, Deleted
         *                  2.) The commmand should have returned a value, but did not:
         *                      This is caused by a failed select Statement.
         *                      In this case, the empty sstring is returned.
         *              If Something is returned from the Database, 
         *                  The data that is returned, comes seperated via a | (pipe symbol)
         *          If Failed, a message saying Falsse:FailedQuery
         ************************************************************************************/
        public String query(String query)
        {
            //Concept from http://stackoverflow.com/questions/14171794/retrieve-data-from-a-sql-server-database-in-c-sharp
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
                        // Goes through all of the data returned from the Query
                        while (reader.Read())
                        {
                            Object[] v = new Object[reader.FieldCount];
                            reader.GetValues(v);    // Dumps the data in reader into Array V
                            for (int i = 0; i < v.Length; i++)  // Goes through and appends 
                                ret += v[i] + " | ";            // each element of V with a | to a return string
                            ret += "\n";                        // Add an new line for good measure
                        }
                      
                        myConnection.Close();
                        return ret;
                    }
                }
            }
            catch (Exception e)
            {
                return "False:queryfunction";          // Error to print/return if something went wrong in the Query.
            }
        }


        /************************************************************************************
         * string getDate()
         * ================
         * This is a simple method that gets the date in a special format that works with SQL
         * It uses a DateTime Object to grab the system time of the machine.  It then
         * Encodes it inot the proper format for SQL and gives it back
         * ----------------------------------------------------------------------------------
         * @Return  The Date String in the format of: yyyy-mm-dd hh:mm:ss 
         ************************************************************************************/
        public string getDate()
        {
            DateTime myDateTime = DateTime.Now;
            string sqlFormattedDate = myDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            return sqlFormattedDate;
        }
        // this is the old and depricated Version of Query that is not very helpful
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
                return "" + e.ToString();
            }
            catch (Exception e)
            {
                return "False:failedQuery";
            }
            return data;
        }

    }
}