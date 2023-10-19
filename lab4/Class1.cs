
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


namespace Lab4_Complete
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

        //
        // LAB CUATRO:
        //
        // 1. Add a Reference to PresentationCore. (Use the .NET tab on 
        // the Add Reference dialog. This is needed for the PaletteSet 
        // we will declare in step 3. 

        // 2. Use the using Statement for namespace Autodesk.AutoCAD.Windows 

        // 3. Declare a PaletteSet variable (global) as a PaletteSet. (It will 
        // only be created once). Add this declaration after AddAnEnt End Sub 
        // from Lab 3. 
        public PaletteSet myPaletteSet;

        // 4. Declare a variable as UserControl1. This is the control created 
        // in the steps in the Lab4 document. This control will be housed 
        // by the PaletteSet created in step 3. Need to use the NameSpace
        // of the UserControl1 in the declaration. (name of the project)
        public Lab4_Complete.UserControl1 myPalette;

        // 5. Add an new command named palette. Use the CommandMethod 
        // attribute and create the function that will run when the command
        // is run in AutoCAD. 
        // Note: Put the closing curly brace after step 10. 
        [CommandMethod("palette")]
        public void palette()
        {

            // 6. Add an "if" statement and check to see if the 
            // PaletteSet declared in step 3 is equal to null. It will be 
            // null the first time the command is run. 
            // Note: Put the closing curly brace after step 9 
            if (myPaletteSet == null)
            {

                // 7. The PaletteSet is nothing here so we create a a new PaletteSet 
                // with a unique GUID. Use the new keyword. Make the Name Parameter 
                // "My Palette". For the ToolID parameter generate a new GUID. 
                // On the Tools menu select "Create Guid". On the Create GUID 
                // Dialog select "Registry Format" Select New GUID and the copy. 
                // Paste the GUID to use as the new System.Guid. Replace the curley 
                // braces with double quotes. (The parameter for New Guid is a string) 
                myPaletteSet = new PaletteSet("My Palette", new System.Guid("D61D0875-A507-4b73-8B5F-9266BEACD596"));

                // 8. Instantiate the UserControl1 variable created in 
                // step 4. Use the new keyword. (New UserControl1 - need to 
                // use the namespace too) 
                // This control houses the tree control. 
                myPalette = new Lab4_Complete.UserControl1();

                // 9. Add the UserControl to the PaletteSet. Use the Add method 
                // of the PaletteSet instantiated in step 7. Use "Palette1" for the 
                // name parameter and the control instantiated in step 8 for the 
                // second parameter. 
                myPaletteSet.Add("Palette1", myPalette);
            }

            // 10. Display the paletteset by making the Visible property of the 
            // PaletteSet equal to true. The second time the command is run 
            // this is the only code in this procedure that will be processed. 
            myPaletteSet.Visible = true;
        }

        // 11. Add a command named "addDBEvents. Use the CommandMethod attribute 
        // and add the function that will run when the commmand is run in AutoCAD 
        // Note: Put the closing curly brace after step 20 
        [CommandMethod("addDBEvents")]
        public void addDBEvents()
        {

            // 12. use an if statement and see if the palette 
            // created in step 4 Is null. 
            // Note: put the closing curley brace after step 15 
            if (myPalette == null)
            {
                // 13. Declare and intantiate a Editor object. Use the Editor 
                // property of Application.DocumentManager.MdiActiveDocument 
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

                // 14. Use the WriteMessage method of the Editor variable 
                // created in step 13. Use this for the message parameter 
                // "\n" + "Please call the 'palette' command first" 
                ed.WriteMessage("\n" + "Please call the 'palette' command first");

                // 15. return
                return;
            }

            // 16. Declare a Database variable and instantiate it by making it 
            // equal to the Database property of the 
            // Application.DocumentManager.MdiActiveDocument 
            Database curDwg = Application.DocumentManager.MdiActiveDocument.Database;

            // 17. Connect to the ObjectAppended event. Use the ObjectAppended
            // event of the database variable created in step 16. Use
            // += and use the new statement and create a new ObjectEventHandler. For 
            // the target parameter use the name of a function we will create in step 21.
            // (callback_ObjectAppended).
            curDwg.ObjectAppended += new ObjectEventHandler(callback_ObjectAppended);

            // 18. Connect to the ObjectErased event. Use the ObjectErased
            // event of the database variable created in step 16. Use
            // += and use the new statement and create a new ObjectErasedEventHandler. For 
            // the target parameter use the name of a function we will create in step 24.
            // (callback_ObjectErased).
            curDwg.ObjectErased += new ObjectErasedEventHandler(callback_ObjectErased);

            // 19. Connect to the ObjectReappended event. Use the ObjectReappended
            // event of the database variable created in step 16. Use
            // += and use the new statement and create a new ObjectEventHandler. For 
            // the target parameter use the name of a function we will create in step 32.
            // (callback_ObjectReappended).
            curDwg.ObjectReappended += new ObjectEventHandler(callback_ObjectReappended);

            // 20. Connect to the ObjectUnappended event. Use the ObjectUnappended
            // event of the database variable created in step 16. Use
            // += and use the new statement and create a new ObjectEventHandler. For 
            // the target parameter use the name of a function we will create in step 35.
            // (callback_ObjectUnappended).
            curDwg.ObjectUnappended += new ObjectEventHandler(callback_ObjectUnappended);
        }

        // 21. Create a private function named callback_ObjectAppended. (returns void)
        // This is the function that will be called when an Object is Appended to 
        // the Database. (The name needs to be the name used in the Delegate parameter
        // of step 17). The first parameter is an object. (use sender as the name of 
        // the Object). The second parameter is an ObjectEventArgs. 
        // (Use e as the name of the ObjectEventArgs) 
        // Note: Put the closing curly brace after step 23 
        private void callback_ObjectAppended(object sender, ObjectEventArgs e)
        {

            // 22. Declare a TreeNode variable. (System.Windows.Forms.TreeNode). 
            // Note: You can save some typing by adding a using statement and add the namespace. 
            // Instantiate it using the Add method of the Nodes property of the TreeView on the 
            // UserControl() created in step 4. Use the ObjectEventArgs passed into the function for 
            // the string parameter and use the "Type" of DBObject. (e.DBObject.GetType().ToString()) 
            System.Windows.Forms.TreeNode newNode = myPalette.treeView1.Nodes.Add(e.DBObject.GetType().ToString());

            // 23. Make the Tag property of the node created in step 22 equal to the ObjectId of 
            // the appended object. This will allow us to record it's ObjectId for recognition in 
            // other events. Use e.DBObject.ObjectId.ToString() 
            newNode.Tag = e.DBObject.ObjectId.ToString();
        }

        // 24. Create a private function named callback_ObjectErased. (returns void)
        // This is the function that will be called when an Object is erased from the 
        // Database. (The name needs to be the name used in the Delegate parameter of 
        // step 18). The first parameter is an object. (use sender as the name of the 
        // Object). The second parameter is an ObjectErasedEventArgs. 
        // Use e as the name of the ObjectErasedEventArgs) 
        // Note: Put the closing curly brace before step 32 
        private void callback_ObjectErased(object sender, ObjectErasedEventArgs e)
        {

            // 25. use an "if else"  statement and check the Erased property of the 
            // ObjectErasedEventArgs passed into the function. (e.Erased) 
            // Note: Put the closing curly brace and "else" stament before step 30. 
            // put the closing curly brace for the "else after step 31 
            if (e.Erased)
            {

                // 26. Here we will search for an object in the treeview control so it can be removed. 
                // Create a foreach statement. Use node for the element name and the type is 
                // System.Windows.Forms.Treenode. The group paramater is the Nodes in the TreeView. 
                // (myPalette.treeView1.Nodes) 
                // Note: put the closing curly brace below step 29. 
                foreach (System.Windows.Forms.TreeNode node in myPalette.treeView1.Nodes)
                {
                    // 27. Use an "if" statement. Test to see if the node Tag is the ObjectId 
                    // of the erased Object. Use the DBObject property of the of the 
                    // ObjectErasedEventArgs passed into the event. (e.DBObject.ObjectId.ToString()) 
                    // Note: put the closing curly brace above the closing curley brace for the for loop 
                    if (node.Tag.ToString() == e.DBObject.ObjectId.ToString())
                    {

                        // 28. Remove the node by calling the Remove method. (The entity was 
                        // erased from the drawing). 
                        node.Remove();

                        // 29. Exit the For loop by adding a break statement. 
                        break;
                    }

                }
            }
            else
            {
                // 30. If this is processed it means that the object was unerased. (e.Erased was false) 
                // Declare a System.Windows.Forms.TreeNode use newNode as the name. Instantiate it by 
                // using the Add method of the Nodes collection of the TreeView created in previous steps. 
                // Use the Type of the object for the parameter. 
                // e.DBObject.GetType().ToString() 
                System.Windows.Forms.TreeNode newNode = myPalette.treeView1.Nodes.Add(e.DBObject.GetType().ToString());

                // 31. Make the Tag property of the node created in step 30 equal to the ObjectId of 
                // the unerased object. This will allow us to record it's ObjectId for recognition in 
                // other events. Use e.DBObject.ObjectId.ToString() 
                newNode.Tag = e.DBObject.ObjectId.ToString();
            }
        }

        // 32. Create a private function named callback_ObjectReappended. This is the func that 
        // will be called when an Object is ReAppended to the Database. (The name needs to be 
        // the name used in the Delegate parameter of step 19). The first parameter is an 
        // object. (Use sender as the name of the Object). The second parameter is 
        // an ObjectEventArgs. (use e as the name of the ObjectEventArgs) 
        // Note: Put the closing curly brace after step 34 
        private void callback_ObjectReappended(object sender, ObjectEventArgs e)
        {

            // 33. Add the class name of the object to the tree view 
            // Declare a TreeNode variable. (System.Windows.Forms.TreeNode). Instantiate 
            // it using the Add method of the Nodes property of the TreeView on the UserForm1 
            // created in step 4. Use the ObjectEventArgs passed into the method for the string 
            // parameter and use the "Type" of DBObject. (e.DBObject.GetType().ToString()) 
            System.Windows.Forms.TreeNode newNode = myPalette.treeView1.Nodes.Add(e.DBObject.GetType().ToString());

            // 34. Record its id for recognition later 
            // Make the Tag property of the node created in step 33 equal to the ObjectId of 
            // the unerased object. This will allow us to record it's ObjectId for recognition in 
            // other events. Use e.DBObject.ObjectId.ToString() 
            newNode.Tag = e.DBObject.ObjectId.ToString();
        }

        // 35. Create a private Sub named callback_ObjectUnappended. (returns void) This is the
        // function that will be called when an Object is UnAppended from the Database. 
        // (The name needs to be the name used in the Delegate parameter of step 20). 
        // The first parameter is an object. (Use sender as the name of the Object). 
        // The second parameter is an ObjectEventArgs. 
        // (Use e as the name of the ObjectEventArgs) 
        // Note: Put the closing curly brace after step 39 
        private void callback_ObjectUnappended(object sender, ObjectEventArgs e)
        {

            // 36. Here we will search for an object in the treeview control so it can be removed. 
            // Create a foreach statement. Use node for the element name and the type is 
            // System.Windows.Forms.TreeNode. The group paramater is the Nodes in the TreeView. 
            // (myPalette.treeView1.Nodes) 
            // Note: Put the closing curly brace after step 39  
            foreach (System.Windows.Forms.TreeNode node in myPalette.treeView1.Nodes)
            {
                // 37. Use and "if" statement and see if the node is the one we want. 
                // compare the node.Tag to the ObjectId. (use e.DBObject.ObjectId.ToString) 
                // Note: Put the closing curly brace after step 39 
                if (node.Tag.ToString() == e.DBObject.ObjectId.ToString())
                {
                    // 38. If we got here then this is the node for the unappended object. 
                    // call the Remove method of the node. 
                    node.Remove();

                    // 39. Exit the For loop by adding a break.
                    break;
                }

            }
        }
    }
}