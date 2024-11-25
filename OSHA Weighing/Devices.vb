Imports System.IO
Imports System.Environment


Module Devices
   Public LogFileName As String

   Public PortCollection As New Collection
   Public Oven1Port As New IO.Ports.SerialPort
   Public Oven2Port As New IO.Ports.SerialPort
   Public Oven3Port As New IO.Ports.SerialPort
   Public Oven4Port As New IO.Ports.SerialPort
   Public Oven5Port As New IO.Ports.SerialPort
   Public CaroselPort As New IO.Ports.SerialPort
   Public BarCodePort As New IO.Ports.SerialPort
   Public BalancePort As New IO.Ports.SerialPort


   Public WithEvents Oven1 As New OvenDef(Oven1Port, "103")  'The address of the oven is the address written to the USB board.  Format:  00000103:OSHA-USB-MG-0-061810
   Public WithEvents Oven2 As New OvenDef(Oven2Port, "104")
   Public WithEvents Oven3 As New OvenDef(Oven3Port, "100")
   Public WithEvents Oven4 As New OvenDef(Oven4Port, "102")
   Public WithEvents Oven5 As New OvenDef(Oven5Port, "101")
   Public WithEvents Carosel As New CaroselDef(CaroselPort)
   Public WithEvents BarCode As New BarCodeDef(BarCodePort)
   Public WithEvents Balance As New XP6_BalanceDef(BalancePort)  'Bob or Scott, you will need to change this to a XP6_BalanceDef when you put the new balance back
   Public Deionizer As New DeionizerDef()
   Public Gripper As New GripperDef()   'The gripper is operated by the motor driver in the carosel
   Public Arm As New EpsonArm()

   Public OvenArray() As OvenDef 'Place the ovens into an array so you can address them as a group.   1 based array.


   Public Sub InitDevices()
      LogFileName = GetFolderPath(SpecialFolder.Desktop) & "\Command Log " & My.Settings.DefaultFileExtension.ToString & ".csv"
      My.Settings.DefaultFileExtension += 1
      My.Settings.Save()

      PortCollection.Add(Oven1Port)
      PortCollection.Add(Oven2Port)
      PortCollection.Add(Oven3Port)
      PortCollection.Add(Oven4Port)
      PortCollection.Add(Oven5Port)
      PortCollection.Add(CaroselPort)
      PortCollection.Add(BarCodePort)
      PortCollection.Add(BalancePort)

      ReDim OvenArray(4)
      OvenArray(0) = Oven1
      OvenArray(1) = Oven2
      OvenArray(2) = Oven3
      OvenArray(3) = Oven4
      OvenArray(4) = Oven5

      '************************************************************************
      '*****************************************************************************
      '*************** How to Calibrate Positions *************************************
      'Controls have  been added to teh carosel and oven screens to all manual control of the Epson arm

      '*********************************************************************************

      'The carosel is driven by a J-KEM motor driver.   There are 5 racks on the carosel, this loads the home positions and delta-X for each rack.
      'Just because of where the home sensor was placed, when at home the carosel is positioned at Rack=2, column=7.  To get to column 8 you need 
      'to go about 188 steps, but this would pass through the home sensor.  To get to column 8,  you need to home, then go to position 58.
      'The MoveToRack command takes care of all of this for you.
      'Delta X for all columns is 188.5, which is very close to the actual number for all racks.  Any error is compenstated for in the Location definitions
      'of the robotic arm for the carosel.
      'Keep these positions as constants.  Adjust the coordinates that the robot arm accesses the rack rather than change these.
      Carosel.MyDriver.AxisLenght = 39050    'One rotation

      Carosel.RackOrigin(1).OriginX = 25250
      Carosel.RackOrigin(1).DeltaX = 979.143  'Column 8 position is 32104
      Carosel.RackOrigin(2).OriginX = 33124
      Carosel.RackOrigin(2).DeltaX = 976.67  'Column 7 position is 38984.  The top right coordinate is on column 7, not 8
      Carosel.RackOrigin(2).SpecialCoordinateRack2 = 508
      Carosel.RackOrigin(3).OriginX = 1570
      Carosel.RackOrigin(3).DeltaX = 979.714  'Column 8 position is 8428
      Carosel.RackOrigin(4).OriginX = 9460
      Carosel.RackOrigin(4).DeltaX = 978.571  'Column 8 position is 16310
      Carosel.RackOrigin(5).OriginX = 17362
      Carosel.RackOrigin(5).DeltaX = 978.571  'Column 8 position is 24212

      'Filter coordinate definition.**********************************************************
      'The coordinate of a filter is defined as the point in space where the gripper touches the front edge when the gripper is closed.
      '*************************************

      'The 3 corners of each of the carosel racks are mapped below.
      'Rack 1
      'Top Left Filter
      Carosel.RackLoc(0, 0).X = -711.39
      Carosel.RackLoc(0, 0).Y = -375.92
      Carosel.RackLoc(0, 0).Z = -128.0
      Carosel.RackLoc(0, 0).U = 273.66
      Carosel.RackLoc(0, 0).InsertX = -0.848 'To use the sub like MoveIn(1mm), you need these to know how much X and Y need to be adjusted to move the
      Carosel.RackLoc(0, 0).InsertY = -0.53   'arm on a streight line.  "Streigh line" is a function of the arms current angle.
      'Top Right Filter
      Carosel.RackLoc(0, 1).X = -711.52
      Carosel.RackLoc(0, 1).Y = -377.67
      Carosel.RackLoc(0, 1).Z = -128.19
      Carosel.RackLoc(0, 1).U = 272.69
      Carosel.RackLoc(0, 1).InsertX = -0.848  'These are generally not adjusted when coordinates are updated.
      Carosel.RackLoc(0, 1).InsertY = -0.53
      'Bottom Left Filter
      Carosel.RackLoc(0, 2).X = -714.66
      Carosel.RackLoc(0, 2).Y = -375.35
      Carosel.RackLoc(0, 2).Z = -355.32
      Carosel.RackLoc(0, 2).U = 273.46
      Carosel.RackLoc(0, 2).InsertX = -0.848
      Carosel.RackLoc(0, 2).InsertY = -0.53

      'Rack 2
      'Top Left Filter
      Carosel.RackLoc(1, 0).X = -711.66
      Carosel.RackLoc(1, 0).Y = -378.26
      Carosel.RackLoc(1, 0).Z = -127.73
      Carosel.RackLoc(1, 0).U = 272.69
      Carosel.RackLoc(1, 0).InsertX = -0.8493
      Carosel.RackLoc(1, 0).InsertY = -0.5279
      'Top Right Filter of column 7, not column 8 like all the others.  NOTE - This is column 7
      Carosel.RackLoc(1, 1).X = -712.36
      Carosel.RackLoc(1, 1).Y = -377.47
      Carosel.RackLoc(1, 1).Z = -127.18
      Carosel.RackLoc(1, 1).U = 272.65
      Carosel.RackLoc(1, 1).InsertX = -0.8493
      Carosel.RackLoc(1, 1).InsertY = -0.5279
      'Bottom Left Filter
      Carosel.RackLoc(1, 2).X = -713.66
      Carosel.RackLoc(1, 2).Y = -375.37
      Carosel.RackLoc(1, 2).Z = -354.5
      Carosel.RackLoc(1, 2).U = 273.05
      Carosel.RackLoc(1, 2).InsertX = -0.8493
      Carosel.RackLoc(1, 2).InsertY = -0.5279

      'Rack 3
      'Top Left Filter
      Carosel.RackLoc(2, 0).X = -712.26
      Carosel.RackLoc(2, 0).Y = -377.47
      Carosel.RackLoc(2, 0).Z = -126.68
      Carosel.RackLoc(2, 0).U = 272.65
      Carosel.RackLoc(2, 0).InsertX = -0.8503
      Carosel.RackLoc(2, 0).InsertY = -0.5264
      'Top Right Filter
      Carosel.RackLoc(2, 1).X = -710.56
      Carosel.RackLoc(2, 1).Y = -378.08
      Carosel.RackLoc(2, 1).Z = -128.14
      Carosel.RackLoc(2, 1).U = 272.649
      Carosel.RackLoc(2, 1).InsertX = -0.8503
      Carosel.RackLoc(2, 1).InsertY = -0.5264
      'Bottom Left Filter
      Carosel.RackLoc(2, 2).X = -713.46
      Carosel.RackLoc(2, 2).Y = -376.94
      Carosel.RackLoc(2, 2).Z = -353.78
      Carosel.RackLoc(2, 2).U = 272.65
      Carosel.RackLoc(2, 2).InsertX = -0.8493
      Carosel.RackLoc(2, 2).InsertY = -0.5279

      'Rack 4
      'Top Left Filter
      Carosel.RackLoc(3, 0).X = -709.46
      Carosel.RackLoc(3, 0).Y = -376.54
      Carosel.RackLoc(3, 0).Z = -127.74
      Carosel.RackLoc(3, 0).U = 272.65
      Carosel.RackLoc(3, 0).InsertX = -0.8455
      Carosel.RackLoc(3, 0).InsertY = -0.534
      'Top Right Filter
      Carosel.RackLoc(3, 1).X = -709.26
      Carosel.RackLoc(3, 1).Y = -377.24
      Carosel.RackLoc(3, 1).Z = -128.2
      Carosel.RackLoc(3, 1).U = 272.65
      Carosel.RackLoc(3, 1).InsertX = -0.8455
      Carosel.RackLoc(3, 1).InsertY = -0.534
      'Bottom Left Filter
      Carosel.RackLoc(3, 2).X = -711.86
      Carosel.RackLoc(3, 2).Y = -376.37
      Carosel.RackLoc(3, 2).Z = -355.06
      Carosel.RackLoc(3, 2).U = 272.65
      Carosel.RackLoc(3, 2).InsertX = -0.8455
      Carosel.RackLoc(3, 2).InsertY = -0.534

      'Rack 5
      'Top Left Filter
      Carosel.RackLoc(4, 0).X = -710.36
      Carosel.RackLoc(4, 0).Y = -377.64
      Carosel.RackLoc(4, 0).Z = -128.0
      Carosel.RackLoc(4, 0).U = 272.65
      Carosel.RackLoc(4, 0).InsertX = -0.8533
      Carosel.RackLoc(4, 0).InsertY = -0.5215
      'Top Right Filter
      Carosel.RackLoc(4, 1).X = -711.23
      Carosel.RackLoc(4, 1).Y = -379.04
      Carosel.RackLoc(4, 1).Z = -128.23
      Carosel.RackLoc(4, 1).U = 272.649
      Carosel.RackLoc(4, 1).InsertX = -0.8533
      Carosel.RackLoc(4, 1).InsertY = -0.5215
      'Bottom Left Filter
      Carosel.RackLoc(4, 2).X = -711.96
      Carosel.RackLoc(4, 2).Y = -376.64
      Carosel.RackLoc(4, 2).Z = -355.23
      Carosel.RackLoc(4, 2).U = 272.65
      Carosel.RackLoc(4, 2).InsertX = -0.8533
      Carosel.RackLoc(4, 2).InsertY = -0.5215


      'The bar code scanner has a single point location.  It is the position to place the filter at to read it
      BarCode.Location.X = -619.445
      BarCode.Location.Y = -333.825
      BarCode.Location.Z = -143.727 'bob was at -153.727
      BarCode.Location.U = 239.286

      'The balance has a single location.  It is the position to place the filter at on the pan.
      Balance.Location.X = -593.044
      Balance.Location.Y = -351.299 'When the XP6 was in place, this coordinate was -351.571
      Balance.Location.Z = -400.819
      Balance.Location.U = 330.088

      'The location to hold the filter in the deionizer
      Deionizer.MyLocation.X = Balance.Location.X   'Make this the same as the balance.X
      Deionizer.MyLocation.Y = -186.65
      Deionizer.MyLocation.Z = -420.0
      Deionizer.MyLocation.U = Balance.Location.U     'Same as the balance.U




      'The four corners of the ovens are mapped and held in RackLoc().  Element 0 is the top left corner, 1 is the top right, 2 is the bottom left,
      'and 3 is the bottom right.   The filter location is defined as the point were when the filter is located at the front edge of the holder, the 
      'gripper is centered and just touching the front edge of the filter.
      'Oven 1, Top Left Filter
      Oven1.RackLoc(0).X = -720.45
      Oven1.RackLoc(0).Y = -17.24
      Oven1.RackLoc(0).Z = -73.31
      Oven1.RackLoc(0).U = 239.19
      Oven1.RackLoc(0).InsertX = -0.9999
      Oven1.RackLoc(0).InsertY = -0.0118
      'Top Right Filter
      Oven1.RackLoc(1).X = -727.05
      Oven1.RackLoc(1).Y = 320.14
      Oven1.RackLoc(1).Z = -71.13
      Oven1.RackLoc(1).U = 239.19
      Oven1.RackLoc(1).InsertX = -0.9999
      Oven1.RackLoc(1).InsertY = -0.015
      'Bottom Left Filter
      Oven1.RackLoc(2).X = -721.05
      Oven1.RackLoc(2).Y = -16.59
      Oven1.RackLoc(2).Z = -300.161
      Oven1.RackLoc(2).U = 239.198
      Oven1.RackLoc(2).InsertX = -0.9999
      Oven1.RackLoc(2).InsertY = -0.0118
      'Bottom Right Filter
      Oven1.RackLoc(3).X = -728.74
      Oven1.RackLoc(3).Y = 321.22
      Oven1.RackLoc(3).Z = -298.661
      Oven1.RackLoc(3).U = 239.198
      Oven1.RackLoc(3).InsertX = -0.9999
      Oven1.RackLoc(3).InsertY = -0.015

      'Oven 2, Top Left Filter
      Oven2.RackLoc(0).X = -654.69
      Oven2.RackLoc(0).Y = 466.83
      Oven2.RackLoc(0).Z = -77.31
      Oven2.RackLoc(0).U = 183.58
      Oven2.RackLoc(0).InsertX = -0.5865
      Oven2.RackLoc(0).InsertY = 0.8099
      'Top Right Filter
      Oven2.RackLoc(1).X = -382.32
      Oven2.RackLoc(1).Y = 664.59
      Oven2.RackLoc(1).Z = -78.32
      Oven2.RackLoc(1).U = 183.48
      Oven2.RackLoc(1).InsertX = -0.58
      Oven2.RackLoc(1).InsertY = 0.8146
      'Bottom Left Filter
      Oven2.RackLoc(2).X = -656.79
      Oven2.RackLoc(2).Y = 469.53
      Oven2.RackLoc(2).Z = -303.78
      Oven2.RackLoc(2).U = 183.58
      Oven2.RackLoc(2).InsertX = -0.5865
      Oven2.RackLoc(2).InsertY = 0.8099
      'Bottom Right Filter
      Oven2.RackLoc(3).X = -384.62
      Oven2.RackLoc(3).Y = 665.9
      Oven2.RackLoc(3).Z = -305.48
      Oven2.RackLoc(3).U = 183.478
      Oven2.RackLoc(3).InsertX = -0.58
      Oven2.RackLoc(3).InsertY = 0.8146

      'Oven 3, Top Left Filter
      Oven3.RackLoc(0).X = -149.32
      Oven3.RackLoc(0).Y = 764.35
      Oven3.RackLoc(0).Z = -75.05
      Oven3.RackLoc(0).U = 149.744
      Oven3.RackLoc(0).InsertX = -0.03
      Oven3.RackLoc(0).InsertY = 0.9995
      'Top Right Filter
      Oven3.RackLoc(1).X = 187.75
      Oven3.RackLoc(1).Y = 771.53
      Oven3.RackLoc(1).Z = -74.84
      Oven3.RackLoc(1).U = 149.74
      Oven3.RackLoc(1).InsertX = -0.012
      Oven3.RackLoc(1).InsertY = 0.9999
      'Bottom Left Filter
      Oven3.RackLoc(2).X = -149.61
      Oven3.RackLoc(2).Y = 765.13
      Oven3.RackLoc(2).Z = -301.15
      Oven3.RackLoc(2).U = 149.744
      Oven3.RackLoc(2).InsertX = -0.03
      Oven3.RackLoc(2).InsertY = 0.9995
      'Bottom Right Filter
      Oven3.RackLoc(3).X = 187.75
      Oven3.RackLoc(3).Y = 772.54
      Oven3.RackLoc(3).Z = -301.15
      Oven3.RackLoc(3).U = 149.74
      Oven3.RackLoc(3).InsertX = -0.012
      Oven3.RackLoc(3).InsertY = 0.9999

      'Oven 4, Top Left Filter
      Oven4.RackLoc(0).X = 387.23
      Oven4.RackLoc(0).Y = 686.78
      Oven4.RackLoc(0).Z = -73.65
      Oven4.RackLoc(0).U = 107.043
      Oven4.RackLoc(0).InsertX = 0.669
      Oven4.RackLoc(0).InsertY = 0.7433
      'Top Right Filter
      Oven4.RackLoc(1).X = 641.97
      Oven4.RackLoc(1).Y = 465.78
      Oven4.RackLoc(1).Z = -73.71
      Oven4.RackLoc(1).U = 107.04
      Oven4.RackLoc(1).InsertX = 0.6538
      Oven4.RackLoc(1).InsertY = 0.7567
      'Bottom Left Filter
      Oven4.RackLoc(2).X = 390.73
      Oven4.RackLoc(2).Y = 688.48
      Oven4.RackLoc(2).Z = -300.82
      Oven4.RackLoc(2).U = 107.043
      Oven4.RackLoc(2).InsertX = 0.669
      Oven4.RackLoc(2).InsertY = 0.7433
      'Bottom Right Filter
      Oven4.RackLoc(3).X = 644.17
      Oven4.RackLoc(3).Y = 468.68
      Oven4.RackLoc(3).Z = -301.22
      Oven4.RackLoc(3).U = 107.043
      Oven4.RackLoc(3).InsertX = 0.6538
      Oven4.RackLoc(3).InsertY = 0.7567

      'Oven 5, Top Left Filter
      Oven5.RackLoc(0).X = 740.47
      Oven5.RackLoc(0).Y = 316.99
      Oven5.RackLoc(0).Z = -71.94
      Oven5.RackLoc(0).U = 58.58
      Oven5.RackLoc(0).InsertX = 0.9996
      Oven5.RackLoc(0).InsertY = 0.0277
      'Top Right Filter
      Oven5.RackLoc(1).X = 745.57
      Oven5.RackLoc(1).Y = -19.05
      Oven5.RackLoc(1).Z = -73.06
      Oven5.RackLoc(1).U = 58.58
      Oven5.RackLoc(1).InsertX = 0.9999
      Oven5.RackLoc(1).InsertY = 0.0047
      'Bottom Left Filter
      Oven5.RackLoc(2).X = 741.77
      Oven5.RackLoc(2).Y = 316.93
      Oven5.RackLoc(2).Z = -297.79
      Oven5.RackLoc(2).U = 58.58
      Oven5.RackLoc(2).InsertX = 0.9996
      Oven5.RackLoc(2).InsertY = 0.0277
      'Bottom Right Filter
      Oven5.RackLoc(3).X = 745.36
      Oven5.RackLoc(3).Y = -19.04
      Oven5.RackLoc(3).Z = -300.29
      Oven5.RackLoc(3).U = 58.48
      Oven5.RackLoc(3).InsertX = 0.9999
      Oven5.RackLoc(3).InsertY = 0.0047
   End Sub

   Private Sub Oven1_TransmissionError(ByVal sender As Object, ByVal message As String) Handles Oven1.TransmissionError, _
   Oven2.TransmissionError, Oven3.TransmissionError, Oven4.TransmissionError, Oven5.TransmissionError, Carosel.TransmissionError, _
   BarCode.TransmissionError

      FileIO.WriteTextFile(frmMain.BaseFilePath & "ErrorLog.txt", message, True, True)
      frmMain.ErrorsWereLogged = True
   End Sub

End Module
