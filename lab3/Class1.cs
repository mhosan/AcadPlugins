

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry; 

public class adskClass
{

    [CommandMethod("addAnEnt")]
    public void AddAnEnt()
    {

        // get the editor object so we can carry out some input 
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        // first decide what type of entity we want to create 
        PromptKeywordOptions getWhichEntityOptions = new PromptKeywordOptions("¿Que es lo que querés crear? (Hay circulometro y bloque) [Circle/Block] : ", "Circle Block");
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
                    PromptPointOptions getPointOptions = new PromptPointOptions("Seleccioná el centro del circulómetro a agregar : ");
                    PromptPointResult getPointResult = ed.GetPoint(getPointOptions);
                    // if ok 
                    if ((getPointResult.Status == PromptStatus.OK))
                    {

                        // the get the radius 
                        PromptDistanceOptions getRadiusOptions = new PromptDistanceOptions("seleccioná el radio : ");
                        // set the start point to the center (the point we just picked) 
                        getRadiusOptions.BasePoint = getPointResult.Value;
                        // now tell the input mechanism to actually use the basepoint! 
                        getRadiusOptions.UseBasePoint = true;
                        // now get the radius 
                        PromptDoubleResult getRadiusResult = ed.GetDistance(getRadiusOptions);
                        // if all is ok 
                        if ((getRadiusResult.Status == PromptStatus.OK))
                        {

                            // Begining of Lab3. Create the Circle or Block and BlockReference 

                            // 1. Declare a Database variable and instantiate it 
                            // using the Document.Database property of the editor created above. (ed) 
                            // Note: Add the Autodesk.AutoCAD.DatabaseServices; namespace for Database 
                            // and Transaction Use the using keyword (above the class declaration) 
                            Database dwg = ed.Document.Database;

                            // 2. Declare a Transaction variable instantiate it using the 
                            // TransactionManager.StartTransaction method of the Databse
                            // created in step 1.
                            Transaction trans = dwg.TransactionManager.StartTransaction();

                            // 3. Add a try, catch and finally block. 
                            try
                            {

                                // 4. Declare a Circle variable and create it using the new keyword. 
                                // Use the the Value property of the PromptPointResult created in 
                                // Lab2 for the first parameter. For the second parameter (normal)use 
                                // the Vector3d.ZAxis. Use the Value property of the PromptDoubleResult 
                                // (created in Lab2) for the radius. 
                                Circle circle = new Circle(getPointResult.Value, Vector3d.ZAxis, getRadiusResult.Value);

                                // 5. Declare a BlockTableRecord variable. Instatiate it using the 
                                // GetObject method of the Transaction variable create in step 2. 
                                // Use the CurrentSpaceId property of the Database variable created in 
                                // step 1 for the first parameter. (ObjectId) For the second parameter 
                                // use OpenMode.ForWrite. We are adding the circle to either ModelSpace 
                                // or PaperSpace. (the CurrentSpaceId determines this) 
                                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(dwg.CurrentSpaceId, OpenMode.ForWrite);

                                // 6. Add the Circle to the BlockTableRecord created in step 5. Use the 
                                // AppendEntity method and pass in the circle created in step 4. 
                                btr.AppendEntity(circle);

                                // 7. Tell the transaction about the new circle so that it can autoclose 
                                // it. Use the AddNewlyCreatedDBObject method. The first argument is the 
                                // circle. Use True for the second argument. 
                                trans.AddNewlyCreatedDBObject(circle, true);

                                // 8. Commit the transaction by calling the Commit method. If the code gets 
                                // this far everything should have worked correctly. 

                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                // 9. Declare an Exception variable for the Catch. 
                                // (add "ex as Exception" to the Catch keyword) 
                                // Use the WriteMessage of the Editor variable (ed) created in Lab2. 
                                // Use "problem due to " + ex.Message for the Message parameter. 
                                // If an error occurs the details of the problem will be printed 
                                // on the AutoCAD command line. 
                                ed.WriteMessage("problem due to " + ex.Message);
                            }
                            finally
                            {
                                // 10. Dispose the transaction by calling the Dispose method 
                                // of the Transaction created in step 2. This will be called 
                                //whether an error on not occurred. 
                                trans.Dispose();

                            }
                        }
                    }
                    break;
                case "Block":

                    // enter the name of the block 
                    PromptStringOptions blockNameOptions = new PromptStringOptions("Ingresá el nombre del bloque este que querés crear : ");
                    // no spaces are allowed in a blockname so disable it 
                    blockNameOptions.AllowSpaces = false;
                    // get the name 
                    PromptResult blockNameResult = ed.GetString(blockNameOptions);
                    // if ok 
                    if ((blockNameResult.Status == PromptStatus.OK))
                    {

                        // 11. Declare a Database variable and instantiate it using the 
                        // Document.Database property of the editor created above. (ed) 
                        Database dwg = ed.Document.Database;


                        // 12. Declare a Transaction variable instantiate it using the 
                        // TransactionManager.StartTransaction method. 
                        Transaction trans = (Transaction)dwg.TransactionManager.StartTransaction();

                        // 13. Add a try, catch and finally block. Move the closing curly
                        // brace right after step 34. Put the catch statement after this.
                        // Enclose  step 35 in the catch call. Enclose step 36 in the finally call.
                        try
                        {

                            // 14. Declare a BlockTableRecord variable. Create it using the 
                            // new keyword. 
                            BlockTableRecord newBlockDef = new BlockTableRecord();

                            // 15. Set the name of the name the BlockTableRecord. Use the 
                            // StringResult property of the PromptResult variable above. 
                            // (created in Lab2) 
                            newBlockDef.Name = blockNameResult.StringResult;

                            // 16. Declare a variable as a BlockTable. Instiate it using the 
                            // GetObject method of the Transaction Use the BlockTableId property 
                            // of the Database variable created in step 11 for the first parameter. 
                            // Use OpenMode.ForRead for the second parameter. We are opening for 
                            // read to check if a block with the name provided by the user already exists. 
                            BlockTable blockTable = (BlockTable)trans.GetObject(dwg.BlockTableId, OpenMode.ForRead);

                            // 17. Add an if statement. Test to see if the BlockTable has a block by 
                            // using the Has method of the variable created in step 16. For the string 
                            // Parameter use the StringResult property of the PromptResult variable above. 
                            // created in Lab2. Check to see if it equals False. 
                            // Move the closing curly brace below Step 34. 
                            if ((blockTable.Has(blockNameResult.StringResult) == false))
                            {

                                // 18. The Block with that name does not exist so add it. 
                                // First make the BlockTable open for write. Do this by calling the 
                                // UpgradeOpen() method of the BlockTable. (created in step 16) 
                                blockTable.UpgradeOpen();

                                // 19. Add the BlockTableRecord created in step 14. Use the Add method 
                                // of the BlockTable and pass in the BlockTableRecord. 
                                blockTable.Add(newBlockDef);

                                // 20. Tell the transaction about the new Block so that it can autoclose 
                                // it. Use the AddNewlyCreatedDBObject method. The first argument is the 
                                // BlockTableRecord. Use True for the second argument. 
                                trans.AddNewlyCreatedDBObject(newBlockDef, true);


                                // 21. In the next two steps you add circles to the BlockTableRecord. 
                                // Declare a variable as a Circle and instantiate it using 
                                // the new Keyword. For the first argument create a new 
                                // Point3d use (0,0,0), for the second arguement use Vector3d.ZAxis 
                                // use 10 for the Radius argument. 
                                // Note: Add the Autodesk.AutoCAD.Geometry Namespace for Point3d 
                                // Use the using keyword (above the class declaration) 
                                Circle circle1 = new Circle(new Point3d(0, 0, 0), Vector3d.ZAxis, 10);

                                // 22. Append the circle to the BlockTableRecord. 
                                // Use the AppendEntity method pass in the circle from step 21 
                                newBlockDef.AppendEntity(circle1);

                                // 23. Now add another circle to the BlockTableRecord 
                                // Declare a variable as a Circle and instantiate it using 
                                // the new Keyword. For the first argument create a new 
                                // Point3d use (20,10,0), for the second arguement use Vector3d.ZAxis 
                                // use 10 for the Radius argument. 
                                Circle circle2 = new Circle(new Point3d(20, 10, 0), Vector3d.ZAxis, 10);

                                // 23. Append the second circle to the BlockTableRecord. 
                                // Use the AppendEntity method pass in the circle from step 23 
                                newBlockDef.AppendEntity(circle2);

                                // 24. Tell the transaction manager about the new objects so that 
                                // the transaction will autoclose them. Call the AddNewlyCreatedDBObject 
                                // pass in the Circle created in step 21. Do this again for the circle 
                                // Created in step 23. (use True for the second arguement). 
                                trans.AddNewlyCreatedDBObject(circle1, true);
                                trans.AddNewlyCreatedDBObject(circle2, true);


                                // 25. We have created a new block definition. (BlockTableRecord). 
                                // Here we will use that Block and add a BlockReference to modelspace. 
                                // First declare a PromptPointOptions and instantiate it with the new 
                                // keyword. For the message parameter use "Pick insertion point of BlockRef : " 
                                PromptPointOptions blockRefPointOptions = new PromptPointOptions("Pick insertion point of BlockRef : ");

                                // 26. Declare a PromptPointResult variable. Use the GetPoint method of 
                                // the Editor created in Lab2 (ed). Pass in the PromptPointOptions created 
                                // in step 25. 
                                PromptPointResult blockRefPointResult = ed.GetPoint(blockRefPointOptions);

                                // 27. Create an if statement and test the Status of the PromptPointResult. 
                                // Test if it is not equal to PromptStatus.OK. 
                                //Place the closing curly brace below step 29. 
                                if ((blockRefPointResult.Status != PromptStatus.OK))
                                {

                                    // 28. If we got here then the GetPoint failed. Call the dispose 
                                    // method of the Transaction created in step 11. 
                                    trans.Dispose();
                                    // 29. return 
                                    return;
                                }

                                // 30. Declare a BlockReference variable. Instatiate it with the new keyword 
                                // Use the Value method of the PromptPointResult for the Position argument. (point3d) 
                                // Use the ObjectId property of the BlockTableRecord created in Step 14 for the 
                                // Second parameter. 
                                BlockReference blockRef = new BlockReference(blockRefPointResult.Value, newBlockDef.ObjectId);

                                // 31. Get the current space. (either ModelSpace or PaperSpace. 
                                // Declare a BlockTableRecord variable instantiate it using the GetObject 
                                // method of the Transaction created in step 12. Use the CurrentSpaceId property 
                                // of the Database created in step 11. Open it for write. (OpenMode.ForWrite) 
                                BlockTableRecord curSpace = (BlockTableRecord)trans.GetObject(dwg.CurrentSpaceId, OpenMode.ForWrite);

                                // 32. Use the AppendEntity method of the BlockTableRecord created in step 31 
                                // and pass in the BlockReference created in step 20. 
                                curSpace.AppendEntity(blockRef);

                                // 33. Tell the transaction about the new block reference so that the transaction 
                                // can autoclose it. Use the AddNewlyCreatedDBObject of the Transaction created in 
                                // step 12. Pass in the BlockReference. Use True for the second parameter. 
                                trans.AddNewlyCreatedDBObject(blockRef, true);

                                // 34. If the code makes it here then all is ok. Commit the transaction by calling 
                                // the Commit method 
                                trans.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            // 35. Declare an Exception variable for the Catch. 
                            // (add "(Exception ex)" to the catch keyword) 
                            // Use the WriteMessage of the Editor variable (ed) created in Lab2. 
                            // Use "a problem occured because " + ex.Message for the Message parameter. 
                            // If an error occurs the details of the problem will be printed 
                            // on the AutoCAD command line. 
                            ed.WriteMessage("a problem occured because " + ex.Message);
                        }
                        finally
                        {
                            // 36. Dispose the transaction by calling the Dispose method 
                            // of the Transaction created in step 12. This will be called 
                            // whether an error on not occurred.  This is the end of Lab3.
                            // Build and debug by loading in AutoCAD. (use NETLOAD) and run
                            // the addAnEnt command.
                            trans.Dispose();

                        }
                    }
                    break;
            }

        }
    }
} 
