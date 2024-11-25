Imports System.IO
Imports System.Math
Imports System.Environment



Public Enum DriverState
   Power_On
   Power_Off
End Enum


'This structure holds the coordinates for the 4 axes in space
Public Structure MySpace
   Dim X As Double
   Dim Y As Double
   Dim Z As Double
   Dim U As Double
   Dim InsertX As Double   'This is the amount of X change needed to move the arm 1mm "into" the oven
   Dim InsertY As Double   'This is the amount of Y change needed to move the arm 1mm "into" the oven
End Structure


Public Structure Point4D
   Dim X As Double
   Dim Y As Double
   Dim Z As Double
   Dim U As Double
End Structure



   'This is a shared variable that can be accessed by any module without the need to 
   'instantiate the class
Public Class ModuleDefMessages
   Public Shared InTestMode As Boolean
   Public Shared AbortRequested As Boolean
End Class


Public Module ModuleDefinitions



   Public Class OvenDef
      'The filter location numbering of the ovens is, Filter 1 is in the top left position, filter 2 is in the same column, 1 position down,
      'Filter 12 is the bottom filter in column 1.  Filter 13 is the top filter in column 2.
      Enum DoorLoc
         Open
         Closed
         Unconfirmed
      End Enum

      Event TransmissionError(ByVal sender As Object, ByVal message As String)

      Structure Wdata
         Dim Weight As Double
         Dim Time As DateTime
      End Structure

      Structure Data
         Dim BarcodeNumber As String   'The bar code number of the filter
         Dim TareWeight As Double   'Tare weight of the new filter
         Dim WeightData() As Wdata     'An array of weights at diffenet times
      End Structure


      Public MyAddress As String 'This is the serial number of the USB board in the oven
      Public MyPort As System.IO.Ports.SerialPort
      Public MyPosition As Int32    'This is the position of the oven on the deck, numbered 1 to 5
      Private TemperatureAddress As Int32 = 1
      Private PressureAddress As Int32 = 2
      Private MyDoorState As DoorLoc
      Private IsCloseSensorOverRiden As Boolean   'Set is the door close proximity sensor is overridden
      Private IsOvenActive As Boolean
      Public Filter(96) As Data
      Public RackLoc(3) As MySpace  'This holds the coordinates of the 4 corners of the oven.
      'Element 0 is the top left, 1 is top right, 2 is bottom left, 3 is bottom right


      Sub New(ByRef serialPort As Ports.SerialPort, ByVal address As String)
         'Create the log file one time
         MyPort = serialPort
         MyPort.BaudRate = 9600
         MyPort.ReadTimeout = 500
         MyPort.NewLine = vbCr
         MyAddress = address
         If address = "100" Then 'These addresses, are the serial number of the USB board storred in EEPROM inside of the oven.
            MyPosition = 3
         ElseIf address = "101" Then
            MyPosition = 5
         ElseIf address = "102" Then
            MyPosition = 4
         ElseIf address = "103" Then
            MyPosition = 1
         ElseIf address = "104" Then
            MyPosition = 2
         End If
         MyDoorState = DoorLoc.Unconfirmed 'Default safety setting
         IsCloseSensorOverRiden = False
         IsOvenActive = True

         For Pass As Int32 = 0 To 96
            ReDim Filter(Pass).WeightData(9) 'Allow for 10 weight/time points
         Next
      End Sub


      Public Property OvenActive() As Boolean
         Get
            Return IsOvenActive
         End Get
         Set(ByVal value As Boolean)
            If value = False Then
               If MessageBox.Show("Are you sure that you want to DEACTIVATE Oven " & MyPosition.ToString & "?", "Deactivate Oven", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                  IsOvenActive = False
               End If
            Else
               IsOvenActive = True
            End If
         End Set
      End Property


      Public ReadOnly Property DoorState() As DoorLoc
         Get
            Return MyDoorState
         End Get
      End Property


      Public ReadOnly Property PortName() As String
         Get
            Return MyPort.PortName
         End Get
      End Property


      Private Function Send(ByRef command As String) As String
         Dim Reply As String


         If ModuleDefMessages.InTestMode = True Then
            DelayMS(100)
            Return command
         End If

         If ModuleDefMessages.AbortRequested = True Then
            Throw New System.Exception("User requested abort.")
         End If

         Try
            If MyPort.IsOpen = False Then
               MyPort.Open()
            End If

            MyPort.DiscardInBuffer()
            MyPort.DiscardOutBuffer()
            MyPort.WriteLine(command)
            Try
               Reply = MyPort.ReadLine()
            Catch ex As Exception   'Try to send the command once again.
               FileIO.WriteTextFile(LogFileName, "Error," & MyPosition.ToString & "," & command & "," & ex.Message & vbCrLf, False, True)
               Reply = "Error"
            End Try
         Catch ex As Exception
            FileIO.WriteTextFile(LogFileName, "Error," & MyPosition.ToString & "," & command & "," & ex.Message & vbCrLf, False, True)
            Reply = "Error"
         End Try

         Return Reply
      End Function


      Private Function SendMyCommand(ByRef command As String) As String
         Dim Reply As String = ""
         Dim Pass As Int32

         Try
            For Pass = 0 To 3
               Reply = Me.Send(command)
               If Reply <> "Error" Then
                  Exit For
               End If
               DelayMS(100 * (Pass + 1))
            Next
            If Pass = 4 Then    'Try to recover port communications.
               FileIO.WriteTextFile(LogFileName, "Error in OvenDef SendMyCommand.  Left loop with Pass= 8.," & MyPosition.ToString & "," & command & vbCrLf, False, True)
               FileIO.WriteTextFile(LogFileName, "Recover Port was called." & vbCrLf, False, True)
               MessageBox.Show("Get Scott.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
               MessageBox.Show("The serial port was dropped on Oven " & MyPosition.ToString & "  Unplug the USB cable leading from the PC to the USB Hub hanging in the center of the robot deck, wait 5 seconds then plug it back in.   Do this for both hubs.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
               MessageBox.Show("Wait 5 seconds, then click OK to continue.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
               Reply = RecoverPort(command)
            End If
         Catch ex As Exception
            FileIO.WriteTextFile(LogFileName, "Error in OvenDef SendMyCommand," & MyPosition.ToString & "," & command & "," & ex.Message & vbCrLf, False, True)
            frmMain.ErrorDump(ex.Message, ex.TargetSite.Name, command)
            MessageBox.Show("Error in function SendMyCommand().   Tell Scott to call J-KEM with this error message.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try

         Return Reply
      End Function


      Function RecoverPort(ByVal command As String) As String
         Dim Pass As Int32
         Dim Reply As String = "Error"

         Try
            For Pass = 0 To 3
               If Send("?(0)").Contains("OSHA") Then
                  DelayMS(500)
                  Reply = Me.Send(command)
                  If Reply <> "Error" Then
                     Exit For
                  End If
                  DelayMS(500)
               End If
            Next
            If Pass = 4 Then
               FileIO.WriteTextFile(LogFileName, "Recover Port did not work." & vbCrLf, False, True)
               MessageBox.Show("The application can not be recovered.   The application will exit.", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
               Throw New System.Exception("User abort requested.")
            End If
         Catch ex As Exception
            FileIO.WriteTextFile(LogFileName, "Recover Port caused an exception." & vbCrLf, False, True)
            MessageBox.Show("The application can not be recovered.   The application will exit.", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
            Throw New System.Exception("User abort requested.")
         End Try

         If Reply = "Error" Then
            FileIO.WriteTextFile(LogFileName, "Recover Port did not work again." & vbCrLf, False, True)
            MessageBox.Show("The application can not be recovered.   The application will exit.", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
            Throw New System.Exception("User abort requested.")
         End If

         Return Reply
      End Function


      Public Property IsCloseDoorSensorInOverride() As Boolean
         'The proximity sensor that detects door close can burn out.   This is a temperary fix to allow them to override the close sensor until
         'it can be replaced
         Get
            Return IsCloseSensorOverRiden
         End Get
         Set(ByVal value As Boolean)
            IsCloseSensorOverRiden = value
         End Set
      End Property


      Function GetDoorState() As DoorLoc
         If TestCloseSensor() = True Then
            Return DoorLoc.Closed
         ElseIf TestOpenSensor() = True Then
            Return DoorLoc.Open
         Else
            Return DoorLoc.Unconfirmed
         End If
      End Function


      ''' <summary>
      ''' Moves the arm to a deck position where it is safe to open the door
      ''' </summary>
      ''' <param name="position">Oven array element.  Range: 1-5</param>
      ''' <remarks>A value of 0 will position the are at a safe point for oven 1</remarks>
      Shared Sub GotoSafeOpenDoorPosition(ByVal position As Int32)
         Dim ThisPoint As Point4D

         ThisPoint = Arm.Position
         If ThisPoint.Z < -170.0 Then
            ThisPoint.Z = -170.0
            Arm.GoToPoint(ThisPoint)
         ElseIf ThisPoint.Z > -120.0 Then
            ThisPoint.Z = -130.0
            Arm.GoToPoint(ThisPoint)
         End If

         If position > 2 Then
            If ThisPoint.X > -500.0 Or ThisPoint.Y > -200.0 Then
               ThisPoint.X = -330.0
               ThisPoint.Y = 500.0
               ThisPoint.U = 335.0
               Arm.GoToPoint(ThisPoint)
            End If
         End If

         ThisPoint.X = -625.0
         ThisPoint.Y = -280.0
         ThisPoint.U = 330.0
         Arm.GoToPoint(ThisPoint)
      End Sub


      ''' <summary>
      ''' Places a filter in the oven.  Transitions the arm to a coordinate that is safe to move to any oven.
      ''' </summary>
      ''' <param name="ovenNum">Oven Number.   Range: 1-5.</param>
      ''' <param name="pos">Rack position in the oven to place the filter.  Range: 1-96.</param>
      ''' <remarks></remarks>
      Shared Sub PutFilter(ByVal ovenNum As Int32, ByVal pos As Int32)
         Dim ThisOven As OvenDef
         Dim MyPoint As Point4D
         Dim MyADC As Int32

         Try
            If ModuleDefMessages.InTestMode = True Then
               Return
            End If

            ThisOven = OvenArray(ovenNum - 1)

            ThisOven.GotoOvenSafePoint()
            MyPoint = ThisOven.GetFilterLocation(pos) 'go to the filter location
            MyPoint = ThisOven.MoveOut(MyPoint, 250.0) 'Keep the arm 250mm outside of the oven.
            MyPoint.Z += 9.5
            Arm.GoToPoint(MyPoint)
            MyPoint = ThisOven.GetFilterLocation(pos)
            MyPoint = ThisOven.MoveOut(MyPoint, 6.0)
            MyPoint.Z += 9.5
            Arm.GoToPoint(MyPoint)
            MyPoint = ThisOven.GetFilterLocation(pos)
            MyPoint = ThisOven.MoveIn(MyPoint, 5.5)  '3mm of the fiter are in the gripper jaws.  This positions the filter just slightly into the holder.
            MyPoint.Z += 3.0
            Arm.GoToPoint(MyPoint)
            MyPoint = ThisOven.MoveOut(MyPoint, 2.0)   'Come out of the oven by 5 mm
            Arm.GoToPoint(MyPoint)
            Gripper.Open()
            'This section tests to see if the filter is off of the gripper jaws
            MyPoint = ThisOven.MoveOut(MyPoint, 15.0)   'Come out of the oven by 5 mm
            Arm.GoToPoint(MyPoint)
            MyADC = Gripper.ReadSensorAverage
            If MyADC < (frmMain.LastAirADCReading - frmMain.OpticalSensor3mmDrop \ 2) Then
               'Filter is stuck to gripper
               MyPoint = ThisOven.GetFilterLocation(pos)
               MyPoint = ThisOven.MoveIn(MyPoint, 3.0)
               MyPoint.Z += 4.0
               Arm.GoToPoint(MyPoint)
               MyPoint.Z -= 6.0
               Arm.GoToPoint(MyPoint)
               MyPoint = ThisOven.MoveOut(MyPoint, 30.0)   'Come out of the oven by 5 mm
               Arm.GoToPoint(MyPoint)
               If Gripper.ReadSensorAverage < (frmMain.LastAirADCReading - frmMain.OpticalSensor3mmDrop \ 2) Then
                  'Filter is stuck to gripper
                  MyPoint = ThisOven.GetFilterLocation(pos)
                  MyPoint.Z += 4.0
                  MyPoint = ThisOven.MoveIn(MyPoint, 3.0)   'Come out of the oven by 5 mm
                  Arm.GoToPoint(MyPoint)
                  MyPoint.Z -= 8.0
                  MyPoint = ThisOven.MoveOut(MyPoint, 50.0)   'Come out of the oven by 5 mm
                  Arm.GoToPoint(MyPoint)
               End If
               If Gripper.ReadSensorAverage < (frmMain.LastAirADCReading - frmMain.OpticalSensor3mmDrop \ 2) Then
                  frmMain.rtbMessages.Text += vbCrLf & "Filter " & pos.ToString & " on Oven " & ovenNum.ToString & " stuck to the gripper."
               End If
            End If
            Gripper.Close()
            MyPoint = ThisOven.GetFilterLocation(pos)
            MyPoint = ThisOven.MoveOut(MyPoint, 2.0)   'Push the filter 1mm into the oven holder.
            MyPoint.Z -= 5.0
            Arm.GoToPoint(MyPoint)
            MyPoint = ThisOven.MoveOut(MyPoint, 250.0)
            Arm.GoToPoint(MyPoint)
         Catch ex As Exception
            frmMain.ErrorDump(ex.Message, ex.TargetSite.Name, ovenNum.ToString & vbCrLf & pos.ToString)
            MessageBox.Show("Error in function PutFilter().   Tell Scott to call J-KEM with this error message." & vbCrLf & ovenNum.ToString & "  " & pos.ToString, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try
      End Sub


      ''' <summary>
      ''' Retreaves a filter from the oven.
      ''' </summary>
      ''' <param name="ovenNum">Oven number.   Range: 1-5.</param>
      ''' <param name="pos">Positon to get the filter from.  Range: 1-96.</param>
      ''' <returns>True if successful.</returns>
      ''' <remarks></remarks>
      Shared Function GetFilter(ByVal ovenNum As Int32, ByVal pos As Int32) As Boolean
         'Picks a filter out of the oven.  At the end of the move, the arm is at a safe position to perform any other motion.
         Dim MyPoint As Point4D
         Dim AirADC As Int32  'This is the value of the ADC when the sensor sees pure air
         Dim ADC As Int32
         Dim Pass As Int32
         Dim ThisOven As OvenDef
         Dim FilterLocated As Boolean


         If ModuleDefMessages.InTestMode = True Then
            Return True
         End If

         Try
            ThisOven = OvenArray(ovenNum - 1)

            ThisOven.GotoOvenSafePoint()
            MyPoint = ThisOven.GetFilterLocation(pos) 'go to the filter location
            MyPoint = ThisOven.MoveOut(MyPoint, 250.0) 'Keep the arm 250mm outside of the oven.
            Arm.GoToPoint(MyPoint)
            Gripper.Open()
            AirADC = Gripper.ReadSensorAverage
            frmMain.LastAirADCReading = AirADC

            'Search for the edge of the filter.   Here are the ADC readings based on condition.
            'Gripper open, sensor unblocked = 681,  Gripper at the filter edge= 645
            'Sensor over filter by 2mm = 578,      3mm = 511
            '10/25/2012  Gripper read 900 when it was open and 802 when it was 2mm over the filter
            '11/3/2014  The reading dropped 23 when it was 2mm over the edge.
            MyPoint = ThisOven.GetFilterLocation(pos)
            MyPoint = ThisOven.MoveIn(MyPoint, 3.0) 'take the gripper 3mm over the filter
            Arm.GoToPoint(MyPoint)
            ADC = Gripper.ReadSensorAverage

            'If you don't have enough drop in the ADC, then continue to move the gripper into the oven.   You can go a maximum of 6mm past
            'the starting location of the filter.
            FilterLocated = False
            For Pass = 1 To 12
               If ADC > (AirADC - frmMain.OpticalSensor3mmDrop) Then 'The ADC reading drops by about 70 counts when you are 3mm over the filter edge
                  MyPoint = ThisOven.MoveIn(MyPoint, 0.5)
                  Arm.GoToPoint(MyPoint)
                  ADC = Gripper.ReadSensorAverage
               Else
                  FilterLocated = True
                  Exit For
               End If
            Next

            If FilterLocated = True Then
               MyPoint.Z += 1   'Keep the gripper 1mm up
               Arm.GoToPoint(MyPoint)
               Gripper.Close()
               MyPoint.Z += 7.0
               Arm.GoToPoint(MyPoint)
               MyPoint = ThisOven.MoveOut(MyPoint, 200.0)
               MyPoint.Z += 20.0
               Arm.GoToPoint(MyPoint)
            Else
               MyPoint = ThisOven.GetFilterLocation(pos)
               MyPoint = ThisOven.MoveOut(MyPoint, 200.0)
               Arm.GoToPoint(MyPoint)
            End If
         Catch ex As Exception
            frmMain.ErrorDump(ex.Message, ex.TargetSite.Name, ovenNum.ToString & vbCrLf & pos.ToString)
            MessageBox.Show("Error in function PutFilter().   Tell Scott to call J-KEM with this error message." & vbCrLf & ovenNum.ToString & "  " & pos.ToString, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
            FilterLocated = False
         End Try

         Return FilterLocated
      End Function


      Private Sub GotoOvenSafePoint()
         'A safe location to begin a move to the oven, or from the oven to the carosel.
         'This is a transition point.  Go to this point when moving from the carosel to the oven, and also from the oven to the carosel.
         Dim MyPoint As Point4D

         MyPoint = Arm.Position
         If MyPoint.Z < -170.0 Then
            MyPoint.Z = -170.0
            Arm.GoToPoint(MyPoint)
         ElseIf MyPoint.Z > -120.0 Then
            MyPoint.Z = -120.0
            Arm.GoToPoint(MyPoint)
         End If


         If Arm.Position.X >= 0 Then
            MyPoint.X = -333.0
            MyPoint.Y = 500.0
            MyPoint.U = 335.0
            Arm.GoToPoint(MyPoint)
         Else
            MyPoint.X = -439.0
            MyPoint.Y = 100.0
            MyPoint.U = 335.0
            Arm.GoToPoint(MyPoint)
            If Me.MyPosition >= 3 Then
               MyPoint.X = -333.0
               MyPoint.Y = 500.0
               MyPoint.U = 335.0
               Arm.GoToPoint(MyPoint)
            End If
         End If
      End Sub




      Private Function GetFilterLocation(ByVal pos As Int32) As Point4D
         'Locatest the coordinates of a filter.   pos is the filter position from 1 to 96.   
         Dim X As Double
         Dim Y As Double
         Dim Z As Double
         Dim U As Double
         Dim Delta As Double
         Dim DeltaDelta As Double
         Dim MyPoint As Point4D
         Dim Row As Int32
         Dim Column As Int32



         Try
            Column = (pos - 1) \ 12
            Row = ((pos - 1) Mod 12)

            Delta = (Me.RackLoc(1).X - Me.RackLoc(0).X) / 7.0 'This is the change in X for Row 1
            DeltaDelta = (Me.RackLoc(3).X - Me.RackLoc(2).X) / 7.0 'This is the change in X for Row 12
            DeltaDelta = (DeltaDelta - Delta) / 11.0   'This is change in deltaX as a function of row
            Delta = Delta + (DeltaDelta * Row)
            X = Me.RackLoc(0).X + (Delta * Column)
            Delta = (Me.RackLoc(2).X - Me.RackLoc(0).X) / 11
            MyPoint.X = X + (Row * Delta)

            Delta = (Me.RackLoc(1).Y - Me.RackLoc(0).Y) / 7.0
            DeltaDelta = (Me.RackLoc(3).Y - Me.RackLoc(2).Y) / 7.0
            DeltaDelta = (DeltaDelta - Delta) / 11.0
            Delta = Delta + (DeltaDelta * Row)
            Y = Me.RackLoc(0).Y + (Delta * Column)
            Delta = (Me.RackLoc(2).Y - Me.RackLoc(0).Y) / 11
            MyPoint.Y = Y + (Row * Delta)

            Delta = (Me.RackLoc(1).Z - Me.RackLoc(0).Z) / 7.0
            DeltaDelta = (Me.RackLoc(3).Z - Me.RackLoc(2).Z) / 7.0
            DeltaDelta = (DeltaDelta - Delta) / 11.0
            Delta = Delta + (DeltaDelta * Row)
            Z = Me.RackLoc(0).Z + (Delta * Column)
            Delta = (Me.RackLoc(2).Z - Me.RackLoc(0).Z) / 11
            MyPoint.Z = Z + (Row * Delta)

            Delta = (Me.RackLoc(1).U - Me.RackLoc(0).U) / 7.0
            DeltaDelta = (Me.RackLoc(3).U - Me.RackLoc(2).U) / 7.0
            DeltaDelta = (DeltaDelta - Delta) / 11.0
            Delta = Delta + (DeltaDelta * Row)
            U = Me.RackLoc(0).U + (Delta * Column)
            Delta = (Me.RackLoc(2).U - Me.RackLoc(0).U) / 11
            MyPoint.U = U + (Row * Delta)
            Return MyPoint
         Catch ex As Exception
            frmMain.ErrorDump(ex.Message, ex.TargetSite.Name, pos.ToString)
            MessageBox.Show("Error in function GetFilterLocation().   Tell Scott to call J-KEM with this error message.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try
      End Function


      ''' <summary>
      ''' This function moves the arm to the right of its current postion by the passed amount.
      ''' "Right" is from the persepctive of looking directly into the oven rack.  It adjusts only the X and Y coordinates.
      ''' </summary>
      ''' <param name="amount">The number of millimeters to move.</param>
      ''' <returns>A 4D point to move to.</returns>
      ''' <remarks></remarks>
      Private Function MoveRight(ByVal point As Point4D, ByVal amount As Double) As Point4D
         point.X = point.X + (amount * Me.RackLoc(0).InsertY)
         point.Y = point.Y + (amount * -Me.RackLoc(0).InsertX)
         Return point
      End Function


      ''' <summary>
      ''' This function moves the arm to the left of its current postion by the passed amount.
      ''' "Left" is from the persepctive of looking directly into the oven rack.  It adjusts only the X and Y coordinates.
      ''' </summary>
      ''' <param name="amount">The number of millimeters to move.</param>
      ''' <returns>A 4D point to move to.</returns>
      ''' <remarks></remarks>
      Private Function MoveLeft(ByVal point As Point4D, ByVal amount As Double) As Point4D
         point.X = point.X + (amount * -Me.RackLoc(0).InsertY)
         point.Y = point.Y + (amount * Me.RackLoc(0).InsertX)
         Return point
      End Function


      ''' <summary>
      ''' Moves the arm into the oven by the amount specifed in millimeters.
      ''' </summary>
      ''' <param name="point">The starting point of the move.</param>
      ''' <param name="amount">The number of millimeters to move the arm into the oven.</param>
      ''' <returns>The end point of the move.</returns>
      ''' <remarks></remarks>
      Private Function MoveIn(ByVal point As Point4D, ByVal amount As Double) As Point4D
         point.X = point.X + (amount * Me.RackLoc(0).InsertX)
         point.Y = point.Y + (amount * Me.RackLoc(0).InsertY)
         Return point
      End Function


      ''' <summary>
      ''' Moves the arm out the oven by the amount specifed in millimeters.
      ''' </summary>
      ''' <param name="point">The starting point of the move.</param>
      ''' <param name="amount">The number of millimeters to move the arm oiut of the oven.</param>
      ''' <returns>The end point of the move.</returns>
      ''' <remarks></remarks>
      Private Function MoveOut(ByVal point As Point4D, ByVal amount As Double) As Point4D
         point.X = point.X - (amount * Me.RackLoc(0).InsertX)
         point.Y = point.Y - (amount * Me.RackLoc(0).InsertY)
         Return point
      End Function


      Public Function GetPressure() As Int32
         'Query the pressure of the oven in units of Torr.   The pressure meter has an address of 1.
         Dim Reply As String = ""
         Dim Pass As Int32
         Dim Pressure As Int32

         For Pass = 0 To 2
            Reply = Me.SendMyCommand("T(" & PressureAddress.ToString & ")")
            If IsDouble(Reply, 0.0, 780.0) = True Then
               Pressure = CType(Reply, Int32)
               Exit For
            End If
         Next

         If Pass = 3 Then
            RaiseEvent TransmissionError(Me, "Pressure from oven " & MyPosition.ToString & " was reported as: " & Reply)
            Pressure = 760
         End If
         Return Pressure
      End Function


      Public Function GetTemperature() As Double
         'Query the temperature of the oven.  The temperature meter has an address of 2.
         Dim Reply As String = ""
         Dim Temperature As Double
         Dim Pass As Int32


         For Pass = 0 To 2
            Reply = Me.SendMyCommand("T(" & TemperatureAddress.ToString & ")")
            If IsDouble(Reply, 0.0, 250.0) = True Then
               Temperature = CType(Reply, Double)
               Exit For
            End If
         Next

         If Pass = 3 Then
            RaiseEvent TransmissionError(Me, "Temperature from oven " & MyPosition.ToString & " was reported as: " & Reply)
            Temperature = 0.0
         End If
         Return Temperature
      End Function



      Public Function SetPressure(ByVal pressure As Int32) As Boolean
         'Set the setpoint of the vacuum controller in units of torr.   Returns True if the value is accepted, otherwise false.
         Dim Reply As String = ""
         Dim Pass As Int32


         pressure = EnforceLimits(pressure, 1, 800)   'Note, 0 is not an allowed value, the USB board hangs when you send 0.

         For Pass = 0 To 2
            Reply = Me.SendMyCommand("S(" & PressureAddress.ToString & "," & pressure.ToString & ")")
            If IsDouble(Reply, 0.0, 800.0) = True Then
               Return True
            End If
         Next

         If Pass = 3 Then
            RaiseEvent TransmissionError(Me, "The pressure setpoint for oven " & MyPosition.ToString & " was not loaded properly, the reply was: " & Reply)
            Return False
         End If
      End Function


      Public Function SetTemperature(ByVal temperature As Double) As Boolean
         'Set the setpoint of the vacuum controller in units of torr.   Returns True if the value is accepted, otherwise false.
         Dim Reply As String = ""
         Dim Pass As Int32

         EnforceLimits(temperature, 0.1, 100.0) 'note 0.0 is not an allowed number, the USB board hangs when you send 0.0.
         For Pass = 0 To 2
            Reply = Me.SendMyCommand("S(" & TemperatureAddress.ToString & "," & temperature.ToString & ")")
            If IsDouble(Reply, 0.0, 100.0) = True Then
               Return True
            End If
         Next

         If Pass = 3 Then
            RaiseEvent TransmissionError(Me, "The temperature setpoint for oven " & MyPosition.ToString & " was not loaded properly, the reply was: " & Reply)
            Return False
         End If
      End Function


      Public Function TestComms() As Boolean
         'Returns true if communications was opened and functioning properly.
         '"?(0)" returns the identity string from the USB board.   Ex:  00000103:OSHA-USB-MG-0-061810
         Dim Reply As String

         If ModuleDefMessages.InTestMode = True Then
            Return True
         End If

         Try
            Reply = Me.SendMyCommand("?(0)")
            Reply = Reply.Substring(5, 3)
            If Reply = MyAddress Then
               Return True
            Else
               Return False
            End If
         Catch ex As Exception
            Return False
         End Try

      End Function

      Public Function SendLiteral(ByVal command As String) As String
         Try
            Return Me.SendMyCommand(command)
         Catch ex As Exception
            Return ""
         End Try
      End Function


      ''' <summary>
      ''' Opens the oven door.
      ''' </summary>
      ''' <returns>True when the door is fully open, or False if the door does not open in 60 seconds.</returns>
      ''' <remarks></remarks>
      Public Function OpenDoor() As Boolean
         Dim Reply As String
         Dim Timeout As DateTime
         Dim FunctionTimeout As Boolean
         Dim LastPressure As Int32
         Dim MyPressure As Int32
         Dim Pass As Int32
         Dim OvenIsConfirmedOpen As Boolean = True


         'First set the vacuum setpoint to 760 to turn off the vacuum valve
         If Me.SetPressure(780) = False Then
            OvenIsConfirmedOpen = False
            FileIO.WriteTextFile(LogFileName, "Failure in OpenDoor function," & MyPosition.ToString & "," & "SetPressure(780)" & vbCrLf, False, True)
         End If

         'Turn heating off
         If OvenIsConfirmedOpen = True Then
            If Me.SetTemperature(0.1) = False Then
               OvenIsConfirmedOpen = False
               FileIO.WriteTextFile(LogFileName, "Failure in OpenDoor function," & MyPosition.ToString & "," & "Me.SetTemperature(0.1)" & vbCrLf, False, True)
            End If
         End If

         'Deenertize the close door solinoid
         If OvenIsConfirmedOpen = True Then
            For Pass = 0 To 2
               Reply = Me.SendMyCommand("L(1)")
               If Reply = "L(1)" Then
                  Exit For
               End If
            Next
            If Pass = 3 Then
               OvenIsConfirmedOpen = False
               FileIO.WriteTextFile(LogFileName, "Failure in OpenDoor function," & MyPosition.ToString & "," & "L(1)" & vbCrLf, False, True)
            End If
         End If

         'Now open the vent solinoid valve
         If OvenIsConfirmedOpen = True Then
            For Pass = 0 To 2
               Reply = Me.SendMyCommand("L(3)")
               If Reply = "L(3)" Then
                  Exit For
               End If
            Next
            If Pass = 3 Then
               OvenIsConfirmedOpen = False
               FileIO.WriteTextFile(LogFileName, "Failure in OpenDoor function," & MyPosition.ToString & "," & "L(3)" & vbCrLf, False, True)
            End If
         End If

         'Now wait until the oven internal pressure stops changing by less than 5 torr in a 3 second period.
         If OvenIsConfirmedOpen = True Then
            LastPressure = 0
            Timeout = Now.AddSeconds(120)
            FunctionTimeout = True
            Do
               DelaySeconds(3)
               MyPressure = Me.GetPressure
               If Abs(MyPressure - LastPressure) <= 3 And MyPressure > 600.0 Then
                  FunctionTimeout = False
                  Exit Do
               Else
                  LastPressure = MyPressure
               End If
            Loop While DateTime.Compare(Now, Timeout) <= 0
            If FunctionTimeout = True Then
               OvenIsConfirmedOpen = False
               FileIO.WriteTextFile(LogFileName, "Failure in OpenDoor function," & MyPosition.ToString & "," & "Timeout for pressure drop." & vbCrLf, False, True)
            End If
         End If

         'Now pressurize the pistons to open the door
         If OvenIsConfirmedOpen = True Then
            For Pass = 0 To 2
               Reply = Me.SendMyCommand("H(0)")
               If Reply = "H(0)" Then
                  Exit For
               End If
            Next
            If Pass = 3 Then
               OvenIsConfirmedOpen = False
               FileIO.WriteTextFile(LogFileName, "Failure in OpenDoor function," & MyPosition.ToString & "," & "H(0)" & vbCrLf, False, True)
            End If
         End If

         'Now monitor for the proximity sensor to indicate that the door is fully open.
         If OvenIsConfirmedOpen = True Then
            Timeout = Now.AddSeconds(60)
            FunctionTimeout = True
            Do
               DelaySeconds(3)
               If TestOpenSensor() = True Then
                  FunctionTimeout = False
                  MyDoorState = DoorLoc.Open
                  Exit Do
               End If
            Loop While DateTime.Compare(Now, Timeout) <= 0
            If FunctionTimeout = True Then
               OvenIsConfirmedOpen = False
               FileIO.WriteTextFile(LogFileName, "Failure in OpenDoor function," & MyPosition.ToString & "," & "Timeout to detect open proximidiy sensor." & vbCrLf, False, True)
            End If
         End If

         'Now close the vent solinoid valve
         If OvenIsConfirmedOpen = True Then
            For Pass = 0 To 2
               Reply = Me.SendMyCommand("H(3)")
               If Reply = "H(3)" Then
                  Exit For
               End If
            Next
            If Pass = 3 Then
               OvenIsConfirmedOpen = False
               FileIO.WriteTextFile(LogFileName, "Failure in OpenDoor function," & MyPosition.ToString & "," & "H(3)" & vbCrLf, False, True)
            End If
         End If

         If OvenIsConfirmedOpen = False Then
            If MessageBox.Show("The system can not confirm that the door for Oven " & MyPosition.ToString & " is open.    Is the door open?", "Confirm Door Open", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
               If MessageBox.Show("Do you want to continue the proceedure?", "Continue Proceedure", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                  CloseVentValve()
                  OvenIsConfirmedOpen = True
               Else
                  Throw New System.Exception("User abort requested.")
               End If
            Else
               MessageBox.Show("The application must terminate.", "Oven Door Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
               Throw New System.Exception("User abort requested.")
            End If
         End If

         Return OvenIsConfirmedOpen
      End Function


      ''' <summary>
      ''' Closes the vacuum oven door.
      ''' </summary>
      ''' <param name="returnWithoutConfirmation">If set to True, the function returns without confirmation that the door completed it's close motion.
      ''' A value of False waits until the door is confirmed closed.</param>
      ''' <remarks></remarks>
      Public Function CloseDoor(Optional ByVal returnWithoutConfirmation As Boolean = False) As Boolean
         Dim Reply As String
         Dim Timeout As DateTime
         Dim FunctionTimeout As Boolean
         Dim Pass As Int32
         Dim OvenIsConfirmedClosed As Boolean = True


         If ModuleDefMessages.InTestMode = True Then
            Return True
         End If

         'Turn off the door open solinoid
         For Pass = 0 To 2
            Reply = Me.SendMyCommand("L(0)")
            If Reply = "L(0)" Then
               Exit For
            End If
         Next
         If Pass = 3 Then
            OvenIsConfirmedClosed = False
            FileIO.WriteTextFile(LogFileName, "Failure in CloseDoor function," & MyPosition.ToString & "," & "L(0)" & vbCrLf, False, True)
         End If
         DelayMS(7000)

         'Energize the door close solinoid.
         If OvenIsConfirmedClosed = True Then
            For Pass = 0 To 2
               Reply = Me.SendMyCommand("H(1)")
               If Reply = "H(1)" Then
                  Exit For
               End If
            Next
            If Pass = 3 Then
               OvenIsConfirmedClosed = False
               FileIO.WriteTextFile(LogFileName, "Failure in CloseDoor function," & MyPosition.ToString & "," & "H(1)" & vbCrLf, False, True)
            End If
         End If

         If returnWithoutConfirmation = False And OvenIsConfirmedClosed = True Then
            'Now monitor for the proximity sensor to indicate that the door is fully closed.
            Timeout = Now.AddSeconds(60)
            FunctionTimeout = True

            'Wait for the close door sensor to report closed.
            If IsCloseSensorOverRiden = True Then
               DelaySeconds(15)  'It takes 8 seconds to close the oven door
            Else
               Do
                  DelaySeconds(3)
                  If TestCloseSensor() = True Then
                     FunctionTimeout = False
                     MyDoorState = DoorLoc.Closed
                     Exit Do
                  End If
               Loop While DateTime.Compare(Now, Timeout) <= 0
               If FunctionTimeout = True Then
                  If MessageBox.Show("The system can not confirm that the door for Oven " & MyPosition.ToString & " is closed.  If the door is closed, you can override the proximity sensor that detects the door closed state.   Do you want to override this sensor?", "Proximity Sensor Error", MessageBoxButtons.YesNo, MessageBoxIcon.Hand) = DialogResult.Yes Then
                     Reply = InputBox("Call Scott Jones for password.   Enter password to confirm change.", "Password Protected Variable")
                     If Reply.ToUpper = "PASSWORD" Then
                        IsCloseSensorOverRiden = True
                     Else
                        OvenIsConfirmedClosed = False
                        FileIO.WriteTextFile(LogFileName, "Failure in CloseDoor function," & MyPosition.ToString & "," & "Door close sensor timeout" & vbCrLf, False, True)
                     End If
                  Else
                     OvenIsConfirmedClosed = False
                     FileIO.WriteTextFile(LogFileName, "Failure in CloseDoor function," & MyPosition.ToString & "," & "Door close sensor timeout" & vbCrLf, False, True)
                  End If
               End If
            End If

            'Deenertize the close door solinoid.   
            If OvenIsConfirmedClosed = True Then
               For Pass = 0 To 2
                  Reply = Me.SendMyCommand("L(1)")
                  If Reply = "L(1)" Then
                     Exit For
                  End If
               Next
               If Pass = 3 Then
                  OvenIsConfirmedClosed = False
                  FileIO.WriteTextFile(LogFileName, "Failure in CloseDoor function," & MyPosition.ToString & "," & "L(1)" & vbCrLf, False, True)
               End If
            End If
         End If

         If OvenIsConfirmedClosed = False Then
            If MessageBox.Show("The system can not confirm that the door for Oven " & MyPosition.ToString & " is closed.  If the door is closed, you can override the proximity sensor that detects the door closed state.   Do you want to override this sensor?", "Proximity Sensor Error", MessageBoxButtons.YesNo, MessageBoxIcon.Hand) = DialogResult.Yes Then
               IsCloseSensorOverRiden = True
            End If
         End If

         Return OvenIsConfirmedClosed
      End Function


      Public Function CloseDoorSetVacuum(ByVal vacuumPressure As Int32) As Boolean
         Dim Reply As String
         Dim Timeout As DateTime
         Dim FunctionTimeout As Boolean
         Dim Pressure As Int32
         Dim Pass As Int32
         Dim DoorConfirmedClosed As Boolean = True


         If ModuleDefMessages.InTestMode = True Then
            Return True
         End If

         'Turn off the door open solinoid
         For Pass = 0 To 2
            Reply = Me.SendMyCommand("L(0)")
            If Reply = "L(0)" Then
               Exit For
            End If
         Next
         If Pass = 3 Then
            DoorConfirmedClosed = False
            FileIO.WriteTextFile(LogFileName, "Failure in CloseDoorSetVacuum function," & MyPosition.ToString & "," & "L(0)" & vbCrLf, False, True)
         End If
         DelayMS(7000)

         'Energize the door close solinoid.
         If DoorConfirmedClosed = True Then
            For Pass = 0 To 2
               Reply = Me.SendMyCommand("H(1)")
               If Reply = "H(1)" Then
                  Exit For
               End If
            Next
            If Pass = 3 Then
               DoorConfirmedClosed = False
               FileIO.WriteTextFile(LogFileName, "Failure in CloseDoorSetVacuum function," & MyPosition.ToString & "," & "H(1)" & vbCrLf, False, True)
            End If
         End If

         'Test that the door is closed
         If DoorConfirmedClosed = True Then
            Timeout = Now.AddSeconds(60)
            FunctionTimeout = True
            Do
               DelaySeconds(3)
               If TestCloseSensor() = True Then
                  FunctionTimeout = False
                  MyDoorState = DoorLoc.Closed
                  Exit Do
               End If
            Loop While DateTime.Compare(Now, Timeout) <= 0
            If FunctionTimeout = True Then
               If MessageBox.Show("The system can not confirm that the door for Oven " & MyPosition.ToString & " is closed.  If the door is closed, you can override the proximity sensor that detects the door closed state.   Do you want to override this sensor?", "Proximity Sensor Error", MessageBoxButtons.YesNo, MessageBoxIcon.Hand) = DialogResult.Yes Then
                  Reply = InputBox("Call Scott Jones for password.   Enter password to confirm change.", "Password Protected Variable")
                  If Reply.ToUpper = "PASSWORD" Then
                     IsCloseSensorOverRiden = True
                  Else
                     DoorConfirmedClosed = False
                     FileIO.WriteTextFile(LogFileName, "Failure in CloseDoorSetVacuum function," & MyPosition.ToString & "," & "Timeout in close door sensor." & vbCrLf, False, True)
                  End If
               Else
                  RaiseEvent TransmissionError(Me, "The door for Oven " & MyPosition.ToString & " failed to close in the allotted time of 60 seconds.")
                  DoorConfirmedClosed = False
                  FileIO.WriteTextFile(LogFileName, "Failure in CloseDoorSetVacuum function," & MyPosition.ToString & "," & "Timeout in close door sensor." & vbCrLf, False, True)
               End If
            End If
         End If

         'Turn on the vacuum
         If DoorConfirmedClosed = True Then
            If vacuumPressure > 500 Then
               vacuumPressure = 500
            End If
            Timeout = Now.AddSeconds(120)
            FunctionTimeout = True

            Me.SetPressure(vacuumPressure)
            Do
               DelaySeconds(3)
               Pressure = Me.GetPressure
               If Pressure < 650 Then
                  FunctionTimeout = False
                  Exit Do
               End If
            Loop While DateTime.Compare(Now, Timeout) <= 0

            If FunctionTimeout = True Then
               RaiseEvent TransmissionError(Me, "The formation of a vacuum in oven " & MyPosition.ToString & " can not be confirmed.")
               DoorConfirmedClosed = False
               FileIO.WriteTextFile(LogFileName, "Failure in CloseDoorSetVacuum function," & MyPosition.ToString & "," & "Timeout in set vacuum." & vbCrLf, False, True)
            End If
         End If

         'Deenertize the close door solinoid
         If DoorConfirmedClosed = True Then
            For Pass = 0 To 2
               Reply = Me.SendMyCommand("L(1)")
               If Reply = "L(1)" Then
                  Exit For
               End If
            Next
            If Pass = 3 Then
               DoorConfirmedClosed = False
               FileIO.WriteTextFile(LogFileName, "Failure in CloseDoorSetVacuum function," & MyPosition.ToString & "," & "L(1)" & vbCrLf, False, True)
            End If
         End If

         If DoorConfirmedClosed = False Then
            If MessageBox.Show("The system can not confirm that the door for Oven " & MyPosition.ToString & " is closed.  If the door is closed, you can override the proximity sensor that detects the door closed state.   Do you want to override this sensor?", "Proximity Sensor Error", MessageBoxButtons.YesNo, MessageBoxIcon.Hand) = DialogResult.Yes Then
               IsCloseSensorOverRiden = True
            End If
         End If

         Return DoorConfirmedClosed
      End Function


      Sub OpenVentValve()
         Dim Reply As String
         Dim Pass As Int32

         For Pass = 0 To 2
            Reply = Me.SendMyCommand("L(3)")
            If Reply = "L(3)" Then
               Exit For
            End If
         Next
         If Pass = 3 Then
            RaiseEvent TransmissionError(Me, "Can not open the vent valve for Oven " & MyPosition.ToString)
            MyPort.Close()
            For Pass = 0 To 2
               Reply = Me.SendMyCommand("L(3)")
               If Reply = "L(3)" Then
                  Exit For
               End If
            Next
         End If
      End Sub

      Sub CloseVentValve()
         Dim Reply As String
         Dim pass As Int32

         For pass = 0 To 2
            Reply = Me.SendMyCommand("H(3)")
            If Reply = "H(3)" Then
               Exit For
            End If
         Next
         If pass = 3 Then
            RaiseEvent TransmissionError(Me, "Can not close the vent valve for Oven " & MyPosition.ToString)
            MyPort.Close()
            For pass = 0 To 2
               Reply = Me.SendMyCommand("H(3)")
               If Reply = "H(3)" Then
                  Exit For
               End If
            Next
         End If

      End Sub


      Public Function TestOpenSensor() As Boolean
         'Returns true when the proximity sensor on the oven door confirms that it is open.
         Dim Reply As String
         Dim Pass As Int32
         Dim State As Boolean = False

         For Pass = 0 To 2
            Reply = Me.SendMyCommand("I(0)")
            If Reply = "0" Then
               State = True
               Exit For
            ElseIf Reply = "1" Then
               Exit For
            End If
         Next

         Return State
      End Function


      Public Function TestCloseSensor() As Boolean
         'Returns true when the proximity sensor on the oven door confirms that it is closed.
         Dim Reply As String
         Dim Pass As Int32
         Dim State As Boolean = False

         If IsCloseSensorOverRiden = True Then
            State = True
         Else
            For Pass = 0 To 2
               Reply = Me.SendMyCommand("I(1)")
               If Reply = "0" Then
                  State = True
                  Exit For
               ElseIf Reply = "1" Then
                  Exit For
               End If
            Next
         End If

         Return State
      End Function



      ''' <summary>
      ''' Tests communications with the ovens and closes the door if it is open
      ''' </summary>
      ''' <param name="doorsAreClosed">It takes 15 seconds to close the door.  If you know the door is closed and don't want this delay, set this parameter to True.</param>
      ''' <returns>True if communcations was opened, False otherwise.</returns>
      ''' <remarks></remarks>
      Public Function Init(Optional ByVal doorsAreClosed As Boolean = False) As Boolean
         Dim Reply As String

         If TestComms() = False Then   'This queries the serial number of the USB board.  This verifies communications and that the proper serial port is connected to the oven.
            Return False
         End If

         If Me.SetPressure(780) = False Then
            Return False
         End If

         If Me.SetTemperature(0.1) = False Then
            Return False
         End If

         'Deenertize the vent valve
         Reply = Me.SendMyCommand("H(3)")
         If Reply <> "H(3)" Then
            Return False
         End If

         'Deenertize the open door solinoid
         Reply = Me.SendMyCommand("L(0)")
         If Reply <> "L(0)" Then
            Return False
         End If

         'Energize the close door solinoid so you can check the state of the close door piston.
         Reply = Me.SendMyCommand("H(1)")
         If Reply <> "H(1)" Then
            Return False
         End If

         'Check the door state sensor
         DelayMS(4000)
         If GetDoorState() = DoorLoc.Closed Then
            MyDoorState = DoorLoc.Closed
         Else
            MyDoorState = DoorLoc.Unconfirmed
         End If

         'Deenertize the close door solinoid.
         Reply = Me.SendMyCommand("L(1)")
         If Reply <> "L(1)" Then
            Return False
         End If

         Return True
      End Function

   End Class



   Class GripperDef
      Event TransmissionError(ByVal sender As Object, ByVal message As String)

      Public MyDriver As JkemMotorDef
      Public MyPort As System.IO.Ports.SerialPort


      Sub New()
         'When the carosel is created, it creates a JkemMotorDef.   The gripper is operated by this driver, so you need a reference to it
         MyDriver = Carosel.MyDriver
      End Sub

      Sub Open()
         MyDriver.SetOutput(2, DriverState.Power_Off)  'Turn off the close solinoid
         MyDriver.SetOutput(1, DriverState.Power_On)
         DelayMS(750)
         MyDriver.SetOutput(1, DriverState.Power_Off)
      End Sub

      Sub Close()
         MyDriver.SetOutput(2, DriverState.Power_On)
         DelayMS(500)
      End Sub


      ''' <summary>
      ''' Returns the ADC valve from a single read of the gripper finger sensor. 
      ''' </summary>
      ''' <returns>Returns the ADC valve from the finger sensors.  Range: 0-1023.</returns>
      ''' <remarks></remarks>
      Function ReadSensor() As Int32
         Dim Value As Int32

         Value = MyDriver.GetADC(0)
         Return Value
      End Function


      ''' <summary>
      ''' Returns the averaged value from the grippers analog sensor.
      ''' </summary>
      ''' <param name="reads">The number of reads to average.</param>
      ''' <returns>The averaged ADC reading from the grippers sensor.  Range: 0-1023.</returns>
      ''' <remarks></remarks>
      Function ReadSensorAverage(Optional ByVal reads As Int32 = 8) As Int32
         Dim Pass As Int32
         Dim Value As Int32 = 0

         For Pass = 1 To reads
            Value += MyDriver.GetADC(0)
         Next
         Value = ((Value + (reads \ 2)) \ reads)

         Return Value
      End Function

   End Class



   Class CaroselDef
      'The carosel has a J-KEM motor driver.   The IO of the driver is used to operate the gripper on the Epson arm
      Event TransmissionError(ByVal sender As Object, ByVal message As String)

      Public RackLoc(4, 2) As MySpace  'This holds the coordinates of the 3 corners of the carosel.
      'Element 0 is the top cell of column 1, 1 is top cell of column 8, 2 is bottom cell of column 1, 3 is bottom cell of column 8
      'Public ZRowOffset As Double
      Private IsInitialized As Boolean
      Private Position As Int32  'Current carosel setp position


      Structure CRack
         Dim OriginX As Integer    'This is the J-KEM driver position needed to place column in front of the gripper
         Dim DeltaX As Double    'Steps to the next column
         Dim SpecialCoordinateRack2 As Int32    'Rack 2 is split around the home sensor.  See comment in PositionRack()
      End Structure

      Public WithEvents MyDriver As New JkemMotorDef(1, MyPort)
      Public MyPort As System.IO.Ports.SerialPort
      Public RackOrigin(5) As CRack     'The starting positions of the racks



      Sub New(ByRef serialPort As Ports.SerialPort)
         MyPort = serialPort
         MyPort.BaudRate = 9600
         MyPort.ReadTimeout = 1000
         MyPort.NewLine = vbCr
         MyDriver.SerialPort = MyPort 'The motordriver is created (above) while MyPort is still 'nothing', so you have to assign it here
         IsInitialized = False
      End Sub


      Public ReadOnly Property PortName() As String
         Get
            Return MyPort.PortName
         End Get
      End Property


      Public Function TestComms() As Boolean
         Dim Reply As String
         Dim Pass As Int32

         For Pass = 0 To 2
            Try
               Reply = MyDriver.SendLiteral(":1ST")   'Query motor status
               If IsInteger(Reply, 1, 20) Then
                  TestComms = True
                  Exit For
               Else
                  TestComms = False
               End If
            Catch ex As Exception
            End Try
         Next
      End Function


      Public Function Init() As Boolean
         'Remember that the gripper is operated from the IO of the driver in the carosel

         If IsInitialized = False Then
            'Turn off the close gripper solinoid
            MyDriver.SetOutput(2, DriverState.Power_Off)
            MyDriver.Home()
            Me.Position = 0
            IsInitialized = True
         End If

         Return IsInitialized
      End Function


      Public Sub Home()
         If IsInitialized = False Then
            Me.Init()
         Else
            MyDriver.Home()
            Me.Position = 0
         End If
      End Sub


      Public Sub Move(ByVal pos As Double)
         If IsInitialized = False Then
            Me.Init()
            MyDriver.Move(pos)
         End If
         If pos <> Me.Position Then
            MyDriver.Move(pos)
            Me.Position = CType(pos, Int32)
         End If
      End Sub


      Public Function PositionRack(ByVal rack As Int32, ByVal cell As Int32) As Double
         'To position the carosel for gripper access, go to the RackOrigin, then add in the correct number of RackOrigin.DeltaX for the number of coumns
         'that you have to advance.   Rack 2 is a special case.  Columns 1-5 have normal access, but when you move from column 5 to column 6, you'd go through
         'the home sensor.  What you need to do to get to columns 6-8, is 1) home the carosel, then position the carosel at RackOrigin.SpecialCoordinateRack2,
         'this will align you with column 6 or rack 2.   From that point, to get to column 7 add in 1 DeltaX, to 8 add in 2 DeltaX
         Dim Position As Double
         Dim Col As Int32

         Try
            'Position the X-axis (the rotary axis)
            Col = (cell - 1) \ 12
            If rack = 2 And Col = 7 Then
               If RackOrigin(2).SpecialCoordinateRack2 <> Me.Position Then
                  Me.Home()
               End If
               Position = RackOrigin(2).SpecialCoordinateRack2
            Else
               Position = RackOrigin(rack).OriginX + (Col * RackOrigin(rack).DeltaX)
            End If
            Me.Move(Position)
         Catch ex As Exception
            frmMain.ErrorDump(ex.Message, ex.TargetSite.Name, rack.ToString & vbCrLf & cell.ToString)
            MessageBox.Show("Error in function PositionRack().   Tell Scott to call J-KEM with this error message." & vbCrLf & rack.ToString & "  " & cell.ToString, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try

         Return Position
      End Function


      'When the carosel is in use, stiffen the holding current
      Public Sub PrepCaroselForAccess()
         Me.SetHoldingCurrent(400)
      End Sub

      Public Sub ReleaseCaroselFromAccess()
         Me.ReleaseMotor() 'Turn off all holding current
      End Sub


      ''' <summary>
      ''' This sets the holding current to 0.5 amps.  Set to high current when you are acessign the rack
      ''' </summary>
      ''' <remarks></remarks>
      Public Sub SetHoldingCurrent(ByVal current_mA As Int32)
         MyDriver.SendLiteral(":1HC" & current_mA.ToString)
      End Sub


      ''' <summary>
      ''' Turns off all holding current in the motor driver
      ''' </summary>
      ''' <remarks></remarks>
      Public Sub ReleaseMotor()
         MyDriver.SendLiteral(":1RM")
      End Sub



      Private Sub JkemDriver_TransmissionError(ByVal sender As ModuleDefinitions.JkemMotorDef, ByVal message As String) Handles MyDriver.TransmissionError

         If MessageBox.Show("The carosel sent and error message." & vbCrLf & message & vbCrLf & "Do you want to abort the application?", "Axis Communication Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1) = DialogResult.Yes Then
            Application.DoEvents()
            Application.Exit()
         Else
            RaiseEvent TransmissionError(Me, "Carosel command error.  Message: " & message)
         End If
      End Sub


      ''' <summary>
      ''' This function is used in the Manual Ovens screen only to determine the amount of ADC drop when the gripper is 3mm over a filter
      ''' </summary>
      ''' <returns>The value of hte ADC when it is 3mm over a filter.</returns>
      ''' <remarks></remarks>
      Public Function DetermineOpticalSensitivity() As Int32
         Dim MyPoint As Point4D
         Dim AirReading As Int32
         Dim EdgeReading As Int32
         Dim Pass As Int32

         GoToCaroselSafePoint()
         Gripper.Open()
         MyPoint = GetFilterLocation(1, 1)
         MyPoint = MoveOut(1, MyPoint, 2.0)
         Arm.GoToPoint(MyPoint)
         AirReading = Gripper.ReadSensorAverage
         For Pass = 1 To 8
            MyPoint = MoveIn(1, MyPoint, 0.5)
            Arm.GoToPoint(MyPoint)
            EdgeReading = Gripper.ReadSensorAverage
            If EdgeReading < (AirReading - 6) Then
               MyPoint = MoveIn(1, MyPoint, 2.75)
               Arm.GoToPoint(MyPoint)
               EdgeReading = Gripper.ReadSensorAverage
               Exit For
            End If
         Next
         MyPoint = MoveOut(1, MyPoint, 100.0)
         Arm.GoToPoint(MyPoint)

         Return EdgeReading
      End Function


      ''' <summary>
      ''' Picks up a filter from the carosel.
      ''' </summary>
      ''' <param name="rack">Carosel rack.  Range: 1-5</param>
      ''' <param name="cell">Filter position.  Range: 1-96.</param>
      ''' <remarks></remarks>
      Public Function GetFilter(ByVal rack As Int32, ByVal cell As Int32) As Boolean
         Dim MyPoint As Point4D
         Dim AirADC As Int32
         Dim ADC As Int32
         Dim Pass As Int32
         Dim FilterLocated As Boolean

         If ModuleDefMessages.InTestMode = True Then
            Return True
         End If

         Try
            PositionRack(rack, cell)   'Positions the correct column of the carosel in front of the gripper
            GoToCaroselSafePoint()
            Gripper.Open()
            MyPoint = GetFilterLocation(rack, cell)
            MyPoint = MoveOut(rack, MyPoint, 100.0)
            Arm.GoToPoint(MyPoint)

            AirADC = Gripper.ReadSensorAverage
            frmMain.LastAirADCReading = AirADC  'Save the last reading.  It is used as a test to make sure the gripper released the filter later
            MyPoint = GetFilterLocation(rack, cell)
            MyPoint = MoveIn(rack, MyPoint, 3.0) 'take the gripper 3mm over the filter
            Arm.GoToPoint(MyPoint)
5:
            ADC = Gripper.ReadSensorAverage
            'If you don't have enough drop in the ADC, then continue to move the gripper into the oven.   You can go a maximum of 10mm pas
            'the starting location of the filter.
            FilterLocated = False
            For Pass = 1 To 8
               If ADC > (AirADC - frmMain.OpticalSensor3mmDrop) Then 'The ADC reading drops by about 90-100 counts when you are 2mm over the filter edge.  2013.
                  MyPoint = MoveIn(rack, MyPoint, 0.5)
                  Arm.GoToPoint(MyPoint)
                  ADC = Gripper.ReadSensorAverage
               Else
                  FilterLocated = True
                  Exit For
               End If
            Next

            If FilterLocated = True Then
               MyPoint.Z += 1.0   'Its good to be 1mm high on the Z axis
               Arm.GoToPoint(MyPoint)
               Gripper.Close()

               'To get the fitler out, go up 7.0mm and then out 100mm
               MyPoint.Z = MyPoint.Z + 7.0
               MyPoint = Me.MoveOut(rack, MyPoint, 3.0)
               Arm.GoToPoint(MyPoint)
               MyPoint = MoveOut(rack, MyPoint, 10.0)
               Arm.GoToPoint(MyPoint)
               MyPoint = MoveOut(rack, MyPoint, 90.0)
               MyPoint.Z = MyPoint.Z + 15.0
               Arm.GoToPoint(MyPoint)
            Else
               MyPoint = MoveOut(rack, MyPoint, 150.0)
               Arm.GoToPoint(MyPoint)
            End If
         Catch ex As Exception
            frmMain.ErrorDump(ex.Message, ex.TargetSite.Name, rack.ToString & vbCrLf & cell.ToString)
            MessageBox.Show("Error in function PutFilter().   Tell Scott to call J-KEM with this error message." & vbCrLf & rack.ToString & "  " & cell.ToString, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try

         Return FilterLocated
      End Function


      ''' <summary>
      ''' Places the filter in the carosel rack.  Takes the arm to a safe position for access.
      ''' </summary>
      ''' <param name="rack">Carosel rack.  Range: 1-5.</param>
      ''' <param name="cell">The cell in the rack to go to.  Range: 1-96.</param>
      ''' <remarks></remarks>
      Public Sub PutFilter(ByVal rack As Int32, ByVal cell As Int32)
         Dim MyPoint As Point4D
         Dim MyADC As Int32

         If ModuleDefMessages.InTestMode = True Then
            Return
         End If

         Try
            PositionRack(rack, cell)   'Positions the correct column of the carosel in front of the gripper

            Me.GoToCaroselSafePoint()
            MyPoint = GetFilterLocation(rack, cell)
            MyPoint.Z += 12.0
            MyPoint = MoveOut(rack, MyPoint, 40.0)
            Arm.GoToPoint(MyPoint)
            MyPoint = GetFilterLocation(rack, cell)
            MyPoint.Z += 6.5
            MyPoint = Me.MoveIn(rack, MyPoint, 3.0)
            Arm.GoToPoint(MyPoint)
            MyPoint.Z -= 3.0
            Arm.GoToPoint(MyPoint)
            Gripper.Open()
            MyPoint.Z -= 4.0
            Arm.GoToPoint(MyPoint)
            'This section tests to make sure the filter is not stuck to the gripper
            MyPoint = GetFilterLocation(rack, cell)
            MyPoint = MoveOut(rack, MyPoint, 12.0)
            Arm.GoToPoint(MyPoint)
            MyADC = Gripper.ReadSensorAverage
            If MyADC < (frmMain.LastAirADCReading - frmMain.OpticalSensor3mmDrop \ 2) Then
               MyPoint = GetFilterLocation(rack, cell)
               MyPoint = Me.MoveIn(rack, MyPoint, 2.0)
               MyPoint.Z += 4.0
               Arm.GoToPoint(MyPoint)
               MyPoint.Z -= 4.5
               Arm.GoToPoint(MyPoint)
               MyPoint = Me.MoveOut(rack, MyPoint, 15.0)
               Arm.GoToPoint(MyPoint)
               If Gripper.ReadSensorAverage < (frmMain.OpticalSensor3mmDrop \ 2) Then
                  MyPoint = GetFilterLocation(rack, cell)
                  MyPoint = Me.MoveIn(rack, MyPoint, 2.0)
                  MyPoint.Z += 4.0
                  Arm.GoToPoint(MyPoint)
                  MyPoint.Z -= 6.5
                  Arm.GoToPoint(MyPoint)
                  MyPoint = Me.MoveOut(rack, MyPoint, 20.0)
                  Arm.GoToPoint(MyPoint)
               End If
               frmMain.rtbMessages.Text += vbCrLf & "Filter " & cell.ToString & " on Carousel Rack " & rack.ToString & " stuck to the gripper."
            End If
            Gripper.Close()
            MyPoint = GetFilterLocation(rack, cell)
            MyPoint.Z -= 5.0
            MyPoint = MoveOut(rack, MyPoint, 2.0)
            Arm.GoToPoint(MyPoint)

            MyPoint = GetFilterLocation(rack, cell)
            MyPoint = MoveOut(rack, MyPoint, 150.0)   'Go to a safe position
            Arm.GoToPoint(MyPoint)
         Catch ex As Exception
            frmMain.ErrorDump(ex.Message, ex.TargetSite.Name, rack.ToString & vbCrLf & cell.ToString)
            MessageBox.Show("Error in function PutFilter().   Tell Scott to call J-KEM with this error message." & vbCrLf & rack.ToString & "  " & cell.ToString, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try
      End Sub


      Private Function GetFilterLocation(ByVal rack As Int32, ByVal cell As Int32) As Point4D
         'Locatest the coordinates of a filter.   rack is the rack number in the carosel (1-5) and pos is the filter position from 1 to 96.   
         'The Carosel is not perfectly round.  The position of the top filter in columns 1 and 8 are mapped.  
         'As the carosel advances from colum to column, it may not advance to the exact position of the last column, so you must correct for this.
         'First, the coordinate is corrected for the column, then its corrected for skew as you go down the column.
         Dim Col As Int32
         Dim Row As Int32
         Dim NewPoint As Point4D

         Try
            rack -= 1    'Correct for 0 bases array
            cell -= 1
            Col = cell \ 12
            Row = cell Mod 12
            NewPoint.X = Me.RackLoc(rack, 0).X + (((Me.RackLoc(rack, 1).X - Me.RackLoc(rack, 0).X) / 7.0) * Col)  'This is the adjustment as you go from column to column
            NewPoint.X = NewPoint.X + ((Me.RackLoc(rack, 2).X - Me.RackLoc(rack, 0).X) / 11.0) * Row   'Now adjust for skew as you go down a column

            NewPoint.Y = Me.RackLoc(rack, 0).Y + (((Me.RackLoc(rack, 1).Y - Me.RackLoc(rack, 0).Y) / 7.0) * Col)
            NewPoint.Y = NewPoint.Y + ((Me.RackLoc(rack, 2).Y - Me.RackLoc(rack, 0).Y) / 11.0) * Row

            NewPoint.Z = Me.RackLoc(rack, 0).Z + (((Me.RackLoc(rack, 1).Z - Me.RackLoc(rack, 0).Z) / 7.0) * Col)
            NewPoint.Z = NewPoint.Z + ((Me.RackLoc(rack, 2).Z - Me.RackLoc(rack, 0).Z) / 11.0) * Row

            NewPoint.U = Me.RackLoc(rack, 0).U + (((Me.RackLoc(rack, 1).U - Me.RackLoc(rack, 0).U) / 7.0) * Col)
         Catch ex As Exception
            frmMain.ErrorDump(ex.Message, ex.TargetSite.Name, rack.ToString & vbCrLf & cell.ToString)
            MessageBox.Show("Error in function PutFilter().   Tell Scott to call J-KEM with this error message." & vbCrLf & rack.ToString & "  " & cell.ToString, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try

         Return NewPoint
      End Function


      Private Sub GoToCaroselSafePoint()
         'A location that is safe to begin your move to the carosel from.  Go to this point before accessing the carosel
         Dim MyPoint As Point4D

         MyPoint = Arm.Position
         If MyPoint.Z < -170.0 Then
            MyPoint.Z = -170.0
            Arm.GoToPoint(MyPoint)
         ElseIf MyPoint.Z > -120.0 Then
            MyPoint.Z = -120.0
            Arm.GoToPoint(MyPoint)
         End If

         If MyPoint.X > 0.0 Then
            MyPoint.X = 0.0
            MyPoint.Y = 400.0
            MyPoint.U = 275.0
            Arm.GoToPoint(MyPoint)
         End If

         MyPoint.X = -540.0
         MyPoint.Y = -175.0
         MyPoint.U = 275.0
         Arm.GoToPoint(MyPoint)
      End Sub


      ''' <summary>
      ''' Moves the arm to the right by the specifed amount.  It adjusts only the X and Y coordinates.
      ''' </summary>
      ''' <param name="rack">Rack number, 1-5.</param>
      ''' <param name="point">Current coordinate to adjust.</param>
      ''' <param name="amount">Amount to move to the right from the corrent coordinate, in millimeters.</param>
      ''' <returns>New point coordiante.</returns>
      ''' <remarks></remarks>
      Private Function MoveRight(ByVal rack As Int32, ByVal point As Point4D, ByVal amount As Double) As Point4D
         'This function is correct.  Remember you are adjusting orthoganlly to the values stored in InsertX and InsertY

         rack -= 1  'Adjust for 0 based array
         point.X = point.X + (amount * Me.RackLoc(rack, 0).InsertY) 'The InsertY is the same for all racks
         point.Y = point.Y + (amount * -Me.RackLoc(rack, 0).InsertX) 'The InsertX is the same for all racks

         Return point
      End Function


      Private Function MoveLeft(ByVal rack As Int32, ByVal point As Point4D, ByVal amount As Double) As Point4D
         rack -= 1  'Adjust for 0 based array
         point.X = point.X + (amount * -Me.RackLoc(rack, 0).InsertY)
         point.Y = point.Y + (amount * Me.RackLoc(rack, 0).InsertX)

         Return point
      End Function


      ''' <summary>
      ''' Moves the arm into the oven by the amount specifed in millimeters.
      ''' </summary>
      ''' <param name="rack">Rack number, 1-5.</param>
      ''' <param name="point">The starting point of the move.</param>
      ''' <param name="amount">The number of millimeters to move the arm into the oven.</param>
      ''' <returns>The end point of the move.</returns>
      ''' <remarks></remarks>
      Private Function MoveIn(ByVal rack As Int32, ByVal point As Point4D, ByVal amount As Double) As Point4D
         rack -= 1  'Adjust for 0 based array
         point.X = point.X + (amount * Me.RackLoc(rack, 0).InsertX)
         point.Y = point.Y + (amount * Me.RackLoc(rack, 0).InsertY)

         Return point
      End Function


      ''' <summary>
      ''' Moves the arm out the oven by the amount specifed in millimeters.
      ''' </summary>
      ''' <param name="rack">Rack number, 1-5.</param>
      ''' <param name="point">The starting point of the move.</param>
      ''' <param name="amount">The number of millimeters to move the arm out from the Carosel.</param>
      ''' <returns>The end point of the move.</returns>
      ''' <remarks></remarks>
      Private Function MoveOut(ByVal rack As Int32, ByVal point As Point4D, ByVal amount As Double) As Point4D
         rack -= 1  'Adjust for 0 based array
         point.X = point.X - (amount * Me.RackLoc(rack, 0).InsertX)
         point.Y = point.Y - (amount * Me.RackLoc(rack, 0).InsertY)

         Return point
      End Function

   End Class




   Class BarCodeDef
      Public MyPort As System.IO.Ports.SerialPort
      Public Location As Point4D 'This is the location to position the filter at to read it.

      Event TransmissionError(ByVal sender As Object, ByVal message As String)


      Sub New(ByRef serialPort As Ports.SerialPort)
         MyPort = serialPort
         MyPort.BaudRate = 115200
         MyPort.ReadTimeout = 2500
         MyPort.NewLine = vbCr
      End Sub


      Public ReadOnly Property PortName() As String
         Get
            Return MyPort.PortName
         End Get
      End Property


      Public Function Send(ByRef command As String) As String
         Dim Reply As String


         If ModuleDefMessages.InTestMode = True Then
            DelayMS(100)
            Return "Hello"
         End If

         If ModuleDefMessages.AbortRequested = True Then
            Throw New System.Exception("User requested abort.")
         End If

         Try
            If MyPort.IsOpen = False Then
               Try
                  MyPort.Open()
               Catch ex As Exception
                  Return "Error"
               End Try
            End If

            'If you send commands to the barcode too fast it hangs
            MyPort.DiscardInBuffer()
            MyPort.DiscardOutBuffer()
            MyPort.Write(command)   'The end of command indicator is the '>'
            Try
               Reply = MyPort.ReadLine()  'End of line charactor is Cr
            Catch ex As Exception   'Try to send the command once again.
               Reply = "Error"
            End Try

         Catch ex As Exception
            Reply = "Error"
         End Try

         'Let the UI Catch up
         Application.DoEvents()
         Return Reply
      End Function


      Private Function SendMyCommand(ByRef command As String) As String
         Dim Reply As String = ""
         Dim Pass As Int32

         Try
            For Pass = 0 To 2
               Reply = Me.Send(command)
               If Reply <> "Error" Then
                  Exit For
               End If
               DelayMS((Pass + 1) * 100)
            Next
            If Pass = 3 Then
               FileIO.WriteTextFile(LogFileName, "Failure in barcodedef, got to pass=3," & vbCrLf, False, True)
               DelayMS(1000)
               For Pass = 0 To 2
                  Reply = Me.Send(command)
                  If Reply <> "Error" Then
                     Exit For
                  End If
               Next
               If Pass = 3 Then
                  Reply = "NOREAD"
               End If
            End If
            Return Reply
         Catch ex As Exception
            MessageBox.Show("An error occured in SendMyCommand() in BarcodeDef. " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return ""
         End Try
      End Function


      Public Function Initialize() As Boolean
         'To test communications, query the number of decodes required to declare a valid read.  It is set to 3.
         If Me.SendMyCommand("<K221?>").Contains("<K221,") Then
            Return True
         Else
            Return False
         End If
      End Function


      Public Function SendLiteral(ByVal command As String) As String
         Return SendMyCommand(command)
      End Function


      Public Function Read() As String
         Dim Code As String = ""
         Dim NewPoint As Point4D
         Dim Pass As Int32
         Dim Tries As Int32
         Dim Attempts As Int32
         Dim StartingLocation As Point4D


         StartingLocation = Me.Location
         NewPoint = Me.Location

         For Attempts = 1 To 3
            For Pass = 1 To 5
               For Tries = 1 To 3
                  Code = Me.SendMyCommand("< >")   'Space, ASCII 32, is the command to read
                  If Code <> "NOREAD" Then
                     Exit For
                  End If
               Next

               If Code = "NOREAD" Then
                  NewPoint.Z -= 7.0  'move the filter down 1/4" and see if you can read it now.
                  NewPoint.X -= 1.5
                  NewPoint.Y -= 1.5
                  Arm.GoToPoint(NewPoint)
               Else
                  Exit For
               End If
            Next

            If Code = "NOREAD" Then
               NewPoint = StartingLocation
               Arm.GoToPoint(NewPoint)
            Else
               Exit For
            End If
         Next

         If Code = "NOREAD" Then
            'Move the gripper to a 60 angle and try again
            StartingLocation.U = 299.141
            Arm.GoToPoint(StartingLocation)
            StartingLocation.X = -706.338
            StartingLocation.Y = -183.862
            StartingLocation.Z = -143.629
            Arm.GoToPoint(StartingLocation)
            NewPoint = StartingLocation

            For Attempts = 1 To 3
               For Pass = 1 To 5
                  For Tries = 1 To 3
                     Code = Me.SendMyCommand("< >")   'Space, ASCII 32, is the command to read
                     If Code <> "NOREAD" Then
                        Exit For
                     End If
                  Next

                  If Code = "NOREAD" Then
                     NewPoint.Z -= 7.0  'move the filter down 1/4" and see if you can read it now.
                     NewPoint.X -= 1.5
                     NewPoint.Y -= 1.5
                     Arm.GoToPoint(NewPoint)
                  Else
                     Exit For
                  End If
               Next

               If Code = "NOREAD" Then
                  NewPoint = StartingLocation
                  Arm.GoToPoint(NewPoint)
               Else
                  Exit For
               End If
            Next

            'you must bring the arm back to its original starting point to transition to any other spot
            Arm.GoToPoint(Me.Location)
         End If

         Return Code
      End Function



      ''' <summary>
      ''' Moves a filter to the scanner position.
      ''' </summary>
      ''' <remarks></remarks>
      Public Sub GoToReader()
         'From any oven or carrosel position you can directly move to the scanner.
         Arm.GoToPoint(Me.Location)
      End Sub
   End Class



   Public Class DeionizerDef
      Public MyLocation As Point4D
   End Class




   Public Class XP6_BalanceDef
      'This class supports the Mettler XP6 balance
      Enum DoorLoc
         Open
         Closed
      End Enum

      Private MyPort As Ports.SerialPort
      Public Location As Point4D
      Private DoorState As DoorLoc
      Public MyDriver As JkemMotorDef
      Private MySwitchIsEnabled As Boolean


      Public Enum StabilityRequirement As Integer
         VeryFast
         Fast
         ReliableFast
         Reliable
         VeryReliable
      End Enum


      Sub New(ByRef port As Ports.SerialPort)
         MyPort = port
         MyPort.ReadTimeout = 120000
         MyPort.NewLine = vbCrLf
         MyPort.BaudRate = 9600
         MyDriver = Carosel.MyDriver
         MySwitchIsEnabled = True
      End Sub


      Public ReadOnly Property PortName() As String
         Get
            Return MyPort.PortName
         End Get
      End Property

      Public Property BalanceSwitchEnabled() As Boolean
         Get
            Return MySwitchIsEnabled
         End Get
         Set(ByVal value As Boolean)
            MySwitchIsEnabled = value
         End Set
      End Property


      ''' <summary>
      ''' Sends commands to Mettler balances.
      ''' </summary>
      ''' <param name="myCommand">The literal string command.  Do not include line terminators.</param>
      ''' <returns>The reply from the balance as a string</returns>
      ''' <remarks></remarks>
      Private Function Send(ByVal myCommand As String) As String
         'Mettler commands are terminated with CrLf
         Dim Reply As String = ""
         Static Dim ReenterTime As DateTime = Now


         If frmMain.TestMode = True Then
            DelayMS(100)
            Return "5.0"
         End If

         If ModuleDefMessages.AbortRequested = True Then
            Throw New System.Exception("User requested abort.")
         End If

         If MyPort.IsOpen = False Then
            Try
               MyPort.Open()
            Catch ex As Exception
               Return "Error"
            End Try
         End If

         'If you send commands too fast to the balance, it will hang.
         Do While DateTime.Compare(Now, ReenterTime) < 0
            Application.DoEvents()
         Loop

         Try
            'Send command through serial port
            MyPort.DiscardInBuffer()
            MyPort.DiscardOutBuffer()
            MyPort.WriteLine(myCommand)
            Try
               Reply = MyPort.ReadLine
               If Reply.Length > 0 Then
                  Reply = Reply.Substring(Reply.IndexOf(Chr(32)) + 1) 'Capture everything after the first space
               End If
            Catch exx As Exception
               Reply = "Error"
            End Try

         Catch ex As Exception
            Reply = "Error"
         End Try

         'Let the UI Catch up
         Application.DoEvents()
         ReenterTime = Now.AddMilliseconds(500)

         Return Reply
      End Function


      Public Function SendMyCommand(ByRef command As String) As String
         Dim Reply As String = ""
         Dim Pass As Int32

         Try
            For Pass = 0 To 2
               Reply = Me.Send(command)
               If Reply <> "Error" Then
                  Exit For
               End If
               DelayMS((Pass + 1) * 100)
            Next
            If Pass = 3 Then
               FileIO.WriteTextFile(LogFileName, "Failure in XP6 balancedef, got to pass=3," & vbCrLf, False, True)
               DelayMS(1000)
               For Pass = 0 To 2
                  Reply = Me.Send(command)
                  If Reply <> "Error" Then
                     Exit For
                  End If
               Next
               If Pass = 3 Then
                  FileIO.WriteTextFile(LogFileName, "Recover Port was called." & vbCrLf, False, True)
                  MessageBox.Show("Get Scott.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                  MessageBox.Show("The serial port was dropped on the balance.  Unplug the USB cable leading from the PC to the USB Hub hanging in the center of the robot deck, wait 5 seconds then plug it back in.   Do this for both hubs.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                  MessageBox.Show("Click OK to continue.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                  Reply = RecoverPort(command)
               End If
            End If
         Catch ex As Exception
            MessageBox.Show("Error in function SendMyCommand() for the XP6 balance.   Tell Scott to call J-KEM with this error message." & vbCrLf & command, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try
         Return Reply
      End Function


      Function RecoverPort(ByVal command As String) As String
         Dim Pass As Int32
         Dim Reply As String = "Error"

         Try
            For Pass = 0 To 3
               If TestComms() = True Then
                  DelayMS(500)
                  Reply = Me.Send(command)
                  If Reply <> "Error" Then
                     Exit For
                  End If
                  DelayMS(500)
               End If
            Next
            If Pass = 4 Then
               FileIO.WriteTextFile(LogFileName, "Recover Port did not work." & vbCrLf, False, True)
               MessageBox.Show("The application can not be recovered.   The application will exit.", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
               Throw New System.Exception("User abort requested.")
            End If
         Catch ex As Exception
            FileIO.WriteTextFile(LogFileName, "Recover Port caused an exception." & vbCrLf, False, True)
            MessageBox.Show("The application can not be recovered.   The application will exit.", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
            Throw New System.Exception("User abort requested.")
         End Try

         Return Reply
      End Function


      ''' <summary>
      ''' Drops off the filter in the balance.  This routein assumes that the filters starting location is the barcode reader
      ''' </summary>
      ''' <returns>Returns True if the filter was dropped off properly, otherwise False</returns>
      ''' <remarks></remarks>
      Public Function DropOffFilter() As Boolean
         Dim MyPoint As Point4D
         Dim Success As Boolean

         If ModuleDefMessages.InTestMode = True Then
            Return True
         End If

         MyPoint = Deionizer.MyLocation   'Move to the ionizer
         Arm.GoToPoint(MyPoint)
         DelaySeconds(10)   'Allow the filter to deionize
         MyPoint.Z = Balance.Location.Z + 10.0
         Arm.GoToPoint(MyPoint)

         Me.OpenDoor()
         MyPoint = Me.Location   'This loads the location of the balance pan
         MyPoint.Z = Balance.Location.Z + 10.0
         MyPoint.Y -= 3.0
         Arm.GoToPoint(MyPoint)
         MyPoint.Z = Me.Location.Z + 3.0
         Arm.GoToPoint(MyPoint)
         Gripper.Open()
         MyPoint.Z = Me.Location.Z - 1.5
         Arm.GoToPoint(MyPoint)
         MyPoint = Me.Location
         MyPoint.Y = Me.Location.Y + 15.0
         Arm.GoToPoint(MyPoint)
         'Check to see if the filter is stuck to the gripper
         If Gripper.ReadSensorAverage < (frmMain.LastAirADCReading - frmMain.OpticalSensor3mmDrop \ 2) Then
            MyPoint.Z = Me.Location.Z + 3.0
            Arm.GoToPoint(MyPoint)
            Success = False
         Else
            Success = True
         End If

         Gripper.Close()
         MyPoint = Me.Location
         MyPoint.Y = MyPoint.Y - 0.5
         Arm.GoToPoint(MyPoint)
         MyPoint.Y += 100.0
         Arm.GoToPoint(MyPoint)
         Me.CloseDoor()
         Return Success
      End Function


      ''' <summary>
      ''' Picks up the filter from the balance and takes it to a safe position
      ''' </summary>
      ''' <remarks></remarks>
      Public Sub PickUpFilter()
         Dim MyPoint As Point4D
         Dim AirADC As Int32
         Dim ADC As Int32
         Dim Pass As Int32

         If ModuleDefMessages.InTestMode = True Then
            Return
         End If

         Gripper.Open()
         Me.OpenDoor()

         'Search for the edge of the filter.   Here are the ADC readings based on condition.
         'Gripper open, sensor unblocked = 681,  Gripper at the filter edge= 645
         'Sensor over filter by 2mm = 578,      3mm = 511
12:      AirADC = Gripper.ReadSensorAverage
         frmMain.LastAirADCReading = AirADC
         MyPoint = Me.Location
         MyPoint.Y -= 2.0
         Arm.GoToPoint(MyPoint)
         MyPoint.Z += 1.5   'Its good to be 1.5mm high on the Z axis
         Arm.GoToPoint(MyPoint)
         ADC = Gripper.ReadSensorAverage

         'If you don't have enough drop in the ADC, then continue to move the gripper into the oven.   You can go a maximum of 10mm pas
         'the starting location of the filter.
         For Pass = 1 To 8
            If ADC > (AirADC - frmMain.OpticalSensor3mmDrop) Then 'The ADC reading drops by about 100 counts when you are 2mm over the filter edge
               MyPoint.Y -= 0.5
               Arm.GoToPoint(MyPoint)
               ADC = Gripper.ReadSensorAverage
            Else
               Exit For
            End If
         Next

         If Pass > 8 Then
            'bob load error message to filter.   Deactivate this filter, or let the user decide
            MyPoint.Y += 200.0
            Arm.GoToPoint(MyPoint)
            If MessageBox.Show("A filter can not be detected on the balance pan.   Place the filter manually on the balance pan." & vbCrLf & _
                            "Place the filter manually on the balance pan and then click OK, or continue the proceedure without the filter, click Cancel", "Filter Detection Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand) = DialogResult.Cancel Then
               Balance.CloseDoor()
               Exit Sub
            Else
               MessageBox.Show("Place the filter on the balance pan, then click OK." & vbCrLf & "NOTE!  The arm will move as soon as the OK button is clicked.", "Arm Ready", MessageBoxButtons.OK, MessageBoxIcon.Hand)
               Me.OpenDoor()
               GoTo 12
            End If
         End If

         Gripper.Close()
         MyPoint.Z += 12.0
         Arm.GoToPoint(MyPoint)
         MyPoint.Y += 100.0
         Arm.GoToPoint(MyPoint)
         Balance.CloseDoor()
      End Sub



      Public Function Init() As Boolean
         Dim Reply As String
         Dim State As Boolean = True

         If frmMain.TestMode = True Then
            Return True
         End If

         Me.DisableAutoCalibration()
         Me.CloseDoor()
         Me.DoorState = DoorLoc.Closed
         Me.Zero()
         Reply = Me.SendMyCommand("M16 0")  'Turn off standby mode
         If Reply <> "A" Then
            MessageBox.Show("Balance initialization error.", "Balance Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            State = False
         End If
         Reply = Me.SendMyCommand("M03 0")  'Turn off autozero
         If Reply <> "A" Then
            MessageBox.Show("Balance initialization error.", "Balance Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            State = False
         End If
         Reply = Me.SendMyCommand("M17 00 00 00 0")  'Turn off ProFACT time criterian
         If Reply <> "A" Then
            MessageBox.Show("Balance initialization error.", "Balance Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            State = False
         End If
         Reply = Me.SendMyCommand("M18 0")  'Turn off ProFACT temperature criterian
         If Reply <> "A" Then
            MessageBox.Show("Balance initialization error.", "Balance Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            State = False
         End If

         Return State
      End Function


      ''' <summary>
      ''' Subfunction accessed by all Weight commands
      ''' </summary>
      ''' <param name="data">The reply string from the weight command</param>
      ''' <returns>Weight as a string or Nothing or Error</returns>
      ''' <remarks></remarks>
      Private Function ExtractWeight(ByVal data As String) As String
         Try
            If data.Chars(0) = "S" Or data.Chars(0) = "D" Then  'Stable or Dynamic Weight received
               data = data.Substring(1)
               data = data.TrimStart(Chr(32))   'Remove all leading spaces
               data = data.Substring(0, data.IndexOf(" "))   'Extract everything up to the first space
            ElseIf data.Chars(0) = "+" Then
               data = "OVERLOAD"
            ElseIf data.Chars(0) = "-" Then
               data = "UNDERLOAD"
            Else
               data = "WeighError"
            End If
         Catch ex As Exception
            data = Nothing
         End Try

         Return data
      End Function


      ''' <summary>
      ''' Returns a stable weight from the balance in the currenly selected units.
      ''' </summary>
      ''' <returns>Weight of sample or the error message of "ERROR".</returns>
      ''' <remarks></remarks>
      Public Function Weight() As String
         Dim Reply As String = ""
         Dim Tries As Int32
         Static Dim BadPasses As Int32 = 0

         If ModuleDefMessages.InTestMode = True Then
            Return "5.000000"
         End If

         Try
            For Tries = 0 To 2
               Try
                  Reply = Me.SendMyCommand("S")
                  Reply = ExtractWeight(Reply)
                  If IsDouble(Reply) = True Then
                     BadPasses = 0
                     Exit For
                  Else
                     BadPasses += 1
                     Reply = "-1.0"
                  End If
               Catch ex As Exception
                  Reply = "-1.0"
               End Try
            Next

            If BadPasses = 6 Then   'It only gets to a value of 6 by two sequential filters failing
               If Reply Is Nothing Or Reply = "-1.0" Or Reply = "OVERLOAD" Or Reply = "UNDERLOAD" Then
                  If MessageBox.Show("The balance is reporting an out of range or other error condition.   Fix this error and then click OK to resume the proceedure or CANCEL to abort the proceedure.", "Balance Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1) = DialogResult.Cancel Then
                     ModuleDefMessages.AbortRequested = True
                     Throw New System.Exception("User abort requested.")
                  End If
               End If
               Reply = "-1.0"
            End If
         Catch ex As Exception
            MessageBox.Show("Error in function Weight().   Tell Scott to call J-KEM with this error message.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try

         Return Reply
      End Function


      ''' <summary>
      ''' Returns the current weight from the balance, even if it is not a stable weight, in the currenly selected units.
      ''' </summary>
      ''' <returns>Weight of sample.</returns>
      ''' <remarks></remarks>
      Public Function WeightImmediately() As String
         Dim Reply As String = ""
         Dim Tries As Int32
         Static Dim BadPasses As Int32 = 0

         If ModuleDefMessages.InTestMode = True Then
            Return "5.000000"
         End If

         Try
            For Tries = 0 To 2
               Try
                  Reply = Me.SendMyCommand("SI")
                  Reply = ExtractWeight(Reply)
                  If IsDouble(Reply) = True Then
                     BadPasses = 0
                     Exit For
                  Else
                     BadPasses += 1
                     Reply = "-1.0"
                  End If
               Catch ex As Exception
                  Reply = "-1.0"
               End Try
            Next

            If BadPasses = 6 Then   'It only gets to a value of 6 by two sequential filters failing
               If Reply Is Nothing Or Reply = "OVERLOAD" Or Reply = "UNDERLOAD" Then
                  If MessageBox.Show("The balance is reporting an out of range or other error condition.   Fix this error and then click OK to resume the proceedure or CANCEL to abort the proceedure.", "Balance Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1) = DialogResult.Cancel Then
                     ModuleDefMessages.AbortRequested = True
                     Throw New System.Exception("User abort requested.")
                  End If
               End If
               Reply = "-1.0"
            End If
         Catch ex As Exception
            MessageBox.Show("Error in function WeighImmediately().   Tell Scott to call J-KEM with this error message.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try

         Return Reply
      End Function


      Public Function Zero() As Boolean
         Dim Reply As String = ""
         Dim State As Boolean = False
         Dim Pass As Int32

         If frmMain.TestMode = True Then
            Return True
         End If

         For Pass = 0 To 3
            Reply = Me.SendMyCommand("Z")
            If Reply = "A" Then
               Exit For
            End If
         Next
         If Pass = 3 Then
            Me.MyPort.Close()
            For Pass = 0 To 3
               Reply = Me.SendMyCommand("Z")
               If Reply = "A" Then
                  Exit For
               End If
            Next
         End If

         If Reply = "A" Then
            State = True
         Else
            State = False
         End If

         Return State
      End Function



      Public Function Tare() As Boolean
         Dim Reply As String = ""
         Dim State As Boolean = False
         Dim Pass As Int32

         If frmMain.TestMode = True Then
            Return True
         End If

         For Pass = 0 To 3
            Reply = Me.SendMyCommand("T")
            If Reply(0) = "S" Then
               State = True
               Exit For
            End If
         Next
         If Pass = 3 Then
            Me.MyPort.Close()
            For Pass = 0 To 3
               Reply = Me.SendMyCommand("T")
               If Reply(0) = "S" Then
                  State = True
                  Exit For
               End If
            Next
         End If

         Return State
      End Function


      Public Function DisableAutoCalibration() As Boolean
         Dim Reply As String = ""
         Dim State As Boolean = False
         Dim Pass As Int32

         If frmMain.TestMode = True Then
            Return True
         End If

         For Pass = 0 To 2
            Reply = Me.SendMyCommand("C0 0 0")
            If Reply = "A" Then
               State = True
               Exit For
            End If
         Next

         Return State
      End Function


      Public Sub OpenDoor()
         Dim Reply As String
         Dim State As Boolean = False
         Dim Pass As Int32

         If frmMain.TestMode = True Then
            Exit Sub
         End If

         Do
            Reply = Me.SendMyCommand("WS 2") 'Open door
            If Reply = "A" Then
               For Pass = 0 To 10
                  DelayMS(300)
                  Reply = Me.SendMyCommand("WS")  'Request percent door open
                  If Reply = "A 2" Then
                     If Me.MySwitchIsEnabled = True Then
                        DelayMS(100)
                        If MyDriver.GetInput(3) = JkemMotorDef.InputState.High Then
                           MessageBox.Show("The external balance door switch can not verify that the balance is open.  Open the door manually, and then click OK to continue.", "Balance Door Not Open", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
                        End If
                     End If
                     Exit Do
                  End If
               Next
               If Pass >= 10 Then
                  Me.CloseDoor()
                  DelayMS(1000)
                  Reply = Me.SendMyCommand("WS 2") 'Open door
                  DelayMS(3000)
                  Reply = Me.SendMyCommand("WS")  'Request percent door open
                  If Reply = "A 2" Then
                     If Me.MySwitchIsEnabled = True Then
                        DelayMS(100)
                        If MyDriver.GetInput(3) = JkemMotorDef.InputState.High Then
                           MessageBox.Show("The external balance door switch can not verify that the balance is open.  Open the door manually, and then click OK to continue.", "Balance Door Not Open", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
                        End If
                     End If
                     Exit Do
                  End If
                  If MessageBox.Show("The program can not verify that the balance door is fully open." & vbCrLf & "Is the door open, and should the proceedure continue?", "Balance Door Verification", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) = DialogResult.Yes Then
                     Exit Do
                  Else
                     MessageBox.Show("The robot can not proceed without the balance.", "Robot Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                     Throw New System.Exception("Balance Door Error")
                  End If
               End If
            End If
         Loop
         DelayMS(100)
      End Sub


      Public Function CloseDoor() As Boolean
         Dim Reply As String
         Dim State As Boolean = False
         Dim Tries As Int32

         If frmMain.TestMode = True Then
            Return True
         End If

         For Tries = 0 To 2
            Reply = Me.SendMyCommand("WS 0")
            If Reply = "A" Then
               State = True
               Exit For
            End If
            DelayMS(300)
         Next

         If Tries = 3 Then
            Me.MyPort.Close()
            For Tries = 0 To 2
               Reply = Me.SendMyCommand("WS 0")
               If Reply = "A" Then
                  State = True
                  Exit For
               End If
               DelayMS(300)
            Next
         End If

         Return State
      End Function


      Public Function SetStability(ByVal stability As StabilityRequirement) As Boolean
         Dim Reply As String
         Dim State As Boolean = False

         If frmMain.TestMode = True Then
            Return True
         End If

         Reply = Me.SendMyCommand("M29 " & stability)
         If Reply = Nothing Then
            If MessageBox.Show("Can not execute SetStability command.   Continue proceedure?", "Serial Communication Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2) = DialogResult.No Then
               Throw New System.Exception("User abort")
            End If
         End If

         If Reply = "A" Then
            State = True
         End If

         Return State
      End Function


      Public Sub LockKeypad()

      End Sub

      Public Sub UnlockKeypad()

      End Sub


      Public Function TestComms(Optional ByRef myReply As String = "") As Boolean
         Dim Reply As String
         Dim Success As Boolean = False
         Dim Pass As Int32

         For Pass = 0 To 2
            Try
               Reply = Me.SendMyCommand("I4")
               If Reply.Contains("1123481116") Then
                  Success = True
                  Exit For
               End If

            Catch ex As Exception

            End Try
         Next

         Return Success
      End Function

   End Class



   Public Class MT5_BalanceDef
      'This class supports the Mettler MT5 balance
      Enum DoorLoc
         Open
         Closed
      End Enum

      Private MyPort As Ports.SerialPort
      Public Location As Point4D
      Private DoorState As DoorLoc


      Sub New(ByRef port As Ports.SerialPort)
         MyPort = port
         MyPort.ReadTimeout = 120000
         MyPort.NewLine = vbCrLf
         MyPort.BaudRate = 2400
         MyPort.Parity = Ports.Parity.Even
         MyPort.DataBits = 7
      End Sub


      Public ReadOnly Property PortName() As String
         Get
            Return MyPort.PortName
         End Get
      End Property

      Private Function Send(ByVal myCommand As String, Optional ByVal replyExpected As Boolean = True) As String
         'Mettler commands are terminated with CrLf
         Dim Reply As String = ""
         Static Dim ReenterTime As DateTime = Now


         If frmMain.TestMode = True Then
            DelayMS(100)
            Return "5.0"
         End If

         If ModuleDefMessages.AbortRequested = True Then
            Throw New System.Exception("User requested abort.")
         End If

         If MyPort.IsOpen = False Then
            Try
               MyPort.Open()
            Catch ex As Exception
               Return "Error"
            End Try
         End If

         'If you send commands too fast to the balance, it will hang.
         Do While DateTime.Compare(Now, ReenterTime) < 0
            Application.DoEvents()
         Loop

         Try
            'Send command through serial port
            MyPort.DiscardInBuffer()
            MyPort.DiscardOutBuffer()
            MyPort.WriteLine(myCommand)
            Try
               If replyExpected = True Then
                  Reply = MyPort.ReadLine
               Else
                  Reply = ""
               End If
            Catch exx As Exception
               Reply = "Error"
            End Try

         Catch ex As Exception
            Reply = "Error"
         End Try

         'Let the UI Catch up
         Application.DoEvents()
         ReenterTime = Now.AddMilliseconds(500)

         Return Reply
      End Function


      Private Function SendMyCommand(ByRef command As String, Optional ByVal replyExpected As Boolean = True) As String
         Dim Reply As String = ""
         Dim Pass As Int32

         Try
            For Pass = 0 To 2
               Reply = Me.Send(command, replyExpected)
               If Reply <> "Error" Then
                  Exit For
               End If
               DelayMS((Pass + 1) * 100)
            Next
            If Pass = 3 Then
               FileIO.WriteTextFile(LogFileName, "Failure in MT5Balancedef, got to pass=3," & vbCrLf, False, True)
               DelayMS(1000)
               For Pass = 0 To 2
                  Reply = Me.Send(command, replyExpected)
                  If Reply <> "Error" Then
                     Exit For
                  End If
               Next
               If Pass = 3 Then
                  FileIO.WriteTextFile(LogFileName, "Recover Port was called." & vbCrLf, False, True)
                  MessageBox.Show("Get Scott.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                  MessageBox.Show("The serial port was dropped on the balance.   Unplug the USB cable leading from the PC to the USB Hub hanging in the center of the robot deck, wait 5 seconds then plug it back in.   Do this for both hubs.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                  MessageBox.Show("Click OK to continue.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                  Reply = RecoverPort(command)
               End If
            End If
         Catch ex As Exception
            MessageBox.Show("Error in function SendMyCommand() for the MT5 balance.   Tell Scott to call J-KEM with this error message." & vbCrLf & command, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try
         Return Reply
      End Function


      Function RecoverPort(ByVal command As String) As String
         Dim Pass As Int32
         Dim Reply As String = "Error"

         Try
            For Pass = 0 To 3
               If TestComms() = True Then
                  DelayMS(500)
                  Reply = Me.Send(command)
                  If Reply <> "Error" Then
                     Exit For
                  End If
                  DelayMS(500)
               End If
            Next
            If Pass = 4 Then
               FileIO.WriteTextFile(LogFileName, "Recover Port did not work." & vbCrLf, False, True)
               MessageBox.Show("The application can not be recovered.   The application will exit.", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
               Throw New System.Exception("User abort requested.")
            End If
         Catch ex As Exception
            FileIO.WriteTextFile(LogFileName, "Recover Port caused an exception." & vbCrLf, False, True)
            MessageBox.Show("The application can not be recovered.   The application will exit.", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
            Throw New System.Exception("User abort requested.")
         End Try

         Return Reply
      End Function


      ''' <summary>
      ''' Drops off the filter in the balance.  This routein assumes that the filters starting location is the barcode reader
      ''' </summary>
      ''' <returns>Returns True if the filter was dropped off properly, otherwise False</returns>
      ''' <remarks></remarks>
      Public Function DropOffFilter() As Boolean
         Dim MyPoint As Point4D
         Dim Success As Boolean

         If ModuleDefMessages.InTestMode = True Then
            Return True
         End If

         MyPoint = Deionizer.MyLocation   'Move to the ionizer
         MyPoint.Y += 150.0
         Arm.GoToPoint(MyPoint)
         MyPoint.Y -= 150.0
         Arm.GoToPoint(MyPoint)
         DelaySeconds(10)   'Allow the filter to deionize

         Me.OpenDoor()
         MyPoint = Me.Location   'This loads the location of the balance pan
         MyPoint.Z = Me.Location.Z + 10.0
         MyPoint.Y -= 2.0
         Arm.GoToPoint(MyPoint)
         MyPoint.Z = Me.Location.Z + 3.0
         Arm.GoToPoint(MyPoint)
         Gripper.Open()
         MyPoint.Z = Me.Location.Z - 1.5
         Arm.GoToPoint(MyPoint)
         MyPoint = Me.Location
         MyPoint.Y = Me.Location.Y + 15.0
         Arm.GoToPoint(MyPoint)
         'Check to see if the filter is stuck to the gripper
         If Gripper.ReadSensorAverage < (frmMain.LastAirADCReading - frmMain.OpticalSensor3mmDrop \ 2) Then
            MyPoint.Z = Me.Location.Z + 3.0
            Arm.GoToPoint(MyPoint)
            Success = False
         Else
            Success = True
         End If

         Gripper.Close()
         MyPoint = Me.Location
         Arm.GoToPoint(MyPoint)
         MyPoint.Y += 150.0
         Arm.GoToPoint(MyPoint)
         Me.CloseDoor()
         Return Success
      End Function


      ''' <summary>
      ''' Picks up the filter from the balance and takes it to a safe position
      ''' </summary>
      ''' <remarks></remarks>
      Public Sub PickUpFilter()
         Dim MyPoint As Point4D
         Dim AirADC As Int32
         Dim ADC As Int32
         Dim Pass As Int32

         If ModuleDefMessages.InTestMode = True Then
            Return
         End If

         Gripper.Open()
         Me.OpenDoor()

         'Search for the edge of the filter.   Here are the ADC readings based on condition.
         'Gripper open, sensor unblocked = 681,  Gripper at the filter edge= 645
         'Sensor over filter by 2mm = 578,      3mm = 511

         'Optics values on 6/_18_2013  Unblocked= 806    Obstructed by 2mm= 701
12:      AirADC = Gripper.ReadSensorAverage
         frmMain.LastAirADCReading = AirADC
         MyPoint = Me.Location
         MyPoint.Y -= 3.0
         Arm.GoToPoint(MyPoint)
         MyPoint.Z += 1.5   'Its good to be 1.5mm high on the Z axis
         Arm.GoToPoint(MyPoint)
         ADC = Gripper.ReadSensorAverage

         'If you don't have enough drop in the ADC, then continue to move the gripper into the oven.   You can go a maximum of 10mm pas
         'the starting location of the filter.
         For Pass = 1 To 8
            If ADC > (AirADC - frmMain.OpticalSensor3mmDrop) Then 'The ADC reading drops by about 100 counts when you are 2mm over the filter edge
               MyPoint.Y -= 0.5
               Arm.GoToPoint(MyPoint)
               ADC = Gripper.ReadSensorAverage
            Else
               Exit For
            End If
         Next

         If Pass > 8 Then
            'bob load error message to filter.   Deactivate this filter, or let the user decide
            MyPoint.Y += 200.0
            Arm.GoToPoint(MyPoint)
            If MessageBox.Show("A filter can not be detected on the balance pan.   Place the filter manually on the balance pan." & vbCrLf & _
                            "Place the filter manually on the balance pan and then click OK, or continue the proceedure without the filter, click Cancel", "Filter Detection Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand) = DialogResult.Cancel Then
               Balance.CloseDoor()
               Exit Sub
            Else
               MessageBox.Show("Place the filter on the balance pan, then click OK." & vbCrLf & "NOTE!  The arm will move as soon as the OK button is clicked.", "Arm Ready", MessageBoxButtons.OK, MessageBoxIcon.Hand)
               Me.OpenDoor()
               GoTo 12
            End If
         End If

         Gripper.Close()
         MyPoint.Z += 12.0
         Arm.GoToPoint(MyPoint)
         MyPoint.Y += 150.0
         Arm.GoToPoint(MyPoint)
         Balance.CloseDoor()
      End Sub



      Public Function Init() As Boolean
         Dim State As Boolean = True

         If frmMain.TestMode = True Then
            Return True
         End If
         Me.TestComms()
         CloseDoor()

         Return State
      End Function


      ''' <summary>
      ''' Subfunction accessed by all Weight commands
      ''' </summary>
      ''' <param name="data">The reply string from the weight command</param>
      ''' <returns>Weight as a string or Nothing or Error</returns>
      ''' <remarks></remarks>
      Private Function ExtractWeight(ByVal data As String) As String
         Try
            If data.Chars(0) = "S" Or data.Chars(0) = "D" Then  'Stable or Dynamic Weight received
               data = data.Substring(1)
               If Char.IsLetter(data(0)) = True Then
                  data = data.Substring(1)
               End If
               data = data.TrimStart(Chr(32))   'Remove all leading spaces
               data = data.Substring(0, data.IndexOf(" "))   'Extract everything up to the first space
            ElseIf data.Chars(0) = "+" Then
               data = "OVERLOAD"
            ElseIf data.Chars(0) = "-" Then
               data = "UNDERLOAD"
            Else
               data = "WeighError"
            End If
         Catch ex As Exception
            data = Nothing
         End Try

         Return data
      End Function


      ''' <summary>
      ''' Returns a stable weight from the balance in the currenly selected units.
      ''' </summary>
      ''' <returns>Weight of sample or the error message of "ERROR".</returns>
      ''' <remarks></remarks>
      Public Function Weight() As String
         Dim Reply As String = ""
         Dim Tries As Int32
         Static Dim BadPasses As Int32 = 0

         If ModuleDefMessages.InTestMode = True Then
            Return "5.000000"
         End If

         Try
            For Tries = 0 To 2
               Try
                  Reply = Me.SendMyCommand("S")
                  Reply = ExtractWeight(Reply)
                  If IsDouble(Reply) = True Then   'The balance returns weights in milligrams and I need it in grams
                     Reply = (CDbl(Reply) / 1000.0).ToString
                     BadPasses = 0
                     Exit For
                  Else
                     BadPasses += 1
                     Reply = "-1.0"
                  End If
               Catch ex As Exception
                  Reply = "-1.0"
               End Try
            Next

            If BadPasses = 6 Then   'It only gets to a value of 6 by two sequential filters failing
               If Reply Is Nothing Or Reply = "-1.0" Or Reply = "OVERLOAD" Or Reply = "UNDERLOAD" Then
                  If MessageBox.Show("The balance is reporting an out of range or other error condition.   Fix this error and then click OK to resume the proceedure or CANCEL to abort the proceedure.", "Balance Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1) = DialogResult.Cancel Then
                     ModuleDefMessages.AbortRequested = True
                     Throw New System.Exception("User abort requested.")
                  End If
               End If
               Reply = "-1.0"
            End If
         Catch ex As Exception
            MessageBox.Show("Error in function Weight().   Tell Scott to call J-KEM with this error message.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try

         Return Reply
      End Function


      ''' <summary>
      ''' Returns the current weight from the balance, even if it is not a stable weight, in the currenly selected units.
      ''' </summary>
      ''' <returns>Weight of sample.</returns>
      ''' <remarks></remarks>
      Public Function WeightImmediately() As String
         Dim Reply As String = ""
         Dim Tries As Int32
         Static Dim BadPasses As Int32 = 0

         If ModuleDefMessages.InTestMode = True Then
            Return "5.000000"
         End If

         Try
            For Tries = 0 To 2
               Try
                  Reply = Me.SendMyCommand("SI")
                  Reply = ExtractWeight(Reply)
                  If IsDouble(Reply) = True Then
                     Reply = (CDbl(Reply) / 1000.0).ToString     'The balance returns weights in milligrams and I need it in grams
                     BadPasses = 0
                     Exit For
                  Else
                     BadPasses += 1
                     Reply = "-1.0"
                  End If
               Catch ex As Exception
                  Reply = "-1.0"
               End Try
            Next

            If BadPasses = 6 Then   'It only gets to a value of 6 by two sequential filters failing
               If Reply Is Nothing Or Reply = "OVERLOAD" Or Reply = "UNDERLOAD" Then
                  If MessageBox.Show("The balance is reporting an out of range or other error condition.   Fix this error and then click OK to resume the proceedure or CANCEL to abort the proceedure.", "Balance Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1) = DialogResult.Cancel Then
                     ModuleDefMessages.AbortRequested = True
                     Throw New System.Exception("User abort requested.")
                  End If
               End If
               Reply = "-1.0"
            End If
         Catch ex As Exception
            MessageBox.Show("Error in function WeighImmediately().   Tell Scott to call J-KEM with this error message.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try

         Return Reply
      End Function



      Public Function Zero() As Boolean
         'This is really the tare command, but it is named Zero to make it compatible with the XP6.
         Dim Reply As String = ""
         Dim State As Boolean = False
         Dim Pass As Int32

         If frmMain.TestMode = True Then
            Return True
         End If

         For Pass = 0 To 3
            SendMyCommand("T", False) 'No reply is sent to the tare command
            'You have to query a stable weight to know when it's tared.
            DelayMS(3000)
            Reply = Me.Weight
            If IsDouble(Reply, -0.01, 0.01) = True Then
               Exit For
            End If
         Next

         Return State
      End Function


      Public Sub OpenDoor()
         Dim Reply As String
         Dim State As Boolean = False
         Dim Pass As Int32

         If frmMain.TestMode = True Then
            Exit Sub
         End If

         Do
            SendMyCommand("WI 0 L", False) 'Open door   No reply to this command
            DelayMS(1800)  'It takes about 1.6 seconds to open the door
            For Pass = 0 To 10
               Reply = Me.SendMyCommand("WI ?")  'Request  door open
               If Reply = "WI=0" Then  'Door is open
                  DoorState = DoorLoc.Open
                  Exit Do
               End If
               DelayMS(300)
            Next
            If Pass >= 10 Then
               Me.MyPort.Close()
               DelayMS(2500)
               Reply = Me.SendMyCommand("WI ?")  'Request percent door open
               DelayMS(100)
               Reply = Me.SendMyCommand("WI ?")  'Request percent door open
               If Reply = "WI=0" Then
                  DoorState = DoorLoc.Open
                  Exit Do
               End If
               If MessageBox.Show("The program can not verify that the balance door is fully open." & vbCrLf & "Is the door open, and should the proceedure continue?", "Balance Door Verification", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) = DialogResult.No Then
                  MessageBox.Show("The robot can not proceed without the balance.", "Robot Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                  Throw New System.Exception("Balance Door Error")
               End If
            End If
            DelayMS(100)
         Loop
      End Sub


      Public Function CloseDoor() As Boolean
         Dim Reply As String
         Dim State As Boolean = False
         Dim Tries As Int32

         If frmMain.TestMode = True Then
            Return True
         End If

         SendMyCommand("WI 1", False)  'No reply to this command
         DelayMS(1800)  'It takes 1.6 seconds to close the door
         For Tries = 0 To 10
            Reply = Me.SendMyCommand("WI ?")
            If Reply = "WI=1" Then
               DoorState = DoorLoc.Closed
               Exit For
            End If
            DelayMS(300)
         Next

         If Tries >= 10 Then
            If MessageBox.Show("The program can not verify that the balance door is closed.  If the door is closed, click OK to continue, or click Cancel to exit program.", "Balance Door Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand) = DialogResult.Cancel Then
               Throw New System.Exception("Balance Door Error")
            End If
         End If

         Return State
      End Function


      Public Function TestComms(Optional ByRef myReply As String = "") As Boolean
         Dim Reply As String
         Dim Success As Boolean = False
         Dim Pass As Int32

         MyPort.ReadTimeout = 1000
         For Pass = 0 To 2
            Try
               Reply = Me.SendMyCommand("I2")
               If Reply.Contains("I2 A") Then
                  Success = True
                  Exit For
               End If
            Catch ex As Exception
            End Try
         Next
         MyPort.ReadTimeout = 120000

         Return Success
      End Function

   End Class



   Public Class MX5_BalanceDef
      'This class supports the Mettler MT5 balance
      Enum DoorLoc
         Open
         Closed
      End Enum

      Private MyPort As Ports.SerialPort
      Public Location As Point4D
      Private DoorState As DoorLoc
      Public MyDriver As JkemMotorDef


      Sub New(ByRef port As Ports.SerialPort)
         MyPort = port
         MyPort.ReadTimeout = 120000
         MyPort.NewLine = vbCrLf
         MyPort.BaudRate = 2400
         MyPort.Parity = Ports.Parity.Even
         MyPort.DataBits = 7
         MyDriver = Carosel.MyDriver
      End Sub


      Public ReadOnly Property PortName() As String
         Get
            Return MyPort.PortName
         End Get
      End Property

      Private Function Send(ByVal myCommand As String, Optional ByVal replyExpected As Boolean = True) As String
         'Mettler commands are terminated with CrLf
         Dim Reply As String = ""
         Static Dim ReenterTime As DateTime = Now


         If frmMain.TestMode = True Then
            DelayMS(100)
            Return "5.0"
         End If

         If ModuleDefMessages.AbortRequested = True Then
            Throw New System.Exception("User requested abort.")
         End If

         If MyPort.IsOpen = False Then
            Try
               MyPort.Open()
            Catch ex As Exception
               Return "Error"
            End Try
         End If

         'If you send commands too fast to the balance, it will hang.
         Do While DateTime.Compare(Now, ReenterTime) < 0
            Application.DoEvents()
         Loop

         Try
            'Send command through serial port
            MyPort.DiscardInBuffer()
            MyPort.DiscardOutBuffer()
            MyPort.WriteLine(myCommand)
            Try
               If replyExpected = True Then
                  Reply = MyPort.ReadLine
               Else
                  Reply = ""
               End If
            Catch exx As Exception
               Reply = "Error"
            End Try

         Catch ex As Exception
            Reply = "Error"
         End Try

         'Let the UI Catch up
         Application.DoEvents()
         ReenterTime = Now.AddMilliseconds(500)

         Return Reply
      End Function


      Private Function SendMyCommand(ByRef command As String, Optional ByVal replyExpected As Boolean = True) As String
         Dim Reply As String = ""
         Dim Pass As Int32

         Try
            For Pass = 0 To 2
               Reply = Me.Send(command, replyExpected)
               If Reply <> "Error" Then
                  Exit For
               End If
               DelayMS((Pass + 1) * 100)
            Next
            If Pass = 3 Then
               FileIO.WriteTextFile(LogFileName, "Failure in MXbalancddef, got to pass=3," & vbCrLf, False, True)
               DelayMS(1000)
               For Pass = 0 To 2
                  Reply = Me.Send(command, replyExpected)
                  If Reply <> "Error" Then
                     Exit For
                  End If
               Next
               If Pass = 3 Then
                  FileIO.WriteTextFile(LogFileName, "Recover Port was called." & vbCrLf, False, True)
                  MessageBox.Show("Get Scott.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                  MessageBox.Show("The serial port was dropped on the balance.   Unplug the USB cable leading from the PC to the USB Hub hanging in the center of the robot deck, wait 5 seconds then plug it back in.   Do this for both hubs.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                  MessageBox.Show("Click OK to continue.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                  Reply = RecoverPort(command)
               End If
            End If
         Catch ex As Exception
            MessageBox.Show("Error in function SendMyCommand() for the MX balance.   Tell Scott to call J-KEM with this error message." & vbCrLf & command, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try
         Return Reply
      End Function


      Function RecoverPort(ByVal command As String) As String
         Dim Pass As Int32
         Dim Reply As String = "Error"

         Try
            For Pass = 0 To 3
               If TestComms() = True Then
                  DelayMS(500)
                  Reply = Me.Send(command)
                  If Reply <> "Error" Then
                     Exit For
                  End If
                  DelayMS(500)
               End If
            Next
            If Pass = 4 Then
               FileIO.WriteTextFile(LogFileName, "Recover Port did not work." & vbCrLf, False, True)
               MessageBox.Show("The application can not be recovered.   The application will exit.", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
               Throw New System.Exception("User abort requested.")
            End If
         Catch ex As Exception
            FileIO.WriteTextFile(LogFileName, "Recover Port caused an exception." & vbCrLf, False, True)
            MessageBox.Show("The application can not be recovered.   The application will exit.", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
            Throw New System.Exception("User abort requested.")
         End Try

         Return Reply
      End Function



      ''' <summary>
      ''' Drops off the filter in the balance.  This routein assumes that the filters starting location is the barcode reader
      ''' </summary>
      ''' <returns>Returns True if the filter was dropped off properly, otherwise False</returns>
      ''' <remarks></remarks>
      Public Function DropOffFilter() As Boolean
         Dim MyPoint As Point4D
         Dim Success As Boolean

         If ModuleDefMessages.InTestMode = True Then
            Return True
         End If

         MyPoint = Deionizer.MyLocation   'Move to the ionizer
         MyPoint.Y += 150.0
         Arm.GoToPoint(MyPoint)
         MyPoint.Y -= 150.0
         Arm.GoToPoint(MyPoint)
         DelaySeconds(10)   'Allow the filter to deionize

         Me.OpenDoor()
         MyPoint = Me.Location   'This loads the location of the balance pan
         MyPoint.Z = Me.Location.Z + 10.0
         MyPoint.Y -= 2.0
         Arm.GoToPoint(MyPoint)
         MyPoint.Z = Me.Location.Z + 3.0
         Arm.GoToPoint(MyPoint)
         Gripper.Open()
         MyPoint.Z = Me.Location.Z - 1.5
         Arm.GoToPoint(MyPoint)
         MyPoint = Me.Location
         MyPoint.Y = Me.Location.Y + 15.0
         Arm.GoToPoint(MyPoint)
         'Check to see if the filter is stuck to the gripper
         If Gripper.ReadSensorAverage < (frmMain.LastAirADCReading - frmMain.OpticalSensor3mmDrop \ 2) Then
            MyPoint.Z = Me.Location.Z + 3.0
            Arm.GoToPoint(MyPoint)
            Success = False
         Else
            Success = True
         End If

         Gripper.Close()
         MyPoint = Me.Location
         Arm.GoToPoint(MyPoint)
         MyPoint.Y += 150.0
         Arm.GoToPoint(MyPoint)
         Me.CloseDoor()
         Return Success
      End Function


      ''' <summary>
      ''' Picks up the filter from the balance and takes it to a safe position
      ''' </summary>
      ''' <remarks></remarks>
      Public Sub PickUpFilter()
         Dim MyPoint As Point4D
         Dim AirADC As Int32
         Dim ADC As Int32
         Dim Pass As Int32

         If ModuleDefMessages.InTestMode = True Then
            Return
         End If

         Gripper.Open()
         Me.OpenDoor()

         'Search for the edge of the filter.   Here are the ADC readings based on condition.
         'Gripper open, sensor unblocked = 681,  Gripper at the filter edge= 645
         'Sensor over filter by 2mm = 578,      3mm = 511

         'Optics values on 6/_18_2013  Unblocked= 806    Obstructed by 2mm= 701
12:      AirADC = Gripper.ReadSensorAverage
         frmMain.LastAirADCReading = AirADC
         MyPoint = Me.Location
         MyPoint.Y -= 3.0
         Arm.GoToPoint(MyPoint)
         MyPoint.Z += 1.5   'Its good to be 1.5mm high on the Z axis
         Arm.GoToPoint(MyPoint)
         ADC = Gripper.ReadSensorAverage

         'If you don't have enough drop in the ADC, then continue to move the gripper into the oven.   You can go a maximum of 10mm pas
         'the starting location of the filter.
         For Pass = 1 To 8
            If ADC > (AirADC - frmMain.OpticalSensor3mmDrop) Then 'The ADC reading drops by about 100 counts when you are 2mm over the filter edge
               MyPoint.Y -= 0.5
               Arm.GoToPoint(MyPoint)
               ADC = Gripper.ReadSensorAverage
            Else
               Exit For
            End If
         Next

         If Pass > 8 Then
            'bob load error message to filter.   Deactivate this filter, or let the user decide
            MyPoint.Y += 200.0
            Arm.GoToPoint(MyPoint)
            If MessageBox.Show("A filter can not be detected on the balance pan.   Place the filter manually on the balance pan." & vbCrLf & _
                            "Place the filter manually on the balance pan and then click OK, or continue the proceedure without the filter, click Cancel", "Filter Detection Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand) = DialogResult.Cancel Then
               Balance.CloseDoor()
               Exit Sub
            Else
               MessageBox.Show("Place the filter on the balance pan, then click OK." & vbCrLf & "NOTE!  The arm will move as soon as the OK button is clicked.", "Arm Ready", MessageBoxButtons.OK, MessageBoxIcon.Hand)
               Me.OpenDoor()
               GoTo 12
            End If
         End If

         Gripper.Close()
         MyPoint.Z += 12.0
         Arm.GoToPoint(MyPoint)
         MyPoint.Y += 150.0
         Arm.GoToPoint(MyPoint)
         Balance.CloseDoor()
      End Sub



      Public Function Init() As Boolean
         Dim State As Boolean = True

         If frmMain.TestMode = True Then
            Return True
         End If
         CloseDoor()

         Return State
      End Function


      Public Sub UnlockKeypad()
         'Dim Reply As String

         'I dont know what thecommand is   Reply = Me.SendMyCommand("K4")
      End Sub


      ''' <summary>
      ''' Subfunction accessed by all Weight commands
      ''' </summary>
      ''' <param name="data">The reply string from the weight command</param>
      ''' <returns>Weight as a string or Nothing or Error</returns>
      ''' <remarks></remarks>
      Private Function ExtractWeight(ByVal data As String) As String
         Try
            If data.Chars(0) = "S" Or data.Chars(0) = "D" Then  'Stable or Dynamic Weight received
               data = data.Substring(3)
               If Char.IsLetter(data(0)) = True Then
                  data = data.Substring(1)
               End If
               data = data.TrimStart(Chr(32))   'Remove all leading spaces
               data = data.Substring(0, data.IndexOf(" "))   'Extract everything up to the first space
            ElseIf data.Chars(0) = "+" Then
               data = "OVERLOAD"
            ElseIf data.Chars(0) = "-" Then
               data = "UNDERLOAD"
            Else
               data = "WeighError"
            End If
         Catch ex As Exception
            data = Nothing
         End Try

         Return data
      End Function


      ''' <summary>
      ''' Returns a stable weight from the balance in the currenly selected units.
      ''' </summary>
      ''' <returns>Weight of sample or the error message of "ERROR".</returns>
      ''' <remarks></remarks>
      Public Function Weight() As String
         Dim Reply As String = ""
         Dim Tries As Int32
         Static Dim BadPasses As Int32 = 0

         If ModuleDefMessages.InTestMode = True Then
            Return "5.000000"
         End If

         Try
            For Tries = 0 To 2
               Try
                  Reply = Me.SendMyCommand("S")
                  Reply = ExtractWeight(Reply)
                  If IsDouble(Reply) = True Then   'The balance returns weights in milligrams and I need it in grams
                     Reply = Format((CDbl(Reply) / 1000.0), "0.000000")
                     BadPasses = 0
                     Exit For
                  Else
                     BadPasses += 1
                     Reply = "-1.0"
                  End If
               Catch ex As Exception
                  Reply = "-1.0"
               End Try
            Next

            If BadPasses = 6 Then   'It only gets to a value of 6 by two sequential filters failing
               If Reply Is Nothing Or Reply = "-1.0" Or Reply = "OVERLOAD" Or Reply = "UNDERLOAD" Then
                  If MessageBox.Show("The balance is reporting an out of range or other error condition.   Fix this error and then click OK to resume the proceedure or CANCEL to abort the proceedure.", "Balance Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1) = DialogResult.Cancel Then
                     ModuleDefMessages.AbortRequested = True
                     Throw New System.Exception("User abort requested.")
                  End If
               End If
               Reply = "-1.0"
            End If
         Catch ex As Exception
            MessageBox.Show("Error in function Weight().   Tell Scott to call J-KEM with this error message.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try

         Return Reply
      End Function


      ''' <summary>
      ''' Returns the current weight from the balance, even if it is not a stable weight, in the currenly selected units.
      ''' </summary>
      ''' <returns>Weight of sample.</returns>
      ''' <remarks></remarks>
      Public Function WeightImmediately() As String
         Dim Reply As String = ""
         Dim Tries As Int32
         Static Dim BadPasses As Int32 = 0

         If ModuleDefMessages.InTestMode = True Then
            Return "5.000000"
         End If

         Try
            For Tries = 0 To 2
               Try
                  Reply = Me.SendMyCommand("SI")
                  Reply = ExtractWeight(Reply)
                  If IsDouble(Reply) = True Then
                     Reply = Format((CDbl(Reply) / 1000.0), "0.000000")    'The balance returns weights in milligrams and I need it in grams
                     BadPasses = 0
                     Exit For
                  Else
                     BadPasses += 1
                     Reply = "-1.0"
                  End If
               Catch ex As Exception
                  Reply = "-1.0"
               End Try
            Next

            If BadPasses = 6 Then   'It only gets to a value of 6 by two sequential filters failing
               If Reply Is Nothing Or Reply = "OVERLOAD" Or Reply = "UNDERLOAD" Then
                  If MessageBox.Show("The balance is reporting an out of range or other error condition.   Fix this error and then click OK to resume the proceedure or CANCEL to abort the proceedure.", "Balance Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1) = DialogResult.Cancel Then
                     ModuleDefMessages.AbortRequested = True
                     Throw New System.Exception("User abort requested.")
                  End If
               End If
               Reply = "-1.0"
            End If
         Catch ex As Exception
            MessageBox.Show("Error in function WeighImmediately().   Tell Scott to call J-KEM with this error message.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try

         Return Reply
      End Function



      Public Function Zero() As Boolean
         'This is really the tare command, but it is named Zero to make it compatible with the XP6.
         Dim Reply As String = ""
         Dim State As Boolean = False
         Dim Pass As Int32

         If frmMain.TestMode = True Then
            Return True
         End If

         For Pass = 0 To 3
            Reply = SendMyCommand("Z") 'No reply is sent to the tare command
            If Reply = "Z A" Then
               State = True
               Exit For
            Else
               State = False
            End If
         Next

         Return State
      End Function


      Public Sub OpenDoor()
         Dim Reply As String
         Dim State As Boolean = False
         Dim Pass As Int32

         If frmMain.TestMode = True Then
            Exit Sub
         End If

         For Pass = 0 To 2
            Reply = SendMyCommand("WS 2") 'Open door   No reply to this command
            DelayMS(1800)
            If Reply = "WS A" Then
               If MyDriver.GetInput(3) = JkemMotorDef.InputState.High Then
                  MessageBox.Show("The program can not verity that the balance door is open.  Open the door manually, and then click OK to continue.", "Balance Door Not Open", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
               End If
               Exit For
            End If
         Next
         If Pass = 3 Then
            If MessageBox.Show("The program can not verify that the balance door is fully open." & vbCrLf & "Is the door open, and should the proceedure continue?", "Balance Door Verification", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) = DialogResult.No Then
               MessageBox.Show("The robot can not proceed without the balance.", "Robot Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
               Throw New System.Exception("Balance Door Error")
            End If
         End If
      End Sub


      Public Function CloseDoor() As Boolean
         Dim Reply As String
         Dim State As Boolean = False
         Dim Tries As Int32

         If frmMain.TestMode = True Then
            Return True
         End If

         For Tries = 0 To 2
            Reply = SendMyCommand("WS 0")  'No reply to this command
            DelayMS(1800)  'It takes 1.6 seconds to close the door
            If Reply = "WS A" Then
               State = True
               Exit For
            End If
         Next
         If Tries = 3 Then
            State = False
            If MessageBox.Show("The program can not verify that the balance door is closed.  If the door is closed, click OK to continue, or click Cancel to exit program.", "Balance Door Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand) = DialogResult.Cancel Then
               Throw New System.Exception("Balance Door Error")
            End If
         End If

         Return State
      End Function


      Public Function TestComms(Optional ByRef myReply As String = "") As Boolean
         Dim Reply As String
         Dim Success As Boolean = False
         Dim pass As Int32

         MyPort.ReadTimeout = 1000
         For pass = 0 To 2
            Try
               Reply = Me.SendMyCommand("I2")
               If Reply.Contains("I2 A") Then
                  Success = True
                  Exit For
               End If
            Catch ex As Exception
            End Try
         Next
         MyPort.ReadTimeout = 120000

         Return Success
      End Function

   End Class




   Public Class JkemMotorDef
      Event PositionChange(ByVal sender As JkemMotorDef, ByVal position As Double)
      Event TransmissionError(ByVal sender As JkemMotorDef, ByVal message As String)
      'This combines the motor and the driver for the motor.

      Public Enum InputState
         High
         Low
      End Enum

      Private MyPort As Ports.SerialPort  'Reference to the comm port the driver is attached to
      Private MyAddress As Int32 'Address of the driver module
      Private MyPosition As Int32
      Private MyStartSpeed As Int32
      Private MyTopSpeed As Int32
      Private OutputStates As Int32    'Holds the status of the output lines
      Private InitialHomePerformed As Boolean
      Public AxisLenght As Int32 'Number of sets in one rotation



      Sub New(ByVal address As Int32, ByRef serialPort As Ports.SerialPort)
         MyPort = serialPort
         MyAddress = address
         MyStartSpeed = 500   'Will be updated in the home command
         MyTopSpeed = 2500   'Will be updated in the home command
         OutputStates = 0
         InitialHomePerformed = False
      End Sub


      Public Property SerialPort() As Ports.SerialPort
         Get
            Return MyPort
         End Get
         Set(ByVal value As Ports.SerialPort)
            MyPort = value
         End Set
      End Property


      ''' <summary>
      ''' Sends the provided command and formats the reply.
      ''' </summary>
      ''' <param name="myCommand">The literal string command.  Do not include a terminating '\r'</param>
      ''' <returns>The reply from the module as a string.</returns>
      ''' <remarks></remarks>
      Private Function Send(ByVal myCommand As String) As String
         'Sends the command to the driver and then examines the reply.
         'Example-      Sent:  :1SA      Reply:    *1SA1000
         'All returned parameters from the motor drivers as int32's
         Dim Reply As String = ""
         Static AllowedReentryTime As DateTime = Now


         If ModuleDefMessages.AbortRequested = True Then
            Throw New System.ApplicationException("User abort requested.")
         End If

         If ModuleDefMessages.InTestMode = True Then
            DelayMS(50)
            Return "5000.0"
         End If

         'When one serial command is executing, block any additional commands
         While DateTime.Compare(Now, AllowedReentryTime) < 0
            Application.DoEvents()
            DelayMS(20)
         End While


         If MyPort.IsOpen = False Then
            Try
               MyPort.Open()
            Catch ex As Exception
               Return "Error"
            End Try
         End If

         Try
            'Send command through serial port
            MyPort.DiscardInBuffer()
            MyPort.DiscardOutBuffer()
            MyPort.WriteLine(myCommand)
            Reply = MyPort.ReadLine

            If Reply.Length > 0 Then
               If Reply.Chars(0) = "*" Then
                  If Reply.Length > myCommand.Length Then
                     If myCommand.Contains("RV") Then   'Read Voltage
                        Reply = Reply.Substring(myCommand.Length - 1)
                     Else
                        Reply = Reply.Substring(myCommand.Length)
                     End If
                  End If
               Else
                  Reply = "Error"
               End If
            Else
               Reply = "Error"
            End If
         Catch ex As Exception
            'While a motor is in motion it ignors a status inquery, which means myport.readline will timeout.
            'you need to capture that here and return normally.
            If myCommand.Contains("ST") Then
               Reply = "0"
            ElseIf ex.Message.Contains("Timeout") Then
               Reply = "Error"
            Else
               Reply = "Error"
            End If
         End Try
         AllowedReentryTime = Now.AddMilliseconds(20.0)

         Return Reply
      End Function


      Private Function SendMyCommand(ByRef command As String) As String
         Dim Reply As String = ""
         Dim Pass As Int32

         Try
            For Pass = 0 To 2
               Reply = Me.Send(command)
               If Reply <> "Error" Then
                  Exit For
               End If
               DelayMS((Pass + 1) * 100)
            Next
            If Pass = 3 Then
               FileIO.WriteTextFile(LogFileName, "Failure in JKEMMotordef, got to pass=3," & vbCrLf, False, True)
               DelayMS(1000)
               For Pass = 0 To 2
                  Reply = Me.Send(command)
                  If Reply <> "Error" Then
                     Exit For
                  End If
               Next
               If Pass = 3 Then
                  FileIO.WriteTextFile(LogFileName, "Recover Port was called." & vbCrLf, False, True)
                  MessageBox.Show("Get Scott.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                  MessageBox.Show("The serial port was dropped on the JKEM Motordef.   Unplug the USB cable leading from the PC to the USB Hub hanging in the center of the robot deck, wait 5 seconds then plug it back in.   Do this for both hubs.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                  MessageBox.Show("Click OK to continue.", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                  Reply = RecoverPort(command)
               End If
            End If
         Catch ex As Exception
            MessageBox.Show("Error in function SendMyCommand for the MotorDef().   Tell Scott to call J-KEM with this error message." & vbCrLf & command, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End Try

         Return Reply
      End Function


      Function RecoverPort(ByVal command As String) As String
         Dim Pass As Int32
         Dim Reply As String = "Error"

         Try
            For Pass = 0 To 3
               DelayMS(500)
               Reply = Me.Send(command)
               If Reply <> "Error" Then
                  Exit For
               End If
               DelayMS(500)
            Next
            If Pass = 4 Then
               FileIO.WriteTextFile(LogFileName, "Recover Port did not work." & vbCrLf, False, True)
               MessageBox.Show("The application can not be recovered.   The application will exit.", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
               Throw New System.Exception("User abort requested.")
            End If
         Catch ex As Exception
            FileIO.WriteTextFile(LogFileName, "Recover Port caused an exception." & vbCrLf, False, True)
            MessageBox.Show("The application can not be recovered.   The application will exit.", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
            Throw New System.Exception("User abort requested.")
         End Try

         Return Reply
      End Function



      ''' <summary>
      ''' Sends the literal command.  Do not include any terminating charactors such as Cr or Lf
      ''' </summary>
      ''' <param name="command">Literal command string</param>
      ''' <returns>Controller reply</returns>
      ''' <remarks></remarks>
      Public Function SendLiteral(ByVal command As String) As String
         Dim Reply As String = ""

         Try
            Reply = Me.SendMyCommand(command)
         Catch ex As Exception
            If ex.Message = "User requested abort." Then
               Throw
            End If
            FileIO.WriteTextFile(LogFileName, "Failure in jkemmotordef SendLiteral," & vbCrLf, False, True)
            RaiseEvent TransmissionError(Me, "An error occured while sending a command to the J-KEM motor driver." & vbCrLf & _
                     "In response to the command " & command & " a reply of " & ex.Message & " was recieved." & vbCrLf & _
                                 "Stack trace: " & ex.StackTrace)
         End Try

         Return Reply
      End Function


      Public Function AxisReady() As Boolean
         'This function can be called to test the state fo the axis as to whether its in motion or not.
         Dim HoldTimeout As Int32
         Dim Status As String
         Dim Reply As Boolean = False

         Try
            'While a driver is in motion, it ignors a status inquery.  The query will timeout with no reply.
            'You need to shorten the reply time out
            If ModuleDefMessages.InTestMode = True Then
               Return True
            End If

            HoldTimeout = MyPort.ReadTimeout
            MyPort.ReadTimeout = 100
            Status = Me.SendMyCommand(":" & MyAddress.ToString & "ST")
            If Status = "1" Then
               Reply = True
            Else
               Reply = False
            End If
            MyPort.ReadTimeout = HoldTimeout

         Catch ex As Exception
            MyPort.ReadTimeout = HoldTimeout
            If ex.Message = "User requested abort." Then
               Throw
            Else
               Return False
            End If
         End Try

         Return Reply
      End Function


      Private Sub ReturnOnReadyStatus()
         'This sub is called when you issue a global motor command.  It queries the drivers
         'until a ready status is returned.
         Dim HoldTimeout As Int32
         Dim Status As String
         Dim BadReply As Boolean

         If ModuleDefMessages.InTestMode = True Then
            Return
         End If

         HoldTimeout = MyPort.ReadTimeout
         MyPort.ReadTimeout = 100
         Try
            Do
               Status = Me.SendMyCommand(":" & MyAddress.ToString & "ST")
               Select Case CType(Status, Int32)
                  Case 0
                     'A status of 0 is a return from the Send command that the port timed out in response to a status command.
                     'Drivers do not reply to a status command while a motor is in motion
                     'Do nothing, stay in the Do loop until you get a different reply
                  Case 1
                     BadReply = False
                     Exit Do
                  Case 7   'WDT timeout
                     BadReply = True
                     Exit Do
                  Case 9
                     MessageBox.Show("Motor axis " & MyAddress.ToString & " reported a STATUS of: 9.  The emergency stop switch is down.", "Motor Driver Status Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                     BadReply = True
                     Exit Do
                  Case 10
                     MessageBox.Show("Motor axis " & MyAddress.ToString & " reported a STATUS of: 10.  The home switch was unexpectedly hit.", "Motor Driver Status Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                     BadReply = True
                     Exit Do
                  Case 14  'Power brownout
                     BadReply = True
                     Exit Do
                  Case 16, 17
                     MessageBox.Show("Motor axis " & MyAddress.ToString & " reported a STATUS of: 16 or 17.  Limit switch error.", "Motor Driver Status Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                     BadReply = True
                     Exit Do
                  Case Else
                     BadReply = True
                     Exit Do
               End Select
            Loop

            If BadReply = True Then
               Me.ClearStatus()
            End If

         Catch ex As Exception
            MyPort.ReadTimeout = HoldTimeout
            If ex.Message = "User requested abort." Then
               Throw
            End If
            If ex.Message <> "Read timeout" Then
               RaiseEvent TransmissionError(Me, "An error occured while sending a command to the J-KEM motor driver." & vbCrLf & _
                  "In response to the ReturnOnReadyStatus() command a reply of " & ex.Message & " was recieved." & vbCrLf & _
                              "Stack trace: " & ex.StackTrace)
            End If
         End Try

         MyPort.ReadTimeout = HoldTimeout
      End Sub




      ''' <summary>
      ''' Performs an axis move to the absolute coordinate immediately.
      ''' </summary>
      ''' <param name="Position">Absolute position to move to.</param>
      ''' <param name="returnOnMotionComplete">Return after the motor completes its motion.</param>
      ''' <remarks></remarks>
      Public Overridable Sub Move(ByVal position As Double, Optional ByVal returnOnMotionComplete As Boolean = True)
         Dim Pos As Int32
         Dim Reply As String = ""

         Try
            Pos = CType(position, Int32)
            If Pos <> Me.MyPosition Then
                Me.SendMyCommand(":" & MyAddress.ToString & "MA" & Pos.ToString)
               If returnOnMotionComplete = True Then
                  ReturnOnReadyStatus()
               End If
               Me.MyPosition = Pos
               RaiseEvent PositionChange(Me, position)
            End If
         Catch ex As Exception
            If ex.Message = "User requested abort." Then
               Throw
            End If
            RaiseEvent TransmissionError(Me, "An error occured while sending a command to the J-KEM motor driver." & vbCrLf & _
                        "In response to the Move() command, a reply of " & ex.Message & " was recieved." & vbCrLf & _
                                 "Stack trace: " & ex.StackTrace)
            Throw New System.Exception("Motor Driver Failure.")
         End Try
      End Sub



      Public Overridable Sub Home()
         'This home routein only uses the HA command to home.
         Try
            'The motor driver does not reply immediately when a Home command (HA) is issued.   You must
            'set the readtimeout to a value high enought to allow the home to complete.
            If ModuleDefMessages.AbortRequested = False Then
               MyPort.ReadTimeout = 120000   'This is reset in ReturnOnReadyStatus
               Me.SendMyCommand(":" & MyAddress.ToString & "HA1")

               If InitialHomePerformed = False Then
                  InitialHomePerformed = True
                  Me.MyStartSpeed = CType(Me.SendMyCommand(":" & MyAddress.ToString & "LS"), Int32)
                  Me.MyTopSpeed = CType(Me.SendMyCommand(":" & MyAddress.ToString & "MS"), Int32)
               End If
               ReturnOnReadyStatus()
               MyPosition = 0
               RaiseEvent PositionChange(Me, 0.0)
            Else
               Throw New System.ApplicationException("User abort requested.")
            End If
         Catch ex As Exception
            If ex.Message = "User requested abort." Then
               Throw
            End If
            RaiseEvent TransmissionError(Me, "An error occured while sending a command to the J-KEM motor driver." & vbCrLf & _
                     "In response to the Home() command, a reply of " & ex.Message & " was recieved." & vbCrLf & _
                                 "Stack trace: " & ex.StackTrace)
            MessageBox.Show("The position of the Carosel can not be confirmed.  The application will exit now.", "Fatal Carosel Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
            Throw New System.Exception("Carosel Failure Abort")
         End Try
      End Sub


      ''' <summary>
      ''' Returns the position of the axis in units of steps.
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property Position() As Int32
         'Holds the current position of the axis
         Get
            Return MyPosition
         End Get
      End Property


      ''' <summary>
      ''' Sets the state of the specified digital output.  
      ''' </summary>
      ''' <param name="output">Digial output.  Range: 1 to 3.</param>
      ''' <param name="state">Enertized or Deentegized</param>
      ''' <remarks></remarks>
      Public Sub SetOutput(ByVal output As Int32, ByVal state As DriverState)
         'The three outputs on the driver are addressed as binary values.  The states of the 3 outlets are set with
         'every command.    State =  6  (turns outputs 2 and 3 on).
         Dim Oldstate As Int32

         Oldstate = OutputStates
         Select Case output
            Case 1
               If state = DriverState.Power_On Then
                  OutputStates = OutputStates Or 1
               Else
                  OutputStates = OutputStates And 6   'Clears the 0 bit
               End If
            Case 2
               If state = DriverState.Power_On Then
                  OutputStates = OutputStates Or 2
               Else
                  OutputStates = OutputStates And 5   'Clears the 1 bit
               End If
            Case 3
               If state = DriverState.Power_On Then
                  OutputStates = OutputStates Or 4
               Else
                  OutputStates = OutputStates And 3   'Clears the 2 bit
               End If
            Case Else
               Exit Sub
         End Select
         Try
            If Oldstate <> OutputStates Then
               Me.SendMyCommand(":" & Me.MyAddress.ToString & "SO" & OutputStates.ToString)
            End If
         Catch ex As Exception
            If ex.Message = "User requested abort." Then
               Throw
            End If
            RaiseEvent TransmissionError(Me, "An error occured while sending a command to the J-KEM motor driver." & vbCrLf & _
                     "In response to the SetOutput() command, a reply of " & ex.Message & " was recieved." & vbCrLf & _
                                 "Stack trace: " & ex.StackTrace)
         End Try
      End Sub


      ''' <summary>
      ''' Returns the state of the specified digital input.
      ''' </summary>
      ''' <param name="input"></param>
      ''' <returns>Input state as High or Low.</returns>
      ''' <remarks></remarks>
      Public Function GetInput(ByVal input As Int32) As InputState
         Dim Value As String = Nothing
         Dim State As InputState

         Try
            Value = Me.SendMyCommand(":" & Me.MyAddress.ToString & "RI")
         Catch ex As Exception
            If ex.Message = "User requested abort." Then
               Throw
            End If
            RaiseEvent TransmissionError(Me, "An error occured while sending a command to the J-KEM motor driver." & vbCrLf & _
                     "In response to the GetInput() command, a reply of " & ex.Message & " was recieved." & vbCrLf & _
                                 "Stack trace: " & ex.StackTrace)
         End Try

         If input = 1 Then
            If Value = "1" Or Value = "3" Or Value = "5" Or Value = "7" Then
               State = InputState.High
            Else
               State = InputState.Low
            End If
         ElseIf input = 2 Then
            If Value = "2" Or Value = "3" Or Value = "6" Or Value = "7" Then
               State = InputState.High
            Else
               State = InputState.Low
            End If
         ElseIf input = 3 Then
            If Value = "4" Or Value = "5" Or Value = "6" Or Value = "7" Then
               State = InputState.High
            Else
               State = InputState.Low
            End If
         End If

         Return State
      End Function


      Function GetADC(ByVal channel As Int32) As Int32
         Dim Reply As String = ""

         Try
            Reply = Me.SendMyCommand(":" & Me.MyAddress & "RV" & channel.ToString)
         Catch ex As Exception
            If ex.Message = "User requested abort." Then
               Throw
            End If
            RaiseEvent TransmissionError(Me, "An error occured while sending a command to the J-KEM motor driver." & vbCrLf & _
                     "In response to the command " & ":" & Me.MyAddress & "RV" & channel.ToString & " a reply of " & ex.Message & " was recieved." & vbCrLf & _
                                 "Stack trace: " & ex.StackTrace)
         End Try

         Return CType(Reply, Int32)
      End Function


      ''' <summary>
      ''' Clears the status word in the driver
      ''' </summary>
      ''' <remarks></remarks>
      Private Sub ClearStatus()
         Try
            Me.SendMyCommand(":" & Me.MyAddress.ToString & "ST1")
         Catch ex As Exception
            If ex.Message = "User requested abort." Then
               Throw
            End If
            RaiseEvent TransmissionError(Me, "An error occured while sending a command to the J-KEM motor driver." & vbCrLf & _
                     "In response to the ClearStatus() command, a reply of " & ex.Message & " was recieved." & vbCrLf & _
                                 "Stack trace: " & ex.StackTrace)
         End Try
      End Sub


      ''' <summary>
      ''' Global command to all drivers to save the current driver settings to EEPROM.
      ''' Valuse are recalled on powerup or by LoadUserDefaults(). 
      ''' </summary>
      ''' <remarks></remarks>
      Public Sub SaveDriverSettings()
         Try
            Me.SendMyCommand(":#SU")
         Catch ex As Exception
            If ex.Message = "User requested abort." Then
               Throw
            End If
            RaiseEvent TransmissionError(Me, "An error occured while sending a command to the J-KEM motor driver." & vbCrLf & _
                     "In response to the SaveDriverSettings() command, a reply of " & ex.Message & " was recieved." & vbCrLf & _
                                 "Stack trace: " & ex.StackTrace)
         End Try
      End Sub


      ''' <summary>
      ''' Default acceleration of the axis.  Default value is 5000.
      ''' </summary>
      ''' <param name="speed">Acceleration of the axis.  Range: 400 to 65500</param>
      ''' <remarks></remarks>
      Public Sub SetAcceleration(ByVal speed As Int32)
         Try
            speed = EnforceLimits(speed, 400, 65500)
            Me.SendMyCommand(":" & MyAddress.ToString & "SA" & speed.ToString)
         Catch ex As Exception
            If ex.Message = "User requested abort." Then
               Throw
            End If
            RaiseEvent TransmissionError(Me, "An error occured while sending a command to the J-KEM motor driver." & vbCrLf & _
                     "In response to the SetAcceleration() command, a reply of " & ex.Message & "was recieved." & vbCrLf & _
                                 "Stack trace: " & ex.StackTrace)
         End Try
      End Sub


      ''' <summary>
      ''' Sets the starting speed of the motor in units of steps per second.
      ''' </summary>
      ''' <param name="speed">Starting speed of the motor.  Range: 300 to 13,000</param>
      ''' <remarks></remarks>
      Public Sub SetStartSpeed(ByVal speed As Int32)
         Try
            speed = EnforceLimits(speed, 300, 13000)
            If speed <= Me.MyTopSpeed Then
               Me.SendMyCommand(":" & MyAddress.ToString & "LS" & speed.ToString)
               Me.MyStartSpeed = speed
            Else
               Me.SendMyCommand(":" & MyAddress.ToString & "LS" & Me.MyTopSpeed.ToString)
               Me.MyStartSpeed = Me.MyTopSpeed
            End If
         Catch ex As Exception
            If ex.Message = "User requested abort." Then
               Throw
            End If
            RaiseEvent TransmissionError(Me, "An error occured while sending a command to the J-KEM motor driver." & vbCrLf & _
                     "In response to the SetStartSpeed() command, a reply of " & ex.Message & " was recieved." & vbCrLf & _
                                 "Stack trace: " & ex.StackTrace)
         End Try
      End Sub


      ''' <summary>
      ''' Sets the run speed of the axis in units of steps/second.
      ''' </summary>
      ''' <param name="speed">The run speed of the axis.  Range: 350 to 13,000</param>
      ''' <remarks></remarks>
      Public Sub SetSpeed(ByVal speed As Double)
         Try
            speed = EnforceLimits(speed, 350, 13000)
            If speed >= Me.MyStartSpeed Then
               Me.SendMyCommand(":" & MyAddress.ToString & "MS" & speed.ToString)
               Me.MyTopSpeed = CType(speed, Int32)
            Else
               Me.SendMyCommand(":" & MyAddress.ToString & "MS" & Me.MyStartSpeed.ToString)
               Me.MyTopSpeed = Me.MyStartSpeed
            End If
         Catch ex As Exception
            If ex.Message = "User requested abort." Then
               Throw
            End If
            RaiseEvent TransmissionError(Me, "An error occured while sending a command to the J-KEM motor driver." & vbCrLf & _
                     "In response to the SetTopSpeed() command, a reply of " & ex.Message & " was recieved." & vbCrLf & _
                                 "Stack trace: " & ex.StackTrace)
         End Try
      End Sub



      ''' <summary>
      ''' Overwrites the motors axis position with the value passed in this property.  Very dangereous, use with caution.
      ''' </summary>
      ''' <value>Current step position of the motor.</value>
      ''' <remarks></remarks>
      Public WriteOnly Property SetAxisPosition() As Int32
         Set(ByVal value As Int32)
            Me.MyPosition = value
         End Set
      End Property

   End Class




   Public Class RackDef

      Public MyModules As New Collection  'This holds coordinate definition objects

      Public Enum FirstVialLocation   'The corner of the rack that has the first vial
         TopLeft
         BottomLeft
         TopRight
         BottomRight
      End Enum

      Public Enum IncDir  'In what direction does the rack increment the vial count
         ByColumn    'Vial numbers increase going down or up a column, then over to the next column
         ByRow       'Vial numbers increase going accross a row then down or up to the next row
      End Enum

      Public Enum CellCondition   'The 4 states of a cell in the paintRack array
         'The four custom states are states that the user can use when special states
         'of the cell exist.  For example, when a cell contains a standard.
         Active      'The cell is presently being operated on
         Done        'No more actions need to be performed on this cell
         Exclude     'This cell is not in use in this proceedure
         Ready       'The cell is ready for action
         Custom_1
         Custom_2
         Custom_3
         Custom_4
         ErrorState  'Marks a cell that some form of error occured
      End Enum


      'For racks of viles
      Public Name As String
      Public Rows As Int32 = 1
      Public Columns As Int32 = 1
      Public CurrentVial As Int32 = 1       'The active position in the rack.
      Public VialPositions As Int32 = 1  'The total vial positions in the rack
      Public VialsActive As Int32 = 1     'the actual number of active vials in the rack
      Public StartPosition As New FirstVialLocation 'Enumeration of the first vial location in the block
      Public IncrementDirection As New IncDir 'Do racks increment by columns or rows.  If you count accross a row and then 
      'advance to the next row, this is Increment ByRow.


      Public Structure PlateMyState
         Dim CellState As CellCondition
         Dim CurrentRack As RackDef   'Use this to track the rack that the vial is currently positioned in.
         Dim Data() As Object
         Dim Text As String
      End Structure
      Public Cell() As PlateMyState

      'Public PlateState() As CellCondition
      Public MyRectangle As New Rectangle
      Public CellSize As New Point(7, 7)      'For drawing a rack using the DrawRack class
      Public CellSeperation As New Point(3, 3) 'for drawing a rack using the drawrack class



      Sub New(ByVal myRackName As String)
         Me.Name = myRackName
      End Sub


      Sub InitializeGraphics(Optional ByVal ArrayDepth As Int32 = 0)
         'Load the Cell() structure with the state of the cells.
         'If you need it, each cell carries and array of objects with it in
         'Cell.data().  If you plan to use this, you must redimension the data array.
         'This can only be dimensioned once.
         Dim Position As Int32

         'If Me.Cell Is Nothing Then
         ReDim Me.Cell(Me.VialPositions)
         'End If
         If ArrayDepth > 0 Then
            If Me.Cell(1).Data Is Nothing Then  'Only redimension this if it hasn't been done yet
               For Position = 1 To Me.VialPositions
                  ReDim Me.Cell(Position).Data(ArrayDepth)
               Next
            End If
         End If

         For Position = 1 To Me.VialPositions
            Me.Cell(Position).CellState = CellCondition.Ready
         Next
      End Sub

      Public Overridable Sub SetAllCellsToReady()
         Dim Position As Int32

         For Position = 1 To Me.VialPositions
            Me.Cell(Position).CellState = CellCondition.Ready
         Next
      End Sub

      Public Overridable Sub SetAllCellsToExclude()
         Dim Position As Int32

         For Position = 1 To Me.VialPositions
            Me.Cell(Position).CellState = CellCondition.Exclude
         Next
      End Sub


      Public Overloads Sub SetCellSize(ByVal myPoint As Point)
         CellSize.X = myPoint.X
         CellSize.Y = myPoint.Y
         Me.CalculatePlateRectangle()
      End Sub

      Public Overloads Sub SetCellSize(Optional ByVal X As Int32 = 7, Optional ByVal Y As Int32 = 7)
         CellSize.X = X
         CellSize.Y = Y
         Me.CalculatePlateRectangle()
      End Sub

      Public Overloads Sub SetCellSeperation(ByVal myPoint As Point)
         CellSeperation.X = myPoint.X
         CellSeperation.Y = myPoint.Y
         Me.CalculatePlateRectangle()
      End Sub

      Public Overloads Sub SetCellSeperation(Optional ByVal X As Int32 = 3, Optional ByVal Y As Int32 = 3)
         CellSeperation.X = X
         CellSeperation.Y = Y
         Me.CalculatePlateRectangle()
      End Sub

      Private Sub CalculatePlateRectangle()
         Dim DeltaX As Int32
         Dim Deltay As Int32
         Dim MyX As Int32
         Dim MyY As Int32


         DeltaX = CellSize.X + CellSeperation.X
         Deltay = CellSize.Y + CellSeperation.Y

         MyX = (DeltaX * (Columns + 1)) + CellSize.X
         If DeltaX > 10 Then
            MyX = MyX - (2 * (DeltaX - 10))
         End If
         MyY = (Deltay * (Rows + 1)) + CellSize.Y
         If Deltay > 10 Then
            MyY = MyY - (2 * (Deltay - 10))
         End If
         Me.MyRectangle.Width = MyX
         Me.MyRectangle.Height = MyY
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
            CellPixels = myWidth / ((Me.Columns * 1.1) + 2)
            If CellPixels > 10 Then
               BorderPixels = 10
            Else
               BorderPixels = CType(CellPixels, Int32)
            End If
            Me.CellSize.X = CType((myWidth - (2 * BorderPixels)) / (Me.Columns * 1.1), Int32)
         Else    'Size determined by height of plate
            CellPixels = myHeight / ((Me.Rows * 1.1) + 2)
            If CellPixels > 10 Then
               BorderPixels = 10
            Else
               BorderPixels = CType(CellPixels, Int32)
            End If
            Me.CellSize.X = CType((myHeight - (2 * BorderPixels)) / (Me.Rows * 1.1), Int32)
         End If

         Me.CellSize.Y = Me.CellSize.X
         Me.CellSeperation.X = CType(Me.CellSize.X / 10, Int32)
         If Me.CellSeperation.X < 2 Then
            Me.CellSeperation.X = 2
         End If
         Me.CellSeperation.Y = Me.CellSeperation.X
         Me.MyRectangle.Width = (BorderPixels * 2) + (Me.Columns * Me.CellSize.X) + ((Me.Columns - 1) * Me.CellSeperation.X)
         Me.MyRectangle.Height = (BorderPixels * 2) + (Me.Rows * Me.CellSize.Y) + ((Me.Rows - 1) * Me.CellSeperation.Y)
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
      ''' Set the display origin of the graphical rack repesentation
      ''' </summary>
      ''' <param name="mypoint">Rack origin</param>
      ''' <remarks></remarks>
      Public Overridable Overloads Sub SetRackGraphicOrigin(ByVal mypoint As Point)
         Me.MyRectangle.X = mypoint.X
         Me.MyRectangle.Y = mypoint.Y
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
         Dim myCell As Int32
         Dim myRow As Int32
         Dim myColumn As Int32
         Dim myX As Int32
         Dim myY As Int32
         Dim DeltaX As Int32
         Dim DeltaY As Int32
         Dim MyFont As New Font("Arial", 8)
         Dim TextX As Single
         Dim TextY As Single
         Dim myGraphics As System.Drawing.Graphics


         myGraphics = Control.CreateGraphics()
         DeltaX = CellSize.X + CellSeperation.X
         DeltaY = CellSize.Y + CellSeperation.Y

         myX = (DeltaX * (Columns + 1)) + CellSize.X
         If DeltaX > 10 Then
            myX = myX - (2 * (DeltaX - 10))
         End If
         myY = (DeltaY * (Rows + 1)) + CellSize.Y
         If DeltaY > 10 Then
            myY = myY - (2 * (DeltaY - 10))
         End If
         Dim myRec As New Rectangle(MyRectangle.X, MyRectangle.Y, myX, myY)

         myGraphics.DrawRectangle(PenColor, myRec)
         myGraphics.FillRectangle(BrushColor, myRec.Left + 1, myRec.Top + 1, myX - 1, myY - 1)
         myCell = 1

         Select Case IncrementDirection
            Case IncDir.ByRow
               Select Case StartPosition
                  Case FirstVialLocation.TopLeft
                     For myRow = 0 To Rows - 1
                        myY = (myRec.Top + DeltaY) + (myRow * DeltaY)
                        If DeltaY > 10 Then
                           myY = myY - (DeltaY - 10)
                        End If
                        For myColumn = 0 To Columns - 1
                           myX = (myRec.Left + DeltaX) + (myColumn * DeltaX)
                           If DeltaX > 10 Then
                              myX = myX - (DeltaX - 10)
                           End If
                           BrushColor.Color = GetColor(myCell)
                           myGraphics.FillEllipse(BrushColor, myX, myY, CellSize.X, CellSize.Y)
                           If Not Me.Cell(myCell).Text = Nothing Then
                              Dim SF As New StringFormat
                              SF.Alignment = StringAlignment.Center
                              SF.LineAlignment = StringAlignment.Center

                              TextX = CType(myX + 1 + (CellSize.X / 2.0), Single)
                              TextY = CType(myY + 1 + (CellSize.Y / 2.0), Single)
                              If BrushColor.Color = Color.Blue Then
                                 TextBrush.Color = Color.White
                              Else
                                 TextBrush.Color = Color.Black
                              End If
                              myGraphics.DrawString(Me.Cell(myCell).Text, MyFont, TextBrush, TextX, TextY, SF)
                           End If
                           myCell += 1
                        Next myColumn
                     Next myRow

                  Case FirstVialLocation.TopRight
                     For myRow = 0 To Rows - 1
                        myY = (myRec.Top + DeltaY) + (myRow * DeltaY)
                        If DeltaY > 10 Then
                           myY = myY - (DeltaY - 10)
                        End If
                        For myColumn = 0 To Columns - 1
                           myX = (myRec.Right - CellSize.X - DeltaX) - (myColumn * DeltaX)
                           If DeltaX > 10 Then
                              myX = myX + (DeltaX - 10)
                           End If
                           BrushColor.Color = GetColor(myCell)
                           myGraphics.FillEllipse(BrushColor, myX, myY, CellSize.X, CellSize.Y)
                           If Not Me.Cell(myCell).Text = Nothing Then
                              Dim SF As New StringFormat
                              SF.Alignment = StringAlignment.Center
                              SF.LineAlignment = StringAlignment.Center

                              TextX = CType(myX + 1 + (CellSize.X / 2.0), Single)
                              TextY = CType(myY + 1 + (CellSize.Y / 2.0), Single)
                              If BrushColor.Color = Color.Blue Then
                                 TextBrush.Color = Color.White
                              Else
                                 TextBrush.Color = Color.Black
                              End If
                              myGraphics.DrawString(Me.Cell(myCell).Text, MyFont, TextBrush, TextX, TextY, SF)
                           End If
                           myCell += 1
                        Next myColumn
                     Next myRow

                  Case FirstVialLocation.BottomRight
                     For myRow = 0 To Rows - 1
                        myY = (myRec.Bottom - CellSize.Y - DeltaY) - (myRow * DeltaY)
                        If DeltaY > 10 Then
                           myY = myY + (DeltaY - 10)
                        End If
                        For myColumn = 0 To Columns - 1
                           myX = (myRec.Right - CellSize.X - DeltaX) - (myColumn * DeltaX)
                           If DeltaX > 10 Then
                              myX = myX + (DeltaX - 10)
                           End If
                           BrushColor.Color = GetColor(myCell)
                           myGraphics.FillEllipse(BrushColor, myX, myY, CellSize.X, CellSize.Y)
                           If Not Me.Cell(myCell).Text = Nothing Then
                              Dim SF As New StringFormat
                              SF.Alignment = StringAlignment.Center
                              SF.LineAlignment = StringAlignment.Center

                              TextX = CType(myX + 1 + (CellSize.X / 2.0), Single)
                              TextY = CType(myY + 1 + (CellSize.Y / 2.0), Single)
                              If BrushColor.Color = Color.Blue Then
                                 TextBrush.Color = Color.White
                              Else
                                 TextBrush.Color = Color.Black
                              End If
                              myGraphics.DrawString(Me.Cell(myCell).Text, MyFont, TextBrush, TextX, TextY, SF)
                           End If
                           myCell += 1
                        Next myColumn
                     Next myRow

                  Case FirstVialLocation.BottomLeft
                     For myRow = 0 To Rows - 1
                        myY = (myRec.Bottom - CellSize.Y - DeltaY) - (myRow * DeltaY)
                        If DeltaY > 10 Then
                           myY = myY + (DeltaY - 10)
                        End If
                        For myColumn = 0 To Columns - 1
                           myX = (myRec.Left + DeltaX) + (myColumn * DeltaX)
                           If DeltaX > 10 Then
                              myX = myX - (DeltaX - 10)
                           End If
                           BrushColor.Color = GetColor(myCell)
                           myGraphics.FillEllipse(BrushColor, myX, myY, CellSize.X, CellSize.Y)
                           If Not Me.Cell(myCell).Text = Nothing Then
                              Dim SF As New StringFormat
                              SF.Alignment = StringAlignment.Center
                              SF.LineAlignment = StringAlignment.Center

                              TextX = CType(myX + 1 + (CellSize.X / 2.0), Single)
                              TextY = CType(myY + 1 + (CellSize.Y / 2.0), Single)
                              If BrushColor.Color = Color.Blue Then
                                 TextBrush.Color = Color.White
                              Else
                                 TextBrush.Color = Color.Black
                              End If
                              myGraphics.DrawString(Me.Cell(myCell).Text, MyFont, TextBrush, TextX, TextY, SF)
                           End If
                           myCell += 1
                        Next myColumn
                     Next myRow
               End Select

            Case IncDir.ByColumn
               Select Case StartPosition
                  Case FirstVialLocation.TopLeft
                     For myColumn = 0 To Columns - 1
                        myX = (myRec.Left + DeltaX) + (myColumn * DeltaX)
                        If DeltaX > 10 Then
                           myX = myX - (DeltaX - 10)
                        End If
                        For myRow = 0 To Rows - 1
                           myY = (myRec.Top + DeltaY) + (myRow * DeltaY)
                           If DeltaY > 10 Then
                              myY = myY - (DeltaY - 10)
                           End If
                           BrushColor.Color = GetColor(myCell)
                           myGraphics.FillEllipse(BrushColor, myX, myY, CellSize.X, CellSize.Y)
                           If Not Me.Cell(myCell).Text = Nothing Then
                              Dim SF As New StringFormat
                              SF.Alignment = StringAlignment.Center
                              SF.LineAlignment = StringAlignment.Center

                              TextX = CType(myX + 1 + (CellSize.X / 2.0), Single)
                              TextY = CType(myY + 1 + (CellSize.Y / 2.0), Single)
                              If BrushColor.Color = Color.Blue Then
                                 TextBrush.Color = Color.White
                              Else
                                 TextBrush.Color = Color.Black
                              End If
                              myGraphics.DrawString(Me.Cell(myCell).Text, MyFont, TextBrush, TextX, TextY, SF)
                           End If
                           myCell += 1
                        Next myRow
                     Next myColumn

                  Case FirstVialLocation.TopRight
                     For myColumn = 0 To Columns - 1
                        myX = (myRec.Right - CellSize.X - DeltaX) - (myColumn * DeltaX)
                        If DeltaX > 10 Then
                           myX = myX + (DeltaX - 10)
                        End If
                        For myRow = 0 To Rows - 1
                           myY = (myRec.Top + DeltaY) + (myRow * DeltaY)
                           If DeltaY > 10 Then
                              myY = myY - (DeltaY - 10)
                           End If
                           BrushColor.Color = GetColor(myCell)
                           myGraphics.FillEllipse(BrushColor, myX, myY, CellSize.X, CellSize.Y)
                           If Not Me.Cell(myCell).Text = Nothing Then
                              Dim SF As New StringFormat
                              SF.Alignment = StringAlignment.Center
                              SF.LineAlignment = StringAlignment.Center

                              TextX = CType(myX + 1 + (CellSize.X / 2.0), Single)
                              TextY = CType(myY + 1 + (CellSize.Y / 2.0), Single)
                              If BrushColor.Color = Color.Blue Then
                                 TextBrush.Color = Color.White
                              Else
                                 TextBrush.Color = Color.Black
                              End If
                              myGraphics.DrawString(Me.Cell(myCell).Text, MyFont, TextBrush, TextX, TextY, SF)
                           End If
                           myCell += 1
                        Next myRow
                     Next myColumn

                  Case FirstVialLocation.BottomRight
                     For myColumn = 0 To Columns - 1
                        myX = (myRec.Right - CellSize.X - DeltaX) - (myColumn * DeltaX)
                        If DeltaX > 10 Then
                           myX = myX + (DeltaX - 10)
                        End If
                        For myRow = 0 To Rows - 1
                           myY = (myRec.Bottom - CellSize.Y - DeltaY) - (myRow * DeltaY)
                           If DeltaY > 10 Then
                              myY = myY + (DeltaY - 10)
                           End If
                           BrushColor.Color = GetColor(myCell)
                           myGraphics.FillEllipse(BrushColor, myX, myY, CellSize.X, CellSize.Y)
                           If Not Me.Cell(myCell).Text = Nothing Then
                              Dim SF As New StringFormat
                              SF.Alignment = StringAlignment.Center
                              SF.LineAlignment = StringAlignment.Center

                              TextX = CType(myX + 1 + (CellSize.X / 2.0), Single)
                              TextY = CType(myY + 1 + (CellSize.Y / 2.0), Single)
                              If BrushColor.Color = Color.Blue Then
                                 TextBrush.Color = Color.White
                              Else
                                 TextBrush.Color = Color.Black
                              End If
                              myGraphics.DrawString(Me.Cell(myCell).Text, MyFont, TextBrush, TextX, TextY, SF)
                           End If
                           myCell += 1
                        Next myRow
                     Next myColumn

                  Case FirstVialLocation.BottomLeft
                     For myColumn = 0 To Columns - 1
                        myX = (myRec.Left + DeltaX) + (myColumn * DeltaX)
                        If DeltaX > 10 Then
                           myX = myX - (DeltaX - 10)
                        End If
                        For myRow = 0 To Rows - 1
                           myY = (myRec.Bottom - CellSize.Y - DeltaY) - (myRow * DeltaY)
                           If DeltaY > 10 Then
                              myY = myY + (DeltaY - 10)
                           End If
                           BrushColor.Color = GetColor(myCell)
                           myGraphics.FillEllipse(BrushColor, myX, myY, CellSize.X, CellSize.Y)
                           If Not Me.Cell(myCell).Text = Nothing Then
                              Dim SF As New StringFormat
                              SF.Alignment = StringAlignment.Center
                              SF.LineAlignment = StringAlignment.Center

                              TextX = CType(myX + 1 + (CellSize.X / 2.0), Single)
                              TextY = CType(myY + 1 + (CellSize.Y / 2.0), Single)
                              If BrushColor.Color = Color.Blue Then
                                 TextBrush.Color = Color.White
                              Else
                                 TextBrush.Color = Color.Black
                              End If
                              myGraphics.DrawString(Me.Cell(myCell).Text, MyFont, TextBrush, TextX, TextY, SF)
                           End If
                           myCell += 1
                        Next myRow
                     Next myColumn
               End Select
         End Select
         myGraphics.Dispose()
      End Sub


      Private Function GetColor(ByVal myCell As Int32) As Color
         Dim Mycolor As Color

         If Me.Cell(myCell).CellState = CellCondition.Ready Then
            Mycolor = Color.Lime
         ElseIf Me.Cell(myCell).CellState = CellCondition.Active Then
            Mycolor = Color.Yellow
         ElseIf Me.Cell(myCell).CellState = CellCondition.Done Then
            Mycolor = Color.Blue
         ElseIf Me.Cell(myCell).CellState = CellCondition.ErrorState Then
            Mycolor = Color.Red
         ElseIf Me.Cell(myCell).CellState = CellCondition.Custom_1 Then
            Mycolor = Color.Magenta
         ElseIf Me.Cell(myCell).CellState = CellCondition.Custom_2 Then
            Mycolor = Color.Cyan
         ElseIf Me.Cell(myCell).CellState = CellCondition.Custom_3 Then
            Mycolor = Color.Peru
         ElseIf Me.Cell(myCell).CellState = CellCondition.Custom_4 Then
            Mycolor = Color.LightSteelBlue
         Else    'Inactive cell
            Mycolor = Color.Gray
         End If
         Return Mycolor
      End Function


      Overloads Function IsInPlate(ByVal X As Int32, ByVal Y As Int32) As Boolean
         'This function will return true if the coordinates are contained in this rack
         'and false if not.   It is useful for processing mouse clicks.

         If MyRectangle.Contains(X, Y) Then
            IsInPlate = True
         Else
            IsInPlate = False
         End If
      End Function

      Overloads Function IsInPlate(ByVal e As System.Windows.Forms.MouseEventArgs) As Boolean
         'This function will return true if the coordinates are contained in this rack
         'and false if not.   It is useful for processing mouse clicks.

         If MyRectangle.Contains(e.X, e.Y) Then
            IsInPlate = True
         Else
            IsInPlate = False
         End If
      End Function

      Overridable Overloads Function SelectCell(ByVal X As Int32, ByVal Y As Int32) As Int32
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


         DeltaX = CellSize.X + CellSeperation.X
         DeltaY = CellSize.Y + CellSeperation.Y

         Width = MyRectangle.Width
         X_Offset = ((Width - (DeltaX * Columns) + CellSeperation.X) \ 2) - (CellSeperation.X \ 2)

         Height = MyRectangle.Height
         Y_Offset = ((Height - (DeltaY * Rows) + CellSeperation.Y) \ 2) - (CellSeperation.Y \ 2)

         If MyRectangle.Contains(X, Y) Then   'Are you in the plate?
            'Determine the row and column assuming the start cell is topleft, by row
            X = X - MyRectangle.X - X_Offset
            Column = X \ DeltaX + 1
            Y = Y - MyRectangle.Y - Y_Offset
            Row = Y \ DeltaY + 1

            Select Case IncrementDirection
               Case IncDir.ByRow
                  Select Case StartPosition
                     Case FirstVialLocation.TopLeft
                        Cell = ((Row - 1) * Me.Columns) + Column
                     Case FirstVialLocation.BottomLeft
                        Row = Me.Rows - Row + 1
                        Cell = ((Row - 1) * Me.Columns) + Column
                     Case FirstVialLocation.TopRight
                        Column = Me.Columns - Column + 1
                        Cell = ((Row - 1) * Me.Columns) + Column
                     Case FirstVialLocation.BottomRight
                        Row = Me.Rows - Row + 1
                        Column = Me.Columns - Column + 1
                        Cell = ((Row - 1) * Me.Columns) + Column
                  End Select

               Case IncDir.ByColumn
                  Select Case StartPosition
                     Case FirstVialLocation.TopLeft
                        Cell = ((Column - 1) * Me.Rows) + Row
                     Case FirstVialLocation.BottomLeft
                        Row = Me.Rows - Row + 1
                        Cell = ((Column - 1) * Me.Rows) + Row
                     Case FirstVialLocation.TopRight
                        Column = Me.Columns - Column + 1
                        Cell = ((Column - 1) * Me.Rows) + Row
                     Case FirstVialLocation.BottomRight
                        Row = Me.Rows - Row + 1
                        Column = Me.Columns - Column + 1
                        Cell = ((Column - 1) * Me.Rows) + Row
                  End Select
            End Select
         Else
            Cell = 0
         End If

         Return Cell
      End Function


      Overridable Overloads Function SelectCell(ByVal e As System.Windows.Forms.MouseEventArgs) As Int32
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


         X = e.X
         Y = e.Y
         DeltaX = CellSize.X + CellSeperation.X
         DeltaY = CellSize.Y + CellSeperation.Y

         Width = MyRectangle.Width
         X_Offset = ((Width - (DeltaX * Columns) + CellSeperation.X) \ 2) - (CellSeperation.X \ 2)

         Height = MyRectangle.Height
         Y_Offset = ((Height - (DeltaY * Rows) + CellSeperation.Y) \ 2) - (CellSeperation.Y \ 2)

         If MyRectangle.Contains(e.X, e.Y) Then   'Are you in the plate?
            'Determine the row and column assuming the start cell is topleft, by row
            X = X - MyRectangle.X - X_Offset
            Column = X \ DeltaX + 1
            Y = Y - MyRectangle.Y - Y_Offset
            Row = Y \ DeltaY + 1

            Select Case IncrementDirection
               Case IncDir.ByRow
                  Select Case StartPosition
                     Case FirstVialLocation.TopLeft
                        Cell = ((Row - 1) * Me.Columns) + Column
                     Case FirstVialLocation.BottomLeft
                        Row = Me.Rows - Row + 1
                        Cell = ((Row - 1) * Me.Columns) + Column
                     Case FirstVialLocation.TopRight
                        Column = Me.Columns - Column + 1
                        Cell = ((Row - 1) * Me.Columns) + Column
                     Case FirstVialLocation.BottomRight
                        Row = Me.Rows - Row + 1
                        Column = Me.Columns - Column + 1
                        Cell = ((Row - 1) * Me.Columns) + Column
                  End Select

               Case IncDir.ByColumn
                  Select Case StartPosition
                     Case FirstVialLocation.TopLeft
                        Cell = ((Column - 1) * Me.Rows) + Row
                     Case FirstVialLocation.BottomLeft
                        Row = Me.Rows - Row + 1
                        Cell = ((Column - 1) * Me.Rows) + Row
                     Case FirstVialLocation.TopRight
                        Column = Me.Columns - Column + 1
                        Cell = ((Column - 1) * Me.Rows) + Row
                     Case FirstVialLocation.BottomRight
                        Row = Me.Rows - Row + 1
                        Column = Me.Columns - Column + 1
                        Cell = ((Column - 1) * Me.Rows) + Row
                  End Select
            End Select
         Else
            Cell = 0
         End If

         Return Cell
      End Function

   End Class


End Module
