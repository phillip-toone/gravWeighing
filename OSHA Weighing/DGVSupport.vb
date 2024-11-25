Imports System.IO


Public Class DGVSupport
    Dim SelectedMethod As String


    ''' <summary>
    ''' Returns the cell content of the specified DGV table, recalled from a text file
    ''' </summary>
    ''' <param name="filepath">Fully qualified file path to the data file</param>
    ''' <param name="identifier">Name of the identifier for the method.
    ''' If an identifer is not specifed, a list of all available methods is displayed for the user to select from.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function ReturnTable(ByVal filepath As String, Optional ByVal identifier As String = "") As String(,)
        Dim FI As New FileInfo(filepath)
        Dim MyData(0, 0) As String

        Try
            If FI.Exists And Name <> "" Then
                Dim SR As New StreamReader(filepath)
                Dim FileText() As String
                Dim LineOfData() As String
                Dim Rows As Int32
                Dim Cols As Int32 = 0
                Dim Element As Int32
                Dim Offset As Int32

                FileText = SR.ReadToEnd.Split(Chr(10))  'Split on LF to get the lines in the file
                SR.Close()
                SR.Dispose()

                'If the user provided the method identifier name, use it, if not display a list and let them select
                If identifier = "" Then
                    DisplayTableNames(filepath)
                    Application.DoEvents()
                    identifier = SelectedMethod
                    If identifier = "" Then
                        Return MyData
                    End If
                End If

                For Element = 0 To FileText.Length - 1
                    If FileText(Element).ToUpper = "IDENTIFIER: " & identifier.ToUpper & vbCr Then
                        Offset = Element + 1
                        Element = 0
                        Exit For    'you found the line in the file with your identifier
                    End If
                Next

                If Element < FileText.Length - 1 Then
                    'See how many columns of data there are by counting the Tabs
                    Cols = FileText(Offset).Split(Chr(9)).Length
                    'Get the number of rows by looking for the last line in the file or "IDENTIFIER: "
                    Rows = 0
                    While Not FileText(Offset + Rows).Contains("IDENTIFIER: ") And (Offset + Rows) < FileText.Length - 1
                        Rows += 1
                    End While
                    If Rows > 0 Then
                        ReDim MyData(Rows - 1, Cols - 1)
                        For ThisRow As Int32 = 0 To Rows - 1
                            FileText(Offset + ThisRow) = FileText(Offset + ThisRow).TrimEnd(Chr(13))    'get rid of the CR
                            LineOfData = FileText(Offset + ThisRow).Split(Chr(9))
                            For ThisCol As Int32 = 0 To Cols - 1
                                MyData(ThisRow, ThisCol) = LineOfData(ThisCol)
                            Next
                        Next
                    End If    'If data exists after the Identifier is found
                End If    'If you are found the Identifier before the end of the file
            End If    'If the file exists
        Catch ex As Exception
            MessageBox.Show(ex.ToString, "Error in Function ReturnTable", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            Array.Clear(MyData, 0, MyData.Length)
        End Try

        FI = Nothing
        Return MyData
    End Function


    ''' <summary>
    ''' Populates the cells of the passed table with the data in the specifed method.
    ''' </summary>
    ''' <param name="thisDGV">Reference to the table to populate.</param>
    ''' <param name="filepath">Fully qualified path to the data file.</param>
    ''' <param name="identifier">Optional method name.  If not supplied, the function displays a list of methods
    ''' contained in the method file.</param>
    ''' <returns>True if the method was found and the data was loaded properly.</returns>
    ''' <remarks></remarks>
    Function PopulateTable(ByRef thisDGV As DataGridView, ByVal filepath As String, Optional ByVal identifier As String = "") As Boolean
        Dim FI As New FileInfo(filepath)
        Dim LoadStatus As Boolean

        Try
            If FI.Exists And Name <> "" Then
                Dim SR As New StreamReader(filepath)
                Dim FileText() As String
                Dim LineOfData() As String
                Dim Rows As Int32
                Dim Cols As Int32 = 0
                Dim Element As Int32
                Dim Offset As Int32

                FileText = SR.ReadToEnd.Split(Chr(10))  'Split on LF to get the lines in the file
                SR.Close()
                SR.Dispose()

                'If the user provided the method identifier name, use it, if not display a list and let them select
                If identifier = "" Then
                    DisplayTableNames(filepath)
                    Application.DoEvents()
                    identifier = SelectedMethod
                    If identifier = "" Then
                        Return False
                    End If
                End If

                For Element = 0 To FileText.Length - 1
                    If FileText(Element).ToUpper = "IDENTIFIER: " & identifier.ToUpper & vbCr Then
                        Offset = Element + 1
                        Element = 0
                        Exit For    'you found the line in the file with your identifier
                    End If
                Next

                If Element < FileText.Length - 1 Then
                    'See how many columns of data there are by counting the Tabs
                    Cols = FileText(Offset).Split(Chr(9)).Length
                    'Get the number of rows by looking for the last line in the file or "IDENTIFIER: "
                    Rows = 0
                    While Not FileText(Offset + Rows).Contains("IDENTIFIER: ") And (Offset + Rows) < FileText.Length - 1
                        Rows += 1
                    End While
                    If Rows > 0 Then
                        For ThisRow As Int32 = 0 To Rows - 1
                            FileText(Offset + ThisRow) = FileText(Offset + ThisRow).TrimEnd(Chr(13))    'get rid of the CR
                            LineOfData = FileText(Offset + ThisRow).Split(Chr(9))
                            For ThisCol As Int32 = 0 To Cols - 1
                                thisDGV.Rows(ThisRow).Cells(ThisCol).Value = LineOfData(ThisCol)
                            Next
                            LoadStatus = True
                        Next
                    End If    'If data exists after the Identifier is found
                End If    'If you are found the Identifier before the end of the file
            End If    'If the file exists
        Catch ex As Exception
            MessageBox.Show(ex.ToString, "Error in Function PopulateTable", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
        End Try

        FI = Nothing
        Return LoadStatus
    End Function


    ''' <summary>
    ''' Saves data from the specifed DGV table to a text file in a format the supports the other DGVSupport functions.
    ''' </summary>
    ''' <param name="table">A reference to that table containing the data to save.</param>
    ''' <param name="filepath">A fully qualified file path.  If the file doesn't exist, it is created.</param>
    ''' <param name="identifier">A string identifier for the saved data.</param>
    ''' <param name="overRideNotice">Optionally, if data with the passed identifer is found, a message box appears to
    ''' verify if the old data shoudl be overwritten with the new data.</param>
    ''' <remarks></remarks>
    Sub SaveTable(ByRef table As DataGridView, ByVal filepath As String, ByVal identifier As String, Optional ByVal overRideNotice As Boolean = False)
        Dim FI As New FileInfo(filepath)
        Dim MyLine As String
        Dim MyRows As Int32
        Dim MyCols As Int32


        Try
            If FI.Exists Then
                Dim MethodExists As Boolean = False
                Dim MyCollection As New Collection

                'First see if a mehtod already exists by this name
                MyCollection = GetTableNames(filepath)
                For Pass As Int32 = 1 To MyCollection.Count
                    If identifier.ToUpper = CType(MyCollection.Item(Pass), String).ToUpper Then 'Collections are 1 based
                        MethodExists = True
                        Exit For
                    End If
                Next
                MyCollection = Nothing

                If MethodExists = True Then
                    If MessageBox.Show("Table: " & identifier & " already exists.  Do you want to over-write it?", "File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button2) = Windows.Forms.DialogResult.No Then
                        Exit Sub
                    End If
                    'You need to read in the entire file, write back the portions that you're not operating on, then write
                    'the new version of this method to the end of the file
                    Dim SR As New StreamReader(filepath)
                    Dim Lines() As String
                    Dim Index As Int32

                    Lines = SR.ReadToEnd.Split(Chr(10)) 'Split on LF
                    SR.Dispose()
                    FI.Delete() 'Delete the old file
                    Application.DoEvents()

                    Dim sw As New StreamWriter(filepath)    'Create the new file of the same name
                    For Index = 0 To Lines.Length - 2
                        If Lines(Index).ToUpper = "IDENTIFIER: " & identifier.ToUpper & vbCr Then
                            Index += 1
                            'This is the section that you are overwritting, skip over it
                            Do
                                If Lines(Index).Contains("IDENTIFIER:") Then 'You found the next file segment
                                    Index -= 1
                                    Exit Do
                                End If
                                Index += 1
                            Loop While Index <= Lines.Length - 2
                        Else
                            sw.Write(Lines(Index) & vbLf)
                        End If
                    Next
                    'The file, except for the section to be overwritten has been written back to disk
                    'Now write the new section
                    MyRows = table.Rows.Count - 1
                    MyCols = table.ColumnCount - 1
                    sw.Write("IDENTIFIER: " & identifier & vbCrLf)
                    For Row As Int32 = 0 To MyRows
                        MyLine = ""
                        For Col As Int32 = 0 To MyCols
                            If Not table.Rows(Row).Cells(Col).Value Is Nothing Then
                                MyLine += table.Rows(Row).Cells(Col).Value.ToString
                            End If
                            If Col < MyCols Then
                                MyLine += vbTab
                            End If
                        Next
                        sw.Write(MyLine & vbCrLf)
                    Next

                    sw.Close()
                    sw.Dispose()

                Else    'append the new method to the end of the file
                    Dim SW As New StreamWriter(filepath, True)

                    MyRows = table.Rows.Count - 1
                    MyCols = table.ColumnCount - 1
                    SW.Write("IDENTIFIER: " & identifier & vbCrLf)
                    For Row As Int32 = 0 To MyRows
                        MyLine = ""
                        For Col As Int32 = 0 To MyCols
                            If Not table.Rows(Row).Cells(Col).Value Is Nothing Then
                                MyLine += table.Rows(Row).Cells(Col).Value.ToString
                            End If
                            If Col < MyCols Then
                                MyLine += vbTab
                            End If
                        Next
                        SW.Write(MyLine & vbCrLf)
                    Next
                    SW.Close()
                    SW.Dispose()
                End If

            Else    'The file doesn't exist, create it and save to it
                Dim SW As New StreamWriter(filepath)

                MyRows = table.Rows.Count - 1
                MyCols = table.ColumnCount - 1
                SW.Write("IDENTIFIER: " & identifier & vbCrLf)
                For Row As Int32 = 0 To MyRows
                    MyLine = ""
                    For Col As Int32 = 0 To MyCols
                        If Not table.Rows(Row).Cells(Col).Value Is Nothing Then
                            MyLine += table.Rows(Row).Cells(Col).Value.ToString
                        End If
                        If Col < MyCols Then
                            MyLine += vbTab
                        End If
                    Next
                    SW.Write(MyLine & vbCrLf)
                Next
                SW.Close()
                SW.Dispose()
            End If
        Catch ex As Exception
            MessageBox.Show(ex.ToString, "Error in Function SaveTable", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
        End Try

        FI = Nothing
    End Sub

    ''' <summary>
    ''' Returns a list of all the method identifier contained in the file.   File must be created by DGVSupport functions
    ''' </summary>
    ''' <param name="fullpath">A fully qualified file name with complete path</param>
    ''' <remarks>Only file created by the DGVSupport fuctions can be accessed using this function</remarks>
    Function GetTableNames(ByVal fullpath As String) As Collection
        Dim FI As New FileInfo(fullpath)
        Dim MyNames As New Collection

        Try
            If FI.Exists Then
                Dim FileText() As String
                Dim SR As New StreamReader(fullpath)

                FileText = SR.ReadToEnd.Split(Chr(10)) 'Split on LF to get the lines in the file
                SR.Close()
                SR.Dispose()

                For Element As Int32 = 0 To FileText.Length - 2
                    If FileText(Element).ToUpper.Contains("IDENTIFIER:") Then
                        MyNames.Add(FileText(Element).Substring(12, FileText(Element).Length - 13))
                    End If
                Next
            Else
                MessageBox.Show("File: " & fullpath & " does not exist")
            End If
        Catch ex As Exception
            MessageBox.Show(ex.ToString, "Error in Function GetTableNames", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            MyNames.Clear()
        End Try

        Return MyNames
    End Function


    ''' <summary>
    ''' Displays all the method identifiers contained in the file
    ''' </summary>
    ''' <param name="filepath">Fully qualifed file path</param>
    ''' <remarks></remarks>
    Sub DisplayTableNames(ByVal filepath As String)
        Dim MyCollection As New Collection

        MyCollection = GetTableNames(filepath)
        If MyCollection.Count > 0 Then
            Me.lsbMyList.BeginUpdate()
            For Element As Int32 = 1 To MyCollection.Count
                Me.lsbMyList.Items.Add(CType(MyCollection.Item(Element), String))   'Collections are 1 based.
            Next
            Me.lsbMyList.EndUpdate()
            Me.txbFileName.Text = filepath.Substring(filepath.LastIndexOf("\") + 1)
            Me.ShowDialog()
            Me.lsbMyList.Items.Clear()
        End If
    End Sub



    ''' <summary>
    ''' Sizes the DGV table to fit the height and width of the data in the table
    ''' </summary>
    ''' <param name="myDGV">A reference to the DGV table to be resized</param>
    ''' <param name="rowsDisplayed">The number of rows to fit in the resized table</param>
    ''' <param name="colsDisplayed">The number of columns to fit in the resized table</param>
    ''' <remarks></remarks>
    Public Sub SizeTable(ByRef myDGV As DataGridView, Optional ByVal rowsDisplayed As Int32 = 0, Optional ByVal colsDisplayed As Int32 = 0)
        Dim Dimension As Int32
        Dim Pass As Int32

      'myDGV.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders)
      'myDGV.AutoResizeColumnHeadersHeight()
        Dimension = myDGV.RowHeadersWidth
        If colsDisplayed = 0 Then
            colsDisplayed = myDGV.ColumnCount - 1
        End If
        For Pass = 0 To colsDisplayed
            Dimension += myDGV.Columns(Pass).Width
        Next
        If myDGV.RowHeadersVisible = False Then
            Dimension = Dimension - myDGV.RowHeadersWidth + 2
        End If
        myDGV.Width = Dimension + 2

        Dimension = myDGV.ColumnHeadersHeight
        If rowsDisplayed = 0 Then
            rowsDisplayed = myDGV.RowCount
        End If
        For Pass = 0 To rowsDisplayed - 1
            If Pass <= myDGV.RowCount - 1 Then
                Dimension += myDGV.Rows(Pass).Height
            End If
        Next
        myDGV.Height = Dimension + 2
        myDGV.Invalidate()
    End Sub

    ''' <summary>
    ''' Returns the size of the DGV table
    ''' </summary>
    ''' <param name="myDGV">Reference to the DGV table</param>
    ''' <param name="startingRow">Row to start from in the calculation</param>
    ''' <param name="endingRow">Row to end with in the calculation</param>
    ''' <param name="startingColumn">Column to start with in the calculation</param>
    ''' <param name="endingColumn">Column to end with in the calculation</param>
    ''' <returns>Returns the size of the table</returns>
    ''' <remarks></remarks>
    Public Function GetSize(ByRef myDGV As DataGridView, Optional ByVal startingRow As Int32 = 0, Optional ByVal endingRow As Int32 = Int32.MaxValue, Optional ByVal startingColumn As Int32 = 0, Optional ByVal endingColumn As Int32 = Int32.MaxValue) As Size
        'This function returns the size of the data grid view needed to house the number and rows and columns passed in
        Dim MySize As Size
        Dim Pass As Int32
        Dim Length As Int32


        MySize.Height = myDGV.ColumnHeadersHeight + 2
        If endingRow > myDGV.Rows().Count Then
            Length = myDGV.Rows().Count - 1
        Else
            Length = endingRow - 1
        End If
        For Pass = startingRow To Length
            MySize.Height = MySize.Height + myDGV.Rows(Pass).Height
        Next

        MySize.Width = myDGV.RowHeadersWidth + 2
        If endingColumn > myDGV.Columns().Count Then
            Length = myDGV.Columns().Count - 1
        Else
            Length = endingColumn - 1
        End If
        For Pass = startingColumn To Length
            MySize.Width = MySize.Width + myDGV.Columns(Pass).Width
        Next

        Return MySize
   End Function


   ''' <summary>
   ''' Clears the value of every cell in the table and sets its value to nothing
   ''' </summary>
   ''' <param name="myDGV">Referecne to the DataGridView Table to Operate on.</param>
   ''' <remarks></remarks>
   Public Sub ClearAllCells(ByRef myDGV As DataGridView)
      Dim Row As Int32
      Dim Col As Int32

      For Row = 0 To myDGV.Rows.Count - 1
         For Col = 0 To myDGV.ColumnCount - 1
            myDGV.Rows(Row).Cells(Col).Value = Nothing
         Next
      Next
   End Sub



   ''' <summary>
   ''' Sets the referenced table cell to the specified color
   ''' </summary>
   ''' <param name="myDGV">Reference to the table to operate on</param>
   ''' <param name="row">Table Row</param>
   ''' <param name="col">Table column</param>
   ''' <param name="mycolor">Color of cell</param>
   ''' <remarks></remarks>
   Public Sub SetCellColor(ByRef myDGV As DataGridView, ByVal row As Int32, ByVal col As Int32, ByVal mycolor As Color)
      Try
         myDGV.Rows(row).Cells(col).Style.BackColor = mycolor
      Catch ex As Exception
      End Try
   End Sub


   ''' <summary>
   ''' Sets the backcolor to the color specified
   ''' </summary>
   ''' <param name="myDGV">Reference to the DGV table to operate on</param>
   ''' <param name="myRow">Row of the table to operate on.  Rows are 0 based.</param>
   ''' <param name="myColor">Color to set</param>
   ''' <remarks></remarks>
   Public Sub SetRowBackColor(ByRef myDGV As DataGridView, ByVal myRow As Int32, ByVal myColor As Color)
      Dim MyStyle As New DataGridViewCellStyle
      Dim Pass As Int32

      MyStyle.BackColor = myColor
      For Pass = 0 To myDGV.Columns.Count - 1
         myDGV.Rows(myRow).Cells(Pass).Style = MyStyle
      Next
   End Sub


   Private Sub lsbMyList_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lsbMyList.SelectedIndexChanged
      Try
         SelectedMethod = lsbMyList.SelectedItem.ToString
         Me.Close()
      Catch ex As Exception
      End Try
   End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        SelectedMethod = ""
        Me.Close()
    End Sub
End Class