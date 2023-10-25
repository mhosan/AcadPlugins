
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


namespace Lab7_Complete
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
        public Lab7_Complete.UserControl1 myPalette;

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
                myPalette = new Lab7_Complete.UserControl1();

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

       

        [CommandMethod("addPointmonitor")]
        public void startMonitor()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            // Connect to the PointMonitor event. 
            ed.PointMonitor += new PointMonitorEventHandler(MyPointMonitor);
        }


        // Create a Public function named MyPointMonitor. 
        public void MyPointMonitor(object sender, PointMonitorEventArgs e)
        {
            if (e.Context == null)
            {
                return;
            }

            // array of the Type FullSubentityPath type. 
            FullSubentityPath[] fullEntPath = e.Context.GetPickedEntities();

            
            if (fullEntPath.Length > 0)
            {
                Transaction trans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();
                
                try
                {
                    
                    Entity ent = (Entity)trans.GetObject((ObjectId)fullEntPath[0].GetObjectIds()[0], OpenMode.ForRead);

                    // Add a tooltip by using the AppendToolTipText method 
                    e.AppendToolTipText("The Entity is a " + ent.GetType().ToString());

                    // Make sure the palette has been created     
                    if (myPalette == null)
                    {
                        return;
                    }

                    // The following steps will make the text of the entry for a DBEntity
                    // in the palette created in Lab4 Bold. 
                    System.Drawing.Font fontRegular = new System.Drawing.Font("Microsoft Sans Serif", 8, System.Drawing.FontStyle.Regular);

                    System.Drawing.Font fontBold = new System.Drawing.Font("Microsoft Sans Serif", 8, System.Drawing.FontStyle.Bold);

                    // Use the SuspendLayout() method of the treeView1 created in Lab4 to 
                    // wait until after the steps below are processed. 
                    this.myPalette.treeView1.SuspendLayout();

                    // Here we will search for an object in the treeview control so the font 
                    // can be chaged to bold.
                    foreach (System.Windows.Forms.TreeNode node in myPalette.treeView1.Nodes)
                    {
                        if (((string)node.Tag == ent.ObjectId.ToString()))
                        {
                            //  If we get here then the node is the one we want.
                            if (!fontBold.Equals(node.NodeFont))
                            {
                                //  Make the NodeFont property of the node equal to the
                                //  System.Drawing.Font variable created above
                                node.NodeFont = fontBold;

                                node.Text = node.Text;
                            }
                        }
                        else
                        {
                            //  If we get here then the node is not the node we want.
                            if (!fontRegular.Equals(node.NodeFont))
                            {
                                node.NodeFont = fontRegular;
                            }
                        }
                    }

                    //  Now it's time to recalc the layout of the treeview. 
                    this.myPalette.treeView1.ResumeLayout();

                    this.myPalette.treeView1.Refresh();

                    this.myPalette.treeView1.Update();

                    // All is ok if we get here so Commit the transaction created in 
                    trans.Commit();
                }
                catch 
                {

                }
                finally
                {
                    //  Whatever happens we must dispose the transaction. 
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

            // Need to enable the AutoCAD input event mechanism to do a pick under the prevailing
            // pick aperture on all digitizer events, regardless of whether a point is being acquired 
            // or whether any OSNAP modes are currently active.
            ed.TurnForcedPickOn();

            // Here we are going to ask the user to pick a point. 
            PromptPointOptions getPointOptions = new PromptPointOptions("Pick A Point : ");
            
            PromptPointResult getPointResult = ed.GetPoint(getPointOptions);

            //  if ok
            //if (getPointResult.Status == PromptStatus.OK)
            //{
            //    //     ' do something...
            //}


            // Now remove our point monitor as we are finished With it.
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

                        // We will draw temporary graphics at certain positions along the entity
                        foreach (TypedValue myTypedVal in myXrecord.Data)
                        {
                            if (myTypedVal.TypeCode == (short)DxfCode.Real)
                            {
                                //  To locate the temporary graphics along the Curve 
                                // to show the distances we need to get the point along the curve.
                                Point3d pnt = ent.GetPointAtDist((double)myTypedVal.Value);

                                //  We need to work out how many pixels are in a unit square
                                // so we can keep the temporary graphics a set size regardless of
                                // the zoom scale. 
                                Point2d pixels = e.Context.DrawContext.Viewport.GetNumPixelsInUnitSquare(pnt);

                                //  We need some constant distances. 
                                double xDist = (10 / pixels.X);
                                                               
                                double yDist = (10 / pixels.Y);

                                // Draw the temporary Graphics. 
                                Circle circle = new Circle(pnt, Vector3d.ZAxis, xDist);

                                e.Context.DrawContext.Geometry.Draw(circle);
                                                                
                                DBText text = new DBText();

                                // Always a good idea to set the Database defaults With things like 
                                // text, dimensions etc. 
                                text.SetDatabaseDefaults();

                                // Set the position of the text to the same point as the circle, 
                                // but offset by the radius. 
                                text.Position = (pnt + new Vector3d(xDist, yDist, 0));

                                // Use the data from the Xrecord for the text. 
                                text.TextString = myTypedVal.Value.ToString();

                                text.Height = yDist;

                                //  Use the Draw method to display the text. 
                                e.Context.DrawContext.Geometry.Draw(text);

                               
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

        // create a command to invoke the EntityJig 
        [CommandMethod("circleJig")]
        public void CircleJig()
        {

            // 39. Create a new instance of a circle we want to form with the jig 
            // Declare a variable as a Circle named circle. Instantiate it by making 
            // it equal to a new Circle. For the center use Point3d.Origin. For the 
            // normal use Vector3d.ZAxis. Make the Radius 10. 
            Circle circle = new Circle(Point3d.Origin, Vector3d.ZAxis, 10);

            // 40. Create a new jig. Declare a variable as a a new MyCircleJig. 
            // (the name of the class created in steps 1-38). Pass in the 
            // Circle created in step 39 
            MyCircleJig jig = new MyCircleJig(circle);

            // 41. Now loop for the inputs. 0 will be to position the circle and 1 will 
            // be to set the radius. Use a for loop with two iterations. 
            // for (int i = 0; i <= 1; i++)
            // Note: Put the closing curley brace after step 46. 
            for (int i = 0; i <= 1; i++)
            {

                // 42. Set the current input to the loop counter. Use the CurrentInput 
                // property of the class variable created in step 40. (make it equal to 
                // the counter variable in the loop. (will be either 0 or 1) 
                jig.CurrentInput = i;

                // 43. Get the editor object. Declare a variable as Editor and instantiate it 
                // with the Editor property of the MdiActiveDocument. 
                Autodesk.AutoCAD.EditorInput.Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                // 44. Invoke the jig. Declare a PromptResult variable and instantite it by 
                // making it equal to the return of the Drag method of the Editor created 
                // in step 43. Pass in the MyCircleJig class created in step 40. 
                PromptResult promptResult = ed.Drag(jig);

                // 45. Make sure the Status property of the PromptResult variable created in 
                // in step 44 is ok. Use an "if" statement. For the test see if the 
                // promptResult.Status is equal to PromptStatus.Cancel Or PromptStatus.Error. 
                // Note: Put the closing curley brace after step 46 
                if (promptResult.Status == PromptStatus.Cancel | promptResult.Status == PromptStatus.Error)
                {
                    // 46. some problem occured. Return 
                    // End of Lab 7. 
                    // Note: If you named your Circle variable something other than 
                    // "circle" then change the code below which adds the circle to
                    // the database to reflect this. 
                    return;

                }
            }

            // once we are finished with the jig, time to add the newly formed circle to the database 
            // get the working database 
            Database dwg = Application.DocumentManager.MdiActiveDocument.Database;
            // now start a transaction 
            Transaction trans = dwg.TransactionManager.StartTransaction();
            try
            {

                // open the current space for write 
                BlockTableRecord currentSpace = (BlockTableRecord)trans.GetObject(dwg.CurrentSpaceId, OpenMode.ForWrite);
                // add it to the current space 
                currentSpace.AppendEntity(circle);
                // tell the transaction manager about it 
                trans.AddNewlyCreatedDBObject(circle, true);

                // all ok, commit it 

                trans.Commit();
            }
            catch 
            {
            }
            finally
            {
                // whatever happens we must dispose the transaction 

                trans.Dispose();

            }
        } 

    }

      
    // Start of Lab7 
    // Note: This project will not compile until the lab steps are completed.
    // There is code in the CircleJig function that needs objects created  
    // by the lab. 

    // 1. Create a Class named MyCircleJig that Inherits from EntityJig. 
    // The EntityJig class allows you to "jig" one entity at a time. 
    // When you inherit from EntityJig you need override two functions.
    // These are Sampler which is used to get input from the user and Update 
    // which is used to update the entity that is being jiged.
    // In this lab we are using a Jig to create a circle. 
    // Note: Put the closing curley brace after step 38. 
    class MyCircleJig : EntityJig
    {

        // 2. We need two inputs for a circle, the center and the radius. 
        // Declare a Private member varaiable of the class as a Point3d named 
        // "centerPoint" and a private member variable as a double named "radius". 
        private Point3d centerPoint;
        private double radius;

        // 3. Because we are going to have 2 inputs, a center point and a radius we need 
        // to keep track of the input number. (used to determine which value we are getting). 
        // Declare a private member variable as a int named "currentInputValue". 
        private int currentInputValue;

        // 4. We will use a Property to get and set the variable created in step 3. 
        // (This value is accessed outside of the class). Declare a int property named 
        // CurrentInput{}. Use get and set to return the member variable "currentInputValue" 
        // Use a set statement to make the member variable "currentInputValue" equal to 
        // the value that will set in when this class property is accessed. 
        public int CurrentInput
        {
            get { return currentInputValue; }
            set { currentInputValue = value; }
        }

         
        // 5. Create the default constructor. Pass in an Entity variable named ent. 
        // Derive from the base class and also pass in the ent passed into the constructor. 
        public MyCircleJig(Entity ent)
            : base(ent)
        {

        }

        // 6. Override the Sampler function. Use the protected keyword and it returns
        // Autodesk.AutoCAD.EditorInput.SamplerStatus. Use Autodesk.AutoCAD.EditorInput.JigPrompts
        // for the single argument. Name the argument "prompts" 
        // Note: put the closing curley brace below step 29.
        protected override Autodesk.AutoCAD.EditorInput.SamplerStatus Sampler(Autodesk.AutoCAD.EditorInput.JigPrompts prompts)
        {

            // 7. (This step is in the Sampler function) Create a switch statement. 
            // For the case use the currentInputValue member variable, 
            // Note: Move the closing curley brace after step 28.
            switch (currentInputValue)
            {

                // 8.Use 0 (zero) for the case. (getting center for the circle) 
                case 0:

                    // 9. Declare a variable named oldPnt as Point3d and instantiate it 
                    // using the centerPoint member varible. This will be used to test to 
                    // see if the cursor has moved during the jig. If the user does 
                    // not change anything Autocad continually calls the sampler function, 
                    // and the update function, you will get a flickering effect on the screen
                    // if the test is not done. 
                    Point3d oldPnt = centerPoint;

                    // 10. Declare a variable as a PromptPointResult. Name it something like 
                    // jigPromptResult. Instantiate it by making it equal to the return value of 
                    // the AcquirePoint method of the JigPrompts oject passed into the function. 
                    // Use something like "Pick center point : " for the message argument. 
                    PromptPointResult jigPromptResult = prompts.AcquirePoint("Pick center point : ");

                    // 11. Check the status of the PromptPointResult created in step 10. Use the 
                    // Status property and check if it is equal to PromptStatus.OK in an "if" 
                    // statement. 
                    // Note: Put the closing curley brace after step 14.
                    if (jigPromptResult.Status == PromptStatus.OK)
                    {

                        // 12. Make the centerPoint member variable equal to the Value 
                        // property of the PromptPointResult created in step 10 
                        centerPoint = jigPromptResult.Value;

                        // 13. Check to see if the cursor has moved. Use an "if" Statement. 
                        // In the test use the DistanceTo property of the Point3d variable created 
                        // in step 9. For the Point3d argument use the centerPoint variable. Use 
                        // less than "<" and see if is smaller than 0.0001 
                        // Note: put the closing curley brace after step 14 
                        if ((oldPnt.DistanceTo(centerPoint) < 0.001))
                        {
                            // 14. If we get here then there has not been any change to the location 
                            // return SamplerStatus.NoChange 
                            return SamplerStatus.NoChange;
                        }
                    }


                    // 15. If the code gets here than there has been a change in the location so 
                    // return SamplerStatus.OK 
                    return SamplerStatus.OK;
                
                // 16. Use 1 for the case. (getting radius for the circle) 
                case 1:

                    // 17. Declare a variable named oldRadius as double and instantiate it 
                    // using the radius member varible. This will be used to test to see if 
                    // the cursor has moved during jigging for the radius. 
                    double oldRadius = radius;


                    // 18. Declare a variable as a JigPromptDistanceOptions. Name it something like 
                    // jigPromptDistanceOpts. Instantiate it by making a new JigPromptDistanceOptions. 
                    // For the Message argument use something like "Pick radius : " 
                    JigPromptDistanceOptions jigPromptDistanceOpts = new JigPromptDistanceOptions("Pick radius : ");

                    // 19. Make the UseBasePoint property of the JigPromptDistanceOptions created 
                    // in step 18 True 
                    jigPromptDistanceOpts.UseBasePoint = true;

                    // 20. Make the BasePoint property of the JigPromptDistanceOptions created 
                    // in step 18 equal to the centerPoint member variable 
                    jigPromptDistanceOpts.BasePoint = centerPoint;

                    // 21. Now we ready to get input. Declare a vaiable as PromptDoubleResult. 
                    // Name it something like "jigPromptDblResult". Instantiate it using the 
                    // AcquireDistance method of the JigPrompts passed into the Sampler function. 
                    // Pass in the JigPromptDistanceOptions created in step 18. 
                    PromptDoubleResult jigPromptDblResult = prompts.AcquireDistance(jigPromptDistanceOpts);


                    // 22. Check the status of the PromptDoubleResult created in step 21. Use the 
                    // Status property and check if it is equal to PromptStatus.OK in an "if" 
                    // statement. 
                    // Note: Put the closing curley brace after step 27 
                    if ((jigPromptDblResult.Status == PromptStatus.OK))
                    {
                        // 23. Make the radius member varialble equal to the Value 
                        // property of the PromptDoubleResult created in step 21 
                        radius = jigPromptDblResult.Value;


                        // 24. Check to see if the radius is too small Use an "if" Statement. 
                        // In the test use the System.Math.Abs() For the Double argument use the 
                        // radius member variable. Use less than "<" and see if it is smaller than 0.1 
                        // Note: put the closing curley brace after step 25 
                        if (System.Math.Abs(radius) < 0.1)
                        {
                            // 25. Make the Member variable radius = to 1. This is 
                            // just an arbitrary value to keep the circle from being too small 
                            radius = 1;
                        }

                        // 26. Check to see if the cursor has moved. Use an "if" Statement 
                        // in the test use the System.Math.Abs() method. For the double argument 
                        // subtract the radius member variable from the oldRadius. Use 
                        // less than "<" and see if is smaller than 0.001 
                        // Note: put the closing curley brace after step 27 
                        if ((System.Math.Abs(oldRadius - radius) < 0.001))
                        {
                            // 27. If we get here then there has not been any change to the location 
                            // Return SamplerStatus.NoChange 
                            return SamplerStatus.NoChange;
                        }
                    }

                    // 28. If we get here the cursor has moved. return SamplerStatus.OK 
                    return SamplerStatus.OK;
            }
            // 29. Return SamplerSataus.NoChange. This will not ever be hit as we are returning
            // in the switch statement. (just avoiding the compile error)
            return SamplerStatus.NoChange;
        }

        // 30. Override the Update function. Use the protected keyword. The function
        // returns a boolean and does not have any arguments 
        // Note: put the closing curley brace below step 38
        protected override bool Update()
        {

            // 31. In this function (Update) for every input, we need to update the entity 
            // Create a switch statement. For the case use the currentInputValue member variable, 
            // Note: Move the closing curley brace after step 37. 

            switch (currentInputValue)
            {
                // 32. Use 0 (zero) for the case. (Updating center for the circle) 
                case 0:

                    // 33. The jig stores the circle as an Entity type. Cast it to a circle 
                    // so we can access the properties easily. Use this.Entity and for the 
                    // cast use the Circle class. Make the Center property equal to the 
                    // centerPoint member variable 
                    ((Circle)this.Entity).Center = centerPoint;

                    // 34. break out of the switch statement
                    break;
                
                // 35. Use 1 for the case. (Updating radius for the circle) 
                case 1:

                    // 36. The jig stores the circle as an Entity type. Cast it to a circle 
                    // so we can access the properties easily. Use this.Entity and for the 
                    // cast use the Circle class. Make the Radius property equal to the radius
                    // member variable. 
                    ((Circle)this.Entity).Radius = radius;

                    // 37. break out of the switch statement
                    break;

            }
            // 38. Return true. 
            // Note: continue to step 39 in circleJig function above 
            return true;
        }
    } 

}