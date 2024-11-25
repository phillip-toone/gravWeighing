


Public Class frmJkemDriverConfiguration
   Dim DataChanged As Boolean
   Dim Address As Int32 = 1  'There is only one J-KEM motor driver in this system, and it has an address of 1
   Dim MyDriver As JkemMotorDef
   Dim ArmPoint As Point4D



   Private Sub frmJkemDriverConfiguration_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
      MyDriver = Carosel.MyDriver
      Me.txbRunCurrent.Text = MyDriver.SendLiteral(":" & Address.ToString & "RC")
      Me.txbHoldCurrent.Text = MyDriver.SendLiteral(":" & Address.ToString & "HC")
      Me.txbHoldTime.Text = MyDriver.SendLiteral(":" & Address.ToString & "HT")
      Me.txbAcceleration.Text = MyDriver.SendLiteral(":" & Address.ToString & "SA")
      Me.txbMinSpeed.Text = MyDriver.SendLiteral(":" & Address.ToString & "LS")
      Me.txbMaxSpeed.Text = MyDriver.SendLiteral(":" & Address.ToString & "MS")
      Me.lsbResolution.SelectedIndex = CType(MyDriver.SendLiteral(":" & Address.ToString & "SR"), Int32)
      Me.txbHomeSpeed.Text = MyDriver.SendLiteral(":" & Address.ToString & "HS")
      Me.txbPauseAfterHome.Text = MyDriver.SendLiteral(":" & Address.ToString & "HP")
      Me.txbHomeSteps.Text = MyDriver.SendLiteral(":" & Address.ToString & "SL")
      Me.txbGroupAddress.Text = MyDriver.SendLiteral(":" & Address.ToString & "GA")
      Me.lsbReply.SelectedIndex = CType(MyDriver.SendLiteral(":" & Address.ToString & "AR"), Int32)
      DataChanged = False
      ArmPoint.X = Arm.Position.X
      ArmPoint.Y = Arm.Position.Y
      ArmPoint.Z = Arm.Position.Z
      ArmPoint.U = Arm.Position.U
      txbXCoordinate.Text = Format(ArmPoint.X, "0.000")
      txbYCoordinate.Text = Format(ArmPoint.Y, "0.000")
      txbZCoordinate.Text = Format(ArmPoint.Z, "0.000")
      txbUCoordinate.Text = Format(ArmPoint.U, "0.000")
   End Sub


   Private Sub UnlockControls(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblRunCurrent.Click, lblHoldCurrent.Click, lblResolution.Click, lblReply.Click, lblGroupAdd.Click
      Dim Password As String

      Password = InputBox("Enter Password:", "Password Protected Motor Driver Variable").ToUpper
      If Password = "PASSWORD" Then
         Me.txbRunCurrent.ReadOnly = False
         Me.txbHoldCurrent.ReadOnly = False
         Me.lsbResolution.Enabled = True
         Me.lsbReply.Enabled = True
         Me.txbGroupAddress.ReadOnly = False
      Else
         MessageBox.Show("The password is 'PASSWORD'.  Modify these variables carefully.", "Passwork Hint", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
      End If
   End Sub


   Private Sub MotorDataChanaged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txbRunCurrent.TextChanged, _
      txbHoldCurrent.TextChanged, txbHoldTime.TextChanged, txbAcceleration.TextChanged, txbMinSpeed.TextChanged, _
      txbMaxSpeed.TextChanged, txbHomeSpeed.TextChanged, txbPauseAfterHome.TextChanged, txbHomeSteps.TextChanged, lsbResolution.SelectedIndexChanged
      DataChanged = True
   End Sub

   Private Sub mnuSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuSave.Click
      SaveData()
   End Sub

   Private Sub mnuExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuExit.Click
      Me.Close()
   End Sub

   Sub SaveData()
      Try
         MyDriver.SendLiteral(":" & Address.ToString & "RC" & Me.txbRunCurrent.Text)
         MyDriver.SendLiteral(":" & Address.ToString & "HC" & Me.txbHoldCurrent.Text)
         MyDriver.SendLiteral(":" & Address.ToString & "HT" & Me.txbHoldTime.Text)
         MyDriver.SendLiteral(":" & Address.ToString & "SA" & Me.txbAcceleration.Text)
         MyDriver.SendLiteral(":" & Address.ToString & "LS" & Me.txbMinSpeed.Text)
         MyDriver.SendLiteral(":" & Address.ToString & "MS" & Me.txbMaxSpeed.Text)
         MyDriver.SendLiteral(":" & Address.ToString & "SR" & Me.lsbResolution.SelectedIndex.ToString)
         MyDriver.SendLiteral(":" & Address.ToString & "HS" & Me.txbHomeSpeed.Text)
         MyDriver.SendLiteral(":" & Address.ToString & "HP" & Me.txbPauseAfterHome.Text)
         MyDriver.SendLiteral(":" & Address.ToString & "SL" & Me.txbHomeSteps.Text)
         If Me.txbGroupAddress.ReadOnly = False Then   'The value may have been changed
            MyDriver.SendLiteral(":" & Address.ToString & "GA" & Me.txbGroupAddress.Text)
         End If
         If Me.lsbReply.Enabled = True Then   'Data may have changed
            MyDriver.SendLiteral(":" & Address.ToString & "AR" & Me.lsbReply.SelectedIndex)
         End If

         MyDriver.SendLiteral(":" & Address.ToString & "SU")
         DataChanged = False
      Catch ex As Exception
         MessageBox.Show("An error occured while saving data to the motor driver.", "Data Transfer Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
         DataChanged = True
      End Try
   End Sub

   Private Sub frmJkemDriverConfiguration_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
      If DataChanged = True Then
         Select Case MessageBox.Show("Save data before exiting?", "Data Not Saved", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
            Case Windows.Forms.DialogResult.Yes
               SaveData()
            Case Windows.Forms.DialogResult.Cancel
               e.Cancel = True
         End Select
      End If
      Carosel.ReleaseMotor()  'Set the holding current to 0
   End Sub

   Private Sub btnHome_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnHome.Click
      Carosel.Home()
      Me.txbPosition.Text = "0"
      Me.tbrCarosel.Value = 0
   End Sub

   Private Sub btnSend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSend.Click
      Me.txbReply.Text = ""
      Application.DoEvents()
      Me.txbReply.Text = MyDriver.SendLiteral(Me.txbCommand.Text)
   End Sub

   Private Sub tbrCarosel_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles tbrCarosel.MouseUp
      Dim Position As Int32

      Position = tbrCarosel.Value
      MyDriver.Move(Position)
      Me.txbPosition.Text = Position.ToString
   End Sub

   Private Sub btnReadSensor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReadSensor.Click
      Me.txbSensor.Text = ""
      Application.DoEvents()
      Me.txbSensor.Text = Gripper.ReadSensorAverage(32).ToString
   End Sub


   Private Sub txbGoToPosition_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txbGoToPosition.KeyDown
      If e.KeyCode = Keys.Enter Then
         Dim Value As Int32

         If IsInteger(Me.txbGoToPosition.Text, 0, 39050) = True Then
            Value = CType(Me.txbGoToPosition.Text, Int32)
            MyDriver.Move(Value)
            Me.txbPosition.Text = Value.ToString
            Me.tbrCarosel.Value = Value
         End If
      End If
   End Sub

   Private Sub lsbForwardSteps_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles lsbForwardSteps.MouseUp
      Dim Value As Int32

      Value = CType(Me.lsbForwardSteps.SelectedItem, Int32)
      Value = Value + MyDriver.Position
      If Value >= 0 And Value <= MyDriver.AxisLenght Then
         MyDriver.Move(Value)
         Me.txbPosition.Text = Value.ToString
         Me.tbrCarosel.Value = Value
      End If
   End Sub

   Private Sub lsbReverseSteps_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles lsbReverseSteps.MouseUp
      Dim Value As Int32

      Value = CType(Me.lsbReverseSteps.SelectedItem, Int32)
      Value = MyDriver.Position - Value
      If Value >= 0 And Value <= MyDriver.AxisLenght Then
         MyDriver.Move(Value)
         Me.txbPosition.Text = Value.ToString
         Me.tbrCarosel.Value = Value
      End If
   End Sub

   Private Sub btnOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOpen.Click
      Gripper.Open()
   End Sub

   Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
      Gripper.Close()
   End Sub

   Private Sub mnuUnlock_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuUnlock.Click
      Dim Password As String

      Password = InputBox("Enter password.", "Unlock Motor Group Box")
      If Password.ToUpper = "PASSWORD" Then
         Me.mnuSave.Enabled = True
         Me.gbxMotorDriver.Enabled = True
      End If
   End Sub

   Private Sub btnRack1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRack1.Click, _
   btnRack2.Click, btnRack3.Click, btnRack4.Click, btnRack5.Click
      Dim Position As Int32

      Position = CType(CType(sender, System.Windows.Forms.Button).Tag, Int32)
      Carosel.Move(Carosel.RackOrigin(Position).OriginX)
      txbPosition.Text = MyDriver.Position.ToString
   End Sub

   Private Sub lblX_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblX_N100.Click, lblX_N25.Click, lblX_N5.Click, lblX_N1.Click, lblX_N05.Click, _
               lblX_N02.Click, lblX_N01.Click, lblX_P01.Click, lblX_P02.Click, lblX_P05.Click, lblX_P1.Click, lblX_P5.Click, lblX_P25.Click, lblX_P100.Click
      Dim Offset As Double

      Try
         Offset = CDbl(ExtractNumberFromString(CType(sender, Label).Text))
         If (Offset + ArmPoint.X) < -740.0 Or (Offset + ArmPoint.X) > 750.0 Then
            MessageBox.Show("This is an axis limit error.", "Axis Limit", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
            Exit Sub
         End If
         ArmPoint.X += Offset
         Arm.GoToPoint(ArmPoint)
         txbXCoordinate.Text = Format(ArmPoint.X, "0.000")
      Catch ex As Exception
      End Try
   End Sub

   Private Sub lblY_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblY_N100.Click, lblY_N25.Click, lblY_N5.Click, lblY_N1.Click, lblY_N05.Click, _
            lblY_N02.Click, lblY_N01.Click, lblY_P01.Click, lblY_P02.Click, lblY_P05.Click, lblY_P1.Click, lblY_P5.Click, lblY_P25.Click, lblY_P100.Click
      Dim Offset As Double

      Try
         Offset = CDbl(ExtractNumberFromString(CType(sender, Label).Text))
         If (Offset + ArmPoint.Y) < -490.0 Or (Offset + ArmPoint.Y) > 790.0 Then
            MessageBox.Show("This is an axis limit error.", "Axis Limit", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
            Exit Sub
         End If
         ArmPoint.Y += Offset
         Arm.GoToPoint(ArmPoint)
         txbYCoordinate.Text = Format(ArmPoint.Y, "0.000")
      Catch ex As Exception
      End Try
   End Sub

   Private Sub lblZ_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblZ_N100.Click, lblZ_N25.Click, lblZ_N5.Click, lblZ_N1.Click, lblZ_N05.Click, _
         lblZ_N02.Click, lblZ_N01.Click, lblZ_P01.Click, lblZ_P02.Click, lblZ_P05.Click, lblZ_P1.Click, lblZ_P5.Click, lblZ_P25.Click, lblZ_P100.Click
      Dim Offset As Double

      Try
         Offset = CDbl(ExtractNumberFromString(CType(sender, Label).Text))
         If (Offset + ArmPoint.Z) < -410.0 Or (Offset + ArmPoint.Z) > 0.0 Then
            MessageBox.Show("This is an axis limit error.", "Axis Limit", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
            Exit Sub
         End If
         ArmPoint.Z += Offset
         Arm.GoToPoint(ArmPoint)
         txbZCoordinate.Text = Format(ArmPoint.Z, "0.000")
      Catch ex As Exception
      End Try
   End Sub

   Private Sub lblU_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblU_N25.Click, lblU_N5.Click, lblU_N1.Click, lblU_N05.Click, _
      lblU_N02.Click, lblU_N01.Click, lblU_N005.Click, lblU_P005.Click, lblU_P01.Click, lblU_P02.Click, lblU_P05.Click, lblU_P1.Click, lblU_P5.Click, lblU_P25.Click
      Dim Offset As Double

      Try
         Offset = CDbl(ExtractNumberFromString(CType(sender, Label).Text))
         If (Offset + ArmPoint.U) < 50.0 Or (Offset + ArmPoint.U) > 340.0 Then
            MessageBox.Show("This is an axis limit error.", "Axis Limit", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
            Exit Sub
         End If
         ArmPoint.U += Offset
         Arm.GoToPoint(ArmPoint)
         txbUCoordinate.Text = Format(ArmPoint.U, "0.000")
      Catch ex As Exception
      End Try
   End Sub


   Private Sub txbXCoordinate_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txbXCoordinate.KeyDown
      If e.KeyCode = Keys.Enter Then
         Dim Value As Double

         If MessageBox.Show("Move to X coordinate " & txbXCoordinate.Text & "?", "X Axis Move", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) = Windows.Forms.DialogResult.OK Then
            If IsDouble(txbXCoordinate.Text, -750.0, 760.0) Then
               Value = CDbl(txbXCoordinate.Text)
               ArmPoint.X = Value
               Arm.GoToPoint(ArmPoint)
               txbXCoordinate.Text = Format(ArmPoint.X, "0.000")
            Else
               txbXCoordinate.Text = Format(ArmPoint.X, "0.000")
            End If
         Else
            txbXCoordinate.Text = Format(ArmPoint.X, "0.000")
         End If
      End If
   End Sub

   Private Sub txbYCoordinate_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txbYCoordinate.KeyDown
      If e.KeyCode = Keys.Enter Then
         Dim Value As Double

         If MessageBox.Show("Move to Y coordinate " & txbYCoordinate.Text & "?", "Y Axis Move", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) = Windows.Forms.DialogResult.OK Then
            If IsDouble(txbYCoordinate.Text, -390.0, 790.0) Then
               Value = CDbl(txbYCoordinate.Text)
               ArmPoint.Y = Value
               Arm.GoToPoint(ArmPoint)
               txbYCoordinate.Text = Format(ArmPoint.Y, "0.000")
            Else
               txbYCoordinate.Text = Format(ArmPoint.Y, "0.000")
            End If
         Else
            txbYCoordinate.Text = Format(ArmPoint.Y, "0.000")
         End If
      End If
   End Sub

   Private Sub txbZCoordinate_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txbZCoordinate.KeyDown
      If e.KeyCode = Keys.Enter Then
         Dim Value As Double

         If MessageBox.Show("Move to Z coordinate " & txbZCoordinate.Text & "?", "Z Axis Move", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) = Windows.Forms.DialogResult.OK Then
            If IsDouble(txbZCoordinate.Text, -410.0, 0.0) Then
               Value = CDbl(txbZCoordinate.Text)
               ArmPoint.Z = Value
               Arm.GoToPoint(ArmPoint)
               txbZCoordinate.Text = Format(ArmPoint.Z, "0.000")
            Else
               txbZCoordinate.Text = Format(ArmPoint.Z, "0.000")
            End If
         Else
            txbZCoordinate.Text = Format(ArmPoint.Z, "0.000")
         End If
      End If
   End Sub

   Private Sub txbUCoordinate_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txbUCoordinate.KeyDown
      If e.KeyCode = Keys.Enter Then
         Dim Value As Double

         If MessageBox.Show("Move to U coordinate " & txbUCoordinate.Text & "?", "U Axis Move", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) = Windows.Forms.DialogResult.OK Then
            If IsDouble(txbUCoordinate.Text, 55.0, 340.0) Then
               Value = CDbl(txbUCoordinate.Text)
               ArmPoint.U = Value
               Arm.GoToPoint(ArmPoint)
               txbUCoordinate.Text = Format(ArmPoint.U, "0.000")
            Else
               txbUCoordinate.Text = Format(ArmPoint.U, "0.000")
            End If
         Else
            txbUCoordinate.Text = Format(ArmPoint.U, "0.000")
         End If
      End If
   End Sub

End Class