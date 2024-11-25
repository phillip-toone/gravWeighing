Public Class frmBlankPicker
   Enum Stg
      PickBlank
      PickSet
   End Enum

   Structure PrivateData  'These are variables that I need in the blank picker graphic screen.  Don't use them anywhere else
      Dim Element As Int32
      Dim Stage As Stg
      Dim Group As Int32
      Dim BlanksInGroup As Int32
      Dim FiltersInGroup As Int32
   End Structure

   Dim Cellsize As Point
   Dim Cellseperation As Point
   Dim X As Int32
   Dim Y As Int32
   Dim MyRectangle As Rectangle
   Dim Columns As Int32 = 8
   Dim Rows As Int32 = 12
   Dim Cell(96) As Int32
   'Dim Stage As Stg
   'Dim Group As Int32   'Don't use this.   Not for general use
   'Dim Element As Int32 'Don't use this.   Not for general use
   Dim Batch As Int32
   Dim CloseAllowed As Boolean
   Dim PenaltimateCellClicked As Int32
   Dim BPData As PrivateData   'Dont use these variables anywhere else


   Sub New(ByVal element As Int32)
      InitializeComponent()
      Batch = element
   End Sub


   Private Sub frmBlankPicker_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
      ClearData()
      SetRackGraphicSize(400, 600)
      SetRackGraphicOrigin(30, 60)
      BPData.Stage = Stg.PickBlank
      BPData.Group = 0
      BPData.Element = 0
      CloseAllowed = True
      PenaltimateCellClicked = 0
      Me.Invalidate()

   End Sub



   Private Sub frmBlankPicker_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
      PaintPlate(Me)
   End Sub


   Public Overridable Overloads Sub PaintPlate(ByVal Control As Control)
      'This method paints the plate taking into account the racks StartPosition and
      'IncrementDireaction settings.   The StartPosition settings are:
      'TopLeft, TopRight, BottomLeft and BottomRight
      'The IncrementDirection settings options are: ByRow and ByColumn

      'This sub handles painting the correct color for each cell in the plate.
      'Lime if the cell is ready
      'Yellow if the cell is active
      'Red if the cell is done
      'Gray if the cell is excluded
      Dim PenColor As New System.Drawing.Pen(System.Drawing.Color.Blue)
      Dim BrushColor As New SolidBrush(Color.White)
      Dim TextBrush As New SolidBrush(Color.Black)
      Dim myRow As Int32
      Dim myColumn As Int32
      Dim myX As Int32
      Dim myY As Int32
      Dim DeltaX As Int32
      Dim DeltaY As Int32
      Dim MyFont As New Font("Arial", 8)
      Dim TextX As Single
      Dim TextY As Single
      Dim MyCell As Int32
      Dim myGraphics As System.Drawing.Graphics


      myGraphics = Control.CreateGraphics()
      DeltaX = Cellsize.X + Cellseperation.X
      DeltaY = Cellsize.Y + Cellseperation.Y

      myX = (DeltaX * (Columns + 1)) + Cellsize.X
      If DeltaX > 10 Then
         myX = myX - (2 * (DeltaX - 10))
      End If
      myY = (DeltaY * (Rows + 1)) + Cellsize.Y
      If DeltaY > 10 Then
         myY = myY - (2 * (DeltaY - 10))
      End If
      Dim myRec As New Rectangle(MyRectangle.X, MyRectangle.Y, myX, myY)

      myGraphics.DrawRectangle(PenColor, myRec)
      myGraphics.FillRectangle(BrushColor, myRec.Left + 1, myRec.Top + 1, myX - 1, myY - 1)
      MyCell = 1


      For myColumn = 0 To 7
         myX = (myRec.Left + DeltaX) + (myColumn * DeltaX)
         If DeltaX > 10 Then
            myX = myX - (DeltaX - 10)
         End If
         For myRow = 0 To 11
            myY = (myRec.Top + DeltaY) + (myRow * DeltaY)
            If DeltaY > 10 Then
               myY = myY - (DeltaY - 10)
            End If

            If Cell(MyCell) = -1 Then
               BrushColor.Color = Color.LightGray 'uncommitted
            ElseIf Cell(MyCell) = 0 Then
               BrushColor.Color = Color.Blue 'uncommitted
            ElseIf Cell(MyCell) = 1 Then
               BrushColor.Color = Color.Yellow  'reference filter
            ElseIf Cell(MyCell) = 2 Then
               BrushColor.Color = Color.Lime  'assigned
            End If
            myGraphics.FillEllipse(BrushColor, myX, myY, Cellsize.X, Cellsize.Y)

            Dim SF As New StringFormat
            SF.Alignment = StringAlignment.Center
            SF.LineAlignment = StringAlignment.Center

            TextX = CType(myX + 1 + (Cellsize.X / 2.0), Single)
            TextY = CType(myY + 1 + (Cellsize.Y / 2.0), Single)
            If BrushColor.Color = Color.Blue Then
               TextBrush.Color = Color.White
            Else
               TextBrush.Color = Color.Black
            End If
            myGraphics.DrawString(MyCell.ToString, MyFont, TextBrush, TextX, TextY, SF)

            MyCell += 1
         Next myRow
      Next myColumn


      myGraphics.Dispose()
   End Sub


   ''' <summary>
   ''' Set the display origin of the graphical rack repesentation
   ''' </summary>
   ''' <param name="myX">X origin</param>
   ''' <param name="myY">Y origin</param>
   ''' <remarks></remarks>
   Public Overridable Overloads Sub SetRackGraphicOrigin(ByVal myX As Int32, ByVal myY As Int32)
      Me.MyRectangle.X = myX
      Me.MyRectangle.Y = myY
   End Sub

   ''' <summary>
   ''' Sets the size of the graphic that represents the plate.  Cellsize and cellseperation are set to best fit within
   ''' the defined rectangle.  Only the width or height needs to be passed in the function.  If both are passed, the 
   ''' rectangle size is determined by the value passed for the width.
   ''' </summary>
   ''' <param name="myWidth">Optimal width of the plate</param>
   ''' <param name="myHeight">Optimal height of the plate</param>
   ''' <remarks></remarks>
   Public Overridable Sub SetRackGraphicSize(Optional ByVal myWidth As Int32 = 0, Optional ByVal myHeight As Int32 = 0)
      'Good appearance results from a cellsize to seperation ratio of 4 to 1
      Dim BorderPixels As Int32
      Dim CellPixels As Double

      If myWidth > 0 Then 'Size determined by width of plate
         CellPixels = myWidth / ((Me.Columns * 1.25) + 2)
         If CellPixels > 10 Then
            BorderPixels = 10
         Else
            BorderPixels = CType(CellPixels, Int32)
         End If
         Me.Cellsize.X = CType((myWidth - (2 * BorderPixels)) / (Me.Columns * 1.25), Int32)
      Else    'Size determined by height of plate
         CellPixels = myHeight / ((Me.Rows * 1.25) + 2)
         If CellPixels > 10 Then
            BorderPixels = 10
         Else
            BorderPixels = CType(CellPixels, Int32)
         End If
         Me.Cellsize.X = CType((myHeight - (2 * BorderPixels)) / (Me.Rows * 1.25), Int32)
      End If

      Me.Cellsize.Y = Me.Cellsize.X
      Me.Cellseperation.X = CType(Me.Cellsize.X / 4, Int32)
      Me.Cellseperation.Y = Me.Cellseperation.X
      Me.MyRectangle.Width = (BorderPixels * 2) + (Me.Columns * Me.Cellsize.X) + ((Me.Columns - 1) * Me.Cellseperation.X)
      Me.MyRectangle.Height = (BorderPixels * 2) + (Me.Rows * Me.Cellsize.Y) + ((Me.Rows - 1) * Me.Cellseperation.Y)
   End Sub


   Private Sub frmBlankPicker_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp
      Dim ThisCell As Int32
      Dim StartCell As Int32
      Dim StopCell As Int32
      Dim Pass As Int32


      Try
         If IsInPlate(e) Then
            ThisCell = SelectCell(e)
            If ThisCell > 0 Then
               If BPData.Stage = Stg.PickBlank Then
                  If Cell(ThisCell) = 0 And BPData.Element <= 3 Then
                     Cell(ThisCell) = 1
                     If Me.txbBlank.Text = "" Then
                        Me.txbBlank.Text = ThisCell.ToString
                     Else
                        Me.txbBlank.Text += vbCrLf & ThisCell.ToString
                     End If
                     frmMain.Experiment(Batch).FilterBlankData(BPData.Group).FilterBlankLocations(BPData.Element) = ThisCell
                     BPData.Element += 1
                     CloseAllowed = False
                     BPData.BlanksInGroup += 1
                  Else
                     BPData.Element = 0
                     Me.txbBlank.Text = ""
                     BPData.BlanksInGroup = 0
                     For Pass = 0 To 3
                        ThisCell = frmMain.Experiment(Batch).FilterBlankData(BPData.Group).FilterBlankLocations(Pass)
                        Cell(ThisCell) = 0
                        frmMain.Experiment(Batch).FilterBlankData(BPData.Group).FilterBlankLocations(Pass) = 0
                        CloseAllowed = True
                     Next
                  End If
                  PenaltimateCellClicked = 0
               Else
                  If Cell(ThisCell) = 0 Then
                     If My.Computer.Keyboard.ShiftKeyDown = False Then
                        Cell(ThisCell) = 2
                        If Me.txbSet.Text = "" Then
                           Me.txbSet.Text = ThisCell.ToString
                        Else
                           Me.txbSet.Text += vbCrLf & ThisCell.ToString
                        End If
                        frmMain.Experiment(Batch).FilterBlankData(BPData.Group).BlankAppliesToTheseFilters(BPData.Element) = ThisCell
                        BPData.Element += 1
                        BPData.FiltersInGroup += 1
                        CloseAllowed = True
                     Else  'The shift key was held down.
                        'Turn all cells from this cell to the cell previously clicked to cells associated with the blank
                        'Decide if the just clicked cell is a higher or lower cell number than the preveous cell
                        If ThisCell > PenaltimateCellClicked Then
                           StartCell = PenaltimateCellClicked + 1
                           StopCell = ThisCell
                        Else
                           StartCell = ThisCell
                           StopCell = PenaltimateCellClicked - 1
                        End If
                        'First, make sure that all cells between the two clicked cells are not committed to a state
                        For Pass = StartCell To StopCell
                           If Cell(Pass) <> 0 Then
                              MessageBox.Show("Can not select the group specifed because some cells are already committed.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
                              Me.Invalidate()
                              Exit Sub
                           End If
                        Next

                        For Pass = StartCell To StopCell
                           Cell(Pass) = 2
                           If Me.txbSet.Text = "" Then
                              Me.txbSet.Text = Pass.ToString
                           Else
                              Me.txbSet.Text += vbCrLf & Pass.ToString
                           End If
                           frmMain.Experiment(Batch).FilterBlankData(BPData.Group).BlankAppliesToTheseFilters(BPData.Element) = Pass
                           BPData.Element += 1
                           BPData.FiltersInGroup += 1
                        Next
                        CloseAllowed = True
                     End If
                     PenaltimateCellClicked = ThisCell
                  ElseIf Cell(ThisCell) = 2 Then
                     Me.txbSet.Text = ""
                     BPData.Element = 0
                     For Pass = 0 To 95
                        ThisCell = frmMain.Experiment(Batch).FilterBlankData(BPData.Group).BlankAppliesToTheseFilters(Pass)
                        If ThisCell = 0 Then
                           Exit For
                        End If
                        Cell(ThisCell) = 0
                        frmMain.Experiment(Batch).FilterBlankData(BPData.Group).BlankAppliesToTheseFilters(Pass) = 0
                     Next
                     CloseAllowed = False
                  End If
               End If

               Me.Invalidate()
            End If
         End If
      Catch ex As Exception
         MessageBox.Show("Exception in BlankPicker 1. " & ex.Message, "Exception", MessageBoxButtons.OK)
      End Try
   End Sub


   Function IsInPlate(ByVal e As System.Windows.Forms.MouseEventArgs) As Boolean
      'This function will return true if the coordinates are contained in this rack
      'and false if not.   It is useful for processing mouse clicks.

      If MyRectangle.Contains(e.X, e.Y) Then
         IsInPlate = True
      Else
         IsInPlate = False
      End If
   End Function


   Function SelectCell(ByVal e As System.Windows.Forms.MouseEventArgs) As Int32
      'This function returns the cell number that was clicked on.
      'It takes into account the start position and the increment direction
      'Returns the cell number that was clicked on.  Returns 0 if click did not occur in rack
      Dim DeltaX As Int32 'Distance between cells
      Dim DeltaY As Int32
      Dim Width As Int32  'Total width of the plate
      Dim Height As Int32 'Total height of the plate
      Dim X_Offset As Int32   'X distance from edge of plate before cell 1 hitbox starts
      Dim Y_Offset As Int32   'Y distance from edge of plate before cell 1 hitbox starts
      Dim Row As Int32
      Dim Column As Int32
      Dim Cell As Int32
      Dim X As Int32
      Dim Y As Int32

      Try
         X = e.X
         Y = e.Y
         DeltaX = Cellsize.X + Cellseperation.X
         DeltaY = Cellsize.Y + Cellseperation.Y

         Width = MyRectangle.Width
         X_Offset = ((Width - (DeltaX * Columns) + Cellseperation.X) \ 2) - (Cellseperation.X \ 2)

         Height = MyRectangle.Height
         Y_Offset = ((Height - (DeltaY * Rows) + Cellseperation.Y) \ 2) - (Cellseperation.Y \ 2)

         If MyRectangle.Contains(e.X, e.Y) Then   'Are you in the plate?
            'Determine the row and column assuming the start cell is topleft, by row
            X = X - MyRectangle.X - X_Offset
            Column = X \ DeltaX + 1
            Y = Y - MyRectangle.Y - Y_Offset
            Row = Y \ DeltaY + 1
            Cell = ((Column - 1) * Me.Rows) + Row
         Else
            Cell = 0
         End If

         Return Cell
      Catch ex As Exception
         MessageBox.Show("Exception in BlankPicker 2. " & ex.Message, "Exception", MessageBoxButtons.OK)
         Return 1
      End Try
   End Function

   Private Sub btnRecord_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRecord.Click
      Try
         If BPData.Stage = Stg.PickBlank Then
            If BPData.BlanksInGroup = 0 Then
               Exit Sub
            End If
            BPData.BlanksInGroup = 0
            BPData.Stage = Stg.PickSet
            Me.lblInstruction.Text = "Click on the filters that these blanks apply to, then click the Record button." & vbCrLf & "To select a group, click on the first filter, then Shift Click on the last."
            BPData.Element = 0
            Me.btnRecord.Text = "Record Filters"
            Me.btnRecord.BackColor = Color.Cyan
         Else
            If BPData.FiltersInGroup = 0 Then
               Exit Sub
            End If
            BPData.FiltersInGroup = 0
            BPData.Stage = Stg.PickBlank
            Me.txbBlank.Text = ""
            Me.txbSet.Text = ""
            Me.btnRecord.Text = "Record Blanks"
            Me.btnRecord.BackColor = Color.Yellow
            Me.lblInstruction.Text = "Click on the first set of Filter Blanks, then click the Record button."
            BPData.Element = 0
            BPData.Group += 1
         End If
      Catch ex As Exception
         MessageBox.Show("Exception in BlankPicker 3. " & ex.Message, "Exception", MessageBoxButtons.OK)
      End Try
   End Sub


   Private Sub frmBlankPicker_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
      If CloseAllowed = False Then
         MessageBox.Show("You can not close the form until you've identified the filters that apply to the Filter Blank set.", "Filter Set", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         e.Cancel = True
      End If
   End Sub


   Private Sub mnuSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuSave.Click
      Me.Close()
   End Sub

   Private Sub mnuReset_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuReset.Click
      If MessageBox.Show("Are you sure you want to reset Blank Data?   This will delete all entered filter blank input.", "Delete Blank Data?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then
         ClearData()
      End If

      Me.Invalidate()
   End Sub


   Sub ClearData()
      Dim Pass As Int32
      Dim Filters As Int32

      Try
         Filters = frmMain.Experiment(Batch).TotalSamples
         For Pass = 1 To Filters
            Cell(Pass) = 0
         Next
         For Pass = Filters + 1 To 96
            Cell(Pass) = -1
         Next

         For Pass = 0 To 47
            For Innerpass As Int32 = 0 To 95
               frmMain.Experiment(Batch).FilterBlankData(Pass).BlankAppliesToTheseFilters(Innerpass) = 0
            Next
            For SecondInnerPass As Int32 = 0 To 3
               frmMain.Experiment(Batch).FilterBlankData(Pass).FilterBlankLocations(SecondInnerPass) = 0
            Next
         Next

         BPData.Stage = Stg.PickBlank
         BPData.Group = 0
         BPData.Element = 0
         BPData.BlanksInGroup = 0
         BPData.FiltersInGroup = 0
         CloseAllowed = True
      Catch ex As Exception
         MessageBox.Show("Exception in BlankPicker 4. " & ex.Message, "Exception", MessageBoxButtons.OK)
      End Try
   End Sub

End Class