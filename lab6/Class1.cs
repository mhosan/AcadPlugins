
// (C) Copyright 2002-2005 by Autodesk, Inc. 
// 
// Permission to use, copy, modify, and distribute this software in 
// object code form for any purpose and without fee is hereby granted, 
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and 
// restricted rights notice below appear in all supporting 
// documentation. 
// 
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF 
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE 
// UNINTERRUPTED OR ERROR FREE. 
// 
// Use, duplication, or disclosure by the U.S. Government is subject to 
// restrictions set forth in FAR 52.227-19 (Commercial Computer 
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii) 
// (Rights in Technical Data and Computer Software), as applicable. 

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using Autodesk.AutoCAD.Windows;


namespace Lab6_Complete
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
        public Lab6_Complete.UserControl1 myPalette;

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
                myPalette = new Lab6_Complete.UserControl1();

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

                    // Declare an Entity variable named ent.  
                    Entity ent = (Entity)trans.GetObject(getEntityResult.ObjectId, OpenMode.ForRead);

                    // test the IsNull property of the ExtensionDictionary of the ent. 
                    if (ent.ExtensionDictionary.IsNull)
                    {
                        // Upgrade the open of the entity. 
                        ent.UpgradeOpen();

                        // Create the ExtensionDictionary
                        ent.CreateExtensionDictionary();
                    }

                    // variable as DBDictionary. 
                    DBDictionary extensionDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);

                    if (extensionDict.Contains("MyData"))
                    {
                        // Check to see if the entry we are going to add is already there. 
                        ObjectId entryId = extensionDict.GetAt("MyData");

                        // If this line gets hit then data is already added 
                       ed.WriteMessage("\nThis entity already has data...");

                        // Extract the Xrecord. Declare an Xrecord variable. 
                        Xrecord myXrecord = default(Xrecord);

                        // Instantiate the Xrecord variable 
                        myXrecord = (Xrecord)trans.GetObject(entryId, OpenMode.ForRead);

                        // Here print out the values in the Xrecord to the command line. 
                        foreach (TypedValue value in myXrecord.Data)
                        {
                                             
                            ed.WriteMessage("\n" + value.TypeCode.ToString() + " . " + value.Value.ToString());

                        }
                    }
                    else
                    {
                        // If the code gets to here then the data entry does not exist 
                        // upgrade the ExtensionDictionary created in step 5 to write
                        extensionDict.UpgradeOpen();


                        //  Create a new XRecord. 
                        Xrecord myXrecord = new Xrecord();

                        // Create the resbuf list. 
                        ResultBuffer data = new ResultBuffer(new TypedValue((int)DxfCode.Int16, 1),
                            new TypedValue((int)DxfCode.Text, "MyStockData"),
                            new TypedValue((int)DxfCode.Real, 51.9),
                            new TypedValue((int)DxfCode.Real, 100.0),
                            new TypedValue((int)DxfCode.Real, 320.6));

                        // Add the ResultBuffer to the Xrecord 
                        myXrecord.Data = data;

                        // Create the entry in the ExtensionDictionary. 
                        extensionDict.SetAt("MyData", myXrecord);

                        // Tell the transaction about the newly created Xrecord 
                        trans.AddNewlyCreatedDBObject(myXrecord, true);

                        // Here we will populate the treeview control with the new data 
                        if (myPalette != null)
                        {

                            foreach (System.Windows.Forms.TreeNode node in myPalette.treeView1.Nodes)
                            {

                                // Test to see if the node Tag is the ObjectId 
                                // of the ent 
                                if (node.Tag.ToString() == ent.ObjectId.ToString())
                                {

                                    // Now add the new data to the treenode. 
                                    System.Windows.Forms.TreeNode childNode = node.Nodes.Add("Extension Dictionary");

                                    // Add the data. 
                                    foreach (TypedValue value in myXrecord.Data)
                                    {
                                        // Add the value from the TypedValue 
                                        childNode.Nodes.Add(value.ToString());
                                    }

                                    // Exit the for loop (all done - break out of the loop) 
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

                //  Here we will add our data to the Named Objects Dictionary.(NOD) 
                DBDictionary nod = (DBDictionary)trans.GetObject(ed.Document.Database.NamedObjectsDictionaryId, OpenMode.ForRead);

                if (nod.Contains("MyData"))
                    {

                    // Check to see if our entry is in Named Objects Dictionary. 
                    ObjectId entryId = nod.GetAt("MyData");

                    // If we are here, then the Name Object Dictionary already has our data 
                    ed.WriteMessage("\n" + "This entity already has data...");

                    // Get the the Xrecord from the NOD. 
                    Xrecord myXrecord = null;

                    // Open the Xrecord for read 
                    myXrecord = (Xrecord)trans.GetObject(entryId, OpenMode.ForRead);

                    // Print out the values of the Xrecord to the command line. 
                    foreach (TypedValue value in myXrecord.Data)
                    {
                        // Use the WriteMessage method of the editor. 
                        ed.WriteMessage("\n" + value.TypeCode.ToString() + " . " + value.Value.ToString());

                    }
                }
                else
                {
                    // Our data is not in the Named Objects Dictionary so need to add it 
                    nod.UpgradeOpen();

                    // Declare a varable as a new Xrecord. 
                    Xrecord myXrecord = new Xrecord();

                    // Create the resbuf list. 
                    ResultBuffer data = new ResultBuffer(new TypedValue((int)DxfCode.Int16, 1),
                        new TypedValue((int)DxfCode.Text, "MyCompanyDefaultSettings"),
                        new TypedValue((int)DxfCode.Real, 51.9),
                        new TypedValue((int)DxfCode.Real, 100.0),
                        new TypedValue((int)DxfCode.Real, 320.6));

                    //  Add the ResultBuffer to the Xrecord 
                    myXrecord.Data = data;

                    // Create the entry in the ExtensionDictionary. 
                    nod.SetAt("MyData", myXrecord);


                    // Tell the transaction about the newly created Xrecord 
                    trans.AddNewlyCreatedDBObject(myXrecord, true);
                }

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
        
        //  Start of Lab6
        
        // 1. Use the CommandMethod attribute and create a command named "addpointmonitor"
        // name the function something like startMonitor
        // Note: put the closing curley brace after step 3
        [CommandMethod("addPointmonitor")]
        public void startMonitor() 
        {
            // 2. Declare an Editor variable named ed. Instantiate it using the Editor property
            // of the Application.DocumentManager.MdiActiveDocument
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            // 3. Connect to the PointMonitor event. Use the PointMonitor
            // event of the editor variable created in step 2. Use
            // += and use the new statement and create a new PointMonitorEventHandler. For 
            // the target parameter use the name of a function we will create in step 4.
            // (MyPointMonitor).
            ed.PointMonitor += new PointMonitorEventHandler(MyPointMonitor);
        }
        
       
        // 4. Create a Public function named MyPointMonitor. This is the functions that 
        // will be called everytime the mouse moves. (The name needs to be
        // the name used in the Delegate parameter of step 3). The first parameter is an 
        // object. (Use sender as the name of the Object). The second parameter is
        // a PointMonitorEventArgs. (Use e as the name of the PointMonitorEventArgs)
        // Note: Put the closing curley brace after step 26
        public void MyPointMonitor(object sender, PointMonitorEventArgs e)
        {
            if (e.Context == null)
            {
                return;
            }

            // 5. Declare an array of the Type FullSubentityPath type. For the name
            // of the array use something like fullEntPath. Instantiate it by 
            // making it equal to the GetPickedEntities method of the Context
            // property of the PointMonitorEventArgs passed into the Sub
            FullSubentityPath[] fullEntPath = e.Context.GetPickedEntities();
            
            // 6. Use an "if" statement and test the Length property of the array
            // created in step 5. Make sure it is greater than zero.
            // Note: Put the closing curley brace after step 26
            if (fullEntPath.Length > 0)
            {
                // 7. Declare a variable named trans as a Transaction. Instantiate it by makine it 
                // equal to the return of the StartTransaction method of the TransactionManager 
                // of the current database. 
                // Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction
                Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();
                
                // 8. Create a Try Catch block. 
                // Note: Put the catch and finally statements after step 25
                // (enclose step 26 in the finally block).
                // Put the closing curley brace for the try after step 26
                try
                {
                    // 9. Declare a variable as an Entity. Instantiate it using the 
                    // GetObject method of the transaction created in step 7. For
                    // the ObjectId parameter use the first element in GetObjectIds()[0]
                    // of the zero element in the array of FullSubentityPath created in
                    // step 5 open the Entity for read
                    Entity ent = (Entity)trans.GetObject((ObjectId)fullEntPath[0].GetObjectIds()[0], OpenMode.ForRead);

                    // 10. Add a tooltip by using the AppendToolTipText method of the 
                    // PointMonitorEventArgs passed into the function. 
                    // Use something like this for the string argument:
                    // "The Entity is a " + ent.GetType().ToString()
                    
                    e.AppendToolTipText("The Entity is a " + ent.GetType().ToString());

                    // 11. Use an "if" statement and Check that the palette (myPalette) has 
                    // been created. (== null) f it is null return.     
                    if (myPalette == null)
                    { 
                        return;
                    }

                    // 12. The following steps will make the text of the entry for a DBEntity
                    // in the palette created in Lab4 Bold. Declare a variable named fontRegular
                    // as a System.Drawing.Font. Instantiate it by making it equal to a New
                    // System.Drawing.Font. For the arguments use the following:
                    // "Microsoft Sans Serif", 8, System.Drawing.FontStyle.Regular
                    System.Drawing.Font fontRegular = new System.Drawing.Font("Microsoft Sans Serif", 8, System.Drawing.FontStyle.Regular);
                    
                    //  13. Declare a variable named fontBold as a System.Drawing.Font. 
                    //  Instantiate it by making it equal to a New
                    //  System.Drawing.Font. For the arguments use the following:
                    //  "Microsoft Sans Serif", 8, System.Drawing.FontStyle.Bold
                    System.Drawing.Font fontBold = new System.Drawing.Font("Microsoft Sans Serif", 8, System.Drawing.FontStyle.Bold);
                    
                    // 14. Use the SuspendLayout() method of the treeView1 created in Lab4 to 
                    // wait until after the steps below are processed. Use the "this" Keyword to
                    // get to the Palette (myPalette) with the treeView.
                    this.myPalette.treeView1.SuspendLayout();
                    
                    // 15. Here we will search for an object in the treeview control so the font 
                    // can be chaged to bold.
                    // Create a foreach statement. Use node for the element name and the type is 
                    // System.Windows.Forms.Treenode. The group paramater is the Nodes in the TreeView.
                    // (myPalette.treeView1.Nodes)
                    // Note: put the closing curley brace below step 21. 
                    // (In this foreach if the cursor is over an entity and the entity is 
                    // an entry in the TreeView it will be highlighted.
                    foreach (System.Windows.Forms.TreeNode node in myPalette.treeView1.Nodes)
                    {
                        //  16. use an "if else" and see if the Tag property of the node
                        //  is equal to the ObjectId of the entity declared in step 9. 
                        //  (Use ToString for the comparison)
                        //  Note: put the "else" after step 19
                        //  Put the closing curley brace after step 21. 
                        if (((string)node.Tag == ent.ObjectId.ToString()))
                        {
                            //  17. If we get here then the node is the one we want.
                            //  Use "!if" (not) and use the Equals method of the 
                            //  System.Drawing.Font variable created in step 13. For 
                            //  the Object parameter use the NodeFont property of the node.
                            //  Note: Put the closing curley brace after step 19
                            if (!fontBold.Equals(node.NodeFont))
                            {
                                //  18. Make the NodeFont property of the node equal to the
                                //  System.Drawing.Font variable created in step 13.
                                node.NodeFont = fontBold;
                                
                                //  19. Make the Text property of the node equal to the 
                                //  node.Text property.
                                node.Text = node.Text;
                            }
                        }
                        else
                        {
                            //  20. If we get here then the node is not the node we want.
                            //  Use "!if" (not) and use the Equals method of the 
                            //  System.Drawing.Font variable created in step 12. For 
                            //  the Object parameter use the NodeFont property of the node.
                            //  Note: Put the closing curley brace after step 20.
                            if (!fontRegular.Equals(node.NodeFont))
                            {
                                //  21. Make the NodeFont property of the node equal to the
                                //  System.Drawing.Font variable created in step 12
                                node.NodeFont = fontRegular;
                            }
                        }
                    }
                    
                    //  22. Now it's time to recalc the layout of the treeview. Use the 
                    //  ResumeLayout() method of the TreeView. Use the this Keyword to
                    //  get to the Palette (myPalette) with the treeView.
                    this.myPalette.treeView1.ResumeLayout();

                    //  23. Refresh the TreeView with the Refresh method() of the TreeView.
                    this.myPalette.treeView1.Refresh();
                    
                    //  24. Update the TreView with the Update method() of the TreeView.
                    this.myPalette.treeView1.Update();
                    
                    //  25. All is ok if we get here so Commit the transaction created in 
                    //  step 7.
                    trans.Commit();
                }
                catch 
                {
                    
                }
                finally
                {
                    //  26. Whatever happens we must dispose the transaction. (This is 
                    //  in the Finally block). Use the Dispose method of the 
                    //  transaction created in step 7.
                    //  Note: continue to step 27 in the NewInput function. You could also build and
                    //  test the addPointmonitor command before completing the following steps.
                    trans.Dispose();
                }
            }
        }


        [CommandMethod("newInput")]
        public void NewInput() 
        {
            // start our input point Monitor    
            // get the editor object
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            
            // now add the delegate to the events list
            ed.PointMonitor += new PointMonitorEventHandler(MyInputMonitor);
            
            // 27. Need to enable the AutoCAD input event mechanism to do a pick under the prevailing
            // pick aperture on all digitizer events, regardless of whether a point is being acquired 
            // or whether any OSNAP modes are currently active. Use the TurnForcedPickOn method
            // of the Editor created above. "ed"
            ed.TurnForcedPickOn();
            
            // 28. Here we are going to ask the user to pick a point. Declare a variable as a 
            // PromptPointOptions. Instantiate it by creating a new PromptPointOptions 
            // for the Message parameter using something like "Pick A Point : "
            PromptPointOptions getPointOptions = new PromptPointOptions("Pick A Point : ");
            
            // 29. Declare a variable as a PromptPointResult. Instantiate it using the GetPoint
            // method of the editor created above. "ed". Pass in the PromptPointOptions created
            // in step 28.
            PromptPointResult getPointResult = ed.GetPoint(getPointOptions);
            
            //  if ok
            //if (getPointResult.Status == PromptStatus.OK)
            //{
            //    //     ' do something...
            //}
            
                
            // 30. Now remove our point monitor as we are finished With it.
            // Use the PointMonitor property of the Editor created above. "ed". 
            // use -= and use new with a PointMonitorEventHandler for the Object
            // parameter use "MyInputMonitor"
            //  Continue to step 31 in the MyInputMonitor function.
            ed.PointMonitor -= new PointMonitorEventHandler(MyInputMonitor);

        }

        public void MyInputMonitor(object sender, PointMonitorEventArgs e)
        {
            if (e.Context == null)
            {
                return;
            }

            //  first lets check what is under the Cursor
            FullSubentityPath[] fullEntPath = e.Context.GetPickedEntities();
            if (fullEntPath.Length > 0)
            {
                //  start a transaction
                Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();
                try
                {
                    //  open the Entity for read, it must be derived from Curve
                    Curve ent = (Curve)trans.GetObject(fullEntPath[0].GetObjectIds()[0], OpenMode.ForRead);
                    
                    //  ok, so if we are over something - then check to see if it has an extension dictionary
                    if (ent.ExtensionDictionary.IsValid)
                    {
                        // open it for read
                        DBDictionary extensionDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);
                        
                        // find the entry
                        ObjectId entryId = extensionDict.GetAt("MyData");
                        
                        // if we are here, then all is ok
                        // extract the xrecord
                        Xrecord myXrecord;
                        
                        //  read it from the extension dictionary
                        myXrecord = (Xrecord)trans.GetObject(entryId, OpenMode.ForRead);
                        
                        // 31. ' We will draw temporary graphics at certain positions along the entity
                        // Create a "foreach" loop. For the element use a TypedValue
                        // named myTypeVal. For the group use the Data property of the Xrecord
                        // instantiated above. "myXrecord".
                        // Note" put the closing curley brace after step 44.
                        foreach (TypedValue myTypedVal in myXrecord.Data)
                        {
                            //  32. Use an "if" statement and see if the TypeCode of the TypedValue
                            //  is a real. (Use DxfCode.Real as the test).
                            //  Note: Put the closing curley brace after step 44.
                            if (myTypedVal.TypeCode == (short)DxfCode.Real)
                            {
                                // 33. To locate the temporary graphics along the Curve 
                                // to show the distances we need to get the point along the curve.
                                // Note: The value of the TypedValue will be 51.9, 100.0 and 320.6. 
                                // These values were added in Lab 5. Because of this the entity that you
                                // hover over needs to be at least 51.9 units long. (keep this in mind 
                                // when you test this). 
                                // Declare a vaiable as a Point3d object. Instantiate it using 
                                // the GetPointAtDist method of the ent instantatied above. "ent"
                                // For the Value parameter using the Value property of the TypedValue
                                Point3d pnt = ent.GetPointAtDist((double)myTypedVal.Value);
                                
                                // 34. We need to work out how many pixels are in a unit square
                                // so we can keep the temporary graphics a set size regardless of
                                // the zoom scale. Declare a variable as a Point2d name it something
                                // like "pixels". Instantiate it using the GetNumPixelsInUnitSquare method
                                // of the current Viewport. (Pass in the Point3d created in step 33).
                                // Use: e.Context.DrawContext.Viewport.GetNumPixelsInUnitSquare()
                                Point2d pixels = e.Context.DrawContext.Viewport.GetNumPixelsInUnitSquare(pnt);
                                
                                //  35. We need some constant distances. Declare a variable as a double
                                //  named something like "xDist". make it equal to 10 divided by the 
                                //  X property of the Point2d variable created in step 34. 
                                double xDist = (10 / pixels.X);
                                
                                // 36. Declare a variable as a double named something like "yDist". 
                                // make it equal to 10 divided by the Y property of the Point2d variable
                                // created in step 34. 
                                double yDist = (10 / pixels.Y);
                                
                                // 37. Draw the temporary Graphics. Declare a variable as a Circle
                                // instantiate is by creating a new Circle. Use the Point3d variable
                                // created in step 33 for the for the center. For the normal use 
                                // Vector3d.ZAxis. For the radius use the double from step 35. 
                                Circle circle = new Circle(pnt, Vector3d.ZAxis, xDist);
                                
                                //  38. Use the Draw method to display the circle. (Pass in the circle). 
                                //  Use:  e.Context.DrawContext.Geometry.Draw()
                                e.Context.DrawContext.Geometry.Draw(circle);
                                
                                // 39. Here we will add more temporary graphics. (text). Declare
                                // a variable as DBText. Instantiate it by creating new DBText.
                                DBText text = new DBText();
                                
                                // 40. Always a good idea to set the Database defaults With things like 
                                // text, dimensions etc. Use the SetDatabaseDefaults method of the DBText
                                // from step 39
                                text.SetDatabaseDefaults();
                                
                                // 41. Set the position of the text to the same point as the circle, 
                                // but offset by the radius. Use the Position property and make it
                                // equal to the Point3d created in step 33 plus a New Vector3d. For
                                // the X parameter use the Double from step 35. For the Y parameter use 
                                // the double from step 36. For Z just use zero.
                                text.Position = (pnt + new Vector3d(xDist, yDist, 0));
                                
                                // 42. Use the data from the Xrecord for the text. Use the TextString
                                // property of the DBText created in step 39. Make it equal to the 
                                // Value of the TypedValue. (use ToString)
                                text.TextString = myTypedVal.Value.ToString();
                                
                                //  43. Make the Height of the DBText equal to the double created in 
                                //  step 36
                                text.Height = yDist;
                                
                                //  44. Use the Draw method to display the text. (Pass in the DBText). 
                                //  Use:  e.Context.DrawContext.Geometry.Draw()
                                //  Note: The backgound color may impact the display of the temporary 
                                //  text. (it displays as white in this example, so may need to change
                                //  the background color to see it, or change the color of the DBText)
                                e.Context.DrawContext.Geometry.Draw(text);
                                
                                //  End of Lab6
                            }
                        }
                    }
                    //  all ok, commit it
                    trans.Commit();
                }
                catch 
                {
                }
                finally
                {
                    //  whatever happens we must dispose the transaction
                    trans.Dispose();
                }
            }
        }
    }
}