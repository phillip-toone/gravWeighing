﻿Public Class frmOvens
   Dim AbortRequested As Boolean


   Private Sub frmOvens_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
      If frmMain.Experiment(0).Status = RunState.Active Then
         Me.gbxOven1.Enabled = False
      End If
      If frmMain.Experiment(1).Status = RunState.Active Then
         Me.gbxOven2.Enabled = False
      End If
      If frmMain.Experiment(2).Status = RunState.Active Then
         Me.gbxOven3.Enabled = False
      End If
      If frmMain.Experiment(3).Status = RunState.Active Then
         Me.gbxOven4.Enabled = False
      End If
      If frmMain.Experiment(4).Status = RunState.Active Then
         Me.gbxOven5.Enabled = False
      End If

      If Oven1.IsCloseDoorSensorInOverride = True Then
         ckbOverride1.Checked = True
      End If
      If Oven1.OvenActive = False Then
         lblDeactive1.Visible = True
      End If

      If Oven2.IsCloseDoorSensorInOverride = True Then
         ckbOverride2.Checked = True
      End If
      If Oven2.OvenActive = False Then
         lblDeactive2.Visible = True
      End If

      If Oven3.IsCloseDoorSensorInOverride = True Then
         ckbOverride3.Checked = True
      End If
      If Oven3.OvenActive = False Then
         lblDeactive3.Visible = True
      End If

      If Oven4.IsCloseDoorSensorInOverride = True Then
         ckbOverride4.Checked = True
      End If
      If Oven4.OvenActive = False Then
         lblDeactive4.Visible = True
      End If

      If Oven5.IsCloseDoorSensorInOverride = True Then
         ckbOverride5.Checked = True
      End If
      If Oven5.OvenActive = False Then
         lblDeactive5.Visible = True
      End If
      AbortRequested = False

      AddHandler ckbOverride1.CheckedChanged, AddressOf ckbOverride1_CheckedChanged
      AddHandler ckbOverride2.CheckedChanged, AddressOf ckbOverride2_CheckedChanged
      AddHandler ckbOverride3.CheckedChanged, AddressOf ckbOverride3_CheckedChanged
      AddHandler ckbOverride4.CheckedChanged, AddressOf ckbOverride4_CheckedChanged
      AddHandler ckbOverride5.CheckedChanged, AddressOf ckbOverride5_CheckedChanged
   End Sub



   Private Sub frmOvens_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
      For Pass As Int32 = 0 To 4
         OvenArray(Pass).CloseVentValve()
      Next
   End Sub


   Sub LockAll()
      'Lock all of the group boxes while a command is being executed so they can issue a second command
      Me.gbxOven1.Enabled = False
      Me.gbxOven2.Enabled = False
      Me.gbxOven3.Enabled = False
      Me.gbxOven4.Enabled = False
      Me.gbxOven5.Enabled = False
   End Sub


   Sub UnlockAll()
      'Unlocks all of the group boxes after a command is done
      Me.gbxOven1.Enabled = True
      Me.gbxOven2.Enabled = True
      Me.gbxOven3.Enabled = True
      Me.gbxOven4.Enabled = True
      Me.gbxOven5.Enabled = True
   End Sub


   Private Sub btnOpen1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOpen1.Click
      LockAll()
      Oven1.OpenDoor()
      UnlockAll()
   End Sub

   Private Sub btnOpen2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOpen2.Click
      LockAll()
      Oven2.OpenDoor()
      UnlockAll()
   End Sub

   Private Sub btnOpen3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOpen3.Click
      LockAll()
      Oven3.OpenDoor()
      UnlockAll()
   End Sub

   Private Sub btnOpen4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOpen4.Click
      LockAll()
      Oven4.OpenDoor()
      UnlockAll()
   End Sub

   Private Sub btnOpen5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOpen5.Click
      LockAll()
      Oven5.OpenDoor()
      UnlockAll()
   End Sub

   Private Sub btnClose1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose1.Click
      LockAll()
      Oven1.CloseDoor()
      UnlockAll()
   End Sub

   Private Sub btnClose2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose2.Click
      LockAll()
      Oven2.CloseDoor()
      UnlockAll()
   End Sub

   Private Sub btnClose3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose3.Click
      LockAll()
      Oven3.CloseDoor()
      UnlockAll()
   End Sub

   Private Sub btnClose4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose4.Click
      LockAll()
      Oven4.CloseDoor()
      UnlockAll()
   End Sub

   Private Sub btnClose5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose5.Click
      LockAll()
      Oven5.CloseDoor()
      UnlockAll()
   End Sub

   Private Sub btnRelease1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRelease1.Click
      If Me.btnRelease1.Text.Contains("Release") Then
         Oven1.SetPressure(780)
         Oven1.OpenVentValve()
         Me.btnRelease1.Text = "Close Vent Valve"
         Me.btnRelease1.BackColor = Color.Yellow
      Else
         Oven1.CloseVentValve()
         Me.btnRelease1.Text = "Release Pressure"
         Me.btnRelease1.BackColor = Color.LightGray
      End If
   End Sub

   Private Sub btnRelease2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRelease2.Click
      If Me.btnRelease2.Text.Contains("Release") Then
         Oven2.SetPressure(780)
         Oven2.OpenVentValve()
         Me.btnRelease2.Text = "Close Vent Valve"
         Me.btnRelease2.BackColor = Color.Yellow
      Else
         Oven2.CloseVentValve()
         Me.btnRelease2.Text = "Release Pressure"
         Me.btnRelease2.BackColor = Color.LightGray
      End If
   End Sub

   Private Sub btnRelease3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRelease3.Click
      If Me.btnRelease3.Text.Contains("Release") Then
         Oven3.SetPressure(780)
         Oven3.OpenVentValve()
         Me.btnRelease3.Text = "Close Vent Valve"
         Me.btnRelease3.BackColor = Color.Yellow
      Else
         Oven3.CloseVentValve()
         Me.btnRelease3.Text = "Release Pressure"
         Me.btnRelease3.BackColor = Color.LightGray
      End If
   End Sub

   Private Sub btnRelease4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRelease4.Click
      If Me.btnRelease4.Text.Contains("Release") Then
         Oven4.SetPressure(780)
         Oven4.OpenVentValve()
         Me.btnRelease4.Text = "Close Vent Valve"
         Me.btnRelease4.BackColor = Color.Yellow
      Else
         Oven4.CloseVentValve()
         Me.btnRelease4.Text = "Release Pressure"
         Me.btnRelease4.BackColor = Color.LightGray
      End If
   End Sub

   Private Sub btnRelease5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRelease5.Click
      If Me.btnRelease5.Text.Contains("Release") Then
         Oven5.SetPressure(780)
         Oven5.OpenVentValve()
         Me.btnRelease5.Text = "Close Vent Valve"
         Me.btnRelease5.BackColor = Color.Yellow
      Else
         Oven5.CloseVentValve()
         Me.btnRelease5.Text = "Release Pressure"
         Me.btnRelease5.BackColor = Color.LightGray
      End If
   End Sub

   Private Sub btnSetPress1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetPress1.Click
      LockAll()
      Oven1.CloseDoorSetVacuum(350)
      UnlockAll()
   End Sub

   Private Sub btnSetPress2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetPress2.Click
      LockAll()
      Oven2.CloseDoorSetVacuum(350)
      UnlockAll()
   End Sub

   Private Sub btnSetPress3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetPress3.Click
      LockAll()
      Oven3.CloseDoorSetVacuum(350)
      UnlockAll()
   End Sub

   Private Sub btnSetPress4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetPress4.Click
      LockAll()
      Oven4.CloseDoorSetVacuum(350)
      UnlockAll()
   End Sub

   Private Sub btnSetPress5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetPress5.Click
      LockAll()
      Oven5.CloseDoorSetVacuum(350)
      UnlockAll()
   End Sub

   Private Sub btnSetTemp1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetTemp1.Click
      If IsDouble(Me.txbTemperature1.Text, 0.1, 120.0) Then
         Oven1.SetTemperature(CType(Me.txbTemperature1.Text, Int32))
      Else
         Me.txbTemperature1.Text = ""
      End If
   End Sub

   Private Sub btnSetTemp2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetTemp2.Click
      If IsDouble(Me.txbTemperature2.Text, 0.1, 120.0) Then
         Oven2.SetTemperature(CType(Me.txbTemperature2.Text, Int32))
      Else
         Me.txbTemperature2.Text = ""
      End If
   End Sub

   Private Sub btnSetTemp3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetTemp3.Click
      If IsDouble(Me.txbTemperature3.Text, 0.1, 120.0) Then
         Oven3.SetTemperature(CType(Me.txbTemperature3.Text, Int32))
      Else
         Me.txbTemperature3.Text = ""
      End If
   End Sub

   Private Sub btnSetTemp4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetTemp4.Click
      If IsDouble(Me.txbTemperature4.Text, 0.1, 120.0) Then
         Oven4.SetTemperature(CType(Me.txbTemperature4.Text, Int32))
      Else
         Me.txbTemperature4.Text = ""
      End If
   End Sub

   Private Sub btnSetTemp5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetTemp5.Click
      If IsDouble(Me.txbTemperature5.Text, 0.1, 120.0) Then
         Oven5.SetTemperature(CType(Me.txbTemperature5.Text, Int32))
      Else
         Me.txbTemperature5.Text = ""
      End If
   End Sub

   Private Sub btnGo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGo.Click
      OvenDef.GotoSafeOpenDoorPosition(0)
   End Sub


   Private Sub ckbOverride1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
      Dim Input As String

      If ckbOverride1.Checked = True Then
         Input = InputBox("This is a protected option.  Enter the password.", "Disable Oven 1 Door Close Proximity Sensor")
         If Input.ToUpper = "PASSWORD" Then
            Oven1.IsCloseDoorSensorInOverride = True
         Else
            Oven1.IsCloseDoorSensorInOverride = False
         End If
      Else
         Oven1.IsCloseDoorSensorInOverride = False
      End If
      frmMain.SaveSpecialSettings()
   End Sub

   Private Sub ckbOverride2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
      Dim Input As String

      If ckbOverride2.Checked = True Then
         Input = InputBox("This is a protected option.  Enter the password.", "Disable Oven 2 Door Close Proximity Sensor")
         If Input.ToUpper = "PASSWORD" Then
            Oven2.IsCloseDoorSensorInOverride = True
         Else
            Oven2.IsCloseDoorSensorInOverride = False
         End If
      Else
         Oven2.IsCloseDoorSensorInOverride = False
      End If
      frmMain.SaveSpecialSettings()
   End Sub

   Private Sub ckbOverride3_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
      Dim Input As String

      If ckbOverride3.Checked = True Then
         Input = InputBox("This is a protected option.  Enter the password.", "Disable Oven 3 Door Close Proximity Sensor")
         If Input.ToUpper = "PASSWORD" Then
            Oven3.IsCloseDoorSensorInOverride = True
         Else
            Oven3.IsCloseDoorSensorInOverride = False
         End If
      Else
         Oven3.IsCloseDoorSensorInOverride = False
      End If
      frmMain.SaveSpecialSettings()
   End Sub

   Private Sub ckbOverride4_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
      Dim Input As String

      If ckbOverride4.Checked = True Then
         Input = InputBox("This is a protected option.  Enter the password.", "Disable Oven 4 Door Close Proximity Sensor")
         If Input.ToUpper = "PASSWORD" Then
            Oven4.IsCloseDoorSensorInOverride = True
         Else
            Oven4.IsCloseDoorSensorInOverride = False
         End If
      Else
         Oven4.IsCloseDoorSensorInOverride = False
      End If
      frmMain.SaveSpecialSettings()
   End Sub

   Private Sub ckbOverride5_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
      Dim Input As String

      If ckbOverride5.Checked = True Then
         Input = InputBox("This is a protected option.  Enter the password.", "Disable Oven 5 Door Close Proximity Sensor")
         If Input.ToUpper = "PASSWORD" Then
            Oven5.IsCloseDoorSensorInOverride = True
         Else
            Oven5.IsCloseDoorSensorInOverride = False
         End If
      Else
         Oven5.IsCloseDoorSensorInOverride = False
      End If
      frmMain.SaveSpecialSettings()
   End Sub

   Private Sub btnTest4CornersOven1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTest4CornersOven1.Click
      'First put the filters in the oven
      Try
         MessageBox.Show("Place filters on the 4 corners of Rack 1, then click OK to start the test", "Calibration Test", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         DisableGroupBoxes()
         OvenDef.GotoSafeOpenDoorPosition(1)
         Oven1.OpenDoor()
         Carosel.Home()
         Carosel.GetFilter(1, 1)
         OvenDef.PutFilter(1, 1)
         Carosel.GetFilter(1, 12)
         OvenDef.PutFilter(1, 12)
         Carosel.GetFilter(1, 85)
         OvenDef.PutFilter(1, 85)
         Carosel.GetFilter(1, 96)
         OvenDef.PutFilter(1, 96)

         'Now put the filters back on the carosel
         Carosel.Home()
         OvenDef.GetFilter(1, 1)
         Carosel.PutFilter(1, 1)
         OvenDef.GetFilter(1, 12)
         Carosel.PutFilter(1, 12)
         OvenDef.GetFilter(1, 85)
         Carosel.PutFilter(1, 85)
         OvenDef.GetFilter(1, 96)
         Carosel.PutFilter(1, 96)
         OvenDef.GotoSafeOpenDoorPosition(1)
         Oven1.CloseDoor()
         EnableGroupBoxes()
      Catch ex As Exception
         EnableGroupBoxes()
         AbortRequested = False
      End Try
   End Sub

   Private Sub btnTest4CornersOven2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTest4CornersOven2.Click
      'First put the filters in the oven
      Try
         MessageBox.Show("Place filters on the 4 corners of Rack 2, then click OK to start the test", "Calibration Test", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         DisableGroupBoxes()
         OvenDef.GotoSafeOpenDoorPosition(2)
         Oven2.OpenDoor()
         Carosel.Home()
         Carosel.GetFilter(2, 1)
         OvenDef.PutFilter(2, 1)
         Carosel.GetFilter(2, 12)
         OvenDef.PutFilter(2, 12)
         Carosel.GetFilter(2, 85)
         OvenDef.PutFilter(2, 85)
         Carosel.GetFilter(2, 96)
         OvenDef.PutFilter(2, 96)

         'Now put the filters back on the carosel
         Carosel.Home()
         OvenDef.GetFilter(2, 1)
         Carosel.PutFilter(2, 1)
         OvenDef.GetFilter(2, 12)
         Carosel.PutFilter(2, 12)
         OvenDef.GetFilter(2, 85)
         Carosel.PutFilter(2, 85)
         OvenDef.GetFilter(2, 96)
         Carosel.PutFilter(2, 96)
         OvenDef.GotoSafeOpenDoorPosition(2)
         Oven2.CloseDoor()
         EnableGroupBoxes()
      Catch ex As Exception
         EnableGroupBoxes()
         AbortRequested = False
      End Try
   End Sub

   Private Sub btnTest4CornersOven3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTest4CornersOven3.Click
      'First put the filters in the oven
      Try
         MessageBox.Show("Place filters on the 4 corners of Rack 3, then click OK to start the test", "Calibration Test", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         DisableGroupBoxes()
         OvenDef.GotoSafeOpenDoorPosition(3)
         Oven3.OpenDoor()
         Carosel.Home()
         Carosel.GetFilter(3, 1)
         OvenDef.PutFilter(3, 1)
         Carosel.GetFilter(3, 12)
         OvenDef.PutFilter(3, 12)
         Carosel.GetFilter(3, 85)
         OvenDef.PutFilter(3, 85)
         Carosel.GetFilter(3, 96)
         OvenDef.PutFilter(3, 96)

         'Now put the filters back on the carosel
         Carosel.Home()
         OvenDef.GetFilter(3, 1)
         Carosel.PutFilter(3, 1)
         OvenDef.GetFilter(3, 12)
         Carosel.PutFilter(3, 12)
         OvenDef.GetFilter(3, 85)
         Carosel.PutFilter(3, 85)
         OvenDef.GetFilter(3, 96)
         Carosel.PutFilter(3, 96)
         OvenDef.GotoSafeOpenDoorPosition(3)
         Oven3.CloseDoor()
         EnableGroupBoxes()
      Catch ex As Exception
         EnableGroupBoxes()
         AbortRequested = False
      End Try
   End Sub

   Private Sub btnTest4CornersOven4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTest4CornersOven4.Click
      'First put the filters in the oven
      Try
         MessageBox.Show("Place filters on the 4 corners of Rack 4, then click OK to start the test", "Calibration Test", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         DisableGroupBoxes()
         OvenDef.GotoSafeOpenDoorPosition(4)
         Oven4.OpenDoor()
         Carosel.Home()
         Carosel.GetFilter(4, 1)
         OvenDef.PutFilter(4, 1)
         Carosel.GetFilter(4, 12)
         OvenDef.PutFilter(4, 12)
         Carosel.GetFilter(4, 85)
         OvenDef.PutFilter(4, 85)
         Carosel.GetFilter(4, 96)
         OvenDef.PutFilter(4, 96)

         'Now put the filters back on the carosel
         Carosel.Home()
         OvenDef.GetFilter(4, 1)
         Carosel.PutFilter(4, 1)
         OvenDef.GetFilter(4, 12)
         Carosel.PutFilter(4, 12)
         OvenDef.GetFilter(4, 85)
         Carosel.PutFilter(4, 85)
         OvenDef.GetFilter(4, 96)
         Carosel.PutFilter(4, 96)
         OvenDef.GotoSafeOpenDoorPosition(4)
         Oven4.CloseDoor()
         EnableGroupBoxes()
      Catch ex As Exception
         EnableGroupBoxes()
         AbortRequested = False
      End Try
   End Sub

   Private Sub btnTest4CornersOven5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTest4CornersOven5.Click
      'First put the filters in the oven
      Try
         MessageBox.Show("Place filters on the 4 corners of Rack 5, then click OK to start the test", "Calibration Test", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         DisableGroupBoxes()
         OvenDef.GotoSafeOpenDoorPosition(5)
         Oven5.OpenDoor()
         Carosel.Home()
         Carosel.GetFilter(5, 1)
         OvenDef.PutFilter(5, 1)
         Carosel.GetFilter(5, 12)
         OvenDef.PutFilter(5, 12)
         Carosel.GetFilter(5, 85)
         OvenDef.PutFilter(5, 85)
         Carosel.GetFilter(5, 96)
         OvenDef.PutFilter(5, 96)

         'Now put the filters back on the carosel
         Carosel.Home()
         OvenDef.GetFilter(5, 1)
         Carosel.PutFilter(5, 1)
         OvenDef.GetFilter(5, 12)
         Carosel.PutFilter(5, 12)
         OvenDef.GetFilter(5, 85)
         Carosel.PutFilter(5, 85)
         OvenDef.GetFilter(5, 96)
         Carosel.PutFilter(5, 96)
         OvenDef.GotoSafeOpenDoorPosition(5)
         Oven5.CloseDoor()
         EnableGroupBoxes()
      Catch ex As Exception
         EnableGroupBoxes()
         AbortRequested = False
      End Try
   End Sub

   Private Sub btnTestAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTestAll.Click
      'First put the filters in the oven
      Try
         MessageBox.Show("Place filters on the 4 corners of Rack 1.  Make sure all other racks and ovens are empty, then click OK to start the test", "Calibration Test", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         DisableGroupBoxes()
         OvenDef.GotoSafeOpenDoorPosition(1)
         Oven1.OpenDoor()
         Carosel.Home()
         Carosel.GetFilter(1, 1)
         OvenDef.PutFilter(1, 1)
         Carosel.GetFilter(1, 12)
         OvenDef.PutFilter(1, 12)
         Carosel.GetFilter(1, 85)
         OvenDef.PutFilter(1, 85)
         Carosel.GetFilter(1, 96)
         OvenDef.PutFilter(1, 96)

         'Now put the filters back on rack 2 the carosel
         OvenDef.GetFilter(1, 1)
         Carosel.PutFilter(2, 1)
         OvenDef.GetFilter(1, 12)
         Carosel.PutFilter(2, 12)
         OvenDef.GetFilter(1, 85)
         Carosel.PutFilter(2, 85)
         OvenDef.GetFilter(1, 96)
         Carosel.PutFilter(2, 96)
         OvenDef.GotoSafeOpenDoorPosition(1)
         Oven1.CloseDoor()

         Oven2.OpenDoor()
         Carosel.Home()
         Carosel.GetFilter(2, 1)
         OvenDef.PutFilter(2, 1)
         Carosel.GetFilter(2, 12)
         OvenDef.PutFilter(2, 12)
         Carosel.GetFilter(2, 85)
         OvenDef.PutFilter(2, 85)
         Carosel.GetFilter(2, 96)
         OvenDef.PutFilter(2, 96)

         'Now put the filters back on rack 2 the carosel
         OvenDef.GetFilter(2, 1)
         Carosel.PutFilter(3, 1)
         OvenDef.GetFilter(2, 12)
         Carosel.PutFilter(3, 12)
         OvenDef.GetFilter(2, 85)
         Carosel.PutFilter(3, 85)
         OvenDef.GetFilter(2, 96)
         Carosel.PutFilter(3, 96)
         OvenDef.GotoSafeOpenDoorPosition(2)
         Oven2.CloseDoor()

         Oven3.OpenDoor()
         Carosel.Home()
         Carosel.GetFilter(3, 1)
         OvenDef.PutFilter(3, 1)
         Carosel.GetFilter(3, 12)
         OvenDef.PutFilter(3, 12)
         Carosel.GetFilter(3, 85)
         OvenDef.PutFilter(3, 85)
         Carosel.GetFilter(3, 96)
         OvenDef.PutFilter(3, 96)

         'Now put the filters back on rack 2 the carosel
         OvenDef.GetFilter(3, 1)
         Carosel.PutFilter(4, 1)
         OvenDef.GetFilter(3, 12)
         Carosel.PutFilter(4, 12)
         OvenDef.GetFilter(3, 85)
         Carosel.PutFilter(4, 85)
         OvenDef.GetFilter(3, 96)
         Carosel.PutFilter(4, 96)
         OvenDef.GotoSafeOpenDoorPosition(3)
         Oven3.CloseDoor()

         Oven4.OpenDoor()
         Carosel.Home()
         Carosel.GetFilter(4, 1)
         OvenDef.PutFilter(4, 1)
         Carosel.GetFilter(4, 12)
         OvenDef.PutFilter(4, 12)
         Carosel.GetFilter(4, 85)
         OvenDef.PutFilter(4, 85)
         Carosel.GetFilter(4, 96)
         OvenDef.PutFilter(4, 96)

         'Now put the filters back on rack 2 the carosel
         OvenDef.GetFilter(4, 1)
         Carosel.PutFilter(5, 1)
         OvenDef.GetFilter(4, 12)
         Carosel.PutFilter(5, 12)
         OvenDef.GetFilter(4, 85)
         Carosel.PutFilter(5, 85)
         OvenDef.GetFilter(4, 96)
         Carosel.PutFilter(5, 96)
         OvenDef.GotoSafeOpenDoorPosition(4)
         Oven4.CloseDoor()

         Oven5.OpenDoor()
         Carosel.Home()
         Carosel.GetFilter(5, 1)
         OvenDef.PutFilter(5, 1)
         Carosel.GetFilter(5, 12)
         OvenDef.PutFilter(5, 12)
         Carosel.GetFilter(5, 85)
         OvenDef.PutFilter(5, 85)
         Carosel.GetFilter(5, 96)
         OvenDef.PutFilter(5, 96)

         'Now put the filters back on rack 2 the carosel
         OvenDef.GetFilter(5, 1)
         Carosel.PutFilter(1, 1)
         OvenDef.GetFilter(5, 12)
         Carosel.PutFilter(1, 12)
         OvenDef.GetFilter(5, 85)
         Carosel.PutFilter(1, 85)
         OvenDef.GetFilter(5, 96)
         Carosel.PutFilter(1, 96)
         OvenDef.GotoSafeOpenDoorPosition(5)
         Oven5.CloseDoor()
         EnableGroupBoxes()
      Catch ex As Exception
         EnableGroupBoxes()
         AbortRequested = False
      End Try
   End Sub

   Sub DisableGroupBoxes()
      gbxOven1.Enabled = False
      gbxOven2.Enabled = False
      gbxOven3.Enabled = False
      gbxOven4.Enabled = False
      gbxOven5.Enabled = False
      gbxBalance.Enabled = False
      btnGo.Enabled = False
      btnAbort.Visible = True
   End Sub

   Sub EnableGroupBoxes()
      gbxOven1.Enabled = True
      gbxOven2.Enabled = True
      gbxOven3.Enabled = True
      gbxOven4.Enabled = True
      gbxOven5.Enabled = True
      gbxBalance.Enabled = True
      btnGo.Enabled = True
      btnAbort.Visible = False
   End Sub

   Private Sub btnAbort_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAbort.Click
      AbortRequested = True
   End Sub

   Private Sub btnBalance_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBalance.Click
      MessageBox.Show("Place a filter in position 1 of carosel rack 1, then click OK to start.", "Balance Position Test", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
      Carosel.Home()
      Do
         Carosel.GetFilter(1, 1)
         Balance.DropOffFilter()
         MessageBox.Show("Click OK to pick up and return the filter.", "Balance Position Test", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
         Balance.PickUpFilter()
         Carosel.PutFilter(1, 1)
         If MessageBox.Show("Do you want to run the test again?", "Balance Test", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) = Windows.Forms.DialogResult.No Then
            Exit Do
         End If
      Loop
      OvenDef.GotoSafeOpenDoorPosition(1)
   End Sub

   Private Sub btnOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOpen.Click
      Gripper.Open()
   End Sub

   Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
      Gripper.Close()
   End Sub

   Private Sub btnReadSensor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReadSensor.Click
      Me.txbSensor.Text = ""
      Application.DoEvents()
      Me.txbSensor.Text = Gripper.ReadSensorAverage(32).ToString
   End Sub

   Private Sub DeactivateOvenToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DeactivateOvenToolStripMenuItem.Click
      Oven1.OvenActive = False
      frmMain.SaveSpecialSettings()
      lblDeactive1.Visible = True
   End Sub

   Private Sub ActivateOvenToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ActivateOvenToolStripMenuItem.Click
      Oven1.OvenActive = True
      frmMain.SaveSpecialSettings()
      lblDeactive1.Visible = False
   End Sub

   Private Sub DeActivateOvenToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DeActivateOvenToolStripMenuItem1.Click
      Oven2.OvenActive = False
      frmMain.SaveSpecialSettings()
      lblDeactive2.Visible = True
   End Sub

   Private Sub ActivateOvenToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ActivateOvenToolStripMenuItem1.Click
      Oven2.OvenActive = True
      frmMain.SaveSpecialSettings()
      gbxOven2.Enabled = True
      lblDeactive2.Visible = False
   End Sub

   Private Sub DeactivateOvenToolStripMenuItem2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DeactivateOvenToolStripMenuItem2.Click
      Oven3.OvenActive = False
      frmMain.SaveSpecialSettings()
      lblDeactive3.Visible = True
   End Sub

   Private Sub ActivateOveToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ActivateOveToolStripMenuItem.Click
      Oven3.OvenActive = True
      frmMain.SaveSpecialSettings()
      lblDeactive3.Visible = False
   End Sub

   Private Sub DeactivateOvenToolStripMenuItem3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DeactivateOvenToolStripMenuItem3.Click
      Oven4.OvenActive = False
      frmMain.SaveSpecialSettings()
      lblDeactive4.Visible = True
   End Sub

   Private Sub ActivateOvenToolStripMenuItem2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ActivateOvenToolStripMenuItem2.Click
      Oven4.OvenActive = True
      frmMain.SaveSpecialSettings()
      lblDeactive4.Visible = False
   End Sub

   Private Sub DeactivateOvenToolStripMenuItem4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DeactivateOvenToolStripMenuItem4.Click
      Oven5.OvenActive = False
      frmMain.SaveSpecialSettings()
      lblDeactive5.Visible = True
   End Sub

   Private Sub ActivateOvenToolStripMenuItem3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ActivateOvenToolStripMenuItem3.Click
      Oven5.OvenActive = True
      frmMain.SaveSpecialSettings()
      lblDeactive5.Visible = False
   End Sub
End Class