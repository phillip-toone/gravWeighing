Public Class frmExperimentBuilder
   Dim ThisExperiment As Int32
   Dim Rack As New RackDef("MyRack")
   Dim SelectCellEnabled As Boolean

   Sub New(ByVal exp As Int32)

      ' This call is required by the Windows Form Designer.
      InitializeComponent()

      ' Add any initialization after the InitializeComponent() call.
      ThisExperiment = exp
   End Sub


   Private Sub frmExperimentBuilder_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
      Dim Pass As Int32

      Me.dgvExperiment.Rows.Add(15)
      Me.dgvExperiment.RowHeadersWidth = 60
      For Pass = 0 To 14
         Me.dgvExperiment.Rows(Pass).HeaderCell.Value = (Pass + 1).ToString
         CType(Me.dgvExperiment.Rows(Pass).Cells(0), DataGridViewComboBoxCell).Value = "None"
      Next
      DGVSupport.SizeTable(Me.dgvExperiment)
      Me.rdoTare.Checked = True
      Application.DoEvents()
      SelectCellEnabled = False

      AddHandler dgvExperiment.CellValidating, AddressOf dgvExperiment_CellValidating
      AddHandler dgvExperiment.CellValueChanged, AddressOf dgvExperiment_CellValueChanged
   End Sub

   Private Sub frmExperimentBuilder_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
      Rack = Nothing
   End Sub


   Private Sub rdoTare_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rdoTare.CheckedChanged
      If Me.rdoTare.Checked = True Then
         frmMain.Experiment(ThisExperiment).ExperimentType = ExpType.TareExperiment
         lblNotice.Visible = True
      End If
   End Sub

   Private Sub rdoWeight_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rdoWeight.CheckedChanged
      If rdoWeight.Checked = True Then
         frmMain.Experiment(ThisExperiment).ExperimentType = ExpType.WeighingExperiment
         lblNotice.Visible = True
      End If
   End Sub

   Private Sub rdoUserDefined_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rdoUserDefined.CheckedChanged
      If rdoUserDefined.Checked = True Then
         frmMain.Experiment(ThisExperiment).ExperimentType = ExpType.UserDefined
         lblNotice.Visible = False
         Me.btnSelect.Visible = True
      Else
         Me.btnSelect.Visible = False
         Me.Invalidate()
      End If
   End Sub

   Private Sub dgvExperiment_CellValidating(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellValidatingEventArgs)
      If e.ColumnIndex = 1 Then   'This is the time
         If Not Me.dgvExperiment.Rows(e.RowIndex).Cells(1).Value Is Nothing Then
            If IsDouble(Me.dgvExperiment.Rows(e.RowIndex).Cells(1).Value.ToString, 0.0) = False Then
               My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Exclamation)
               Me.dgvExperiment.Rows(e.RowIndex).Cells(1).Value = ""
            End If
         End If
      ElseIf e.ColumnIndex = 2 Then   'This is the temperature
         If Not Me.dgvExperiment.Rows(e.RowIndex).Cells(2).Value Is Nothing Then
            If IsDouble(Me.dgvExperiment.Rows(e.RowIndex).Cells(2).Value.ToString, 0.0, 100.0) = False Then
               My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Exclamation)
               Me.dgvExperiment.Rows(e.RowIndex).Cells(2).Value = ""
            End If
         End If
      End If
   End Sub

   Private Sub dgvExperiment_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs)

      If e.ColumnIndex = 0 Then
         RemoveHandler dgvExperiment.CellValueChanged, AddressOf dgvExperiment_CellValueChanged
         Me.dgvExperiment.Rows(e.RowIndex).Cells(2).Value = ""
         If Me.dgvExperiment.Rows(e.RowIndex).Cells(0).Value.ToString = "None" Then
            Me.dgvExperiment.Rows(e.RowIndex).Cells(1).Value = ""
            Me.dgvExperiment.Rows(e.RowIndex).Cells(2).Value = ""
         ElseIf Me.dgvExperiment.Rows(e.RowIndex).Cells(0).Value.ToString.Contains("Take") Then
            Me.dgvExperiment.Rows(e.RowIndex).Cells(1).Value = "0.0"
            CType(Me.dgvExperiment.Rows(e.RowIndex + 1).Cells(0), DataGridViewComboBoxCell).Value = "Remove Filter From Oven"
            Me.dgvExperiment.Rows(e.RowIndex + 1).Cells(1).Value = "0.0"
            Me.dgvExperiment.Rows(e.RowIndex + 1).Cells(2).Value = "- - -"
         ElseIf Me.dgvExperiment.Rows(e.RowIndex).Cells(0).Value.ToString.Contains("to Oven") Then
            Me.dgvExperiment.Rows(e.RowIndex).Cells(1).Value = "0.0"
            CType(Me.dgvExperiment.Rows(e.RowIndex + 1).Cells(0), DataGridViewComboBoxCell).Value = "Remove Filter From Oven"
            Me.dgvExperiment.Rows(e.RowIndex + 1).Cells(1).Value = "0.0"
            Me.dgvExperiment.Rows(e.RowIndex + 1).Cells(2).Value = "- - -"
         ElseIf Me.dgvExperiment.Rows(e.RowIndex).Cells(0).Value.ToString.Contains("From Oven") Then
            Me.dgvExperiment.Rows(e.RowIndex).Cells(1).Value = "0.0"
         ElseIf Me.dgvExperiment.Rows(e.RowIndex).Cells(0).Value.ToString.Contains("to Carousel") Then
            Me.dgvExperiment.Rows(e.RowIndex).Cells(1).Value = "0.0"
            Me.dgvExperiment.Rows(e.RowIndex).Cells(2).Value = "- - -"
         ElseIf Me.dgvExperiment.Rows(e.RowIndex).Cells(0).Value.ToString.Contains("Delay") Then
            Me.dgvExperiment.Rows(e.RowIndex).Cells(1).Value = "0.0"
            Me.dgvExperiment.Rows(e.RowIndex).Cells(2).Value = "- - -"
         End If
         AddHandler dgvExperiment.CellValueChanged, AddressOf dgvExperiment_CellValueChanged
      End If
   End Sub

   Private Sub btnRun_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRun.Click
      Dim Pass As Int32
      Dim Hours As Double
      Dim Temperature As Double
      Dim ExperimentIncomplete As Boolean = False
      Dim Weighsteps As Int32


      If Me.rdoTare.Checked = True Then
         If MessageBox.Show("The experiment you've selected is Tare Experiment.    Is this the experiment you want to run?", "Confirm Experiment Selection", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.No Then
            Exit Sub
         End If
      ElseIf Me.rdoWeight.Checked = True Then
         If MessageBox.Show("The experiment you've selected is the Weight Experiment.    Is this the experiment you want to run?", "Confirm Experiment Selection", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.No Then
            Exit Sub
         End If
      ElseIf Me.rdoUserDefined.Checked = True Then
         If MessageBox.Show("The experiment you've selected is a User Defined Experiment.    Is this the experiment you want to run?", "Confirm Experiment Selection", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.No Then
            Exit Sub
         End If
      End If
      
      Try
         Weighsteps = 0
         With frmMain.Experiment(ThisExperiment)
            For Pass = 0 To 14
               If Me.dgvExperiment.Rows(Pass).Cells(0).Value.ToString.Contains("None") Then  'First line with None, ends the experiment
                  .RunSteps(Pass).StepType = Tasks.CloseExperiment
                  .RunSteps(Pass).StepDuration = TimeSpan.Zero
                  If Pass = 0 Then
                     ExperimentIncomplete = True
                  End If
                  Exit For
               ElseIf Me.dgvExperiment.Rows(Pass).Cells(0).Value.ToString.Contains("Take") Then 'Take filters to Oven
                  .RunSteps(Pass).StepType = Tasks.TakeToOven
                  If CellValueOK(Me.dgvExperiment.Rows(Pass).Cells(1).Value) = False Then
                     ExperimentIncomplete = True
                     Exit For
                  End If
                  Hours = CType(Me.dgvExperiment.Rows(Pass).Cells(1).Value, Double)
                  .RunSteps(Pass).StepDuration = TimeSpan.FromHours(Hours)
                  If CellValueOK(Me.dgvExperiment.Rows(Pass).Cells(2).Value) = False Then
                     ExperimentIncomplete = True
                     Exit For
                  End If
                  Temperature = CType(Me.dgvExperiment.Rows(Pass).Cells(2).Value, Double)
                  .RunSteps(Pass).Temperature = Temperature
               ElseIf Me.dgvExperiment.Rows(Pass).Cells(0).Value.ToString.Contains("Remove") Then  'Remove the filters from the oven
                  .RunSteps(Pass).StepType = Tasks.TakeToCarosel
                  If CellValueOK(Me.dgvExperiment.Rows(Pass).Cells(1).Value) = False Then
                     ExperimentIncomplete = True
                     Exit For
                  End If
                  Hours = CType(Me.dgvExperiment.Rows(Pass).Cells(1).Value, Double)
                  .RunSteps(Pass).StepDuration = TimeSpan.FromHours(Hours) 'Weigh the filters, then take them to the oven
               ElseIf Me.dgvExperiment.Rows(Pass).Cells(0).Value.ToString.Contains("to Oven") Then
                  .RunSteps(Pass).StepType = Tasks.WeighSamplesTakeToOven
                  Weighsteps += 1
                  If CellValueOK(Me.dgvExperiment.Rows(Pass).Cells(1).Value) = False Then
                     ExperimentIncomplete = True
                     Exit For
                  End If
                  Hours = CType(Me.dgvExperiment.Rows(Pass).Cells(1).Value, Double)
                  .RunSteps(Pass).StepDuration = TimeSpan.FromHours(Hours)
                  If CellValueOK(Me.dgvExperiment.Rows(Pass).Cells(2).Value) = False Then
                     ExperimentIncomplete = True
                     Exit For
                  End If
                  Temperature = CType(Me.dgvExperiment.Rows(Pass).Cells(2).Value, Double)
                  .RunSteps(Pass).Temperature = Temperature 'Weigh the filters, then take them to the carousel
               ElseIf Me.dgvExperiment.Rows(Pass).Cells(0).Value.ToString.Contains("to Carousel") Then
                  .RunSteps(Pass).StepType = Tasks.WeighSamplesTakeToCarosel
                  Weighsteps += 1
                  If CellValueOK(Me.dgvExperiment.Rows(Pass).Cells(1).Value) = False Then
                     ExperimentIncomplete = True
                     Exit For
                  End If
                  Hours = CType(Me.dgvExperiment.Rows(Pass).Cells(1).Value, Double)
                  .RunSteps(Pass).StepDuration = TimeSpan.FromHours(Hours)
               Else 'Delay
                  .RunSteps(Pass).StepType = Tasks.Delay
                  If CellValueOK(Me.dgvExperiment.Rows(Pass).Cells(1).Value) = False Then
                     ExperimentIncomplete = True
                     Exit For
                  End If
                  Hours = CType(Me.dgvExperiment.Rows(Pass).Cells(1).Value, Double)
                  .RunSteps(Pass).StepDuration = TimeSpan.FromHours(Hours)
               End If
            Next
            .RunSteps(15).StepType = Tasks.CloseExperiment
            .RunSteps(15).StepDuration = TimeSpan.Zero
         End With

      Catch ex As Exception
         ExperimentIncomplete = True
      End Try

      If ExperimentIncomplete = True Then
         MessageBox.Show("The information needed to run this experiment is not complete.  Fill in all the required fields in the table.", "Incomplete Experiment", MessageBoxButtons.OK, MessageBoxIcon.Asterisk)
         Exit Sub
      End If

      If Me.rdoUserDefined.Checked = True Then
         If frmMain.Experiment(ThisExperiment).Filter(0).Status = RunState.Active Then  'The user elected to select specific filters
            Dim ClickedOne As Boolean = False
            For Pass = 1 To frmMain.Experiment(ThisExperiment).TotalSamples  'Make sure that the user has clicked at least one cell.
               If frmMain.Experiment(ThisExperiment).Filter(Pass).Status = RunState.Active Then
                  ClickedOne = True
                  Exit For
               End If
            Next
            If ClickedOne = False Then
               MessageBox.Show("You must select at lease one filter to run the experiment on.", "Filter Selection", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
               Exit Sub
            End If
         End If
      ElseIf Weighsteps < 4 Then
         MessageBox.Show("A custom experiment must have at least 4 weighing steps defined.  The program will only use the number of weighing steps necessary to complete the experiment, but at lease 4 steps must be defined.", "Weighing Steps Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         Exit Sub
      End If

      If Me.rdoWeight.Checked = True Then
         If MessageBox.Show("Do you want to input Filter Blank data?", "Filter Blank Input", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) = Windows.Forms.DialogResult.Yes Then
            Dim FB As New frmBlankPicker(0)
            FB.ShowDialog()
            FB.Dispose()
         End If
      End If
      Me.Close()
   End Sub


   Function CellValueOK(ByVal value As Object) As Boolean
      Dim GoodData As Boolean

      Try
         GoodData = False
         If Not value Is Nothing Then
            If Not CType(value, String) = "" Then
               If Not CType(value, Double) = 0.0 Then
                  GoodData = True
               End If
            End If
         End If
      Catch ex As Exception
         GoodData = False
      End Try

      Return GoodData
   End Function


   Private Sub btnSelect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSelect.Click
      'Create a graphic rack
      Dim Pass As Int32

      For Pass = 1 To 96
         frmMain.Experiment(ThisExperiment).Filter(Pass).Status = RunState.Done
      Next
      'Filter position 0 is not used in the experiment.  It is used here as a flag that you only want to run specific filters
      'and that you have manually set the filter state.
      frmMain.Experiment(ThisExperiment).Filter(0).Status = RunState.Active

      Rack.Rows = 12
      Rack.Columns = 8
      Rack.IncrementDirection = RackDef.IncDir.ByColumn
      Rack.StartPosition = RackDef.FirstVialLocation.TopLeft
      Rack.VialPositions = 96
      Rack.SetRackGraphicSize(300)
      Rack.SetRackGraphicOrigin(670, 90)
      Rack.InitializeGraphics()
      Rack.SetAllCellsToExclude()
      Rack.PaintPlate(Me)
      Me.btnSelect.Visible = False
      Me.lblSelect.Visible = True
      SelectCellEnabled = True
   End Sub

   Private Sub frmExperimentBuilder_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp
      If SelectCellEnabled = True Then
         If Rack.IsInPlate(e) Then
            Dim Cell As Int32

            Cell = Rack.SelectCell(e)
            If Cell <= frmMain.Experiment(ThisExperiment).TotalSamples Then
               If Rack.Cell(Cell).CellState = RackDef.CellCondition.Exclude Then
                  Rack.Cell(Cell).CellState = RackDef.CellCondition.Ready
                  frmMain.Experiment(ThisExperiment).Filter(Cell).Status = RunState.Active
               Else
                  Rack.Cell(Cell).CellState = RackDef.CellCondition.Exclude
                  frmMain.Experiment(ThisExperiment).Filter(Cell).Status = RunState.Done
               End If
            End If
            Rack.PaintPlate(Me)
         End If
      End If
   End Sub
End Class