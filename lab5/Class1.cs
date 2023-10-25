

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using Autodesk.AutoCAD.Windows;


namespace Lab5_Complete
{

    public class adskClass
    {

        [CommandMethod("addAnEnt")]
        public void AddAnEnt()
        {

            // get the editor object so we can carry out some input 
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            // first decide what type of entity we want to create 
            PromptKeywordOptions getWhichEntityOptions = new PromptKeywordOptions("Which entity do you want to create? [Circle/Block] : ", "Circle Block");
            // now input it 
            PromptResult getWhichEntityResult = ed.GetKeywords(getWhichEntityOptions);
            // if ok 
            if ((getWhichEntityResult.Status == PromptStatus.OK))
            {

                // test which one is required 
                switch (getWhichEntityResult.StringResult)
                {

                    case "Circle":

                        // pick the center point of the circle 
                        PromptPointOptions getPointOptions = new PromptPointOptions("Pick Center Point : ");
                        PromptPointResult getPointResult = ed.GetPoint(getPointOptions);
                        // if ok 
                        if ((getPointResult.Status == PromptStatus.OK))
                        {

                            // the get the radius 
                            PromptDistanceOptions getRadiusOptions = new PromptDistanceOptions("Pick Radius : ");
                            // set the start point to the center (the point we just picked) 
                            getRadiusOptions.BasePoint = getPointResult.Value;
                            // now tell the input mechanism to actually use the basepoint! 
                            getRadiusOptions.UseBasePoint = true;
                            // now get the radius 
                            PromptDoubleResult getRadiusResult = ed.GetDistance(getRadiusOptions);
                            // if all is ok 
                            if ((getRadiusResult.Status == PromptStatus.OK))
                            {

                                // need to add the circle to the current space
                                //get the current working database
                                Database dwg = ed.Document.Database;

                                // now start a transaction
                                Transaction trans = dwg.TransactionManager.StartTransaction();
                                try
                                {
                                    //create a new circle
                                    Circle circle = new Circle(getPointResult.Value, Vector3d.ZAxis, getRadiusResult.Value);

                                    // open the current space (block table record) for write
                                    BlockTableRecord btr = (BlockTableRecord)trans.GetObject(dwg.CurrentSpaceId, OpenMode.ForWrite);

                                    // now the circle to the current space, model space more than likely
                                    btr.AppendEntity(circle);

                                    // tell the transaction about the new circle so that it can autoclose it
                                    trans.AddNewlyCreatedDBObject(circle, true);

                                    // now commit the transaction
                                    trans.Commit();
                                }
                                catch (Exception ex)
                                {
                                    // ok so we have an exception
                                    ed.WriteMessage("problem due to " + ex.Message);
                                }
                                finally
                                {
                                    // all done, whether an error on not - dispose the transaction.
                                    trans.Dispose();

                                }
                            }
                        }
                        break;
                    case "Block":

                        // enter the name of the block 
                        PromptStringOptions blockNameOptions = new PromptStringOptions("Enter name of the Block to create : ");
                        // no spaces are allowed in a blockname so disable it 
                        blockNameOptions.AllowSpaces = false;
                        // get the name 
                        PromptResult blockNameResult = ed.GetString(blockNameOptions);
                        // if ok 
                        if ((blockNameResult.Status == PromptStatus.OK))
                        {

                            // lets create the block definition
                            // get the current drawing
                            Database dwg = ed.Document.Database;

                            // now start a transaction
                            Transaction trans = (Transaction)dwg.TransactionManager.StartTransaction();
                            try
                            {

                                // create the new block definition
                                BlockTableRecord newBlockDef = new BlockTableRecord();

                                // name the block definition
                                newBlockDef.Name = blockNameResult.StringResult;

                                // now add the new block defintion to the block table
                                // open the blok table for read so we can check to see if the name already exists
                                BlockTable blockTable = (BlockTable)trans.GetObject(dwg.BlockTableId, OpenMode.ForRead);

                                // check to see if the block already exists
                                if ((blockTable.Has(blockNameResult.StringResult) == false))
                                {

                                    // if it's not there, then we are ok to add it
                                    // but first we need to upgrade the open to write 
                                    blockTable.UpgradeOpen();

                                    // Add the BlockTableRecord to the blockTable
                                    blockTable.Add(newBlockDef);

                                    // tell the transaction manager about the new object so that the transaction will autoclose it
                                    trans.AddNewlyCreatedDBObject(newBlockDef, true);

                                    // now add some objects to the block definition
                                    Circle circle1 = new Circle(new Point3d(0, 0, 0), Vector3d.ZAxis, 10);
                                    newBlockDef.AppendEntity(circle1);

                                    Circle circle2 = new Circle(new Point3d(20, 10, 0), Vector3d.ZAxis, 10);
                                    newBlockDef.AppendEntity(circle2);

                                    // tell the transaction manager about the new objects 
                                    //so that the transaction will autoclose it
                                    trans.AddNewlyCreatedDBObject(circle1, true);
                                    trans.AddNewlyCreatedDBObject(circle2, true);


                                    // now set where it should appear in the current space
                                    PromptPointOptions blockRefPointOptions = new PromptPointOptions("Pick insertion point of BlockRef : ");
                                    PromptPointResult blockRefPointResult = ed.GetPoint(blockRefPointOptions);

                                    // check to see if everything was ok - if not
                                    if ((blockRefPointResult.Status != PromptStatus.OK))
                                    {
                                        //dispose of everything that we have done so far and return
                                        trans.Dispose();
                                        return;
                                    }

                                    // now we have the block defintion in place and the position we need to create the reference to it
                                    BlockReference blockRef = new BlockReference(blockRefPointResult.Value, newBlockDef.ObjectId);

                                    // otherwise add it to the current space, first open the current space for write
                                    BlockTableRecord curSpace = (BlockTableRecord)trans.GetObject(dwg.CurrentSpaceId, OpenMode.ForWrite);

                                    // now add the block reference to it
                                    curSpace.AppendEntity(blockRef);

                                    // remember to tell the transaction about the new block reference so that the transaction can autoclose it
                                    trans.AddNewlyCreatedDBObject(blockRef, true);

                                    // all ok, commit it
                                    trans.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                // a problem occured, lets print it
                                ed.WriteMessage("a problem occured because " + ex.Message);
                            }
                            finally
                            {
                                // whatever happens we must dispose the transaction
                                trans.Dispose();

                            }
                        }
                        break;
                }

            }
        }
       
       // declare a paletteset object, this will only be created once
        public PaletteSet myPaletteSet;

        // we need a palette which will be housed by the paletteSet
        public Lab5_Complete.UserControl1 myPalette;
        
        // palette command
        [CommandMethod("palette")]
        public void palette()
        {

           // check to see if it is valid
            if (myPaletteSet == null)
            {
                // create a new palette set, with a unique guid
                myPaletteSet = new PaletteSet("My Palette", new System.Guid("D61D0875-A507-4b73-8B5F-9266BEACD596"));

                // now create a palette inside, this has our tree control
                myPalette = new Lab5_Complete.UserControl1();

                // now add the palette to the paletteset
                myPaletteSet.Add("Palette1", myPalette);
            }

            // now display the paletteset
            myPaletteSet.Visible = true;
        }

        [CommandMethod("addDBEvents")]
        public void addDBEvents()
        {

            // the palette needs to be created for this
            if (myPalette == null)
            {
                //  get the editor object
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

               // now write to the command line
                ed.WriteMessage("\n" + "Please call the 'palette' command first");
                return;
            }

            // get the current working database
            Database curDwg = Application.DocumentManager.MdiActiveDocument.Database;

            // add a handlers for what we need
            curDwg.ObjectAppended += new ObjectEventHandler(callback_ObjectAppended);
            curDwg.ObjectErased += new ObjectErasedEventHandler(callback_ObjectErased);
            curDwg.ObjectReappended += new ObjectEventHandler(callback_ObjectReappended);
            curDwg.ObjectUnappended += new ObjectEventHandler(callback_ObjectUnappended);
        }


        private void callback_ObjectAppended(object sender, ObjectEventArgs e)
        {

            // add the class name of the object to the tree view
            System.Windows.Forms.TreeNode newNode = myPalette.treeView1.Nodes.Add(e.DBObject.GetType().ToString());

            // we need to record its id for recognition later
            newNode.Tag = e.DBObject.ObjectId.ToString();
        }

         private void callback_ObjectErased(object sender, ObjectErasedEventArgs e)
        {

            // if the object was erased
            if (e.Erased)
            {

              // find the object in the treeview control so we can remove it
                foreach (System.Windows.Forms.TreeNode node in myPalette.treeView1.Nodes)
                {
                    // is this the one we want
                    if (node.Tag.ToString() == e.DBObject.ObjectId.ToString())
                    {
                        node.Remove();
                        break;
                    }

                }
            }
            else
            {
                // if the object was unerased
                // add the class name of the object to the tree view
                System.Windows.Forms.TreeNode newNode = myPalette.treeView1.Nodes.Add(e.DBObject.GetType().ToString());

                // we need to record its id for recognition later
                newNode.Tag = e.DBObject.ObjectId.ToString();
            }
        }

        private void callback_ObjectReappended(object sender, ObjectEventArgs e)
        {

            // add the class name of the object to the tree view
            System.Windows.Forms.TreeNode newNode = myPalette.treeView1.Nodes.Add(e.DBObject.GetType().ToString());

            // we need to record its id for recognition later
            newNode.Tag = e.DBObject.ObjectId.ToString();
        }

        private void callback_ObjectUnappended(object sender, ObjectEventArgs e)
        {

            // find the object in the treeview control so we can remove it 
            foreach (System.Windows.Forms.TreeNode node in myPalette.treeView1.Nodes)
            {
                // is this the one we want
                if (node.Tag.ToString() == e.DBObject.ObjectId.ToString())
                {
                   node.Remove();
                   break;
                }

            }
        }
        
        //lab 5
        [CommandMethod("addData")]
        public void addData()
        {

            // get the editor object 
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            // pick entity to add data to! 
            PromptEntityResult getEntityResult = ed.GetEntity("Pick an entity to add an Extension Dictionary to : ");
            // if all was ok 
            if ((getEntityResult.Status == PromptStatus.OK))
            {
                // now start a transaction 
                Transaction trans = ed.Document.Database.TransactionManager.StartTransaction();
                try
                {

                    // Start of Lab5 
                    // Here we will add XData to a selected entity. 

                    // 1. Declare an Entity variable named ent. Instantiate it using 
                    // GetOBject of the Transaction created above. (trans) For the 
                    // ObjectId parameter use the ObjectId property of the the 
                    // PromptEntityResult created above.(getEntityResult) Open the entity for read. 
                    Entity ent = (Entity)trans.GetObject(getEntityResult.ObjectId, OpenMode.ForRead);

                    // 2. Use an "if" statement and test the IsNull property of the 
                    // ExtensionDictionary of the ent. If it is Null then we create 
                    // the ExtensionDictionary. 
                    // Note: Place the closing curly brace after step 4. 
                    if (ent.ExtensionDictionary.IsNull)
                    {
                        // 3. Upgrade the open of the entity. Because it does 
                        // not have an extenstion dictionary and we want to add it 
                        // the ent needs to be open for write. 
                        ent.UpgradeOpen();

                        // 4. Create the ExtensionDictionary by calling 
                        // CreateExtensionDictionary of the entity. 
                        ent.CreateExtensionDictionary();
                    }

                    // 5. Declare a variable as DBDictionary. Instantiate it by using the 
                    // GetObject method of the Transaction created above. (trans). For the 
                    // ObjectId parameter use the ExtensionDictionary property of the ent 
                    // variable created in step 1. Open it for read 
                    DBDictionary extensionDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);

                    // 6. Check to see if the entry we are going to add to the dictionary is 
                    // already there. Use the Contains property of the dictionary in an "if else
                    // statement.
                    // Note: Place the closing curly brace and the "else" after step 12. Place the
                    // closing curly brace of the "else" after after step 25 
                    if (extensionDict.Contains("MyData"))
                    {
                        // 7. Declare an ObjectId variable named entryId and instantiate it
                        // using the GetAt method of the ExtenstionDictionary from step 5. Use
                        // "Mydata" for the entryName
                        ObjectId entryId = extensionDict.GetAt("MyData");

                        // 8. If this line gets hit then data is already added 
                        // Use the WriteMessage method of the Editor (ed) created above 
                        // to print the data. For the string argument use something like this: 
                        // "\nThis entity already has data..." 
                        ed.WriteMessage("\nThis entity already has data...");

                        // 9. Now extract the Xrecord. Declare an Xrecord variable. 
                        Xrecord myXrecord = default(Xrecord);

                        // 10. Instantiate the Xrecord variable using the 
                        // GetObject method of the Transaction created above. (trans). 
                        // For the ObjectId argument use the ObjectId created in step 7
                        // open for read.
                        myXrecord = (Xrecord)trans.GetObject(entryId, OpenMode.ForRead);

                        // 11. Here print out the values in the Xrecord to the command line. 
                        // Use a "foreach" statement. For the Element type use a TypedValue. 
                        // (Use value for the name of the TypedValue) For the Group use the 
                        // Data property of the Xrecord 
                        // Note: Put the closing curly brace below step 12 
                        foreach (TypedValue value in myXrecord.Data)
                        {

                            // 12. Use the WriteMessage method of the Editor created above. (ed). 
                            // for the string argument use something like this: 
                            // "\n" + value.TypeCode.ToString() + " . " + value.Value.ToString() 
                            ed.WriteMessage("\n" + value.TypeCode.ToString() + " . " + value.Value.ToString());

                        }
                    }
                    else
                    {
                        // 13. If the code gets to here then the data entry does not exist 
                        // upgrade the ExtensionDictionary created in step 5 to write by calling 
                        // the UpgradeOpen() method 
                        extensionDict.UpgradeOpen();

                        System.Type t = typeof(DBDictionary);


                        // 14. Create a new XRecord. Declare an Xrecord variable as a New Xrecord 
                        Xrecord myXrecord = new Xrecord();

                        // 15. Create the resbuf list. Declare a ResultBuffer variable. Instantiate it 
                        // by creating a New ResultBuffer. For the ParamArray of TypeValue for the new 
                        // ResultBuffer use the following: 
                        //new TypedValue((int)DxfCode.Int16, 1),
                        //new TypedValue((int)DxfCode.Text, "MyStockData"),
                        //new TypedValue((int)DxfCode.Real, 51.9),
                        //new TypedValue((int)DxfCode.Real, 100.0),
                        //new TypedValue((int)DxfCode.Real, 320.6)
                        ResultBuffer data = new ResultBuffer(new TypedValue((int)DxfCode.Int16, 1),
                            new TypedValue((int)DxfCode.Text, "MyStockData"),
                            new TypedValue((int)DxfCode.Real, 51.9),
                            new TypedValue((int)DxfCode.Real, 100.0),
                            new TypedValue((int)DxfCode.Real, 320.6));

                        // 16. Add the ResultBuffer to the Xrecord using the Data 
                        // property of the Xrecord. (make it equal the ResultBuffer 
                        // from step 15) 
                        myXrecord.Data = data;

                        // 17. Create the entry in the ExtensionDictionary. Use the SetAt 
                        // method of the ExtensionDictionary from step 5. For the SearchKey 
                        // argument use "MyData". For the DBObject argument use the Xrecord 
                        // created in step 14. 
                        extensionDict.SetAt("MyData", myXrecord);

                        // 18. Tell the transaction about the newly created Xrecord 
                        // using the AddNewlyCreatedDBObject of the Transaction (trans) 
                        trans.AddNewlyCreatedDBObject(myXrecord, true);

                        // 19. Here we will populate the treeview control with the new data 
                        // Use an "if" statement to check to see if the 
                        // palette (myPalette created in Lab 4) is not equal to null. 
                        // (If not then it will crash) 
                        // Note: Put the closing curly brace after step 25 
                        if (myPalette != null)
                        {

                            // 20. Create a foreach statement. Use node for the element name and 
                            // the type is System.Windows.Forms.Treenode. The group paramater is the Nodes in the 
                            // TreeView. (myPalette.treeView1.Nodes) 
                            // Note: put the closing curly brace after step 25 
                            foreach (System.Windows.Forms.TreeNode node in myPalette.treeView1.Nodes)
                            {

                                // 21. Use an "if" statement. Test to see if the node Tag is the ObjectId 
                                // of the ent from step 1. Use the ObjectId. (ent.ObjectId.ToString) 
                                // Note: put the closing curly brace after step 25 
                                if (node.Tag.ToString() == ent.ObjectId.ToString())
                                {

                                    // 22. Now add the new data to the treenode. Declare a variable as a 
                                    // System.Windows.Forms.Treenode. (name it something like childNode). 
                                    // Instantiate it by making it equal to the return of calling the Add
                                    // method of the Nodes collection of the node from the loop. 
                                    // (node.Nodes.Add) For the string argument use "Extension Dictionary" 
                                    System.Windows.Forms.TreeNode childNode = node.Nodes.Add("Extension Dictionary");

                                    // 23. Now add the data. Create a foreach statement. Use value for the element 
                                    // name and the type is TypedValue. Use the Data property of the Xrecord created in 
                                    // step 14 for the group. 
                                    // Note: put the closing curly brace after step 24 
                                    foreach (TypedValue value in myXrecord.Data)
                                    {
                                        // 24. Add the TypeValue from the For Each loop to the 
                                        // TreeNode created in step 22. Use the Add method of the 
                                        // Nodes Collection. (childNode.Nodes.Add) For the string 
                                        // argument use the TypeValue from the loop. (value.ToString) 
                                        childNode.Nodes.Add(value.ToString());
                                    }

                                    // 25. Exit the for loop (all done - break out of the loop) 
                                    break;
                                }
                            }
                        }
                    }

                    // all ok, commit it 

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    // a problem occured, lets print it 
                    ed.WriteMessage("a problem occured because " + ex.Message);
                }
                finally
                {
                    // whatever happens we must dispose the transaction 

                    trans.Dispose();

                }

            }
        }
        [CommandMethod("addDataToNOD")]
        public void addDataToNOD()
        {

            // get the editor object 
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            // pick entity to add data to! 
            Transaction trans = ed.Document.Database.TransactionManager.StartTransaction();
            try
            {

                // 26. Here we will add our data to the Named Objects Dictionary.(NOD) 
                // Declare a variable as a DBDictionary. (name it nod). Instantiate it 
                // by making it equal to the return of the GetObject method of the 
                // Transaction created above. (trans). For the ObjectId argument use the 
                // NamedObjectsDictionaryId property of the current Database: 
                // (ed.Document.Database.NamedObjectsDictionaryId) The Editor (ed) was 
                // instantiated above. Open it for read. 
                DBDictionary nod = (DBDictionary)trans.GetObject(ed.Document.Database.NamedObjectsDictionaryId, OpenMode.ForRead);

                // 27. Check to see if the entry we are going to add to the NOD is 
                // already there. Use the Contains property of the dictionary in an "if Else"
                // statement.
                // Note: put the closing curly brace and the else after step 33. Put the closing
                // curley brace for the else after step 39 
                if (nod.Contains("MyData"))
                {

                    // 28. Declare an ObjectId variable named entryId. Instantiate it by making 
                    // it equal to the return of the GetAt method of the NOD (DBDictionary) 
                    // from step 26. For the EntryName agrument use "MyData" 
                    ObjectId entryId = nod.GetAt("MyData");

                    // 29. If we are here, then the Name Object Dictionary already has our data 
                    // Use the WriteMessage method of the editor. Use this for the Message argument 
                    // "\n" + "This entity already has data..." 
                    ed.WriteMessage("\n" + "This entity already has data...");

                    // 30. Get the the Xrecord from the NOD. Declare a variable as a new Xrecord 
                    Xrecord myXrecord = null;

                    // 31. USe the Transaction (trans) and use the GetObject method to 
                    // get the the Xrecord from the NOD. For the ObjectId argument use the 
                    // ObjectId from step 28. Open the Xrecord for read 
                    myXrecord = (Xrecord)trans.GetObject(entryId, OpenMode.ForRead);

                    // 32. Print out the values of the Xrecord to the command line. Use 
                    // a "foreach" statement. Use value for the element 
                    // name and the type is TypedValue. Use the Data property of the Xrecord from 
                    // step 31 for the group argument 
                    // Note: put the closing curly brace after step 33 
                    foreach (TypedValue value in myXrecord.Data)
                    {
                        // 33. Use the WriteMessage method of the editor. Use this as the message: 
                        // "\n" + value.TypeCode.ToString() + " . " + value.Value.ToString() 
                        ed.WriteMessage("\n" + value.TypeCode.ToString() + " . " + value.Value.ToString());

                    }
                }
                else
                {
                    // 34. Our data is not in the Named Objects Dictionary so need to add it 
                    // upgrade the status of the NOD variable from step 26 to write status 
                    nod.UpgradeOpen();

                    // 35. Declare a varable as a new Xrecord. 
                    Xrecord myXrecord = new Xrecord();

                    // 36. Create the resbuf list. Declare a ResultBuffer variable. Instantiate it 
                    // by creating a New ResultBuffer. For the ParamArray of TypeValue for the new 
                    // ResultBuffer use the following: 
                    // new TypedValue((int)DxfCode.Int16, 1),
                    //new TypedValue((int)DxfCode.Text, "MyCompanyDefaultSettings"),
                    //new TypedValue((int)DxfCode.Real, 51.9),
                    //new TypedValue((int)DxfCode.Real, 100.0),
                    //new TypedValue((int)DxfCode.Real, 320.6)
                    ResultBuffer data = new ResultBuffer(new TypedValue((int)DxfCode.Int16, 1),
                        new TypedValue((int)DxfCode.Text, "MyCompanyDefaultSettings"),
                        new TypedValue((int)DxfCode.Real, 51.9),
                        new TypedValue((int)DxfCode.Real, 100.0),
                        new TypedValue((int)DxfCode.Real, 320.6));

                    // 37. Add the ResultBuffer to the Xrecord using the Data 
                    // property of the Xrecord. (make it equal the ResultBuffer 
                    // from step 36) 
                    myXrecord.Data = data;

                    // 38. Create the entry in the ExtensionDictionary. Use the SetAt 
                    // method of the Named Objects Dictionary from step 26. For the SearchKey 
                    // argument use "MyData". For the DBObject argument use the Xrecord 
                    // created in step 35. 
                    nod.SetAt("MyData", myXrecord);


                    // 39. Tell the transaction about the newly created Xrecord 
                    // using the AddNewlyCreatedDBObject of the Transaction (trans) 
                    trans.AddNewlyCreatedDBObject(myXrecord, true);
                }

                // End of LAB 5

                // all ok, commit it 
                trans.Commit();
            }
            catch (Exception ex)
            {
                // a problem occurred, lets print it 
                ed.WriteMessage("a problem occurred because " + ex.Message);
            }
            finally
            {
                // whatever happens we must dispose the transaction 

                trans.Dispose();

            }
        } 
    }
}