
Module Epson


   Public Class EpsonArm
      Public WithEvents Spel As SpelNetLib.Spel
      Private MoveComplete As Boolean
      Private EpsonTestMode As Boolean = False
      Private EmergencyStopIsActive As Boolean
      Private RequestedPosition As Point4D
      Public Position As Point4D    'Current arm position


      Enum EpsonAxis
         X
         Y
         Z
         U
      End Enum


      Sub New()
         Try
            If EpsonTestMode = True Then
               Return
            End If

            MoveComplete = False
            EmergencyStopIsActive = False
            Spel = New SpelNetLib.Spel
            Spel.NoProjectSync = True 'Uses the current project loaded in the controller.
            Spel.OperationMode = SpelOperationMode.Auto
            Spel.Initialize()
            Spel.Reset()
            Spel.Start(0)
         Catch ex As Exception
            MessageBox.Show("An error occured initializing the Spel object.  The error message was: " & ex.Message, "Spel Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            Spel.Reset()
         End Try
      End Sub


      Public Function TestComms() As Boolean
         Dim Status As Boolean

         If EpsonTestMode = False Then
            If Spel.Version = "5.3.4" Then
               Status = True
            Else
               Status = False
            End If
         Else
            Status = True
         End If

         Return Status
      End Function


      Public Sub Reset()
         Spel.Reset()
         Spel.Connect(1)
         MoveComplete = False
         Spel.Initialize()
         'Spel.NoProjectSync = True 'Uses the current project loaded in the controller.
         'Spel.OperationMode = SpelOperationMode.Auto
      End Sub

      Public Sub Close()
         Try
            Spel.Disconnect()
            Spel.Dispose()
         Catch ex As Exception
            MessageBox.Show("The Spel  object did not dispose properly.", "Spel Object Disposal", MessageBoxButtons.OK)
         End Try
      End Sub

      Public Sub Reconnect()
         Try
            Spel.Start(0)
         Catch ex As Exception
            MessageBox.Show("Connection was not reestablished with the Spel object.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try
      End Sub


      ''' <summary>
      ''' Moves the Epson arm to the 4-coordinate point
      ''' </summary>
      ''' <param name="point">4 coordinate point to move to.</param>
      ''' <remarks></remarks>
      Sub GoToPoint(ByVal point As Point4D)
         RequestedPosition = point    'Used in error messages if needed.

         If EpsonTestMode = True Then
            Exit Sub
         End If

         Try
            If EmergencyStopIsActive = True Then
               Do
                  DelayMS(500)
               Loop While EmergencyStopIsActive = True
            End If
            Spel.SetVar("Xpos", point.X)
            Spel.SetVar("Ypos", point.Y)
            Spel.SetVar("Zpos", point.Z)
            Spel.SetVar("Upos", point.U)
            MoveComplete = False
            Spel.SetVar("CommandReceived", True)
            While MoveComplete = False
               Application.DoEvents()
            End While
            frmMain.txbPositionX.Text = Format(point.X, "0.000")
            frmMain.txbPositionY.Text = Format(point.Y, "0.000")
            frmMain.txbPositionZ.Text = Format(point.Z, "0.000")
            frmMain.txbPositionU.Text = Format(point.U, "0.000")
         Catch ex As Exception
            MessageBox.Show("An exception occured in the Epson Sub GoToPoint().  The message was: " & ex.Message, "Epson Arm Exception", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
         End Try

         Arm.Position = point
      End Sub


      'This sub tucks the gripper under the arm.   It uses joint angle
      Public Sub ProtectGripper()

         If EpsonTestMode = True Then
            Exit Sub
         End If

         MoveComplete = False
         Spel.SetVar("ProtectTool", True)
         Spel.SetVar("CommandReceived", True)
         While MoveComplete = False
            Application.DoEvents()
         End While
      End Sub


      Sub Home()
         'These coordinates were emperically found.  They get the arm out of the way and protect the gripper.
         Dim HomePoint As Point4D

         If EpsonTestMode = True Then
            Exit Sub
         End If

         Try
            If Arm.Position.Z < -170.0 Then
               HomePoint = Arm.Position
               HomePoint.Z = -170.0
               GoToPoint(HomePoint)
            End If
            HomePoint.X = -177.0
            HomePoint.Y = 282.0
            HomePoint.Z = 0.0
            HomePoint.U = 286.0
            GoToPoint(HomePoint)
         Catch ex As Exception
            MessageBox.Show("An exception occured in the Epson Sub Home().  The message was: " & ex.Message, "Epson Arm Exception", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
         End Try
      End Sub


      Private Sub Spel_EventReceived(ByVal sender As Object, ByVal e As SpelNetLib.SpelEventArgs) Handles Spel.EventReceived
         Dim Message As String

         Try
            Select Case e.Event
               Case CType(3000, SpelNetLib.SpelEvents)
                  MoveComplete = True
               Case SpelEvents.Error
                  If Spel.ErrorCode = 1502 Then
                     MessageBox.Show("Communications was lost between the RC180 controller and the PC.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                     Spel.Reset()
                  Else
                     If MessageBox.Show("The Epson controller returned the Error Code: " & Spel.ErrorCode.ToString & vbCrLf & "Do you want to abort the proceedure?", "Epson Error Code", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1) = DialogResult.Yes Then
                        Throw New System.Exception("Abort Requested")
                     End If
                  End If
               Case SpelEvents.AllTasksStopped
                  If Spel.ErrorCode = 4001 Then
                     Message = "Attempted motion to an illegal coordinate." & vbCrLf & "Current position is: " & Format(Arm.Position.X, "0.0") & ", " & _
                     Format(Arm.Position.Y, "0.0") & ", " & Format(Arm.Position.Z, "0.0") & ", " & Format(Arm.Position.U, "0.0") & vbCrLf & _
                     "Requested position was: " & Format(Me.RequestedPosition.X, "0.0") & ", " & Format(Me.RequestedPosition.Y, "0.0") & ", " & _
                     Format(Me.RequestedPosition.Z, "0.0") & ", " & Format(Me.RequestedPosition.U, "0.0")
                  ElseIf Spel.ErrorCode = 1502 Then
                     Message = "Serial communications was lost with the PC from the USB port."
                  Else
                     Message = ""
                  End If
                  If MessageBox.Show("The Epson controller threw an event of type AllTasksStopped.  The Error Code was: " & Spel.ErrorCode.ToString & vbCrLf & _
                                     "The message was: " & Message & vbCrLf & "Do you want to abort the proceedure?", "Epson Error Code", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1) = DialogResult.Yes Then
                     Throw New System.Exception("Abort Requested")
                  Else
                     Me.Reset()
                  End If
               Case SpelEvents.EstopOn
                  EmergencyStopIsActive = True
                  MessageBox.Show("The Emergency Stop button has been pressed.   Fix the error, then release the button to continue.", "Emergency Stop Detected", MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1)
                  If MessageBox.Show("Do you want to abort the proceedure?", "Continue Proceedure", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) = DialogResult.Yes Then
                     Throw New System.Exception("Abort Requested")
                  End If
               Case SpelEvents.EstopOff
                  MessageBox.Show("The Emergency Stop button has been reset.  Click OK to resume the proceedure.", "Emergency Stop Reset", MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1)
                  EmergencyStopIsActive = False
               Case Else
                  MessageBox.Show("An unknown event was raised from the Spel object.  The event was " & e.Event.ToString, "Spel Event", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
            End Select

         Catch ex As Exception
            MessageBox.Show("An error was thrown in the handler Spel_EventReceived.  The error was: " & ex.Message, "Spel Event Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
         End Try
      End Sub

   End Class

End Module
