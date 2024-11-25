Imports System.Math
Imports System.Xml.Serialization
Imports System.IO
Imports System.Environment


Public Enum RunState
   None           'No run logged
   Active         'Run is in progress
   Done           'Run is complete
   ErrorState     'Run is in Error state
End Enum

Public Enum Tasks
   WeighSamplesTakeToOven
   WeighSamplesTakeToCarosel
   TakeToCarosel
   TakeToOven
   CloseExperiment
   Delay
End Enum

Public Enum Locations
   Oven
   Carosel
End Enum


Public Enum ExpType
   TareExperiment       'Tare a new set of filters
   WeighingExperiment   'Weight a set of filters to determine partical weight
   UserDefined   'This lets them construct anything they want
End Enum


Public Structure ExpSteps
   Dim StepType As Tasks
   Dim StepDuration As TimeSpan
   Dim Temperature As Double
End Structure

Public Structure UserInfo
   Dim FullName As String
   Dim Initials As String
   Dim LISAName As String
End Structure

Public Structure StepData
   Dim StartTime As DateTime  'The time that samples were placed in the oven
   Dim StopTime As DateTime   'the time that samples were taken out of the oven
   Dim Humidity As Double     'The relitive humidity when the samples started to be weighed
   Dim Temperature As Double  'The lab temperature when the samples were started to be weighed
End Structure

Public Structure BlankData
   Dim FilterBlankLocations() As Int32          '0 based.  Fliter locations that are blanks
   Dim LocationOfBlankUsed As Int32    'If the set has more than one blank, then which one was used for weight correction
   Dim BlankAppliesToTheseFilters() As Int32    '0 based.  This set of blanks should be applied to these filters
End Structure


Public Structure FilterInfo
   Dim Barcode As String      'Barcode number on the filter
   Dim Status As RunState
   Dim TareWeight As Double
   Dim Weights() As Double    '0 based array
   Dim SelectedWeight As Double  'The weight the algorythm determines to be the best weight from teh Weights() array
   Dim SampleWeight As Double     'The blank corrected weight of the sample
   Dim FilterBlankPosition As Int32  'If this filter has a filter blank associated with it, the position of the blank is storred here.
End Structure


Public Structure ExpData
   Dim ExperimentType As ExpType
   Dim RunSteps() As ExpSteps    'The sequence of steps that define the experiment
   Dim CurrentRunStep As Int32
   Dim Status As RunState
   Dim CurrentTask As Tasks   'What am I currently doing
   Dim NextAccessTime As DateTime
   Dim MyOperator As UserInfo
   Dim Oven As OvenDef           'Reference to the oven the filter is drying in
   Dim TotalSamples As Int32     'Number of samples in the experiment
   Dim Filter() As FilterInfo    '1 based array
   Dim WeighingStep As Int32     'Logs whether this is the first, second, or third weighing
   Dim DryingTimes() As StepData  'Information about weighing start and stop times, and humidity and temperature
   Dim FilterBlankData() As BlankData  'A single rack can have multiple sets of filters.  Each filter set has blanks that must be weighted.
   Dim MyDGV As DataGridView     'A reference to the DGV that holds this tables data
   Dim Filename As String     'The filename that a weight experiment will be storred under.
   Dim ArmIsInMotion As Boolean  'used only for automatic recovery from a power failure.
   Dim CurrentFilter As Int32    'used only for automatic recovery from a power failure.
End Structure



Public Class frmMain
   Public BaseFilePath As String = "C:\J-KEM Data Files\"
   Public BaseInfoPath As String = GetFolderPath(SpecialFolder.MyDocuments) & "\J-KEM Scientific\Data"
   Public AbortRequested As Boolean
   Dim CommStatus(8) As Boolean
   Public TestMode As Boolean
   Public Experiment(4) As ExpData
   Dim WithEvents MasterTimer As New Timer
   Dim MasterTimerLockout As Boolean  'This flags that you are activly inside the Mastertimer loop
   Dim MasterTimerIsActive As Boolean   'This flags that some function has already stated the Mastertimer.   It is set to false only in the mastertimer loop.
   Public ErrorsWereLogged As Boolean
   Dim ActionTimeLables(4) As Label
   Dim StatusLables(4) As Label
   Dim PauseRequested As Boolean
   Public LastAirADCReading As Int32
   Dim RunStepLockoutFlag As Boolean   'Prevents entry into a run step if the user is manually operating the ovens.
   Dim ManualDoorOpenRequest As Int32  'Holds the over number that the user has issued a manual request to open
   Public OpticalSensor3mmDrop As Int32  'The amount of optical drop ot be expected when the gripper is 3mm over the filter



   Private Sub frmMain_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
      TestMode = False

      CreateDataFileFolders(BaseInfoPath)
      CreateDataFileFolders(BaseFilePath)
      If TestMode = True Then
         If MessageBox.Show("Program is set to Test Mode.   Do you want to continue to run in test mode?", "Test Mode Setting", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) = Windows.Forms.DialogResult.Yes Then
            ModuleDefMessages.InTestMode = True
         Else
            TestMode = False
            ModuleDefMessages.InTestMode = False
         End If
      End If

      MessageBox.Show("Turn power on to the Robot's arm, then click OK to initialize the robot." & vbCrLf & "Verify that all oven doors are closed and that there are no obstructions on the robot deck." _
                      , "Initialize Robot", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
      If SetupDefaults() = True Then
         Carosel.Init()
         Balance.Init()
         lblInit.Visible = False
         If CheckForPowerFailureRecovery() = False Then   'This returns true if the program exited normally.
            'If you just recovered from a power failure, you need to restart the master timer so that the program will service the running experiments
            MasterTimer.Start()
            MasterTimerIsActive = True
         End If
      Else
         Me.Close()
      End If
   End Sub


   Private Sub frmMain_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
      Arm.Close()
      Try
         If My.Computer.FileSystem.FileExists(LogFileName) Then
            MessageBox.Show("Tell Scott that a J-KEM Error Log file was created.", "Log File Creation", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
         End If
      Catch ex As Exception
      End Try
   End Sub


#Region "Start up Tasks"

   Sub LoadSettings()
      Dim Data() As String

      Try
         Data = FileIO.ReadTextFile(BaseInfoPath & "\PortData.txt").Split(Chr(44))
         Oven1Port.PortName = Data(0)
         Oven2Port.PortName = Data(1)
         Oven3Port.PortName = Data(2)
         Oven4Port.PortName = Data(3)
         Oven5Port.PortName = Data(4)
         CaroselPort.PortName = Data(5)
         BarCodePort.PortName = Data(6)
         BalancePort.PortName = Data(7)
         OpticalSensor3mmDrop = CInt(Data(8))
      Catch ex As Exception
         Oven1Port.PortName = "COM7"
         Oven2Port.PortName = "COM8"
         Oven3Port.PortName = "COM9"
         Oven4Port.PortName = "COM14"
         Oven5Port.PortName = "COM15"
         CaroselPort.PortName = "COM6"
         BarCodePort.PortName = "COM16"
         BalancePort.PortName = "COM5"
         OpticalSensor3mmDrop = 69
         SaveSettings()
      End Try
   End Sub


   Public Sub SaveSettings()
      Dim Data As String
      Data = Oven1Port.PortName & ","
      Data += Oven2Port.PortName & ","
      Data += Oven3Port.PortName & ","
      Data += Oven4Port.PortName & ","
      Data += Oven5Port.PortName & ","
      Data += CaroselPort.PortName & ","
      Data += BarCodePort.PortName & ","
      Data += BalancePort.PortName & ","
      Data += OpticalSensor3mmDrop.ToString & ","
      Try
         FileIO.WriteTextFile(BaseInfoPath & "\PortData.txt", Data)
      Catch ex As Exception
      End Try
   End Sub


   Function SetupDefaults() As Boolean
      'Load proceedure default values
      Dim Batch As Int32
      Dim Success As Boolean


      LoadSettings()
      Devices.InitDevices()   'Racalls coordinates and other tasks
      LoadSpecialSettings()   'A new sub that loads any special settings, like oven proximity sensor overrides

      MasterTimer.Interval = 1000
      RunStepLockoutFlag = False
      MasterTimerLockout = False
      MasterTimerIsActive = False
      ErrorsWereLogged = False
      PauseRequested = False
      For Batch = 0 To 4
         Experiment(Batch).Status = RunState.None
         Experiment(Batch).Oven = OvenArray(Batch)
         ReDim Experiment(Batch).Filter(96)
         For Pass As Int32 = 1 To 96
            Experiment(Batch).Filter(Pass).Status = RunState.None
            Experiment(Batch).Filter(Pass).TareWeight = 0.0
            ReDim Experiment(Batch).Filter(Pass).Weights(3)
            For N As Int32 = 0 To 3
               Experiment(Batch).Filter(Pass).Weights(N) = 0.0
            Next
         Next
         Experiment(Batch).Filter(0).Status = RunState.None  'This filter location is not used in the experiments, but it is used as a flag for the custom experiment, so set its state here.
         ReDim Experiment(Batch).RunSteps(15) 'Maximun of 16 steps
         ReDim Experiment(Batch).DryingTimes(3)  'Records a maximum of 4 weighing info

         ReDim Experiment(Batch).FilterBlankData(47)   'Allow up to 48 filter blanks
         For Pass As Int32 = 0 To 47
            ReDim Experiment(Batch).FilterBlankData(Pass).FilterBlankLocations(3) 'You can have up to 4 blanks per set
            ReDim Experiment(Batch).FilterBlankData(Pass).BlankAppliesToTheseFilters(95)
         Next
      Next

      Me.cbxExp1Experiment.SelectedIndex = 0
      Me.cbxExp2Experiment.SelectedIndex = 0
      Me.cbxExp3Experiment.SelectedIndex = 0
      Me.cbxExp4Experiment.SelectedIndex = 0
      Me.cbxExp5Experiment.SelectedIndex = 0

      LoadUserComboBoxes()
      Me.cbxExp1User.SelectedIndex = 0
      Me.cbxExp2User.SelectedIndex = 0
      Me.cbxExp3User.SelectedIndex = 0
      Me.cbxExp4User.SelectedIndex = 0
      Me.cbxExp5User.SelectedIndex = 0

      ActionTimeLables(0) = Me.lblDisplay1Time
      ActionTimeLables(1) = Me.lblDisplay2Time
      ActionTimeLables(2) = Me.lblDisplay3Time
      ActionTimeLables(3) = Me.lblDisplay4Time
      ActionTimeLables(4) = Me.lblDisplay5Time
      StatusLables(0) = Me.lblExp1Status
      StatusLables(1) = Me.lblExp2Status
      StatusLables(2) = Me.lblExp3Status
      StatusLables(3) = Me.lblExp4Status
      StatusLables(4) = Me.lblExp5Status

      Success = True
      If TestMode = False Then
         Success = TestCommunications()
         If Success = False Then
            MessageBox.Show("The robot did not initialize properly.  The application will exit.", "Proceedure Abort", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End If
      End If
      ManualDoorOpenRequest = 0

      Return Success
   End Function


   Sub LoadSpecialSettings()
      'This sub reads the special settings.   Note that the order of reading settings must match that in the sub StoreSpecialSettings()
      Dim Data() As String

      Try
         Data = FileIO.ReadTextFile(BaseInfoPath & "\SpecialSettings.txt").Split(Chr(44))
         If Data(0) = "True" Then
            Oven1.IsCloseDoorSensorInOverride = True
            MessageBox.Show("The close door sensor for oven 1 is disabled.  It is not recommended that you continue this run in this state.  Exit the program and replace the door sensor.", "Replace Door Sensor", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         End If
         If Data(1) = "False" Then
            Oven1.OvenActive = False
            gbxExp1.Enabled = False
          End If
         If Data(2) = "True" Then
            Oven2.IsCloseDoorSensorInOverride = True
            MessageBox.Show("The close door sensor for oven 2 is disabled.  It is not recommended that you continue this run in this state.  Exit the program and replace the door sensor.", "Replace Door Sensor", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         End If
         If Data(3) = "False" Then
            Oven2.OvenActive = False
            gbxExp2.Enabled = False
         End If
         If Data(4) = "True" Then
            Oven3.IsCloseDoorSensorInOverride = True
            MessageBox.Show("The close door sensor for oven 3 is disabled.  It is not recommended that you continue this run in this state.  Exit the program and replace the door sensor.", "Replace Door Sensor", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         End If
         If Data(5) = "False" Then
            Oven3.OvenActive = False
            gbxExp3.Enabled = False
         End If
         If Data(6) = "True" Then
            Oven4.IsCloseDoorSensorInOverride = True
            MessageBox.Show("The close door sensor for oven 4 is disabled.  It is not recommended that you continue this run in this state.  Exit the program and replace the door sensor.", "Replace Door Sensor", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         End If
         If Data(7) = "False" Then
            Oven4.OvenActive = False
            gbxExp4.Enabled = False
         End If
         If Data(8) = "True" Then
            Oven5.IsCloseDoorSensorInOverride = True
            MessageBox.Show("The close door sensor for oven 5 is disabled.  It is not recommended that you continue this run in this state.  Exit the program and replace the door sensor.", "Replace Door Sensor", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         End If
         If Data(9) = "False" Then
            Oven5.OvenActive = False
            gbxExp5.Enabled = False
         End If
      Catch ex As Exception
         If TestMode = False Then
            MessageBox.Show("An error occured in sub LoadSpecialSettings().   Default values will be loaded.", "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
            SaveSpecialSettings()
         End If
      End Try
   End Sub


   Public Sub SaveSpecialSettings()
      'You can store any special setting here that you don't want to save in the settings file.   The settings are read in the sub LoadSpecialSettings()
      Dim Data As String

      Data = Oven1.IsCloseDoorSensorInOverride.ToString & ","
      Data += Oven1.OvenActive.ToString & ","
      Data += Oven2.IsCloseDoorSensorInOverride.ToString & ","
      Data += Oven2.OvenActive.ToString & ","
      Data += Oven3.IsCloseDoorSensorInOverride.ToString & ","
      Data += Oven3.OvenActive.ToString & ","
      Data += Oven4.IsCloseDoorSensorInOverride.ToString & ","
      Data += Oven4.OvenActive.ToString & ","
      Data += Oven5.IsCloseDoorSensorInOverride.ToString & ","
      Data += Oven5.OvenActive.ToString & ","
      FileIO.WriteTextFile(BaseInfoPath & "\SpecialSettings.txt", Data)
   End Sub


   Function TestCommunications() As Boolean
      Dim Element As Int32
      Dim Success As Boolean = True

      AddHandler Me.Paint, AddressOf frmMain_PaintgbxComms
      For Element = 0 To 8
         CommStatus(Element) = False
      Next
      Me.gbxComms.Size = New Size(216, 496)
      Me.gbxComms.Visible = True
      Me.Invalidate()
      DelayMS(500)
      Me.Invalidate()
      Application.DoEvents()

      '
      '=====================================================
      'Note on ovens.   To turn a nonfunctioning oven off, you must go into the SpecialSettings.txt file and set the oven of itnerest
      'to True or False, then resetart the program.
      '

      If Arm.TestComms() = True Then
         CommStatus(8) = True
         Me.Invalidate()
         OvenDef.GotoSafeOpenDoorPosition(0)
         Application.DoEvents()
      End If

      If Oven1.OvenActive = True Then
         If Oven1.Init() = True Then
            CommStatus(0) = True
            Me.Invalidate()
            Application.DoEvents()
         Else
            If MessageBox.Show("Oven 1 did not initialize properly.   Do you want to disable it?", "Disable Oven", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then
               Oven1.OvenActive = False
               gbxExp1.Enabled = False
               SaveSpecialSettings()
               CommStatus(0) = True
            End If
         End If
      Else
         CommStatus(0) = True
         Me.Invalidate()
      End If

      If Oven2.OvenActive = True Then
         If Oven2.Init() = True Then
            CommStatus(1) = True
            Me.Invalidate()
            Application.DoEvents()
         Else
            If MessageBox.Show("Oven 2 did not initialize properly.   Do you want to disable it?", "Disable Oven", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then
               Oven2.OvenActive = False
               gbxExp2.Enabled = False
               SaveSpecialSettings()
               CommStatus(1) = True
            End If
         End If
      Else
         CommStatus(1) = True
         Me.Invalidate()
      End If

      If Oven3.OvenActive = True Then
         If Oven3.Init() = True Then
            CommStatus(2) = True
            Me.Invalidate()
            Application.DoEvents()
         Else
            If MessageBox.Show("Oven 3 did not initialize properly.   Do you want to disable it?", "Disable Oven", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then
               Oven3.OvenActive = False
               gbxExp3.Enabled = False
               SaveSpecialSettings()
               CommStatus(2) = True
            End If
         End If
      Else
         CommStatus(2) = True
         Me.Invalidate()
      End If

      If Oven4.OvenActive = True Then
         If Oven4.Init() = True Then
            CommStatus(3) = True
            Me.Invalidate()
            Application.DoEvents()
         Else
            If MessageBox.Show("Oven 4 did not initialize properly.   Do you want to disable it?", "Disable Oven", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then
               Oven4.OvenActive = False
               gbxExp4.Enabled = False
               SaveSpecialSettings()
               CommStatus(3) = True
            End If
         End If
      Else
         CommStatus(3) = True
         Me.Invalidate()
      End If

      If Oven5.OvenActive = True Then
         If Oven5.Init() = True Then
            CommStatus(4) = True
            Me.Invalidate()
            Application.DoEvents()
         Else
            If MessageBox.Show("Oven 5 did not initialize properly.   Do you want to disable it?", "Disable Oven", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then
               Oven5.OvenActive = False
               gbxExp5.Enabled = False
               SaveSpecialSettings()
               CommStatus(4) = True
            End If
         End If
      Else
         CommStatus(4) = True
         Me.Invalidate()
      End If
      
      If Carosel.TestComms() = True Then
         Carosel.Home()
         CommStatus(5) = True
         Me.Invalidate()
         Application.DoEvents()
      End If

      If Balance.TestComms() = True Then
         CommStatus(6) = True
         Me.Invalidate()
         Application.DoEvents()
      End If

      If BarCode.Initialize() = True Then
         CommStatus(7) = True
         Me.Invalidate()
         Application.DoEvents()
      End If

      DelaySeconds(2)
      For Element = 0 To 8
         If CommStatus(Element) = False Then
            If MessageBox.Show("An error occured while trying to initialize one of the workstation devices.   Do you want to continue to Start the application?", "Communication Error", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.No Then
               Success = False
               Exit For
            End If
         End If
      Next

      'Now confirm that all the doors are closed.
      If Success = True Then
         For Element = 0 To 4
            If OvenArray(Element).OvenActive = True Then
               If Not OvenArray(Element).DoorState = OvenDef.DoorLoc.Closed Then   'This property is set in Init
                  Dim State As Boolean

                  State = My.Settings.NormalProgramExit  'If you are recovering from a power failure, this will have a state of False
                  If MessageBox.Show("The program can not confirm that the door on Oven " & (Element + 1).ToString & " is closed.   Is it safe to close the door now?", "Oven Door Position Unknown", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) = Windows.Forms.DialogResult.Yes Then
                     If OvenArray(Element).CloseDoor() = False Then
                        Success = False
                     End If
                  Else
                     If State = False Then
                        MessageBox.Show("The system indicates that it is recovering from a power failure.   Turn power to the Epson arm off and manually reposition the arm to a location where it is safe to close the over door." & _
                                        "   After the arm is in a safe location, turn power to the Epson controller back on, and restart the program.", "Manually Position Epson Arm", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                     End If
                     Success = False
                  End If
               End If
            End If
         Next
      End If

      Me.gbxComms.Visible = False
      RemoveHandler Me.Paint, AddressOf frmMain_PaintgbxComms
      'AddHandler Me.Paint, AddressOf frmMain_PaintForm

      Return Success
   End Function


   Private Sub frmMain_PaintgbxComms(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs)
      'This is only operational when the communications combo box is visible
      Dim MyGraphics As System.Drawing.Graphics
      Dim BrushColor As New SolidBrush(Color.Red)
      Dim Pass As Int32


      MyGraphics = Me.gbxComms.CreateGraphics
      For Pass = 0 To 8
         If CommStatus(Pass) = True Then
            BrushColor.Color = Color.LimeGreen
         Else
            BrushColor.Color = Color.Red
         End If
         MyGraphics.FillEllipse(BrushColor, 155, 44 + (Pass * 50), 26, 26)
      Next
   End Sub

   Sub LoadUserComboBoxes()
      Dim ModList() As String
      Dim UserNames() As String

      Try
         UserNames = FileIO.ReadTextFile(BaseInfoPath & "\UserNames.txt").Split(Chr(10)) 'split on lf
         ReDim ModList(UserNames.Length - 1)
         ModList(0) = "None"
         For Pass As Int32 = 0 To UserNames.Length - 2
            ModList(Pass + 1) = UserNames(Pass).Substring(0, UserNames(Pass).Length - 1)
         Next

         Me.cbxExp1User.Items.Clear()
         Me.cbxExp2User.Items.Clear()
         Me.cbxExp3User.Items.Clear()
         Me.cbxExp4User.Items.Clear()
         Me.cbxExp5User.Items.Clear()
         Me.cbxExp1User.Items.AddRange(ModList)
         Me.cbxExp2User.Items.AddRange(ModList)
         Me.cbxExp3User.Items.AddRange(ModList)
         Me.cbxExp4User.Items.AddRange(ModList)
         Me.cbxExp5User.Items.AddRange(ModList)
      Catch ex As Exception
      End Try
   End Sub

#End Region



   Private Sub MasterTimer_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles MasterTimer.Tick
      Dim Pass As Int32
      Dim RestartTimer As Boolean
      Dim TimeLeft As Double
      Static IToldThemFlag As Boolean = False


      MasterTimerLockout = True
      Try
         MasterTimer.Stop()
         RestartTimer = False
         If ManualDoorOpenRequest > 0 Then   'This checks to see of the user has asked to open an oven door manually.
            ServiceManualDoorOpenRequest()
         End If
         mnuCarousel.Enabled = True
         For Pass = 0 To 4
            If Experiment(Pass).Status = RunState.Active Then
               TimeLeft = Experiment(Pass).NextAccessTime.Subtract(Now).TotalSeconds
               If TimeLeft <= 0 Then
                  If RunStepLockoutFlag = False Then
                     mnuCarousel.Enabled = False
                     mnuModules.Enabled = False
                     IToldThemFlag = False
                     Experiment(Pass).CurrentRunStep += 1
                     Experiment(Pass).CurrentTask = Experiment(Pass).RunSteps(Experiment(Pass).CurrentRunStep).StepType

                     If Experiment(Pass).CurrentTask = Tasks.WeighSamplesTakeToOven Then
                        Me.StatusLables(Pass).Text = "Weighing Samples"
                        WeighFiltersFromCarosel(Pass, Locations.Oven, 1)
                        Me.StatusLables(Pass).Text = "Drying"
                     ElseIf Experiment(Pass).CurrentTask = Tasks.WeighSamplesTakeToCarosel Then
                        Me.StatusLables(Pass).Text = "Weighing Samples"
                        WeighFiltersFromCarosel(Pass, Locations.Carosel, 1)
                        Me.StatusLables(Pass).Text = "On Carousel"
                     ElseIf Experiment(Pass).CurrentTask = Tasks.TakeToCarosel Then
                        Me.StatusLables(Pass).Text = "Loading Carousel"
                        GetFilterFromOven(Pass, 1)
                        Me.StatusLables(Pass).Text = "On Carousel"
                     ElseIf Experiment(Pass).CurrentTask = Tasks.TakeToOven Then
                        Me.StatusLables(Pass).Text = "Loading Oven"
                        TakeFilterToOven(Pass, True, 1)
                        Me.StatusLables(Pass).Text = "Drying"
                     ElseIf Experiment(Pass).CurrentTask = Tasks.CloseExperiment Then
                        Experiment(Pass).Status = RunState.Done
                        Me.StatusLables(Pass).Text = "Done"
                        CloseExperiment(Pass)
                        ProcessExperimentData(Pass)
                     ElseIf Experiment(Pass).CurrentTask = Tasks.Delay Then
                        Me.StatusLables(Pass).Text = "Delay"
                        'Do nothing.  Time is loaded below
                     End If
                     Me.mnuModules.Enabled = True
                  Else
                     If IToldThemFlag = False Then
                        MessageBox.Show("The program is attemping to perform a run step, but must delay this step until the Ovens or Carousel Manual Control screens are closed.", "Run Step Delayed", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
                        IToldThemFlag = True
                     End If
                  End If
               ElseIf TimeLeft < 180 Then
                  mnuCarousel.Enabled = False  'Dont allow the carousel to move when the robot is within 3 minutes of accessing it
               End If

               RestartTimer = True
               UpdateDisplayTimers()
            End If
            Application.DoEvents()
         Next

         If RestartTimer = True Then
            MasterTimer.Start()
         Else
            Me.lblMasterTimer.Visible = False
            My.Settings.NormalProgramExit = True
            My.Settings.Save()
            MasterTimerIsActive = False  'This must be the only place where this is set to false
         End If
         MasterTimerLockout = False
      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name, Pass.ToString)
         frmSMTPService.SendMail("An Error occured with the weighing robot process and the robot has stopped.  The error message was:  " & ex.Message, False, True)
         MessageBox.Show("An exception was caught in function MasterTimerTick()." & vbCrLf & "The exception was: " & ex.Message _
                         & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                         & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                         "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub


   Sub TakeFilterToOven(ByVal batch As Int32, ByVal aquireBarcode As Boolean, ByVal startFilter As Int32)
      'This sub takes the filters from the carosel to the oven.   If the barcode field
      'is empty, the sub reads the barcode label and looks up the tare weight.
      Dim Pass As Int32
      Dim Lable As String
      Dim Skipit As Boolean


      Try
         OvenDef.GotoSafeOpenDoorPosition(batch)
         UpdateDisplayTimers()
         Experiment(batch).Oven.OpenDoor()
         UpdateDisplayTimers()
         Carosel.Home()
         UpdateDisplayTimers()
         DelayMS(1000)
         Carosel.Home()

         Experiment(batch).ArmIsInMotion = True    'Used for power failure recovery
         For Pass = startFilter To Experiment(batch).TotalSamples
            If Experiment(batch).Filter(Pass).Status = RunState.Active Then
               Experiment(batch).CurrentFilter = Pass
               Me.SaveExperimentData(batch)
               If Carosel.GetFilter(batch + 1, Pass) Then
                  UpdateDisplayTimers()
                  Skipit = False
                  If aquireBarcode = True Then
                     BarCode.GoToReader()
                     Lable = BarCode.Read
                     If ManageBarcodeData(batch, Pass, Lable) = False Then
                        Carosel.PutFilter(batch + 1, Pass)
                        Experiment(batch).Filter(Pass).Status = RunState.ErrorState
                        Experiment(batch).MyDGV.Rows(Pass - 1).Cells(6).Value = "Error"
                        DGVSupport.SetCellColor(Experiment(batch).MyDGV, Pass - 1, 6, Color.Red)
                        Skipit = True
                     End If
                     UpdateDisplayTimers()
                  End If

                  If Skipit = False Then
                     OvenDef.PutFilter(batch + 1, Pass)
                     UpdateDisplayTimers()
                  End If
               Else  'Filter was not located by the gripper
                  Experiment(batch).Filter(Pass).Status = RunState.ErrorState
                  Experiment(batch).MyDGV.Rows(Pass - 1).Cells(6).Value = "Error"
                  DGVSupport.SetCellColor(Experiment(batch).MyDGV, Pass - 1, 6, Color.Red)
               End If
            End If
         Next

         UpdateDisplayTimers()
         OvenDef.GotoSafeOpenDoorPosition(batch)
         UpdateDisplayTimers()
         Experiment(batch).Oven.CloseDoorSetVacuum(350)
         UpdateDisplayTimers()
         Experiment(batch).DryingTimes(Experiment(batch).WeighingStep).StartTime = Now
         Dim Temperature As Double
         Temperature = Experiment(batch).RunSteps(Experiment(batch).CurrentRunStep).Temperature
         Experiment(batch).Oven.SetTemperature(Temperature)

         If Experiment(batch).ExperimentType = ExpType.WeighingExperiment Then
            LookupTareWeights(batch)
         End If

         'Now load the next time this experiment needs to be serviced.
         Experiment(batch).NextAccessTime = Now.Add(Experiment(batch).RunSteps(Experiment(batch).CurrentRunStep).StepDuration)
         Experiment(batch).ArmIsInMotion = False
         Me.SaveExperimentData(batch)
         Carosel.Move(1000)   'Position the carousel close to its home position so that it doesn't take forever to home
         Carosel.ReleaseCaroselFromAccess()
      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name, batch.ToString & vbCrLf & startFilter.ToString)
         frmSMTPService.SendMail("An Error occured in the function TakeFilterToOven.  Batch: " & batch.ToString & ", Filter: " & startFilter.ToString & ".  The error message was:  " & ex.Message, False, True)
         MessageBox.Show("An exception was caught in function TakeFilterToOven()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub


   Sub WeighFiltersFromCarosel(ByVal batch As Int32, ByVal endingLocation As Locations, ByVal startFilter As Int32)
      'This sub is called anytime filters are accessed from the carosel and a weight is obtained.
      'Batch is the experiment number, 0-4.
      'EndingLocation is where the filter should be placed.  In a tare experiment, the ending location will be the carosel,
      'in a transfer step, it will be the oven.
      Dim Pass As Int32
      Dim Lable As String
      Dim FirstWeighing As Double
      Dim SecondWeighing As Double
      Dim FilterDone As Boolean
      Dim ExperimentDone As Boolean


      Try
         If endingLocation = Locations.Oven Then
            OvenDef.GotoSafeOpenDoorPosition(batch)
            Experiment(batch).Oven.OpenDoor()
            UpdateDisplayTimers()
         End If
         Carosel.Home()
         UpdateDisplayTimers()
         DelayMS(1000)
         Carosel.Home()
         Experiment(batch).DryingTimes(Experiment(batch).WeighingStep).Humidity = -20.0  'bob, not implemented
         Experiment(batch).DryingTimes(Experiment(batch).WeighingStep).Temperature = -20.0
         Balance.Zero()
         UpdateDisplayTimers()

         Experiment(batch).ArmIsInMotion = True 'Only used for power failure recovery function
         For Pass = startFilter To Experiment(batch).TotalSamples
            Experiment(batch).CurrentFilter = Pass
            Me.SaveExperimentData(batch)
            If Experiment(batch).Filter(Pass).Status = RunState.Active Then
               If Carosel.GetFilter(batch + 1, Pass) Then
                  UpdateDisplayTimers()
                  BarCode.GoToReader()
                  Lable = BarCode.Read
                  UpdateDisplayTimers()
                  'The first time that tare weights are looked up is in the TakeFilterToOven() function, but if the barcode
                  'did not read, then a tareweight was not loaded.  In that event, load it now
                  If Experiment(batch).ExperimentType = ExpType.WeighingExperiment Then
                     If Experiment(batch).Filter(Pass).Barcode = "NOREAD" Then
                        If Lable <> "NOREAD" Then
                           Experiment(batch).Filter(Pass).Barcode = Lable
                           Experiment(batch).MyDGV.Rows(Pass - 1).Cells(0).Value = Lable
                           LookupTareWeights(batch, Pass)
                        End If
                     End If
                  End If

                  If ManageBarcodeData(batch, Pass, Lable) = True Then
                     'Weigh the filter, store the value
                     If Balance.DropOffFilter() = True Then  'This is true if the filter was dropped off correctly
                        UpdateDisplayTimers()
                        Balance.Weight()  'let it go to waste
                        UpdateDisplayTimers()
                        DelaySeconds(10)
                        UpdateDisplayTimers()
                        Try
                           FirstWeighing = CType(Balance.Weight(), Double) * 1000.0
                           UpdateDisplayTimers()
                           DelaySeconds(10)
                           UpdateDisplayTimers()
                           SecondWeighing = CType(Balance.Weight(), Double) * 1000.0
                           UpdateDisplayTimers()
                           FirstWeighing = (FirstWeighing + SecondWeighing) / 2.0
                           Experiment(batch).Filter(Pass).Weights(Experiment(batch).WeighingStep) = FirstWeighing
                           Experiment(batch).MyDGV.Rows(Pass - 1).Cells(Experiment(batch).WeighingStep + 2).Value = Format(FirstWeighing, "0.000")
                        Catch ex As Exception
                           Experiment(batch).Filter(Pass).Weights(Experiment(batch).WeighingStep + 2) = 0.0
                        End Try
                        Balance.PickUpFilter()
                     Else
                        Experiment(batch).Filter(Pass).Weights(Experiment(batch).WeighingStep) = 0.0
                        Experiment(batch).MyDGV.Rows(Pass - 1).Cells(Experiment(batch).WeighingStep + 2).Value = "0.000"
                     End If

                     'Look at the weight data and see if you have enough good readings to declair the filter done.
                     If Experiment(batch).ExperimentType = ExpType.TareExperiment Then
                        FilterDone = AnalyzeTareWeightData(batch, Pass)
                        If FilterDone = True Then
                           Experiment(batch).Filter(Pass).Status = RunState.Done
                           Experiment(batch).MyDGV.Rows(Pass - 1).Cells(6).Value = "Done"
                        End If
                     ElseIf Experiment(batch).ExperimentType = ExpType.WeighingExperiment Then
                        FilterDone = AnalyzeWeightData(batch, Pass)
                        If FilterDone = True Then
                           Experiment(batch).Filter(Pass).Status = RunState.Done
                           Experiment(batch).MyDGV.Rows(Pass - 1).Cells(6).Value = "Done"
                        End If
                     End If
                  Else
                     Carosel.PutFilter(batch + 1, Pass)
                     Experiment(batch).Filter(Pass).Status = RunState.ErrorState
                     Experiment(batch).MyDGV.Rows(Pass - 1).Cells(6).Value = "Error"
                     DGVSupport.SetCellColor(Experiment(batch).MyDGV, Pass - 1, 6, Color.Red)
                  End If
                  UpdateDisplayTimers()

                  If endingLocation = Locations.Carosel Or FilterDone = True Then
                     Carosel.PutFilter(batch + 1, Pass)
                  ElseIf endingLocation = Locations.Oven Then
                     OvenDef.PutFilter(batch + 1, Pass)
                  End If
                  UpdateDisplayTimers()
               Else  'The filter was not found in the carosel
                  Experiment(batch).Filter(Pass).Status = RunState.ErrorState
                  Experiment(batch).MyDGV.Rows(Pass - 1).Cells(6).Value = "Error"
                  DGVSupport.SetCellColor(Experiment(batch).MyDGV, Pass - 1, 6, Color.Red)
               End If
            End If
         Next

         UpdateDisplayTimers()
         OvenDef.GotoSafeOpenDoorPosition(batch)
         UpdateDisplayTimers()
         'See if all filters in the run have a status of Done
         ExperimentDone = AnalyzeExperimentForDone(batch)  'Returns true if all filters are done
         If ExperimentDone = True Then
            'The Timer function will close the experiment and set the status to done.
            Experiment(batch).RunSteps(Experiment(batch).CurrentRunStep + 1).StepType = Tasks.CloseExperiment
            Experiment(batch).RunSteps(Experiment(batch).CurrentRunStep + 1).StepDuration = TimeSpan.Zero
            Experiment(batch).RunSteps(Experiment(batch).CurrentRunStep).StepDuration = TimeSpan.Zero
         End If

         If endingLocation = Locations.Oven Then
            If ExperimentDone = False Then
               Dim Temperature As Double

               Experiment(batch).Oven.CloseDoorSetVacuum(350)
               Experiment(batch).DryingTimes(Experiment(batch).WeighingStep + 1).StartTime = Now
               Temperature = Experiment(batch).RunSteps(Experiment(batch).CurrentRunStep).Temperature
               Experiment(batch).Oven.SetTemperature(Temperature)
            Else
               Experiment(batch).Oven.CloseDoor()
            End If
         End If
         UpdateDisplayTimers()

         'Now load the next time this experiment needs to be serviced.
         Experiment(batch).NextAccessTime = Now.Add(Experiment(batch).RunSteps(Experiment(batch).CurrentRunStep).StepDuration)
         Experiment(batch).ArmIsInMotion = False
         Experiment(batch).WeighingStep += 1
         SaveExperimentData(batch)
         Carosel.Move(1000)   'Position the carousel close to its home position so that it doesn't take forever to home
         Carosel.ReleaseCaroselFromAccess()
      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name, batch.ToString & vbCrLf & startFilter.ToString & vbCrLf & endingLocation.ToString)
         frmSMTPService.SendMail("An Error occured in the function WeighFiltersFromCarosel.  Batch: " & batch.ToString & ", Filter: " & startFilter.ToString & ".  The error message was:  " & ex.Message, False, True)
         MessageBox.Show("An exception was caught in function WeighFiltersFromCarosel()." & vbCrLf & "The exception was: " & ex.Message _
                         & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                         & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                         "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub


   Sub GetFilterFromOven(ByVal batch As Int32, ByVal startFilter As Int32)
      'When a filter is taken from the oven, the only location for it to go to is directly to the carosel.

      Try
         OvenDef.GotoSafeOpenDoorPosition(batch)
         Experiment(batch).Oven.OpenDoor()
         Experiment(batch).DryingTimes(Experiment(batch).WeighingStep).StopTime = Now
         UpdateDisplayTimers()
         Carosel.Home()
         UpdateDisplayTimers()
         DelayMS(1000)
         Carosel.Home()

         Experiment(batch).ArmIsInMotion = True 'Used for power failure recovery
         For Pass As Int32 = startFilter To Experiment(batch).TotalSamples
            Experiment(batch).CurrentFilter = Pass
            Me.SaveExperimentData(batch)
            If Experiment(batch).Filter(Pass).Status = RunState.Active Then
               If OvenDef.GetFilter(batch + 1, Pass) = True Then
                  UpdateDisplayTimers()
                  Carosel.PutFilter(batch + 1, Pass)
                  UpdateDisplayTimers()
               End If
            End If
         Next

         UpdateDisplayTimers()
         OvenDef.GotoSafeOpenDoorPosition(batch)
         UpdateDisplayTimers()
         Experiment(batch).Oven.CloseDoor()

         'Now load the next time this experiment needs to be serviced.
         Experiment(batch).NextAccessTime = Now.Add(Experiment(batch).RunSteps(Experiment(batch).CurrentRunStep).StepDuration)
         Experiment(batch).ArmIsInMotion = False
         Me.SaveExperimentData(batch)
         Carosel.Move(1000)   'Position the carousel close to its home position so that it doesn't take forever to home
         Carosel.ReleaseCaroselFromAccess()
      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name, batch.ToString & vbCrLf & startFilter.ToString)
         frmSMTPService.SendMail("An Error occured in the function GetFilterFromOven.  Batch: " & batch.ToString & ", Filter: " & startFilter.ToString & ".  The error message was:  " & ex.Message, False, True)
         MessageBox.Show("An exception was caught in function GetFilterFromOven()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub



   Sub UpdateDisplayTimers()
      'Updates the times displayed for Next Action Time on the user interface
      Dim pass As Int32
      Dim ShortestTime As TimeSpan
      Dim Difference As TimeSpan
      Dim ShortStep As Int32

      Try
         ShortestTime = TimeSpan.MaxValue
         For pass = 0 To 4
            If Experiment(pass).Status = RunState.Active Then
               If DateTime.Compare(Experiment(pass).NextAccessTime, Now) > 0 Then
                  Difference = Experiment(pass).NextAccessTime.Subtract(Now)
                  ActionTimeLables(pass).Text = "Next Action Time: " & Experiment(pass).NextAccessTime.DayOfWeek.ToString & ", " & _
                  Experiment(pass).NextAccessTime.ToLongTimeString & "   [Remaining: " & FormatTime(Difference, False, True, True, True) & "]"
                  If Difference < ShortestTime Then
                     ShortestTime = Difference
                     ShortStep = pass + 1
                  End If
               Else
                  ActionTimeLables(pass).Text = "Next Action Time: " & Experiment(pass).NextAccessTime.ToLongTimeString & "   [Remaining: 0:00:00]"
               End If
            End If
         Next

         If ShortestTime.TotalSeconds > 0 And ShortestTime <> TimeSpan.MaxValue Then
            Me.lblMasterTimer.Text = "Next Action: " & FormatTime(ShortestTime, False, True, True, True) & " for Group " & ShortStep.ToString
         Else
            Me.lblMasterTimer.Text = "Next Action: 0:00:00"
         End If

         If PauseRequested = True Then
            MessageBox.Show("System is Paused.   Click OK to resume.", "System Paused", MessageBoxButtons.OK, MessageBoxIcon.Hand)
            PauseRequested = False
         End If
      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name)
         frmSMTPService.SendMail("An Error occured in the function UpdateDisplayTimers.  The error message was:  " & ex.Message, False, True)
         MessageBox.Show("An exception was caught in function UpdateDisplayTimers()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub


   Function AnalyzeExperimentForDone(ByVal batch As Int32) As Boolean
      'If all filter status' are done or in error, the experiment is set to done
      Dim Pass As Int32
      Dim Done As Boolean = True

      Try
         For Pass = 1 To Experiment(batch).TotalSamples
            If Experiment(batch).Filter(Pass).Status = RunState.Active Then
               Done = False
               Exit For
            End If
         Next
         Return Done
      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name, batch.ToString)
         frmSMTPService.SendMail("An Error occured in the function AnalyzeExperimentForDone.  The error message was:  " & ex.Message, False, True)
         MessageBox.Show("An exception was caught in function AnalyzeExperimentForDone()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Function


   Sub CloseExperiment(ByVal batch As Int32)
      'General Cleanup function.  This function is called just as an experiment is completing.  Put anything in here that
      'that you want to run when an experiment completes.
      Dim AllDone As Boolean

      Try
         Select Case batch
            Case 0
               Me.mnuExp1Abort.Enabled = False
               Me.mnuManualOven1.Enabled = False
               My.Settings.Experiment1IsActive = False
               Try
                  frmSMTPService.SendMail("The filter experiment #1 is complete.", True, False)
               Catch ex As Exception
               End Try

            Case 1
               Me.mnuExp2Abort.Enabled = False
               Me.mnuManualOven2.Enabled = False
               My.Settings.Experiment2IsActive = False
               Try
                  frmSMTPService.SendMail("The filter experiment #2 is complete.", True, False)
               Catch ex As Exception
               End Try
            Case 2
               Me.mnuExp3Abort.Enabled = False
               Me.mnuManualOven3.Enabled = False
               My.Settings.Experiment3IsActive = False
               Try
                  frmSMTPService.SendMail("The filter experiment #3 is complete.", True, False)
               Catch ex As Exception
               End Try
            Case 3
               Me.mnuExp4Abort.Enabled = False
               Me.mnuManualOven4.Enabled = False
               My.Settings.Experiment4IsActive = False
               Try
                  frmSMTPService.SendMail("The filter experiment #4 is complete.", True, False)
               Catch ex As Exception
               End Try
            Case 4
               Me.mnuExp5Abort.Enabled = False
               Me.mnuManualOven1.Enabled = False
               My.Settings.Experiment5IsActive = False
               Try
                  frmSMTPService.SendMail("The filter experiment #5 is complete.", True, False)
               Catch ex As Exception
               End Try
         End Select

         If Me.Experiment(batch).ExperimentType = ExpType.UserDefined Then
            Experiment(batch).Status = RunState.Done
         End If

         AllDone = True
         For pass As Int32 = 0 To 4
            If Experiment(pass).Status = RunState.Active Then
               AllDone = False
            End If
         Next
         If AllDone = True Then
            Me.mnuModules.Enabled = True
            My.Settings.NormalProgramExit = True
         End If

         My.Settings.Save()
      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name, batch.ToString)
         MessageBox.Show("An exception was caught in function CloseExpeiment()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub


   Sub ProcessExperimentData(ByVal batch As Int32)
      'This sub closes and processes an expeiment.  It writes data to the appropriate files on the C drive
      Dim Data As String = ""
      Dim Pass As Int32

      Try
         If Experiment(batch).ExperimentType = ExpType.TareExperiment Then
            For Pass = 1 To Experiment(batch).TotalSamples
               If Experiment(batch).Filter(Pass).Status = RunState.Done Then
                  Data += vbCrLf & Experiment(batch).Filter(Pass).Barcode
                  If Experiment(batch).Filter(Pass).Weights(0) > 0.0 Then
                     Data += "," & Format(Experiment(batch).Filter(Pass).Weights(0), "0.000")
                  End If
                  If Experiment(batch).Filter(Pass).Weights(1) > 0.0 Then
                     Data += "," & Format(Experiment(batch).Filter(Pass).Weights(1), "0.000")
                  End If
                  If Experiment(batch).Filter(Pass).Weights(2) > 0.0 Then
                     Data += "," & Format(Experiment(batch).Filter(Pass).Weights(2), "0.000")
                  End If
                  If Experiment(batch).Filter(Pass).Weights(3) > 0.0 Then
                     Data += "," & Format(Experiment(batch).Filter(Pass).Weights(3), "0.000")
                  End If
               End If
            Next
            FileIO.WriteTextFile(BaseFilePath & "TareWeights.csv", Data, True, True)

            'Now write a printable copy of the data
            Data = "File Name:  " & Experiment(batch).Filename & "        Tare Experiment data" & vbCrLf
            Data += "Number of samples:  " & Experiment(batch).TotalSamples.ToString & vbCrLf
            Data += "Analyst:  " & Experiment(batch).MyOperator.FullName & vbCrLf
            Data += "LISA Username:  " & Experiment(batch).MyOperator.LISAName & vbCrLf
            Data += "Step 1 Oven Drying Times -     Start: " & Experiment(batch).DryingTimes(0).StartTime.ToShortDateString & "   " & Experiment(batch).DryingTimes(0).StartTime.ToLongTimeString & "      End: " & Experiment(batch).DryingTimes(0).StopTime.ToShortDateString & "   " & Experiment(batch).DryingTimes(0).StopTime.ToLongTimeString & vbCrLf
            Data += "Step 2 Oven Drying Times -     Start: " & Experiment(batch).DryingTimes(1).StartTime.ToShortDateString & "   " & Experiment(batch).DryingTimes(1).StartTime.ToLongTimeString & "      End: " & Experiment(batch).DryingTimes(1).StopTime.ToShortTimeString & "   " & Experiment(batch).DryingTimes(1).StopTime.ToLongTimeString & vbCrLf
            If Experiment(batch).WeighingStep > 2 Then
               Data += "Step 3 Oven Drying Times -     Start: " & Experiment(batch).DryingTimes(2).StartTime.ToShortDateString & "   " & Experiment(batch).DryingTimes(2).StartTime.ToLongTimeString & "      End: " & Experiment(batch).DryingTimes(2).StopTime.ToShortDateString & "   " & Experiment(batch).DryingTimes(2).StopTime.ToLongTimeString & vbCrLf
            End If
            ' When you implement humidity and temperature, then the output lines must look like this:   Data += "Step 1 Weighing Environment -     Humidity: " & Experiment(batch).DryingTimes(0).Humidity.ToString & "      Temperature: " & Experiment(batch).DryingTimes(0).Temperature.ToString & vbCrLf
            Data += "Step 1 Weighing Environment -     Humidity: - - -      Temperature: - - -" & vbCrLf
            Data += "Step 2 Weighing Environment -     Humidity: - - -      Temperature: - - -" & vbCrLf
            If Experiment(batch).WeighingStep > 2 Then
               Data += "Step 3 Weighing Environment -     Humidity: - - -      Temperature: - - -" & vbCrLf
            End If
            If Experiment(batch).WeighingStep > 3 Then
               Data += "Step 4 Weighing Environment -     Humidity: - - -      Temperature: - - -" & vbCrLf
            End If
            Data += vbCrLf & "Filter" & "," & "Bar Code #" & "," & _
            "Weight 1 (mg)" & "," & "Weight 2 (mg)" & "," & "Weight 3 (mg)" & "," & "Weight 4 (mg)" & vbCrLf
            For Pass = 1 To Experiment(batch).TotalSamples
               Data += Pass.ToString & "," & Experiment(batch).Filter(Pass).Barcode & ","

               If Experiment(batch).Filter(Pass).Weights(0) > 0.0 Then
                  Data += Format(Experiment(batch).Filter(Pass).Weights(0), "0.000") & ","
               Else
                  Data += "- - -,"
               End If
               If Experiment(batch).Filter(Pass).Weights(1) > 0.0 Then
                  Data += Format(Experiment(batch).Filter(Pass).Weights(1), "0.000") & ","
               Else
                  Data += "- - -,"
               End If
               If Experiment(batch).Filter(Pass).Weights(2) > 0.0 Then
                  Data += Format(Experiment(batch).Filter(Pass).Weights(2), "0.000") & ","
               Else
                  Data += "- - -,"
               End If
               If Experiment(batch).Filter(Pass).Weights(3) > 0.0 Then
                  If Experiment(batch).Filter(Pass).Status = RunState.ErrorState Then
                     Data += Format(Experiment(batch).Filter(Pass).Weights(3), "0.000") & ",Error" & vbCrLf
                  Else
                     Data += Format(Experiment(batch).Filter(Pass).Weights(3), "0.000") & vbCrLf
                  End If
               Else
                  If Experiment(batch).Filter(Pass).Status = RunState.ErrorState Then
                     Data += "- - -,Error" & vbCrLf
                  Else
                     Data += "- - -" & vbCrLf
                  End If

               End If
            Next
            FileIO.WriteTextFile(BaseFilePath & Experiment(batch).Filename, Data)

         ElseIf Experiment(batch).ExperimentType = ExpType.WeighingExperiment Then
            ProcessFilterBlank(batch)  'Select the filter blank used for each filter group

            Data = "File Name:  " & Experiment(batch).Filename & vbCrLf
            Data += "Number of samples:  " & Experiment(batch).TotalSamples.ToString & vbCrLf
            Data += "Analyst:  " & Experiment(batch).MyOperator.FullName & vbCrLf
            Data += "LISA Username:  " & Experiment(batch).MyOperator.LISAName & vbCrLf
            Data += "Step 1 Oven Drying Times -     Start: " & Experiment(batch).DryingTimes(0).StartTime.ToShortDateString & "   " & Experiment(batch).DryingTimes(0).StartTime.ToLongTimeString & "      End: " & Experiment(batch).DryingTimes(0).StopTime.ToShortDateString & "   " & Experiment(batch).DryingTimes(0).StopTime.ToLongTimeString & vbCrLf
            Data += "Step 2 Oven Drying Times -     Start: " & Experiment(batch).DryingTimes(1).StartTime.ToShortDateString & "   " & Experiment(batch).DryingTimes(1).StartTime.ToLongTimeString & "      End: " & Experiment(batch).DryingTimes(1).StopTime.ToShortTimeString & "   " & Experiment(batch).DryingTimes(1).StopTime.ToLongTimeString & vbCrLf
            If Experiment(batch).WeighingStep > 2 Then
               Data += "Step 3 Oven Drying Times -     Start: " & Experiment(batch).DryingTimes(2).StartTime.ToShortDateString & "   " & Experiment(batch).DryingTimes(2).StartTime.ToLongTimeString & "      End: " & Experiment(batch).DryingTimes(2).StopTime.ToShortDateString & "   " & Experiment(batch).DryingTimes(2).StopTime.ToLongTimeString & vbCrLf
            End If
            ' When you implement humidity and temperature, then the output lines must look like this:   Data += "Step 1 Weighing Environment -     Humidity: " & Experiment(batch).DryingTimes(0).Humidity.ToString & "      Temperature: " & Experiment(batch).DryingTimes(0).Temperature.ToString & vbCrLf
            Data += "Step 1 Weighing Environment -     Humidity: - - -      Temperature: - - -" & vbCrLf
            Data += "Step 2 Weighing Environment -     Humidity: - - -      Temperature: - - -" & vbCrLf
            If Experiment(batch).WeighingStep > 2 Then
               Data += "Step 3 Weighing Environment -     Humidity: - - -      Temperature: - - -" & vbCrLf
            End If
            Data += vbCrLf & "Filter" & "," & "Bar Code #" & "," & "Blank?" & "," & "Blank Corrected From" & "," & "Tare Weight (mg)" & _
               "," & "Weight 1 (mg)" & "," & "Weight 2 (mg)" & "," & "Weight 3 (mg)" & "," & "Selected Weight (mg)" & _
               "," & "Sample Weight (mg)" & vbCrLf
            For Pass = 1 To Experiment(batch).TotalSamples
               Data += Pass.ToString & "," & Experiment(batch).Filter(Pass).Barcode & ","
               If Experiment(batch).Filter(Pass).Status = RunState.Done Then
                  Data += IsBlankFilter(batch, Pass) 'This fills in both the blank? and Blank corrected columns
                  Data += Format(Experiment(batch).Filter(Pass).TareWeight, "0.000") & ","
                  If Experiment(batch).Filter(Pass).Weights(0) > 0.0 Then
                     Data += Format(Experiment(batch).Filter(Pass).Weights(0), "0.000") & ","
                  Else
                     Data += "- - -,"
                  End If
                  If Experiment(batch).Filter(Pass).Weights(1) > 0.0 Then
                     Data += Format(Experiment(batch).Filter(Pass).Weights(1), "0.000") & ","
                  Else
                     Data += "- - -,"
                  End If
                  If Experiment(batch).Filter(Pass).Weights(2) > 0.0 Then
                     Data += Format(Experiment(batch).Filter(Pass).Weights(2), "0.000") & ","
                  Else
                     Data += "- - -,"
                  End If
                  If Experiment(batch).Filter(Pass).SelectedWeight > 0.0 Then
                     Data += Format(Experiment(batch).Filter(Pass).SelectedWeight, "0.000") & ","
                  Else
                     Data += "- - -,"
                  End If
                  Data += Format(Experiment(batch).Filter(Pass).SampleWeight, "0.000") & vbCrLf

               Else  'Error state
                  Data += ",,Error" & vbCrLf
               End If
            Next
            FileIO.WriteTextFile(BaseFilePath & Experiment(batch).Filename, Data)

         ElseIf Experiment(batch).ExperimentType = ExpType.UserDefined Then
            Data = "File Name:  " & Experiment(batch).Filename & vbCrLf
            Data += "Number of samples:  " & Experiment(batch).TotalSamples.ToString & vbCrLf
            Data += "Analyst:  " & Experiment(batch).MyOperator.FullName & vbCrLf
            Data += "LISA Username:  " & Experiment(batch).MyOperator.LISAName & vbCrLf
            Data += "Step 1 Oven Drying Times -     Start: " & Experiment(batch).DryingTimes(0).StartTime.ToShortDateString & "   " & Experiment(batch).DryingTimes(0).StartTime.ToLongTimeString & "      End: " & Experiment(batch).DryingTimes(0).StopTime.ToShortDateString & "   " & Experiment(batch).DryingTimes(0).StopTime.ToLongTimeString & vbCrLf
            Data += "Step 2 Oven Drying Times -     Start: " & Experiment(batch).DryingTimes(1).StartTime.ToShortDateString & "   " & Experiment(batch).DryingTimes(1).StartTime.ToLongTimeString & "      End: " & Experiment(batch).DryingTimes(1).StopTime.ToShortTimeString & "   " & Experiment(batch).DryingTimes(1).StopTime.ToLongTimeString & vbCrLf
            If Experiment(batch).WeighingStep > 2 Then
               Data += "Step 3 Oven Drying Times -     Start: " & Experiment(batch).DryingTimes(2).StartTime.ToShortDateString & "   " & Experiment(batch).DryingTimes(2).StartTime.ToLongTimeString & "      End: " & Experiment(batch).DryingTimes(2).StopTime.ToShortDateString & "   " & Experiment(batch).DryingTimes(2).StopTime.ToLongTimeString & vbCrLf
            End If
            ' When you implement humidity and temperature, then the output lines must look like this:   Data += "Step 1 Weighing Environment -     Humidity: " & Experiment(batch).DryingTimes(0).Humidity.ToString & "      Temperature: " & Experiment(batch).DryingTimes(0).Temperature.ToString & vbCrLf
            Data += "Step 1 Weighing Environment -     Humidity: - - -      Temperature: - - -" & vbCrLf
            Data += "Step 2 Weighing Environment -     Humidity: - - -      Temperature: - - -" & vbCrLf
            If Experiment(batch).WeighingStep > 2 Then
               Data += "Step 3 Weighing Environment -     Humidity: - - -      Temperature: - - -" & vbCrLf
            End If
            Data += vbCrLf & "Filter" & "," & "Bar Code #" & "," & "Weight 1 (mg)" & "," & "Weight 2 (mg)" & "," & "Weight 3 (mg)" & vbCrLf
            For Pass = 1 To Experiment(batch).TotalSamples
               Data += Pass.ToString & "," & Experiment(batch).Filter(Pass).Barcode & ","
               If Experiment(batch).Filter(Pass).Weights(0) > 0.0 Then
                  Data += Format(Experiment(batch).Filter(Pass).Weights(0), "0.000") & ","
               Else
                  Data += "- - -,"
               End If
               If Experiment(batch).Filter(Pass).Weights(1) > 0.0 Then
                  Data += Format(Experiment(batch).Filter(Pass).Weights(1), "0.000") & ","
               Else
                  Data += "- - -,"
               End If
               If Experiment(batch).Filter(Pass).Weights(2) > 0.0 Then
                  Data += Format(Experiment(batch).Filter(Pass).Weights(2), "0.000") & vbCrLf
               Else
                  Data += "- - -" & vbCrLf
               End If
            Next

            FileIO.WriteTextFile(BaseFilePath & Experiment(batch).Filename, Data)
         End If

         Select Case batch
            Case 0
               My.Settings.Experiment1IsActive = False
            Case 1
               My.Settings.Experiment2IsActive = False
            Case 2
               My.Settings.Experiment3IsActive = False
            Case 3
               My.Settings.Experiment4IsActive = False
            Case 4
               My.Settings.Experiment5IsActive = False
         End Select
         My.Settings.Save()
      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name, batch.ToString)
         MessageBox.Show("An exception was caught in function ProcessExperimentData()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub


   Function IsBlankFilter(ByVal batch As Int32, ByVal cell As Int32) As String
      'In the output table, you must label whether a filter is a blank of not.
      'Also, if it is not a blank, then what blank was used to correct its sample weight.   This sub prepares those
      'two columns of the table.   The output line will look something like this:
      'Yes <tab> <tab>    or    No <tab> 23
      Dim Data As String = ""
      Dim Pass As Int32
      Dim InnerPass As Int32
      Dim Loc As Int32

      Try
         For Pass = 0 To 47   'array depth is 47.   There is room for 48 filter blank sets
            For InnerPass = 0 To 3  'array depth is 3.   Each set can have up to 4 blanks in it
               If Experiment(batch).FilterBlankData(Pass).FilterBlankLocations(InnerPass) = cell Then
                  Data = "Yes,,"
                  Pass = 47
                  Exit For
               End If
            Next
         Next

         If Data = "" Then
            'This is not a fliter blank
            Loc = Experiment(batch).Filter(cell).FilterBlankPosition
            If Loc = 0 Then   'There is no filter blank
               Data = ",,"
            ElseIf Loc <= 0 Then 'It had a filter blank assigned to it, but the blanks had invalid weights
               Data = "," & (-Loc).ToString & " (invalid)" & ","
            Else
               Data = "," & Loc.ToString & ","
            End If
         End If

         Return Data
      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name, batch.ToString & vbCrLf & cell.ToString)
         MessageBox.Show("An exception was caught in function IsBlankFilter()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         Return ",,"
      End Try
   End Function


   Sub ProcessFilterBlank(ByVal batch As Int32)
      'For each filter group, do these tasks.
      '1. Select the filter blank used to process the data.  They use the blank with the highest weight wnless it is 50ug or higher.
      '2. Calculate the sample weight corrected for the filter blank
      Dim Pass As Int32
      Dim InnerPass As Int32
      Dim Weight1 As Double
      Dim Weight2 As Double
      Dim Weight3 As Double
      Dim Weight4 As Double
      Dim Loc As Int32
      Dim String1 As String
      Dim String2 As String


      Try
         'First calculate all the sample weights without filter blank correction, because some samples do not have blanks
         'If a sample has a blank, this value will be overwritten later.
         For Pass = 1 To Experiment(batch).TotalSamples
            If Experiment(batch).Filter(Pass).Status <> RunState.ErrorState Then
               '7/8/2011  They has an issue with rounding errors in the sample weight.  The trick here is to convert the
               'tare and selectedweight into strings, then back into double before doing the substraction.  This will turn
               '123.4999999999 into 123.500, which should eliminate rounding errors.
               String1 = Format(Experiment(batch).Filter(Pass).SelectedWeight, "0.000")
               String2 = Format(Experiment(batch).Filter(Pass).TareWeight, "0.000")
               Experiment(batch).Filter(Pass).SelectedWeight = CType(String1, Double)
               Experiment(batch).Filter(Pass).TareWeight = CType(String2, Double)

               Experiment(batch).Filter(Pass).SampleWeight = Experiment(batch).Filter(Pass).SelectedWeight - Experiment(batch).Filter(Pass).TareWeight
               Experiment(batch).MyDGV.Rows(Pass - 1).Cells(7).Value = Format(Experiment(batch).Filter(Pass).SampleWeight, "0.000")
            End If
            Application.DoEvents()
         Next

         For Pass = 0 To 47  'There can be up to 48 filter grups in a run
            If Experiment(batch).FilterBlankData(Pass).FilterBlankLocations(0) = 0 Then
               Exit For
            Else
               'get the blank weights for the filter blanks in this set
               Loc = Experiment(batch).FilterBlankData(Pass).FilterBlankLocations(0)
               If Loc = 0 Then
                  Weight1 = 0.0
               Else
                  If Experiment(batch).Filter(Loc).SampleWeight < 0.05 And Experiment(batch).Filter(Loc).SampleWeight >= 0.01 Then
                     Weight1 = Experiment(batch).Filter(Loc).SampleWeight
                  Else
                     Weight1 = 0.0
                  End If
               End If
               Loc = Experiment(batch).FilterBlankData(Pass).FilterBlankLocations(1)
               If Loc = 0 Then
                  Weight2 = 0.0
               Else
                  If Experiment(batch).Filter(Loc).SampleWeight < 0.05 And Experiment(batch).Filter(Loc).SampleWeight >= 0.01 Then
                     Weight2 = Experiment(batch).Filter(Loc).SampleWeight
                  Else
                     Weight2 = 0.0
                  End If
               End If
               Loc = Experiment(batch).FilterBlankData(Pass).FilterBlankLocations(2)
               If Loc = 0 Then
                  Weight3 = 0.0
               Else
                  If Experiment(batch).Filter(Loc).SampleWeight < 0.05 And Experiment(batch).Filter(Loc).SampleWeight >= 0.01 Then
                     Weight3 = Experiment(batch).Filter(Loc).SampleWeight
                  Else
                     Weight3 = 0.0
                  End If
               End If
               Loc = Experiment(batch).FilterBlankData(Pass).FilterBlankLocations(3)
               If Loc = 0 Then
                  Weight4 = 0.0
               Else
                  If Experiment(batch).Filter(Loc).SampleWeight < 0.05 And Experiment(batch).Filter(Loc).SampleWeight >= 0.01 Then
                     Weight4 = Experiment(batch).Filter(Loc).SampleWeight
                  Else
                     Weight4 = 0.0
                  End If
               End If

               Loc = 0
               If Weight1 > 0.0 Or Weight2 > 0.0 Or Weight3 > 0.0 Or Weight4 > 0.0 Then
                  If Weight1 >= Weight2 And Weight1 >= Weight3 And Weight1 >= Weight4 Then
                     Loc = Experiment(batch).FilterBlankData(Pass).FilterBlankLocations(0)
                  ElseIf Weight2 > Weight1 And Weight2 >= Weight3 And Weight2 >= Weight4 Then
                     Loc = Experiment(batch).FilterBlankData(Pass).FilterBlankLocations(1)
                  ElseIf Weight3 > Weight1 And Weight3 > Weight2 And Weight3 >= Weight4 Then
                     Loc = Experiment(batch).FilterBlankData(Pass).FilterBlankLocations(2)
                  Else
                     Loc = Experiment(batch).FilterBlankData(Pass).FilterBlankLocations(3)
                  End If
                  Experiment(batch).FilterBlankData(Pass).LocationOfBlankUsed = Loc

                  'There is filter blank data, but none of the blanks qualified.
               ElseIf Experiment(batch).FilterBlankData(Pass).FilterBlankLocations(0) > 0 Then
                  Experiment(batch).FilterBlankData(Pass).LocationOfBlankUsed = -Experiment(batch).FilterBlankData(Pass).FilterBlankLocations(0)  'Marking this as a negative number is a flag for the printout.
               Else
                  Experiment(batch).FilterBlankData(Pass).LocationOfBlankUsed = 0   'This filter is not blank corrected
               End If
            End If
         Next

         'For each filter group, you now know the location of the filter blank used for that set.
         'Now, for the filters in that set, load this filter blank then correct the sample weight by this blank weight
         For Pass = 0 To 47
            'For each filter in this group, mark it with the location of its filter blank.
            If Experiment(batch).FilterBlankData(Pass).LocationOfBlankUsed <> 0 Then
               For InnerPass = 0 To Experiment(batch).TotalSamples - 1
                  Loc = Experiment(batch).FilterBlankData(Pass).BlankAppliesToTheseFilters(InnerPass)
                  If Loc > 0 Then
                     Experiment(batch).Filter(Loc).FilterBlankPosition = Experiment(batch).FilterBlankData(Pass).LocationOfBlankUsed
                  Else
                     Exit For
                  End If
               Next
            End If
         Next

         'All filters are marked with the location of their filter blanks
         'Now correct sample weights
         For Pass = 1 To Experiment(batch).TotalSamples
            If Experiment(batch).Filter(Pass).FilterBlankPosition > 0 Then
               Loc = Experiment(batch).Filter(Pass).FilterBlankPosition
               'This corrects for rounding errors as explained above
               String1 = Format(Experiment(batch).Filter(Pass).SampleWeight, "0.000")
               Experiment(batch).Filter(Pass).SampleWeight = CType(String1, Double)
               String2 = Format(Experiment(batch).Filter(Loc).SampleWeight, "0.000")
               Experiment(batch).Filter(Loc).SampleWeight = CType(String2, Double)

               Experiment(batch).Filter(Pass).SampleWeight -= Experiment(batch).Filter(Loc).SampleWeight
               Experiment(batch).MyDGV.Rows(Pass - 1).Cells(7).Value = Format(Experiment(batch).Filter(Pass).SampleWeight, "0.000")
               Application.DoEvents()
            End If
         Next
      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name, batch.ToString)
         MessageBox.Show("An exception was caught in function ProcessFilterBlank()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub


   Function AnalyzeWeightData(ByVal batch As Int32, ByVal Pass As Int32) As Boolean
      'Examine the weight data to see if the analysis of the filter is complete.
      'In a weight experiment, once two weighings are within 10 micrograms, the experiment is over
      'A maximum of 3 weighings are allowed.
      Dim FilterAnalysisComplete As Boolean = False

      Try
         If Experiment(batch).WeighingStep >= 1 Then 'You must have done at least 2 weighing to analize data
            Dim Weight1 As Double
            Dim Weight2 As Double
            Dim Weight3 As Double
            Dim Lowest As Double
            Dim Steps As Int32


            Steps = Experiment(batch).WeighingStep
            Weight1 = Experiment(batch).Filter(Pass).Weights(0)
            Weight2 = Experiment(batch).Filter(Pass).Weights(1)
            If Steps = 2 Then
               Weight3 = Experiment(batch).Filter(Pass).Weights(2)
            End If

            If Steps = 1 Then 'Only two weights are present
               If Abs(Weight2 - Weight1) <= 0.01 Then  'if weights are within 10ug, you are done
                  If Weight1 < Weight2 Then
                     Experiment(batch).Filter(Pass).SelectedWeight = Weight1
                  Else
                     Experiment(batch).Filter(Pass).SelectedWeight = Weight2
                  End If
                  FilterAnalysisComplete = True
               End If
            Else  'three weights are present
               'Weight 1 will never be within 10ug of weight 2, or the filter would have already evaluted as being complete
               If Abs(Weight3 - Weight2) <= 0.01 And Abs(Weight3 - Weight1) <= 0.01 Then   'Both sets are less than 10ug
                  'Find the lowest number
                  If Weight1 < Weight2 Then
                     If Weight1 < Weight3 Then
                        Lowest = Weight1
                     Else
                        Lowest = Weight3
                     End If
                  ElseIf Weight2 < Weight3 Then
                     Lowest = Weight2
                  Else
                     Lowest = Weight3
                  End If
               ElseIf Abs(Weight2 - Weight3) <= 0.01 Or Abs(Weight1 - Weight3) <= 0.01 Then 'only one set is within 10ug
                  'find the one set that is less than 10ug
                  If Abs(Weight2 - Weight3) <= 0.01 Then
                     If Weight2 < Weight3 Then
                        Lowest = Weight2
                     Else
                        Lowest = Weight3
                     End If
                  Else  'Its set 1 and 3
                     If Weight1 < Weight3 Then
                        Lowest = Weight1
                     Else
                        Lowest = Weight3
                     End If
                  End If
               Else  'No weights are within 10ug
                  Dim NetWeight As Double
                  NetWeight = Experiment(batch).Filter(Pass).Weights(0) - Experiment(batch).Filter(Pass).TareWeight
                  If NetWeight < 2.0 Then 'find lowest weight with smallest difference
                     If Abs(Weight1 - Weight2) < Abs(Weight2 - Weight3) Then  'Find lowest difference set, then lowest weight
                        If Abs(Weight1 - Weight2) < Abs(Weight1 - Weight3) Then
                           If Weight1 < Weight2 Then
                              Lowest = Weight1
                           Else
                              Lowest = Weight2
                           End If
                        Else
                           If Weight1 < Weight3 Then
                              Lowest = Weight1
                           Else
                              Lowest = Weight3
                           End If
                        End If
                     Else
                        If Abs(Weight2 - Weight3) < Abs(Weight1 - Weight3) Then
                           If Weight2 < Weight3 Then
                              Lowest = Weight2
                           Else
                              Lowest = Weight3
                           End If
                        Else
                           If Weight1 < Weight3 Then
                              Lowest = Weight1
                           Else
                              Lowest = Weight3
                           End If
                        End If
                     End If
                  Else  'Sample weight is above 2mg
                     If Weight1 > Weight2 And Weight2 > Weight3 Then 'Do weights consecutivly go down?
                        Lowest = Weight3
                     Else  'Just find the lowest weight in the group with the smallest difference
                        If Abs(Weight1 - Weight2) < Abs(Weight2 - Weight3) Then  'Find lowest difference set, then lowest weight
                           If Abs(Weight1 - Weight2) < Abs(Weight1 - Weight3) Then
                              If Weight1 < Weight2 Then
                                 Lowest = Weight1
                              Else
                                 Lowest = Weight2
                              End If
                           Else
                              If Weight1 < Weight3 Then
                                 Lowest = Weight1
                              Else
                                 Lowest = Weight3
                              End If
                           End If
                        Else
                           If Abs(Weight2 - Weight3) < Abs(Weight1 - Weight3) Then
                              If Weight2 < Weight3 Then
                                 Lowest = Weight2
                              Else
                                 Lowest = Weight3
                              End If
                           Else
                              If Weight1 < Weight3 Then
                                 Lowest = Weight1
                              Else
                                 Lowest = Weight3
                              End If
                           End If
                        End If
                     End If
                  End If
               End If
               'Now assign the lowest weight when you have 3 weights
               Experiment(batch).Filter(Pass).SelectedWeight = Lowest
               FilterAnalysisComplete = True
            End If
         End If
         Return FilterAnalysisComplete
      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name, batch.ToString & vbCrLf & Pass.ToString)
         MessageBox.Show("An exception was caught in function AnalyzeWeightData()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         Return False
      End Try
   End Function


   Function AnalyzeTareWeightData(ByVal batch As Int32, ByVal Pass As Int32) As Boolean
      'This function is called during a tareweight experiment to see if two qualifing tare weights have been collected.
      'The main difference between this function and PickTareWeights is that PickTareWeigts analizes existing tare weights
      'and must select a value, while this one does not.
      Dim TareWeight As Double
      Dim TestWeights(11) As Double
      Dim MyW(3) As Double
      Dim GoodSet As Boolean = False

      Try
         For Element As Int32 = 0 To 3
            If Experiment(batch).Filter(Pass).Weights(Element) > 0.0 Then
               MyW(Element) = Experiment(batch).Filter(Pass).Weights(Element)
            Else
               MyW(Element) = Element + 1000.0  'just load a big garbage number
            End If
         Next
         For Element As Int32 = 0 To 11
            TestWeights(Element) = 0.0
         Next

         If Abs(MyW(0) - MyW(1)) <= 0.01 Then
            TestWeights(0) = MyW(0)
            TestWeights(1) = MyW(1)
            GoodSet = True
         End If
         If Abs(MyW(0) - MyW(2)) <= 0.01 Then
            TestWeights(2) = MyW(0)
            TestWeights(3) = MyW(2)
            GoodSet = True
         End If
         If Abs(MyW(0) - MyW(3)) <= 0.01 Then
            TestWeights(4) = MyW(0)
            TestWeights(5) = MyW(3)
            GoodSet = True
         End If
         If Abs(MyW(1) - MyW(2)) <= 0.01 Then
            TestWeights(6) = MyW(1)
            TestWeights(7) = MyW(2)
            GoodSet = True
         End If
         If Abs(MyW(1) - MyW(3)) <= 0.01 Then
            TestWeights(8) = MyW(1)
            TestWeights(9) = MyW(3)
            GoodSet = True
         End If
         If Abs(MyW(2) - MyW(3)) <= 0.01 Then
            TestWeights(10) = MyW(2)
            TestWeights(11) = MyW(3)
            GoodSet = True
         End If

         If GoodSet = True Then
            TareWeight = FindMaximum(TestWeights)
            Experiment(batch).Filter(Pass).TareWeight = TareWeight
         Else
            If Experiment(batch).Filter(Pass).Weights(3) > 0.0 Then  'This is the 4th filter weight.  
               Experiment(batch).Filter(Pass).Status = RunState.ErrorState
               Experiment(batch).MyDGV.Rows(Pass - 1).Cells(6).Value = "Error"
               DGVSupport.SetCellColor(Experiment(batch).MyDGV, Pass - 1, 6, Color.Red)
            End If
         End If
         Return GoodSet
      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name, batch.ToString & vbCrLf & Pass.ToString)
         MessageBox.Show("An exception was caught in function AnalyzeTareWeightData()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         Return False
      End Try
   End Function


   Function realPickTareWeight(ByRef weights() As Double) As Double
      'This fuontion is only called when reading in historical tare weights to find the weight that qualifies
      'YOU MUST send an array of 4 weights to this function.   Elements 0 and 1 MUST have valves greater than 0.0.
      'Any unused elements should be filled with 0.0
      'This function returns the highest weight of any two numbers within 0.01ug of each other.
      'If a weight is found that qualifies, then it is returned, if not a value of 0.0 is returned.
      Dim TareWeight As Double
      Dim TestWeights(11) As Double
      Dim Pass As Int32
      Dim GoodSet As Boolean = False


      Try
         For Pass = 0 To 11
            TestWeights(Pass) = 0.0
         Next

         If weights(2) = 0.0 Then   'Group has 2 weights, you must pick one
            If weights(0) > weights(1) Then
               TareWeight = weights(0)
            Else
               TareWeight = weights(1)
            End If

         ElseIf weights(3) = 0.0 Then  'Group has 3 weights
            If Abs(weights(0) - weights(1)) <= 0.01 Then
               TestWeights(0) = weights(0)
               TestWeights(1) = weights(1)
               GoodSet = True
            End If
            If Abs(weights(0) - weights(2)) <= 0.01 Then
               TestWeights(2) = weights(0)
               TestWeights(3) = weights(2)
               GoodSet = True
            End If
            If Abs(weights(1) - weights(2)) <= 0.01 Then
               TestWeights(4) = weights(1)
               TestWeights(5) = weights(2)
               GoodSet = True
            End If
            If GoodSet = False Then
               TestWeights(0) = weights(0)
               TestWeights(1) = weights(1)
               TestWeights(2) = weights(2)
            End If
            TareWeight = FindMaximum(TestWeights)

         Else  'This experiment has 4 weighings completed.
            If Abs(weights(0) - weights(1)) <= 0.01 Then
               TestWeights(0) = weights(0)
               TestWeights(1) = weights(1)
               GoodSet = True
            End If
            If Abs(weights(0) - weights(2)) <= 0.01 Then
               TestWeights(2) = weights(0)
               TestWeights(3) = weights(2)
               GoodSet = True
            End If
            If Abs(weights(0) - weights(3)) <= 0.01 Then
               TestWeights(4) = weights(0)
               TestWeights(5) = weights(3)
               GoodSet = True
            End If
            If Abs(weights(1) - weights(2)) <= 0.01 Then
               TestWeights(6) = weights(1)
               TestWeights(7) = weights(2)
               GoodSet = True
            End If
            If Abs(weights(1) - weights(3)) <= 0.01 Then
               TestWeights(8) = weights(1)
               TestWeights(9) = weights(3)
               GoodSet = True
            End If
            If Abs(weights(2) - weights(3)) <= 0.01 Then
               TestWeights(10) = weights(2)
               TestWeights(11) = weights(3)
               GoodSet = True
            End If
            If GoodSet = False Then
               TestWeights(0) = weights(0)
               TestWeights(1) = weights(1)
               TestWeights(2) = weights(2)
               TestWeights(3) = weights(3)
            End If
            TareWeight = FindMaximum(TestWeights)
         End If
         Return TareWeight
      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name)
         MessageBox.Show("An exception was caught in function PickTareWeight()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         If weights(0) > 0.0 Then
            Return weights(0)
         Else
            Return 0.0
         End If
      End Try
   End Function



   Function PickTareWeight(ByRef weights() As Double) As Double
      'This fuontion is only called when reading in historical tare weights to find the weight that qualifies
      'YOU MUST send an array of 4 weights to this function.   Elements 0 and 1 MUST have valves greater than 0.0.
      'Any unused elements should be filled with 0.0
      'This function returns the highest weight of any two numbers within 0.01ug of each other.
      'If a weight is found that qualifies, then it is returned, if not a value of 0.0 is returned.
      Dim TareWeight As Double
      Dim TestWeights(11) As Double
      Dim Pass As Int32
      Dim GoodSet As Boolean = False


      Try
         For Pass = 0 To 11
            TestWeights(Pass) = 0.0
         Next

         If Abs(weights(0) - weights(1)) <= 0.01 And weights(0) > 0.0 Then
            TestWeights(0) = weights(0)
            TestWeights(1) = weights(1)
            GoodSet = True
         End If
         If Abs(weights(0) - weights(2)) <= 0.01 And weights(0) > 0.0 Then
            TestWeights(2) = weights(0)
            TestWeights(3) = weights(2)
            GoodSet = True
         End If
         If Abs(weights(0) - weights(3)) <= 0.01 And weights(0) > 0.0 Then
            TestWeights(4) = weights(0)
            TestWeights(5) = weights(3)
            GoodSet = True
         End If
         If Abs(weights(1) - weights(2)) <= 0.01 And weights(1) > 0.0 Then
            TestWeights(6) = weights(1)
            TestWeights(7) = weights(2)
            GoodSet = True
         End If
         If Abs(weights(1) - weights(3)) <= 0.01 And weights(1) > 0.0 Then
            TestWeights(8) = weights(1)
            TestWeights(9) = weights(3)
            GoodSet = True
         End If
         If Abs(weights(2) - weights(3)) <= 0.01 And weights(2) > 0.0 Then
            TestWeights(10) = weights(2)
            TestWeights(11) = weights(3)
            GoodSet = True
         End If
         If GoodSet = False Then
            TestWeights(0) = weights(0)
            TestWeights(1) = weights(1)
            TestWeights(2) = weights(2)
            TestWeights(3) = weights(3)
         End If
         TareWeight = FindMaximum(TestWeights)

         Return TareWeight
      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name)
         MessageBox.Show("An exception was caught in function PickTareWeight()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         If weights(0) > 0.0 Then
            Return weights(0)
         Else
            Return 0.0
         End If
      End Try
   End Function



   Sub LookupTareWeights(ByVal batch As Int32, Optional ByVal cell As Int32 = 0)
      'These are read from the OSHA Master Tare Table, which has a file name of TareWeights.csv
      'The format of the table is"
      'barcode,TareWeight1,TareWeight2,TareWeight3,TareWeight4 <Cr><Lf>
      Dim Data() As String
      Dim Pass As Int32
      Dim Element As Int32
      Dim LineData() As String
      Dim Weight(3) As Double


      Try
         Data = FileIO.ReadTextFile(BaseFilePath & "TareWeights.csv").Split(Chr(10))  'split on LF
         If cell = 0 Then
            For Pass = 1 To Experiment(batch).TotalSamples
               If Experiment(batch).Filter(Pass).Status = RunState.Active And Experiment(batch).Filter(Pass).Barcode <> "NOREAD" Then
                  For Element = Data.Length - 1 To 1 Step -1
                     Try
                        LineData = Data(Element).Split(Chr(44))  'Split on comma
                        If Experiment(batch).Filter(Pass).Barcode = LineData(0) Then
                           If LineData.Length = 2 Then 'Has just a single tareweight
                              CleanData(LineData(1))  'Removes and garbage in the line that Excel inserts when the file is opened then saved in excel format.
                              If IsDouble(LineData(1), 0.1) = True Then
                                 Experiment(batch).Filter(Pass).TareWeight = CType(LineData(1), Double)
                              End If
                           ElseIf LineData.Length = 3 Then  'has 2 weights
                              CleanData(LineData(1))
                              If IsDouble(LineData(1), 0.1) = True Then
                                 Weight(0) = CType(LineData(1), Double)
                              Else
                                 Weight(0) = 0.0
                              End If
                              CleanData(LineData(2))
                              If IsDouble(LineData(2), 0.1) = True Then
                                 Weight(1) = CType(LineData(2), Double)
                              Else
                                 Weight(1) = 0.0
                              End If
                              Weight(2) = 0.0
                              Weight(3) = 0.0
                              Experiment(batch).Filter(Pass).TareWeight = PickTareWeight(Weight)
                           ElseIf LineData.Length = 4 Then  'has 3 weights
                              CleanData(LineData(1))
                              If IsDouble(LineData(1), 0.1) = True Then
                                 Weight(0) = CType(LineData(1), Double)
                              Else
                                 Weight(0) = 0.0
                              End If
                              CleanData(LineData(2))
                              If IsDouble(LineData(2), 0.1) = True Then
                                 Weight(1) = CType(LineData(2), Double)
                              Else
                                 Weight(1) = 0.0
                              End If
                              CleanData(LineData(3))
                              If IsDouble(LineData(3), 0.1) = True Then
                                 Weight(2) = CType(LineData(3), Double)
                              Else
                                 Weight(2) = 0.0
                              End If
                              Weight(3) = 0.0
                              Experiment(batch).Filter(Pass).TareWeight = PickTareWeight(Weight)
                           Else  'has at least 4 weights
                              CleanData(LineData(1))
                              If IsDouble(LineData(1), 0.1) = True Then
                                 Weight(0) = CType(LineData(1), Double)
                              Else
                                 Weight(0) = 0.0
                              End If
                              CleanData(LineData(2))
                              If IsDouble(LineData(2), 0.1) = True Then
                                 Weight(1) = CType(LineData(2), Double)
                              Else
                                 Weight(1) = 0.0
                              End If
                              CleanData(LineData(3))
                              If IsDouble(LineData(3), 0.1) = True Then
                                 Weight(2) = CType(LineData(3), Double)
                              Else
                                 Weight(2) = 0.0
                              End If
                              CleanData(LineData(4))
                              If IsDouble(LineData(4), 0.1) = True Then
                                 Weight(3) = CType(LineData(4), Double)
                              Else
                                 Weight(3) = 0.0
                              End If
                              Experiment(batch).Filter(Pass).TareWeight = PickTareWeight(Weight)
                           End If

                           Experiment(batch).MyDGV.Rows(Pass - 1).Cells(1).Value = Format(Experiment(batch).Filter(Pass).TareWeight, "0.000")
                           Exit For
                        End If
                     Catch ex As Exception
                        Me.rtbMessages.Text += "Line " & Element.ToString & " of the tare file is not formatted properly."
                     End Try
                  Next
               End If
            Next

         Else
            If Experiment(batch).Filter(cell).Status = RunState.Active Then
               For Element = Data.Length - 1 To 0 Step -1
                  Try
                     LineData = Data(Element).Split(Chr(44))  'Split on comma
                     If Experiment(batch).Filter(cell).Barcode = LineData(0) Then
                        If LineData.Length = 2 Then 'Has just a single tareweight
                           CleanData(LineData(1))
                           Experiment(batch).Filter(cell).TareWeight = CType(LineData(1), Double)
                        ElseIf LineData.Length = 3 Then  'has 2 weights
                           CleanData(LineData(1))
                           If IsDouble(LineData(1), 0.1) = True Then
                              Weight(0) = CType(LineData(1), Double)
                           Else
                              Weight(0) = 0.0
                           End If
                           CleanData(LineData(2))
                           If IsDouble(LineData(2), 0.1) = True Then
                              Weight(1) = CType(LineData(2), Double)
                           Else
                              Weight(1) = 0.0
                           End If
                           Weight(2) = 0.0
                           Weight(3) = 0.0
                           Experiment(batch).Filter(cell).TareWeight = PickTareWeight(Weight)
                        ElseIf LineData.Length = 4 Then  'has 3 weights
                           CleanData(LineData(1))
                           If IsDouble(LineData(1), 0.1) = True Then
                              Weight(0) = CType(LineData(1), Double)
                           Else
                              Weight(0) = 0.0
                           End If
                           CleanData(LineData(2))
                           If IsDouble(LineData(2), 0.1) = True Then
                              Weight(1) = CType(LineData(2), Double)
                           Else
                              Weight(1) = 0.0
                           End If
                           CleanData(LineData(3))
                           If IsDouble(LineData(3), 0.1) = True Then
                              Weight(2) = CType(LineData(3), Double)
                           Else
                              Weight(2) = 0.0
                           End If
                           Weight(3) = 0.0
                           Experiment(batch).Filter(cell).TareWeight = PickTareWeight(Weight)
                        Else  'has at least 4 weights
                           CleanData(LineData(1))
                           If IsDouble(LineData(1), 0.1) = True Then
                              Weight(0) = CType(LineData(1), Double)
                           Else
                              Weight(0) = 0.0
                           End If
                           CleanData(LineData(2))
                           If IsDouble(LineData(2), 0.1) = True Then
                              Weight(1) = CType(LineData(2), Double)
                           Else
                              Weight(1) = 0.0
                           End If
                           CleanData(LineData(3))
                           If IsDouble(LineData(3), 0.1) = True Then
                              Weight(2) = CType(LineData(3), Double)
                           Else
                              Weight(2) = 0.0
                           End If
                           CleanData(LineData(4))
                           If IsDouble(LineData(4), 0.1) = True Then
                              Weight(3) = CType(LineData(4), Double)
                           Else
                              Weight(3) = 0.0
                           End If
                           Experiment(batch).Filter(cell).TareWeight = PickTareWeight(Weight)
                        End If

                        Experiment(batch).MyDGV.Rows(cell - 1).Cells(1).Value = Format(Experiment(batch).Filter(cell).TareWeight, "0.000")
                        Exit For
                     End If
                  Catch ex As Exception
                     Me.rtbMessages.Text += "Line " & Element.ToString & " of the tare file is not formatted properly."
                  End Try
               Next
            End If
         End If

      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name, batch.ToString & vbCrLf & cell.ToString)
         MessageBox.Show("An exception was caught in function LookupTareWeights(), Outer Try." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         Experiment(batch).Status = RunState.ErrorState
      End Try
   End Sub


   Sub CleanData(ByRef data As String)
      'Pass this function a line that should contain only numeric charactors and it will remove junk from teh beginning and end of the string
      'Excel puts stray charactors at the beginning and end of the data in the .csv file.   This removes non-numeric charactors
      Dim ReturnLine As String = ""
      Dim Pass As Int32
      Dim Value As Int32

      Try
         For Pass = 0 To data.Length - 1  'Trim bad charactors at the beginning
            Value = AscW(data.Chars(Pass))
            If Value = 46 Or (Value >= 48 And Value <= 57) Then  'Selects only decimal points and the numbers 0 - 9
               ReturnLine = data.Substring(Pass)
               Exit For
            End If
         Next

         If ReturnLine = "" Then
            data = ""
            Exit Sub
         End If

         For Pass = ReturnLine.Length - 1 To 0 Step -1
            Value = AscW(ReturnLine.Chars(Pass))
            If Value = 46 Or (Value >= 48 And Value <= 57) Then
               data = ReturnLine.Substring(0, Pass + 1)
               Exit For
            End If
         Next

      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name, data)
      End Try
   End Sub



   Function ManageBarcodeData(ByVal batch As Int32, ByVal cell As Int32, ByVal reading As String) As Boolean
      'This function looks at the barcode that was just read and compares it to previous readings to make sure that it
      'is the same barcode. Or, if the previous reading was NOREAD, then it now inserts the newly read barcode.
      Dim GoodReading As Boolean = True

      Try
         If reading = "NOREAD" Then
            If Experiment(batch).Filter(cell).Barcode = "" Then
               Experiment(batch).Filter(cell).Barcode = reading
               Experiment(batch).MyDGV.Rows(cell - 1).Cells(0).Value = reading
            End If
         Else  'a barcode was decoded
            If Experiment(batch).Filter(cell).Barcode = "" Or Experiment(batch).Filter(cell).Barcode = "NOREAD" Then
               Experiment(batch).Filter(cell).Barcode = reading
               Experiment(batch).MyDGV.Rows(cell - 1).Cells(0).Value = reading
            Else
               If reading <> Experiment(batch).Filter(cell).Barcode Then
                  LogError(Now.ToShortDateString & ", " & Now.ToLongTimeString & "   Experiment " & (batch + 1).ToString & " recoreded a barcode of " & reading & " for position " & cell.ToString & " whos barcode is " & Experiment(batch).Filter(cell).Barcode & vbCrLf)
                  Me.rtbMessages.Text += vbCrLf & "The filter for Carousel " & (batch + 1).ToString & " in position " & cell.ToString & " reported two different bar code numbers."
                  GoodReading = False
               End If
            End If
         End If
         Return GoodReading
      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name, batch.ToString & vbCrLf & cell.ToString)
         MessageBox.Show("An exception was caught in function ManageBarCodeData()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         Return False
      End Try
   End Function


   Sub LogError(ByRef message As String)
      Try
         FileIO.WriteTextFile(BaseFilePath & "ErrorLog.txt", message, True, True)
         ErrorsWereLogged = True
      Catch ex As Exception
         ErrorDump(ex.Message, ex.TargetSite.Name, message)
         MessageBox.Show("An exception was caught in function ErrorLog()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub


#Region "Main Form Menu Commands"


   Private Sub mnuExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuExit.Click
      Me.Close()
   End Sub


   Private Sub mnuCarosel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuCarosel.Click
      Dim Password As String

      Try
         RunStepLockoutFlag = True  'Prevents the timer loop from starting a program step while this window is open
         Password = InputBox("The Carousel screen is password protected.   Enter the acess password.", "Protected Screen")
         If Password.ToUpper = "PASSWORD" Then
            Dim MD As New frmJkemDriverConfiguration()
            MD.ShowDialog()
            MD.Dispose()
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function mnuCarosel_Click()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
      RunStepLockoutFlag = False
   End Sub


   Private Sub mnuOvens_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuOvens.Click
      Dim Password As String

      Try
         RunStepLockoutFlag = True  'Prevents the timer loop from starting a program step while this window is open
         Password = InputBox("The Ovens screen is password protected.   Enter the acess password.", "Protected Screen")
         If Password.ToUpper = "PASSWORD" Then
            Dim FO As New frmOvens
            FO.ShowDialog()
            FO.Dispose()
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function mnuOvens_Click()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
      RunStepLockoutFlag = False
   End Sub

#End Region


#Region "General Form Events"

   Private Sub cbxExp1Experiment_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbxExp1Experiment.SelectedIndexChanged
      Try
         If Me.cbxExp1Experiment.SelectedIndex = 0 Then  'Selection is NONE
            Me.btnExp1Start.Enabled = False
         ElseIf Me.cbxExp1Experiment.SelectedIndex = 1 Then 'Selection is Tare New Filters
            Me.btnExp1Start.Enabled = True
         ElseIf Me.cbxExp1Experiment.SelectedIndex = 2 Then 'Selection is Weigh Filter Set
            If MessageBox.Show("Do you want to input Filter Blank data?", "Filter Blank Input", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) = Windows.Forms.DialogResult.Yes Then
               Dim FB As New frmBlankPicker(0)
               FB.ShowDialog()
               FB.Dispose()
            End If
            Me.btnExp1Start.Enabled = True
         ElseIf Me.cbxExp1Experiment.SelectedIndex = 3 Then 'Selection is Custom Experiment
            Dim FE As New frmExperimentBuilder(0)
            FE.ShowDialog()
            FE.Dispose()
            Me.btnExp1Start.Enabled = True
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function cbxExp1Experiment_SelectedIndexChanged()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Private Sub cbxExp1User_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbxExp1User.SelectedIndexChanged
      Try
         If Me.cbxExp1User.SelectedIndex > 0 Then
            Me.txbExp1Filters.Enabled = True
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function cbxExp1User_SelectedIndexChanged()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Private Sub txbExp1Filters_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbExp1Filters.TextChanged
      Try
         If IsInteger(Me.txbExp1Filters.Text, 0, 96) = True Then
            Me.cbxExp1Experiment.Enabled = True
            Me.Experiment(0).TotalSamples = CType(Me.txbExp1Filters.Text, Int32)
         Else
            Me.cbxExp1Experiment.Enabled = False
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function txbExp1Filters_TextChanged()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Private Sub cbxExp2Experiment_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbxExp2Experiment.SelectedIndexChanged
      Try
         If Me.cbxExp2Experiment.SelectedIndex = 0 Then
            Me.btnExp2Start.Enabled = False
         ElseIf Me.cbxExp2Experiment.SelectedIndex = 1 Then
            Me.btnExp2Start.Enabled = True
         ElseIf Me.cbxExp2Experiment.SelectedIndex = 2 Then
            If MessageBox.Show("Do you want to input Filter Blank data?", "Filter Blank Input", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) = Windows.Forms.DialogResult.Yes Then
               Dim FB As New frmBlankPicker(1)
               FB.ShowDialog()
               FB.Dispose()
            End If
            Me.btnExp2Start.Enabled = True
         ElseIf Me.cbxExp2Experiment.SelectedIndex = 3 Then 'This is a custom experiment
            Dim FE As New frmExperimentBuilder(1)
            FE.ShowDialog()
            FE.Dispose()
            Me.btnExp2Start.Enabled = True
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function cbxExp2Experiment_SelectedIndexChanged()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Private Sub cbxExp2User_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbxExp2User.SelectedIndexChanged
      Try
         If Me.cbxExp2User.SelectedIndex > 0 Then
            Me.txbExp2Filters.Enabled = True
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function cbxExp2User_SelectedIndexChanged()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Private Sub txbExp2Filters_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbExp2Filters.TextChanged
      Try
         If IsInteger(Me.txbExp2Filters.Text, 0, 96) = True Then
            Me.cbxExp2Experiment.Enabled = True
            Me.Experiment(1).TotalSamples = CType(Me.txbExp2Filters.Text, Int32)
         Else
            Me.cbxExp2Experiment.Enabled = False
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function txbExp2Filters_TextChanged()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Private Sub cbxExp3Experiment_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbxExp3Experiment.SelectedIndexChanged
      Try
         If Me.cbxExp3Experiment.SelectedIndex = 0 Then
            Me.btnExp3Start.Enabled = False
         ElseIf Me.cbxExp3Experiment.SelectedIndex = 1 Then
            Me.btnExp3Start.Enabled = True
         ElseIf Me.cbxExp3Experiment.SelectedIndex = 2 Then
            If MessageBox.Show("Do you want to input Filter Blank data?", "Filter Blank Input", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) = Windows.Forms.DialogResult.Yes Then
               Dim FB As New frmBlankPicker(2)
               FB.ShowDialog()
               FB.Dispose()
            End If
            Me.btnExp3Start.Enabled = True
         ElseIf Me.cbxExp3Experiment.SelectedIndex = 3 Then 'This is a custom experiment
            Dim FE As New frmExperimentBuilder(2)
            FE.ShowDialog()
            FE.Dispose()
            Me.btnExp3Start.Enabled = True
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function cbxExp3Experiment_SelectedIndexChanged()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Private Sub cbxExp3User_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbxExp3User.SelectedIndexChanged
      Try
         If Me.cbxExp3User.SelectedIndex > 0 Then
            Me.txbExp3Filters.Enabled = True
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function cbxExp3User_SelectedIndexChanged()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Private Sub txbExp3Filters_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbExp3Filters.TextChanged
      Try
         If IsInteger(Me.txbExp3Filters.Text, 0, 96) = True Then
            Me.cbxExp3Experiment.Enabled = True
            Me.Experiment(2).TotalSamples = CType(Me.txbExp3Filters.Text, Int32)
         Else
            Me.cbxExp3Experiment.Enabled = False
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function txbExp3Filters_TextChanged()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Private Sub cbxExp4Experiment_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbxExp4Experiment.SelectedIndexChanged
      Try
         If Me.cbxExp4Experiment.SelectedIndex = 0 Then
            Me.btnExp4Start.Enabled = False
         ElseIf Me.cbxExp4Experiment.SelectedIndex = 1 Then
            Me.btnExp4Start.Enabled = True
         ElseIf Me.cbxExp4Experiment.SelectedIndex = 2 Then
            If MessageBox.Show("Do you want to input Filter Blank data?", "Filter Blank Input", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) = Windows.Forms.DialogResult.Yes Then
               Dim FB As New frmBlankPicker(3)
               FB.ShowDialog()
               FB.Dispose()
            End If
            Me.btnExp4Start.Enabled = True
         ElseIf Me.cbxExp4Experiment.SelectedIndex = 3 Then 'This is a custom experiment
            Dim FE As New frmExperimentBuilder(3)
            FE.ShowDialog()
            FE.Dispose()
            Me.btnExp4Start.Enabled = True
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function cbxExp4Experiment_SelectedIndexChanged()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Private Sub cbxExp4User_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbxExp4User.SelectedIndexChanged
      Try
         If Me.cbxExp4User.SelectedIndex > 0 Then
            Me.txbExp4Filters.Enabled = True
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function cbxExp4User_SelectedIndexChanged()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Private Sub txbExp4Filters_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbExp4Filters.TextChanged
      Try
         If IsInteger(Me.txbExp4Filters.Text, 0, 96) = True Then
            Me.cbxExp4Experiment.Enabled = True
            Me.Experiment(3).TotalSamples = CType(Me.txbExp4Filters.Text, Int32)
         Else
            Me.cbxExp4Experiment.Enabled = False
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function txbExp4Filters_TextChanged()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Private Sub cbxExp5Experiment_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbxExp5Experiment.SelectedIndexChanged
      Try
         If Me.cbxExp5Experiment.SelectedIndex = 0 Then
            Me.btnExp5Start.Enabled = False
         ElseIf Me.cbxExp5Experiment.SelectedIndex = 1 Then
            Me.btnExp5Start.Enabled = True
         ElseIf Me.cbxExp5Experiment.SelectedIndex = 2 Then
            If MessageBox.Show("Do you want to input Filter Blank data?", "Filter Blank Input", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) = Windows.Forms.DialogResult.Yes Then
               Dim FB As New frmBlankPicker(4)
               FB.ShowDialog()
               FB.Dispose()
            End If
            Me.btnExp5Start.Enabled = True
         ElseIf Me.cbxExp5Experiment.SelectedIndex = 3 Then 'This is a custom experiment
            Dim FE As New frmExperimentBuilder(4)
            FE.ShowDialog()
            FE.Dispose()
            Me.btnExp5Start.Enabled = True
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function cbxExp5Experiment_SelectedIndexChanged()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Private Sub cbxExp5User_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cbxExp5User.SelectedIndexChanged
      Try
         If Me.cbxExp5User.SelectedIndex > 0 Then
            Me.txbExp5Filters.Enabled = True
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function cbxExp5User_SelectedIndexChanged()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Private Sub txbExp5Filters_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txbExp5Filters.TextChanged
      Try
         If IsInteger(Me.txbExp5Filters.Text, 0, 96) = True Then
            Me.cbxExp5Experiment.Enabled = True
            Me.Experiment(4).TotalSamples = CType(Me.txbExp5Filters.Text, Int32)
         Else
            Me.cbxExp5Experiment.Enabled = False
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function txbExp5Filters_TextChanged()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Private Sub mnuReset_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuReset.Click
      Dim Exp As String
      Dim Num As Int32

      Try
         Exp = InputBox("Enter the experiment number to reset.", "Reset Experiment")
         If IsInteger(Exp, 1, 5) = True Then
            Num = CType(Exp, Int32) - 1
            If Experiment(Num).Status = RunState.None Or Experiment(Num).Status = RunState.Done Or Experiment(Num).Status = RunState.ErrorState Then
               ResetExperiment(Num)
            End If
         Else
            MessageBox.Show("This is not a valid experiment number.", "Invalid Experiment", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function mnuReset_Click()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub


   Sub ResetExperiment(ByVal batch As Int32)
      Dim Pass As Int32
      Dim InnerPass As Int32

      Try
         Select Case batch
            Case 0
               Me.cbxExp1Experiment.SelectedIndex = 0
               Me.cbxExp1User.SelectedIndex = 0
               Me.txbExp1Filters.Text = "0"
               Me.btnExp1Start.Enabled = False
               Me.cbxExp1Experiment.Enabled = False
               Me.txbExp1Filters.Enabled = False
               Me.cbxExp1User.Enabled = True
               Me.lblExp1Status.Text = "Ready"
               Me.lblExp1Status.BackColor = Color.Lime
               Me.gbxExp1.Enabled = True
               Me.dgvExp1.Rows.Clear() 'This should clear the Rows collection, deleting all rows

            Case 1
               Me.cbxExp2Experiment.SelectedIndex = 0
               Me.cbxExp2User.SelectedIndex = 0
               Me.txbExp2Filters.Text = "0"
               Me.btnExp2Start.Enabled = False
               Me.cbxExp2Experiment.Enabled = False
               Me.txbExp2Filters.Enabled = False
               Me.cbxExp2User.Enabled = True
               Me.lblExp2Status.Text = "Ready"
               Me.lblExp2Status.BackColor = Color.Lime
               Me.gbxExp2.Enabled = True
               Me.dgvExp2.Rows.Clear() 'This should clear the Rows collection, deleting all rows

            Case 2
               Me.cbxExp3Experiment.SelectedIndex = 0
               Me.cbxExp3User.SelectedIndex = 0
               Me.txbExp3Filters.Text = "0"
               Me.btnExp3Start.Enabled = False
               Me.cbxExp3Experiment.Enabled = False
               Me.txbExp3Filters.Enabled = False
               Me.cbxExp3User.Enabled = True
               Me.lblExp3Status.Text = "Ready"
               Me.lblExp3Status.BackColor = Color.Lime
               Me.gbxExp3.Enabled = True
               Me.dgvExp3.Rows.Clear() 'This should clear the Rows collection, deleting all rows

            Case 3
               Me.cbxExp4Experiment.SelectedIndex = 0
               Me.cbxExp4User.SelectedIndex = 0
               Me.txbExp4Filters.Text = "0"
               Me.btnExp4Start.Enabled = False
               Me.cbxExp4Experiment.Enabled = False
               Me.txbExp4Filters.Enabled = False
               Me.cbxExp4User.Enabled = True
               Me.lblExp4Status.Text = "Ready"
               Me.lblExp4Status.BackColor = Color.Lime
               Me.gbxExp4.Enabled = True
               Me.dgvExp4.Rows.Clear() 'This should clear the Rows collection, deleting all rows

            Case 4
               Me.cbxExp5Experiment.SelectedIndex = 0
               Me.cbxExp5User.SelectedIndex = 0
               Me.txbExp5Filters.Text = "0"
               Me.btnExp5Start.Enabled = False
               Me.cbxExp5Experiment.Enabled = False
               Me.txbExp5Filters.Enabled = False
               Me.cbxExp5User.Enabled = True
               Me.lblExp5Status.Text = "Ready"
               Me.lblExp5Status.BackColor = Color.Lime
               Me.gbxExp5.Enabled = True
               Me.dgvExp5.Rows.Clear() 'This should clear the Rows collection, deleting all rows
         End Select


         For Pass = 1 To 96
            Experiment(batch).Filter(Pass).Barcode = ""
            Experiment(batch).Filter(Pass).Status = RunState.None
            Experiment(batch).Filter(Pass).SelectedWeight = 0.0
            Experiment(batch).Filter(Pass).SampleWeight = 0.0
            Experiment(batch).Filter(Pass).TareWeight = 0.0
            Experiment(batch).Filter(Pass).FilterBlankPosition = 0
            For InnerPass = 0 To 3
               Experiment(batch).Filter(Pass).Weights(InnerPass) = 0.0
            Next
         Next

         For Pass = 1 To 47
            Experiment(batch).FilterBlankData(Pass).LocationOfBlankUsed = 0
            For InnerPass = 0 To 3
               Experiment(batch).FilterBlankData(Pass).FilterBlankLocations(InnerPass) = 0
            Next
            For InnerPass = 0 To 95
               Experiment(batch).FilterBlankData(Pass).BlankAppliesToTheseFilters(InnerPass) = 0
            Next
         Next
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function ResetExperiment()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

#End Region



   Private Sub btnExp1Start_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExp1Start.Click
      Dim StringHolder As String
      Dim Pass As Int32
      Dim Names() As String
      Dim InnerPass As Int32

      Try
         StringHolder = Me.cbxExp1User.SelectedItem.ToString
         Names = StringHolder.Split(Chr(44)) 'split on comma
         Experiment(0).MyOperator.FullName = Names(0)
         Experiment(0).MyOperator.Initials = Names(1).Replace(" ", "")  'Get rid of the space
         Experiment(0).MyOperator.LISAName = Names(2).Replace(" ", "")  'Get rid of the space
         Me.lblExp1Stage.Text = "Starting Experiment"

      Catch ex As Exception
         Experiment(0).MyOperator.FullName = "NONE"
         Experiment(0).MyOperator.Initials = "NONE"
         Experiment(0).MyOperator.LISAName = "NONE"
         MessageBox.Show("An exception was caught in function btnExp1Start_Click(), Try #1." & vbCrLf & "The exception was: " & ex.Message _
                               & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                               & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                               "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try

      Try
         mnuCarousel.Enabled = False   'bob 6/13 change
         Experiment(0).Oven = Oven1
         Experiment(0).NextAccessTime = Now
         Experiment(0).CurrentRunStep = -1
         Experiment(0).TotalSamples = CType(Me.txbExp1Filters.Text, Int32)
         Experiment(0).WeighingStep = 0
         Experiment(0).Filename = Experiment(0).MyOperator.Initials & Now.Year.ToString & Format(Now.Month, "00") & _
               Format(Now.Day, "00") & "_" & Format(Now.Hour, "00") & Format(Now.Minute, "00") & Format(Now.Second, "00") & ".csv"

         If Me.cbxExp1Experiment.SelectedIndex = 1 Then  'This is the tare experiment
            Experiment(0).ExperimentType = ExpType.TareExperiment
            Me.lblDisplay1Experiment.Text = "Experiment: Tare Experiment"
            LoadTareExperiment(0)
         ElseIf Me.cbxExp1Experiment.SelectedIndex = 2 Then   'This is the weighing experiment
            Experiment(0).ExperimentType = ExpType.WeighingExperiment
            Me.lblDisplay1Experiment.Text = "Experiment: Weigh Samples Experiment"
            LoadWeightExperiment(0)
         ElseIf Me.cbxExp1Experiment.SelectedIndex = 3 Then 'This is a custom experiment
            If Experiment(0).ExperimentType = ExpType.TareExperiment Then
               Me.lblDisplay1Experiment.Text = "Experiment: Custom Tare Experiment"
            ElseIf Experiment(0).ExperimentType = ExpType.WeighingExperiment Then
               Me.lblDisplay1Experiment.Text = "Experiment: Custom Weigh Experiment"
            ElseIf Experiment(0).ExperimentType = ExpType.UserDefined Then
               Me.lblDisplay1Experiment.Text = "Experiment: Custom User Defined Experiment"
            End If
         End If

         'Format the DGV
         Me.dgvExp1.Rows.Clear()
         Me.dgvExp1.RowHeadersWidth = 60
         Me.dgvExp1.Rows.Add(Experiment(0).TotalSamples)
         If Experiment(0).Filter(0).Status = RunState.Active Then
            'The only time this filter, which the experiment does not use, is set to active is if the user created a custom experiment
            'and has set the state of the filters manually.
            For Pass = 1 To Experiment(0).TotalSamples
               Me.dgvExp1.Rows(Pass - 1).HeaderCell.Value = Pass.ToString
               If Experiment(0).Filter(Pass).Status = RunState.Active Then
                  Me.dgvExp1.Rows(Pass - 1).Cells(6).Value = "Active"
               Else
                  Me.dgvExp1.Rows(Pass - 1).Cells(6).Value = "Done"
               End If
            Next
         Else
            For Pass = 1 To 96
               If Pass <= Experiment(0).TotalSamples Then
                  Experiment(0).Filter(Pass).Status = RunState.Active
                  Experiment(0).Filter(Pass).TareWeight = 0.0
                  For InnerPass = 0 To 3
                     Experiment(0).Filter(Pass).Weights(InnerPass) = 0.0
                  Next
                  Me.dgvExp1.Rows(Pass - 1).HeaderCell.Value = Pass.ToString
                  Me.dgvExp1.Rows(Pass - 1).Cells(6).Value = "Active"
               Else
                  Experiment(0).Filter(Pass).Status = RunState.None
               End If
            Next
         End If

         Experiment(0).MyDGV = dgvExp1
         DGVSupport.SizeTable(dgvExp1)
         Me.gbxExp1.Enabled = False
         Me.lblExp1Status.Text = "Active"
         Me.lblExp1Status.BackColor = Color.Yellow
         Me.TabControl1.SelectedIndex = 1
         Application.DoEvents()

         Me.mnuExp1Abort.Enabled = True
         Me.mnuManualOven1.Enabled = True
         Experiment(0).Status = RunState.Active

         'Set this to false.  On exit, it is set to true
         My.Settings.NormalProgramExit = False
         My.Settings.Experiment1IsActive = True
         My.Settings.Save()
         Me.lblMasterTimer.Visible = True

         If MasterTimerLockout = False And MasterTimerIsActive = False Then
            MasterTimer.Start()
            MasterTimerIsActive = True
         End If

      Catch ex As Exception
         MessageBox.Show("An exception was caught in function btnExp1Start_Click(), Try #2." & vbCrLf & "The exception was: " & ex.Message _
                               & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                               & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                               "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         Experiment(0).Status = RunState.None
      End Try
   End Sub


   Private Sub btnExp2Start_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExp2Start.Click
      Dim StringHolder As String
      Dim Pass As Int32
      Dim Names() As String
      Dim InnerPass As Int32


      Try
         StringHolder = Me.cbxExp2User.SelectedItem.ToString
         Names = StringHolder.Split(Chr(44)) 'split on comma
         Experiment(1).MyOperator.FullName = Names(0)
         Experiment(1).MyOperator.Initials = Names(1).Replace(" ", "")  'Get rid of the space
         Experiment(1).MyOperator.LISAName = Names(2).Replace(" ", "")  'Get rid of the space
         Me.lblExp2Stage.Text = "Starting Experiment"

      Catch ex As Exception
         Experiment(1).MyOperator.FullName = "NONE"
         Experiment(1).MyOperator.Initials = "NONE"
         Experiment(1).MyOperator.LISAName = "NONE"
         MessageBox.Show("An exception was caught in function btnExp2Start_Click(), Try #1." & vbCrLf & "The exception was: " & ex.Message _
                               & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                               & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                               "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try

      Try
         mnuCarousel.Enabled = False   'bob 6/13 change
         Experiment(1).Oven = Oven2
         Experiment(1).NextAccessTime = Now
         Experiment(1).CurrentRunStep = -1
         Experiment(1).TotalSamples = CType(Me.txbExp2Filters.Text, Int32)
         Experiment(1).WeighingStep = 0
         Experiment(1).Filename = Experiment(1).MyOperator.Initials & Now.Year.ToString & Format(Now.Month, "00") & _
         Format(Now.Day, "00") & "_" & Format(Now.Hour, "00") & Format(Now.Minute, "00") & Format(Now.Second, "00") & ".csv"

         If Me.cbxExp2Experiment.SelectedIndex = 1 Then  'This is the tare experiment
            Experiment(1).ExperimentType = ExpType.TareExperiment
            Me.lblDisplay2Experiment.Text = "Experiment: Tare Experiment"
            LoadTareExperiment(1)
         ElseIf Me.cbxExp2Experiment.SelectedIndex = 2 Then 'This is the weighing experiment
            Experiment(1).ExperimentType = ExpType.WeighingExperiment
            Me.lblDisplay2Experiment.Text = "Experiment: Weigh Samples Experiment"
            LoadWeightExperiment(1)
         ElseIf Me.cbxExp2Experiment.SelectedIndex = 3 Then 'This is a custom experiment
            If Experiment(1).ExperimentType = ExpType.TareExperiment Then
               Me.lblDisplay2Experiment.Text = "Experiment: Custom Tare Experiment"
            ElseIf Experiment(1).ExperimentType = ExpType.WeighingExperiment Then
               Me.lblDisplay2Experiment.Text = "Experiment: Custom Weigh Experiment"
            ElseIf Experiment(1).ExperimentType = ExpType.UserDefined Then
               Me.lblDisplay2Experiment.Text = "Experiment: Custom User Defined Experiment"
            End If
         End If

         'Format the DGV
         Me.dgvExp2.Rows.Clear() 'This should clear the Rows collection, deleting all rows
         Me.dgvExp2.RowHeadersWidth = 60
         Me.dgvExp2.Rows.Add(Experiment(1).TotalSamples)
         If Experiment(1).Filter(0).Status = RunState.Active Then
            'The only time this filter, which the experiment does not use, is set to active is if the user created a custom experiment
            'and has set the state of the filters manually.
            For Pass = 1 To Experiment(1).TotalSamples
               Me.dgvExp2.Rows(Pass - 1).HeaderCell.Value = Pass.ToString
               If Experiment(1).Filter(Pass).Status = RunState.Active Then
                  Me.dgvExp2.Rows(Pass - 1).Cells(6).Value = "Active"
               Else
                  Me.dgvExp2.Rows(Pass - 1).Cells(6).Value = "Done"
               End If
            Next
         Else
            For Pass = 1 To 96
               If Pass <= Experiment(1).TotalSamples Then
                  Experiment(1).Filter(Pass).Status = RunState.Active
                  Experiment(1).Filter(Pass).TareWeight = 0.0
                  For InnerPass = 0 To 3
                     Experiment(1).Filter(Pass).Weights(InnerPass) = 0.0
                  Next
                  Me.dgvExp2.Rows(Pass - 1).HeaderCell.Value = Pass.ToString
                  Me.dgvExp2.Rows(Pass - 1).Cells(6).Value = "Active"
               Else
                  Experiment(1).Filter(Pass).Status = RunState.None
               End If
            Next
         End If

         Experiment(1).MyDGV = dgvExp2
         DGVSupport.SizeTable(dgvExp2)
         Me.gbxExp2.Enabled = False
         Me.lblExp2Status.Text = "Active"
         Me.lblExp2Status.BackColor = Color.Yellow
         Me.TabControl1.SelectedIndex = 2
         Application.DoEvents()

         Me.mnuExp2Abort.Enabled = True
         Me.mnuManualOven2.Enabled = True
         Experiment(1).Status = RunState.Active

         'Set this to false.  On exit, it is set to true
         My.Settings.NormalProgramExit = False
         My.Settings.Experiment2IsActive = True
         My.Settings.Save()
         Me.lblMasterTimer.Visible = True
         If MasterTimerLockout = False And MasterTimerIsActive = False Then
            MasterTimer.Start()
            MasterTimerIsActive = True
         End If

      Catch ex As Exception
         MessageBox.Show("An exception was caught in function btnExp2Start_Click(), Try #2." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         Experiment(1).Status = RunState.None
      End Try
   End Sub


   Private Sub btnExp3Start_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExp3Start.Click
      Dim StringHolder As String
      Dim Pass As Int32
      Dim Names() As String
      Dim InnerPass As Int32


      Try
         StringHolder = Me.cbxExp3User.SelectedItem.ToString
         Names = StringHolder.Split(Chr(44)) 'split on comma
         Experiment(2).MyOperator.FullName = Names(0)
         Experiment(2).MyOperator.Initials = Names(1).Replace(" ", "")  'Get rid of the space
         Experiment(2).MyOperator.LISAName = Names(2).Replace(" ", "")  'Get rid of the space
         Me.lblExp3Stage.Text = "Starting Experiment"

      Catch ex As Exception
         Experiment(2).MyOperator.FullName = "NONE"
         Experiment(2).MyOperator.Initials = "NONE"
         Experiment(2).MyOperator.LISAName = "NONE"
         MessageBox.Show("An exception was caught in function btnExp3Start_Click(), Try #1." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try

      Try
         mnuCarousel.Enabled = False   'bob 6/13 change
         Experiment(2).Oven = Oven3
         Experiment(2).NextAccessTime = Now
         Experiment(2).CurrentRunStep = -1
         Experiment(2).TotalSamples = CType(Me.txbExp3Filters.Text, Int32)
         Experiment(2).WeighingStep = 0
         Experiment(2).Filename = Experiment(2).MyOperator.Initials & Now.Year.ToString & Format(Now.Month, "00") & _
         Format(Now.Day, "00") & "_" & Format(Now.Hour, "00") & Format(Now.Minute, "00") & Format(Now.Second, "00") & ".csv"

         If Me.cbxExp3Experiment.SelectedIndex = 1 Then  'This is the tare experiment
            Experiment(2).ExperimentType = ExpType.TareExperiment
            Me.lblDisplay3Experiment.Text = "Experiment: Tare Experiment"
            LoadTareExperiment(2)
         ElseIf Me.cbxExp3Experiment.SelectedIndex = 2 Then  'This is the weighing experiment
            Experiment(2).ExperimentType = ExpType.WeighingExperiment
            Me.lblDisplay3Experiment.Text = "Experiment: Weigh Samples Experiment"
            LoadWeightExperiment(2)
         ElseIf Me.cbxExp3Experiment.SelectedIndex = 3 Then 'This is a custom experiment
            If Experiment(2).ExperimentType = ExpType.TareExperiment Then
               Me.lblDisplay3Experiment.Text = "Experiment: Custom Tare Experiment"
            ElseIf Experiment(2).ExperimentType = ExpType.WeighingExperiment Then
               Me.lblDisplay3Experiment.Text = "Experiment: Custom Weigh Experiment"
            ElseIf Experiment(2).ExperimentType = ExpType.UserDefined Then
               Me.lblDisplay3Experiment.Text = "Experiment: Custom User Defined Experiment"
            End If
         End If

         'Format the DGV
         Me.dgvExp3.Rows.Clear() 'This should clear the Rows collection, deleting all rows
         Me.dgvExp3.RowHeadersWidth = 60
         Me.dgvExp3.Rows.Add(Experiment(2).TotalSamples)
         If Experiment(2).Filter(0).Status = RunState.Active Then
            'The only time this filter, which the experiment does not use, is set to active is if the user created a custom experiment
            'and has set the state of the filters manually.
            For Pass = 1 To Experiment(2).TotalSamples
               Me.dgvExp3.Rows(Pass - 1).HeaderCell.Value = Pass.ToString
               If Experiment(2).Filter(Pass).Status = RunState.Active Then
                  Me.dgvExp3.Rows(Pass - 1).Cells(6).Value = "Active"
               Else
                  Me.dgvExp3.Rows(Pass - 1).Cells(6).Value = "Done"
               End If
            Next
         Else
            For Pass = 1 To 96
               If Pass <= Experiment(2).TotalSamples Then
                  Experiment(2).Filter(Pass).Status = RunState.Active
                  Experiment(2).Filter(Pass).TareWeight = 0.0
                  For InnerPass = 0 To 3
                     Experiment(2).Filter(Pass).Weights(InnerPass) = 0.0
                  Next
                  Me.dgvExp3.Rows(Pass - 1).HeaderCell.Value = Pass.ToString
                  Me.dgvExp3.Rows(Pass - 1).Cells(6).Value = "Active"
               Else
                  Experiment(2).Filter(Pass).Status = RunState.None
               End If
            Next
         End If

         Experiment(2).MyDGV = dgvExp3
         DGVSupport.SizeTable(dgvExp3)
         Me.gbxExp3.Enabled = False
         Me.lblExp3Status.Text = "Active"
         Me.lblExp3Status.BackColor = Color.Yellow
         Me.TabControl1.SelectedIndex = 3
         Application.DoEvents()

         Me.mnuExp3Abort.Enabled = True
         Me.mnuManualOven3.Enabled = True
         Experiment(2).Status = RunState.Active

         'Set this to false.  On exit, it is set to true
         My.Settings.NormalProgramExit = False
         My.Settings.Experiment3IsActive = True
         My.Settings.Save()
         Me.lblMasterTimer.Visible = True
         If MasterTimerLockout = False And MasterTimerIsActive = False Then
            MasterTimer.Start()
            MasterTimerIsActive = True
         End If

      Catch ex As Exception
         MessageBox.Show("An exception was caught in function btnExp3Start_Click(), Try #2." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         Experiment(2).Status = RunState.None
      End Try
   End Sub


   Private Sub btnExp4Start_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExp4Start.Click
      Dim StringHolder As String
      Dim Pass As Int32
      Dim Names() As String
      Dim InnerPass As Int32


      Try
         StringHolder = Me.cbxExp4User.SelectedItem.ToString
         Names = StringHolder.Split(Chr(44)) 'split on comma
         Experiment(3).MyOperator.FullName = Names(0)
         Experiment(3).MyOperator.Initials = Names(1).Replace(" ", "")  'Get rid of the space
         Experiment(3).MyOperator.LISAName = Names(2).Replace(" ", "")  'Get rid of the space
         Me.lblExp4Stage.Text = "Starting Experiment"

      Catch ex As Exception
         Experiment(3).MyOperator.FullName = "NONE"
         Experiment(3).MyOperator.Initials = "NONE"
         Experiment(3).MyOperator.LISAName = "NONE"
         MessageBox.Show("An exception was caught in function btnExp4Start_Click(), Try #1." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try

      Try
         mnuCarousel.Enabled = False   'bob 6/13 change
         Experiment(3).Oven = Oven4
         Experiment(3).NextAccessTime = Now
         Experiment(3).CurrentRunStep = -1
         Experiment(3).TotalSamples = CType(Me.txbExp4Filters.Text, Int32)
         Experiment(3).WeighingStep = 0
         Experiment(3).Filename = Experiment(3).MyOperator.Initials & Now.Year.ToString & Format(Now.Month, "00") & _
         Format(Now.Day, "00") & "_" & Format(Now.Hour, "00") & Format(Now.Minute, "00") & Format(Now.Second, "00") & ".csv"

         If Me.cbxExp4Experiment.SelectedIndex = 1 Then  'This is the tare experiment
            Experiment(3).ExperimentType = ExpType.TareExperiment
            Me.lblDisplay4Experiment.Text = "Experiment: Tare Experiment"
            LoadTareExperiment(3)
         ElseIf Me.cbxExp4Experiment.SelectedIndex = 2 Then  'This is the weighing experiment
            Experiment(3).ExperimentType = ExpType.WeighingExperiment
            Me.lblDisplay4Experiment.Text = "Experiment: Weigh Samples Experiment"
            LoadWeightExperiment(3)
         ElseIf Me.cbxExp4Experiment.SelectedIndex = 3 Then 'This is a custom experiment
            If Experiment(3).ExperimentType = ExpType.TareExperiment Then
               Me.lblDisplay4Experiment.Text = "Experiment: Custom Tare Experiment"
            ElseIf Experiment(3).ExperimentType = ExpType.WeighingExperiment Then
               Me.lblDisplay4Experiment.Text = "Experiment: Custom Weigh Experiment"
            ElseIf Experiment(3).ExperimentType = ExpType.UserDefined Then
               Me.lblDisplay4Experiment.Text = "Experiment: Custom User Defined Experiment"
            End If
         End If

         'Format the DGV
         Me.dgvExp4.Rows.Clear() 'This should clear the Rows collection, deleting all rows
         Me.dgvExp4.RowHeadersWidth = 60
         Me.dgvExp4.Rows.Add(Experiment(3).TotalSamples)
         If Experiment(3).Filter(0).Status = RunState.Active Then
            'The only time this filter, which the experiment does not use, is set to active is if the user created a custom experiment
            'and has set the state of the filters manually.
            For Pass = 1 To Experiment(3).TotalSamples
               Me.dgvExp4.Rows(Pass - 1).HeaderCell.Value = Pass.ToString
               If Experiment(3).Filter(Pass).Status = RunState.Active Then
                  Me.dgvExp4.Rows(Pass - 1).Cells(6).Value = "Active"
               Else
                  Me.dgvExp4.Rows(Pass - 1).Cells(6).Value = "Done"
               End If
            Next
         Else
            For Pass = 1 To 96
               If Pass <= Experiment(3).TotalSamples Then
                  Experiment(3).Filter(Pass).Status = RunState.Active
                  Experiment(3).Filter(Pass).TareWeight = 0.0
                  For InnerPass = 0 To 3
                     Experiment(3).Filter(Pass).Weights(InnerPass) = 0.0
                  Next
                  Me.dgvExp4.Rows(Pass - 1).HeaderCell.Value = Pass.ToString
                  Me.dgvExp4.Rows(Pass - 1).Cells(6).Value = "Active"
               Else
                  Experiment(3).Filter(Pass).Status = RunState.None
               End If
            Next
         End If

         Experiment(3).MyDGV = dgvExp4
         DGVSupport.SizeTable(dgvExp4)
         Me.gbxExp4.Enabled = False
         Me.lblExp4Status.Text = "Active"
         Me.lblExp4Status.BackColor = Color.Yellow
         Me.TabControl1.SelectedIndex = 4
         Application.DoEvents()

         Me.mnuExp4Abort.Enabled = True
         Me.mnuManualOven4.Enabled = True
         Experiment(3).Status = RunState.Active

         'Set this to false.  On exit, it is set to true
         My.Settings.NormalProgramExit = False
         My.Settings.Experiment4IsActive = True
         My.Settings.Save()
         Me.lblMasterTimer.Visible = True
         If MasterTimerLockout = False And MasterTimerIsActive = False Then
            MasterTimer.Start()
            MasterTimerIsActive = True
         End If

      Catch ex As Exception
         MessageBox.Show("An exception was caught in function btnExp4Start_Click(), Try #2." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         Experiment(3).Status = RunState.None
      End Try
   End Sub


   Private Sub btnExp5Start_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExp5Start.Click
      Dim StringHolder As String
      Dim Pass As Int32
      Dim Names() As String
      Dim InnerPass As Int32


      Try
         StringHolder = Me.cbxExp5User.SelectedItem.ToString
         Names = StringHolder.Split(Chr(44)) 'split on comma
         Experiment(4).MyOperator.FullName = Names(0)
         Experiment(4).MyOperator.Initials = Names(1).Replace(" ", "")  'Get rid of the space
         Experiment(4).MyOperator.LISAName = Names(2).Replace(" ", "")  'Get rid of the space
         Me.lblExp5Stage.Text = "Starting Experiment"

      Catch ex As Exception
         Experiment(4).MyOperator.FullName = "NONE"
         Experiment(4).MyOperator.Initials = "NONE"
         Experiment(4).MyOperator.LISAName = "NONE"
         MessageBox.Show("An exception was caught in function btnExp5Start_Click(), Try #1." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try

      Try
         mnuCarousel.Enabled = False   'bob 6/13 change
         Experiment(4).Oven = Oven5
         Experiment(4).NextAccessTime = Now
         Experiment(4).CurrentRunStep = -1
         Experiment(4).TotalSamples = CType(Me.txbExp5Filters.Text, Int32)
         Experiment(4).WeighingStep = 0
         Experiment(4).Filename = Experiment(4).MyOperator.Initials & Now.Year.ToString & Format(Now.Month, "00") & _
         Format(Now.Day, "00") & "_" & Format(Now.Hour, "00") & Format(Now.Minute, "00") & Format(Now.Second, "00") & ".csv"

         If Me.cbxExp5Experiment.SelectedIndex = 1 Then  'This is the tare experiment
            Experiment(4).ExperimentType = ExpType.TareExperiment
            Me.lblDisplay5Experiment.Text = "Experiment: Tare Experiment"
            LoadTareExperiment(4)
         ElseIf Me.cbxExp5Experiment.SelectedIndex = 2 Then  'This is the weighing experiment
            Experiment(4).ExperimentType = ExpType.WeighingExperiment
            Me.lblDisplay5Experiment.Text = "Experiment: Weigh Samples Experiment"
            LoadWeightExperiment(4)
         ElseIf Me.cbxExp5Experiment.SelectedIndex = 3 Then 'This is a custom experiment
            If Experiment(4).ExperimentType = ExpType.TareExperiment Then
               Me.lblDisplay5Experiment.Text = "Experiment: Custom Tare Experiment"
            ElseIf Experiment(4).ExperimentType = ExpType.WeighingExperiment Then
               Me.lblDisplay5Experiment.Text = "Experiment: Custom Weigh Experiment"
            ElseIf Experiment(4).ExperimentType = ExpType.UserDefined Then
               Me.lblDisplay5Experiment.Text = "Experiment: Custom User Defined Experiment"
            End If
         End If

         'Format the DGV
         Me.dgvExp5.Rows.Clear() 'This should clear the Rows collection, deleting all rows
         Me.dgvExp5.RowHeadersWidth = 60
         Me.dgvExp5.Rows.Add(Experiment(4).TotalSamples)
         If Experiment(4).Filter(0).Status = RunState.Active Then
            'The only time this filter, which the experiment does not use, is set to active is if the user created a custom experiment
            'and has set the state of the filters manually.
            For Pass = 1 To Experiment(4).TotalSamples
               Me.dgvExp5.Rows(Pass - 1).HeaderCell.Value = Pass.ToString
               If Experiment(4).Filter(Pass).Status = RunState.Active Then
                  Me.dgvExp5.Rows(Pass - 1).Cells(6).Value = "Active"
               Else
                  Me.dgvExp5.Rows(Pass - 1).Cells(6).Value = "Done"
               End If
            Next
         Else
            For Pass = 1 To 96
               If Pass <= Experiment(4).TotalSamples Then
                  Experiment(4).Filter(Pass).Status = RunState.Active
                  Experiment(4).Filter(Pass).TareWeight = 0.0
                  For InnerPass = 0 To 3
                     Experiment(4).Filter(Pass).Weights(InnerPass) = 0.0
                  Next
                  Me.dgvExp5.Rows(Pass - 1).HeaderCell.Value = Pass.ToString
                  Me.dgvExp5.Rows(Pass - 1).Cells(6).Value = "Active"
               Else
                  Experiment(4).Filter(Pass).Status = RunState.None
               End If
            Next
         End If

         Experiment(4).MyDGV = dgvExp5
         DGVSupport.SizeTable(dgvExp5)
         Me.gbxExp5.Enabled = False
         Me.lblExp5Status.Text = "Active"
         Me.lblExp5Status.BackColor = Color.Yellow
         Me.TabControl1.SelectedIndex = 5
         Application.DoEvents()

         Me.mnuExp5Abort.Enabled = True
         Me.mnuManualOven5.Enabled = True
         Experiment(4).Status = RunState.Active

         'Set this to false.  On exit, it is set to true
         My.Settings.NormalProgramExit = False
         My.Settings.Experiment5IsActive = True
         My.Settings.Save()
         Me.lblMasterTimer.Visible = True
         If MasterTimerLockout = False And MasterTimerIsActive = False Then
            MasterTimer.Start()
            MasterTimerIsActive = True
         End If

      Catch ex As Exception
         MessageBox.Show("An exception was caught in function btnExp5Start_Click(), Try #2." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         Experiment(4).Status = RunState.None
      End Try
   End Sub



   Sub LoadWeightExperiment(ByVal batch As Int32)
      Try
         'Define the experiment and load it into the RunSteps array.
         'This is a standard OSHA program that can not change its steps
         Experiment(batch).RunSteps(0).StepType = Tasks.TakeToOven
         Experiment(batch).RunSteps(0).StepDuration = TimeSpan.FromHours(35.0)
         Experiment(batch).RunSteps(0).Temperature = 40.0
         Experiment(batch).RunSteps(1).StepType = Tasks.TakeToCarosel
         Experiment(batch).RunSteps(1).StepDuration = TimeSpan.FromHours(6.0)
         Experiment(batch).RunSteps(2).StepType = Tasks.WeighSamplesTakeToOven
         Experiment(batch).RunSteps(2).StepDuration = TimeSpan.FromHours(15.0)
         Experiment(batch).RunSteps(2).Temperature = 40.0
         Experiment(batch).RunSteps(3).StepType = Tasks.TakeToCarosel
         Experiment(batch).RunSteps(3).StepDuration = TimeSpan.FromHours(6.0)
         Experiment(batch).RunSteps(4).StepType = Tasks.WeighSamplesTakeToOven
         Experiment(batch).RunSteps(4).StepDuration = TimeSpan.FromHours(15.0)
         Experiment(batch).RunSteps(4).Temperature = 40.0
         Experiment(batch).RunSteps(5).StepType = Tasks.TakeToCarosel
         Experiment(batch).RunSteps(5).StepDuration = TimeSpan.FromHours(6.0)
         Experiment(batch).RunSteps(6).StepType = Tasks.WeighSamplesTakeToCarosel
         Experiment(batch).RunSteps(6).StepDuration = TimeSpan.Zero
         Experiment(batch).RunSteps(7).StepType = Tasks.CloseExperiment
         Experiment(batch).RunSteps(7).StepDuration = TimeSpan.Zero
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function LoadWeightExperiment()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Sub LoadTareExperiment(ByVal batch As Int32)
      Try
         'Define the experiment and load it into the RunSteps array
         'This is a standard OSHA program that can not change its steps
         Experiment(batch).RunSteps(0).StepType = Tasks.TakeToOven
         Experiment(batch).RunSteps(0).StepDuration = TimeSpan.FromHours(36.0)
         Experiment(batch).RunSteps(0).Temperature = 40.0
         Experiment(batch).RunSteps(1).StepType = Tasks.TakeToCarosel
         Experiment(batch).RunSteps(1).StepDuration = TimeSpan.FromHours(6.0)
         Experiment(batch).RunSteps(2).StepType = Tasks.WeighSamplesTakeToOven
         Experiment(batch).RunSteps(2).StepDuration = TimeSpan.FromHours(15.0)
         Experiment(batch).RunSteps(2).Temperature = 40.0
         Experiment(batch).RunSteps(3).StepType = Tasks.TakeToCarosel
         Experiment(batch).RunSteps(3).StepDuration = TimeSpan.FromHours(6.0)
         Experiment(batch).RunSteps(4).StepType = Tasks.WeighSamplesTakeToOven
         Experiment(batch).RunSteps(4).StepDuration = TimeSpan.FromHours(15.0)
         Experiment(batch).RunSteps(4).Temperature = 40.0
         Experiment(batch).RunSteps(5).StepType = Tasks.TakeToCarosel
         Experiment(batch).RunSteps(5).StepDuration = TimeSpan.FromHours(6.0)
         Experiment(batch).RunSteps(6).StepType = Tasks.WeighSamplesTakeToOven
         Experiment(batch).RunSteps(6).StepDuration = TimeSpan.FromHours(15.0)
         Experiment(batch).RunSteps(6).Temperature = 40.0
         Experiment(batch).RunSteps(7).StepType = Tasks.TakeToCarosel
         Experiment(batch).RunSteps(7).StepDuration = TimeSpan.FromHours(6.0)
         Experiment(batch).RunSteps(8).StepType = Tasks.WeighSamplesTakeToCarosel
         Experiment(batch).RunSteps(8).StepDuration = TimeSpan.Zero
         Experiment(batch).RunSteps(9).StepType = Tasks.CloseExperiment
         Experiment(batch).RunSteps(9).StepDuration = TimeSpan.Zero
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function LoadTareExperiment()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub



   Private Sub mnuAddUser_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAddUser.Click
      Dim User As String
      Dim Initials As String
      Dim LISAUserName As String

      Try
         User = InputBox("Enter User's full name.", "User Input")
         If User <> "" Then
            Initials = InputBox("Enter the Initials of this user.  This will be added to data file names.", "Enter Initials").ToUpper
            If Initials = "" Then
               Initials = User.Chars(0)
            End If
            LISAUserName = InputBox("Enter your LISA User Name.", "LISA Name Input")
            FileIO.WriteTextFile(BaseInfoPath & "\UserNames.txt", User & ", " & Initials & ", " & LISAUserName & vbCrLf, True, True)
            LoadUserComboBoxes()
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function mnuAddUser_Click()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Private Sub mnuDeleteUser_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuDeleteUser.Click
      Dim Data As String
      Dim Names() As String
      Dim Output As String
      Dim Pass As Int32
      Dim Namefound As Boolean

      Try
         Names = FileIO.ReadTextFile(BaseInfoPath & "\UserNames.txt").Split(ChrW(10))
         Data = InputBox("Enter the LISA name of the person you want to delete.", "LISA Name Entry")
         Output = ""
         Namefound = False
         For Pass = 0 To Names.Length - 2
            If Not Names(Pass).Contains(Data) Then
               Output += Names(Pass) & vbLf 'restore the LF that you split on
            Else
               Namefound = True
            End If
         Next
         If Namefound = False Then
            MessageBox.Show("The LISA name: " & Data & " was not found.  Nothing was deleted.", "Name Not Found", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
         Else
            FileIO.WriteTextFile(BaseInfoPath & "\UserNames.txt", Output, True, False)
            LoadUserComboBoxes()
         End If
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function mnuDeleteUser_Click()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   Private Sub mnuExp1Abort_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuExp1Abort.Click
      If MessageBox.Show("Are you sure you want to ABORT this experiment?", "Abort Request", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) = Windows.Forms.DialogResult.Yes Then
         Experiment(0).Status = RunState.Done
         Me.mnuExp1Abort.Enabled = False
         My.Settings.Experiment1IsActive = False
         My.Settings.Save()
      End If
   End Sub

   Private Sub mnuExp2Abort_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuExp2Abort.Click
      If MessageBox.Show("Are you sure you want to ABORT this experiment?", "Abort Request", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) = Windows.Forms.DialogResult.Yes Then
         Experiment(1).Status = RunState.Done
         Me.mnuExp2Abort.Enabled = False
         My.Settings.Experiment2IsActive = False
         My.Settings.Save()
      End If
   End Sub

   Private Sub mnuExp3Abort_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuExp3Abort.Click
      If MessageBox.Show("Are you sure you want to ABORT this experiment?", "Abort Request", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) = Windows.Forms.DialogResult.Yes Then
         Experiment(2).Status = RunState.Done
         Me.mnuExp3Abort.Enabled = False
         My.Settings.Experiment3IsActive = False
         My.Settings.Save()
      End If
   End Sub

   Private Sub mnuExp4Abort_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuExp4Abort.Click
      If MessageBox.Show("Are you sure you want to ABORT this experiment?", "Abort Request", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) = Windows.Forms.DialogResult.Yes Then
         Experiment(3).Status = RunState.Done
         Me.mnuExp4Abort.Enabled = False
         My.Settings.Experiment4IsActive = False
         My.Settings.Save()
      End If
   End Sub

   Private Sub mnuExp5Abort_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuExp5Abort.Click
      If MessageBox.Show("Are you sure you want to ABORT this experiment?", "Abort Request", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) = Windows.Forms.DialogResult.Yes Then
         Experiment(4).Status = RunState.Done
         Me.mnuExp5Abort.Enabled = False
         My.Settings.Experiment5IsActive = False
         My.Settings.Save()
      End If
   End Sub



#Region "Functions for Power Failure Recovery"

   Sub SaveExperimentData(ByVal element As Int32)
      Dim Data As String
      Dim Pass As Int32
      Dim InnerPass As Int32

      Try
         Data = Experiment(element).ExperimentType.ToString & ","
         For Pass = 0 To 15
            Data += Experiment(element).RunSteps(Pass).StepDuration.TotalSeconds & ","
            Data += Experiment(element).RunSteps(Pass).StepType.ToString & ","
            Data += Experiment(element).RunSteps(Pass).Temperature.ToString & ","
         Next
         Data += Experiment(element).CurrentRunStep.ToString & ","
         Data += Experiment(element).Status.ToString & ","
         Data += Experiment(element).CurrentTask.ToString & ","
         Data += Experiment(element).NextAccessTime.ToBinary.ToString & ","
         Data += Experiment(element).MyOperator.FullName & ","
         Data += Experiment(element).MyOperator.Initials & ","
         Data += Experiment(element).MyOperator.LISAName & ","
         Data += Experiment(element).TotalSamples.ToString & ","
         For Pass = 1 To 96
            Data += Experiment(element).Filter(Pass).Barcode & ","
            Data += Experiment(element).Filter(Pass).Status.ToString & ","
            Data += Experiment(element).Filter(Pass).TareWeight.ToString & ","
            For InnerPass = 0 To 3
               Data += Experiment(element).Filter(Pass).Weights(InnerPass).ToString & ","
            Next
         Next
         Data += Experiment(element).WeighingStep.ToString & ","
         For Pass = 0 To 3
            Data += Experiment(element).DryingTimes(Pass).Humidity.ToString & ","
            Data += Experiment(element).DryingTimes(Pass).StartTime.ToBinary.ToString & ","
            Data += Experiment(element).DryingTimes(Pass).StopTime.ToBinary.ToString & ","
            Data += Experiment(element).DryingTimes(Pass).Temperature.ToString & ","
         Next
         Data += Experiment(element).Filename & ","
         If Experiment(element).ArmIsInMotion = True Then
            Data += "True,"
         Else
            Data += "False,"
         End If

         'Save FilterBlankData
         For Pass = 0 To 47
            For InnerPass = 0 To 95
               Data += Experiment(element).FilterBlankData(Pass).BlankAppliesToTheseFilters(InnerPass).ToString & ","
            Next
            For InnerPass = 0 To 3
               Data += Experiment(element).FilterBlankData(Pass).FilterBlankLocations(InnerPass).ToString & ","
            Next
         Next
         Data += Experiment(element).CurrentFilter.ToString

         FileIO.WriteTextFile(BaseFilePath & "ExpData" & element.ToString & ".txt", Data, True, False)
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function SaveExperimentData()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub


   Sub RecallExperimentData(ByVal element As Int32)
      Dim Data() As String
      Dim N As Int32
      Dim Pass As Int32
      Dim InnerPass As Int32


      Try
         Data = FileIO.ReadTextFile(BaseFilePath & "ExpData" & element.ToString & ".txt").Split(ChrW(44))
         N = 0
         If Data(N) = "TareExperiment" Then
            Experiment(element).ExperimentType = ExpType.TareExperiment
         ElseIf Data(N) = "WeighingExperiment" Then
            Experiment(element).ExperimentType = ExpType.WeighingExperiment
         ElseIf Data(N) = "UserDefined" Then
            Experiment(element).ExperimentType = ExpType.UserDefined
         End If

         For Pass = 0 To 15
            N += 1
            Experiment(element).RunSteps(Pass).StepDuration = TimeSpan.FromSeconds(CType(Data(N), Double))
            N += 1
            If Data(N) = "WeighSamplesTakeToOven" Then
               Experiment(element).RunSteps(Pass).StepType = Tasks.WeighSamplesTakeToOven
            ElseIf Data(N) = "WeighSamplesTakeToCarosel" Then
               Experiment(element).RunSteps(Pass).StepType = Tasks.WeighSamplesTakeToCarosel
            ElseIf Data(N) = "TakeToCarosel" Then
               Experiment(element).RunSteps(Pass).StepType = Tasks.TakeToCarosel
            ElseIf Data(N) = "TakeToOven" Then
               Experiment(element).RunSteps(Pass).StepType = Tasks.TakeToOven
            ElseIf Data(N) = "CloseExperiment" Then
               Experiment(element).RunSteps(Pass).StepType = Tasks.CloseExperiment
            Else
               Experiment(element).RunSteps(Pass).StepType = Tasks.Delay
            End If
            N += 1
            Experiment(element).RunSteps(Pass).Temperature = CType(Data(N), Double)
         Next

         N += 1
         Experiment(element).CurrentRunStep = CType(Data(N), Int32)

         N += 1
         If Data(N) = "None" Then
            Experiment(element).Status = RunState.None
         ElseIf Data(N) = "Active" Then
            Experiment(element).Status = RunState.Active
         ElseIf Data(N) = "Done" Then
            Experiment(element).Status = RunState.Done
         Else
            Experiment(element).Status = RunState.ErrorState
         End If

         N += 1
         If Data(N) = "WeighSamplesTakeToOven" Then
            Experiment(element).CurrentTask = Tasks.WeighSamplesTakeToOven
         ElseIf Data(N) = "WeighSamplesTakeToCarosel" Then
            Experiment(element).CurrentTask = Tasks.WeighSamplesTakeToCarosel
         ElseIf Data(N) = "TakeToCarosel" Then
            Experiment(element).CurrentTask = Tasks.TakeToCarosel
         ElseIf Data(N) = "TakeToOven" Then
            Experiment(element).CurrentTask = Tasks.TakeToOven
         ElseIf Data(N) = "CloseExperiment" Then
            Experiment(element).CurrentTask = Tasks.CloseExperiment
         Else
            Experiment(element).CurrentTask = Tasks.Delay
         End If

         N += 1
         Experiment(element).NextAccessTime = DateTime.FromBinary(CType(Data(N), Long))

         N += 1
         Experiment(element).MyOperator.FullName = Data(N)
         N += 1
         Experiment(element).MyOperator.Initials = Data(N)
         N += 1
         Experiment(element).MyOperator.LISAName = Data(N)

         N += 1
         Experiment(element).TotalSamples = CType(Data(N), Int32)

         For Pass = 1 To 96
            N += 1
            Experiment(element).Filter(Pass).Barcode = Data(N)
            N += 1
            If Data(N) = "None" Then
               Experiment(element).Filter(Pass).Status = RunState.None
            ElseIf Data(N) = "Active" Then
               Experiment(element).Filter(Pass).Status = RunState.Active
            ElseIf Data(N) = "Done" Then
               Experiment(element).Filter(Pass).Status = RunState.Done
            Else
               Experiment(element).Filter(Pass).Status = RunState.ErrorState
            End If
            N += 1
            Experiment(element).Filter(Pass).TareWeight = CType(Data(N), Double)
            For InnerPass = 0 To 3
               N += 1
               Experiment(element).Filter(Pass).Weights(InnerPass) = CType(Data(N), Double)
            Next
         Next

         N += 1
         Experiment(element).WeighingStep = CType(Data(N), Int32)

         For Pass = 0 To 3
            N += 1
            Experiment(element).DryingTimes(Pass).Humidity = CType(Data(N), Double)
            N += 1
            Experiment(element).DryingTimes(Pass).StartTime = DateTime.FromBinary(CType(Data(N), Long))
            N += 1
            Experiment(element).DryingTimes(Pass).StopTime = DateTime.FromBinary(CType(Data(N), Long))
            N += 1
            Experiment(element).DryingTimes(Pass).Temperature = CType(Data(N), Double)
         Next

         N += 1
         Experiment(element).Filename = Data(N)

         N += 1
         If Data(N) = "True" Then
            Experiment(element).ArmIsInMotion = True
         Else
            Experiment(element).ArmIsInMotion = False
         End If

         'Recall filer blank data
         For Pass = 0 To 47
            For InnerPass = 0 To 95
               N += 1
               Experiment(element).FilterBlankData(Pass).BlankAppliesToTheseFilters(InnerPass) = CType(Data(N), Int32)
            Next
            For InnerPass = 0 To 3
               N += 1
               Experiment(element).FilterBlankData(Pass).FilterBlankLocations(InnerPass) = CType(Data(N), Int32)
            Next
         Next

         N += 1
         Experiment(element).CurrentFilter = CType(Data(N), Int32)
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function RecallExperimentData()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub


   Function CheckForPowerFailureRecovery() As Boolean
      Try
         If My.Settings.NormalProgramExit = False Then    'NormalProgramExit is false if a power failure occured.
            If MessageBox.Show("The system detected a recovering from a power failure.   Click OK to initiate the automatic experiment recovery proceedure.", "Power Failure Recovery", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1) = Windows.Forms.DialogResult.OK Then
               If MessageBox.Show("Do you want to recover the experiments in progress?   If you answer No, then all experiment data will be deleted.", "Automatic Recovery Option", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) = Windows.Forms.DialogResult.No Then
                  If MessageBox.Show("Are you sure you want to abandon all running experiments?", "Think About It", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) = Windows.Forms.DialogResult.Yes Then
                     My.Settings.NormalProgramExit = True
                     My.Settings.Save()
                     Return True
                  End If
               End If
               If MessageBox.Show("Is the arm in a position where is can be safely moved to its home position?", "Save to Home Arm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.No Then
                  MessageBox.Show("This program will exit.   When it does, turn off power to the Epson arm and manually move it to a safe postion.", "Manually Position Arm", MessageBoxButtons.OK, MessageBoxIcon.Information)
                  Me.Close()
               Else
                  Arm.Home()
                  OvenDef.GotoSafeOpenDoorPosition(0)
                  Carosel.ReleaseMotor()

                  If My.Settings.Experiment1IsActive = True Then
                     Me.RecallExperimentData(0)
                     ReinitializeExperiment(0)
                  End If
                  If My.Settings.Experiment2IsActive = True Then
                     Me.RecallExperimentData(1)
                     ReinitializeExperiment(1)
                  End If
                  If My.Settings.Experiment3IsActive = True Then
                     Me.RecallExperimentData(2)
                     ReinitializeExperiment(2)
                  End If
                  If My.Settings.Experiment4IsActive = True Then
                     Me.RecallExperimentData(3)
                     ReinitializeExperiment(3)
                  End If
                  If My.Settings.Experiment5IsActive = True Then
                     Me.RecallExperimentData(4)
                     ReinitializeExperiment(4)
                  End If

                  'check to see if any experiment was in action, that is, the arm was moving.  If so, then complete the step
                  'Test to see if experiment 1 was running
                  If My.Settings.Experiment1IsActive = True Then
                     If Experiment(0).ArmIsInMotion = True Then
                        mnuCarousel.Enabled = False
                        'The arm was in motion either putting filters in the oven, or on the carosel.
                        'There are two possibilities
                        '1. If the current task was TakeToCarosel then the oven door is open, so have the user complete this step manually.
                        '2. If the current task was taking them to the oven, or weighing the filters and returning them ot the carosel then manually place them 
                        'back on the carosel and start the step over again.   The oven door would be open.
                        If Experiment(0).CurrentTask = Tasks.TakeToCarosel Then
                           MessageBox.Show("I'm about to open the door for Oven 1.", "Open Oven Door", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(0).Oven.OpenDoor()
                           MessageBox.Show("For Experiment 1, manually locate filter number " & Experiment(0).CurrentFilter.ToString & _
                                           " and place it on the Carousel now.  When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           GetFilterFromOven(0, Experiment(0).CurrentFilter + 1)
                        ElseIf Experiment(0).CurrentTask = Tasks.TakeToOven Then
                           MessageBox.Show("I'm about to open the door for Oven 1.", "Open Oven Door", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(0).Oven.OpenDoor()
                           MessageBox.Show("For Experiment 1, locate filter " & Experiment(0).CurrentFilter.ToString & _
                                           " and place in back on the Carousel now.   When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           TakeFilterToOven(0, True, Experiment(0).CurrentFilter)
                        ElseIf Experiment(0).CurrentTask = Tasks.WeighSamplesTakeToCarosel Then
                           MessageBox.Show("For Experiment 1, locate filter " & Experiment(0).CurrentFilter.ToString & _
                                           " and place in back on the Carousel now.   When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           WeighFiltersFromCarosel(0, Locations.Carosel, Experiment(0).CurrentFilter)
                        ElseIf Experiment(0).CurrentTask = Tasks.WeighSamplesTakeToOven Then
                           MessageBox.Show("I'm about to open the door for Oven 1.", "Open Oven Door", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(0).Oven.OpenDoor()
                           MessageBox.Show("For Experiment 1, locate filter " & Experiment(0).CurrentFilter.ToString & _
                                           " and place in back on the Carousel now.   When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           WeighFiltersFromCarosel(0, Locations.Oven, Experiment(0).CurrentFilter)
                        Else
                           MessageBox.Show("The Experiment can not be recovered from this error.", "Unrecoverable Experiment", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(0).Status = RunState.None
                        End If
                     End If
                  End If

                  'See if Experiment 2 needs recovery.
                  If My.Settings.Experiment2IsActive = True Then
                     If Experiment(1).ArmIsInMotion = True Then
                        mnuCarousel.Enabled = False
                        'The arm was in motion either putting filters in the oven, or on the carosel.
                        'There are two possibilities
                        '1. If the current task was TakeToCarosel then the oven door is open, so have the user complete this step manually.
                        '2. If the current task was taking them to the oven, or weighing the filters and returning them ot the carosel then manually place them 
                        'back on the carosel and start the step over again.   The oven door would be open.
                        If Experiment(1).CurrentTask = Tasks.TakeToCarosel Then
                           MessageBox.Show("I'm about to open the door for Oven 2.", "Open Oven Door", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(1).Oven.OpenDoor()
                           MessageBox.Show("For Experiment 2, manually locate filter number " & Experiment(1).CurrentFilter.ToString & _
                                           " and place it on the Carousel now.  When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           GetFilterFromOven(1, Experiment(1).CurrentFilter + 1)
                        ElseIf Experiment(1).CurrentTask = Tasks.TakeToOven Then
                           MessageBox.Show("I'm about to open the door for Oven 2.", "Open Oven Door", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(1).Oven.OpenDoor()
                           MessageBox.Show("For Experiment 2, locate filter " & Experiment(1).CurrentFilter.ToString & _
                                           " and place in back on the Carousel now.   When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           TakeFilterToOven(1, True, Experiment(1).CurrentFilter)
                        ElseIf Experiment(1).CurrentTask = Tasks.WeighSamplesTakeToCarosel Then
                           MessageBox.Show("For Experiment 2, locate filter " & Experiment(1).CurrentFilter.ToString & _
                                           " and place in back on the Carousel now.   When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           WeighFiltersFromCarosel(1, Locations.Carosel, Experiment(1).CurrentFilter)
                        ElseIf Experiment(1).CurrentTask = Tasks.WeighSamplesTakeToOven Then
                           MessageBox.Show("I'm about to open the door for Oven 2.", "Open Oven Door", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(1).Oven.OpenDoor()
                           MessageBox.Show("For Experiment 2, locate filter " & Experiment(1).CurrentFilter.ToString & _
                                           " and place in back on the Carousel now.   When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           WeighFiltersFromCarosel(1, Locations.Oven, Experiment(1).CurrentFilter)
                        Else
                           MessageBox.Show("The Experiment can not be recovered from this error.", "Unrecoverable Experiment", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(1).Status = RunState.None
                        End If
                     End If
                  End If

                  'See if Experiment 3 needs recovery.
                  If My.Settings.Experiment3IsActive = True Then
                     If Experiment(2).ArmIsInMotion = True Then
                        mnuCarousel.Enabled = False
                        'The arm was in motion either putting filters in the oven, or on the carosel.
                        'There are two possibilities
                        '1. If the current task was TakeToCarosel then the oven door is open, so have the user complete this step manually.
                        '2. If the current task was taking them to the oven, or weighing the filters and returning them ot the carosel then manually place them 
                        'back on the carosel and start the step over again.   The oven door would be open.
                        If Experiment(2).CurrentTask = Tasks.TakeToCarosel Then
                           MessageBox.Show("I'm about to open the door for Oven 3.", "Open Oven Door", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(2).Oven.OpenDoor()
                           MessageBox.Show("For Experiment 3, manually locate filter number " & Experiment(2).CurrentFilter.ToString & _
                                           " and place it on the Carousel now.  When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           GetFilterFromOven(2, Experiment(2).CurrentFilter + 1)
                        ElseIf Experiment(2).CurrentTask = Tasks.TakeToOven Then
                           MessageBox.Show("I'm about to open the door for Oven 3.", "Open Oven Door", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(2).Oven.OpenDoor()
                           MessageBox.Show("For Experiment 3, locate filter " & Experiment(2).CurrentFilter.ToString & _
                                           " and place in back on the Carousel now.   When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           TakeFilterToOven(2, True, Experiment(2).CurrentFilter)
                        ElseIf Experiment(2).CurrentTask = Tasks.WeighSamplesTakeToCarosel Then
                           MessageBox.Show("For Experiment 3, locate filter " & Experiment(2).CurrentFilter.ToString & _
                                           " and place in back on the Carousel now.   When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           WeighFiltersFromCarosel(2, Locations.Carosel, Experiment(2).CurrentFilter)
                        ElseIf Experiment(2).CurrentTask = Tasks.WeighSamplesTakeToOven Then
                           MessageBox.Show("I'm about to open the door for Oven 3.", "Open Oven Door", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(2).Oven.OpenDoor()
                           MessageBox.Show("For Experiment 3, locate filter " & Experiment(2).CurrentFilter.ToString & _
                                           " and place in back on the Carousel now.   When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           WeighFiltersFromCarosel(2, Locations.Oven, Experiment(2).CurrentFilter)
                        Else
                           MessageBox.Show("The Experiment can not be recovered from this error.", "Unrecoverable Experiment", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(2).Status = RunState.None
                        End If
                     End If
                  End If

                  'See if Experiment 4 needs recovery.
                  If My.Settings.Experiment4IsActive = True Then
                     If Experiment(3).ArmIsInMotion = True Then
                        mnuCarousel.Enabled = False
                        'The arm was in motion either putting filters in the oven, or on the carosel.
                        'There are two possibilities
                        '1. If the current task was TakeToCarosel then the oven door is open, so have the user complete this step manually.
                        '2. If the current task was taking them to the oven, or weighing the filters and returning them ot the carosel then manually place them 
                        'back on the carosel and start the step over again.   The oven door would be open.
                        If Experiment(3).CurrentTask = Tasks.TakeToCarosel Then
                           MessageBox.Show("I'm about to open the door for Oven 4.", "Open Oven Door", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(3).Oven.OpenDoor()
                           MessageBox.Show("For Experiment 4, manually locate filter number " & Experiment(3).CurrentFilter.ToString & _
                                           " and place it on the Carousel now.  When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           GetFilterFromOven(3, Experiment(3).CurrentFilter + 1)
                        ElseIf Experiment(3).CurrentTask = Tasks.TakeToOven Then
                           MessageBox.Show("I'm about to open the door for Oven 4.", "Open Oven Door", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(3).Oven.OpenDoor()
                           MessageBox.Show("For Experiment 4, locate filter " & Experiment(3).CurrentFilter.ToString & _
                                           " and place in back on the Carousel now.   When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           TakeFilterToOven(3, True, Experiment(3).CurrentFilter)
                        ElseIf Experiment(3).CurrentTask = Tasks.WeighSamplesTakeToCarosel Then
                           MessageBox.Show("For Experiment 4, locate filter " & Experiment(3).CurrentFilter.ToString & _
                                           " and place in back on the Carousel now.   When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           WeighFiltersFromCarosel(3, Locations.Carosel, Experiment(3).CurrentFilter)
                        ElseIf Experiment(3).CurrentTask = Tasks.WeighSamplesTakeToOven Then
                           MessageBox.Show("I'm about to open the door for Oven 4.", "Open Oven Door", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(3).Oven.OpenDoor()
                           MessageBox.Show("For Experiment 4, locate filter " & Experiment(3).CurrentFilter.ToString & _
                                           " and place in back on the Carousel now.   When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           WeighFiltersFromCarosel(3, Locations.Oven, Experiment(3).CurrentFilter)
                        Else
                           MessageBox.Show("The Experiment can not be recovered from this error.", "Unrecoverable Experiment", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(3).Status = RunState.None
                        End If
                     End If
                  End If

                  'See if Experiment 5 needs recovery.
                  If My.Settings.Experiment5IsActive = True Then
                     If Experiment(4).ArmIsInMotion = True Then
                        mnuCarousel.Enabled = False
                        'The arm was in motion either putting filters in the oven, or on the carosel.
                        'There are two possibilities
                        '1. If the current task was TakeToCarosel then the oven door is open, so have the user complete this step manually.
                        '2. If the current task was taking them to the oven, or weighing the filters and returning them ot the carosel then manually place them 
                        'back on the carosel and start the step over again.   The oven door would be open.
                        If Experiment(4).CurrentTask = Tasks.TakeToCarosel Then
                           MessageBox.Show("I'm about to open the door for Oven 5.", "Open Oven Door", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(4).Oven.OpenDoor()
                           MessageBox.Show("For Experiment 5, manually locate filter number " & Experiment(4).CurrentFilter.ToString & _
                                           " and place it on the Carousel now.  When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           GetFilterFromOven(4, Experiment(4).CurrentFilter + 1)
                        ElseIf Experiment(4).CurrentTask = Tasks.TakeToOven Then
                           MessageBox.Show("I'm about to open the door for Oven 5.", "Open Oven Door", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(4).Oven.OpenDoor()
                           MessageBox.Show("For Experiment 5, locate filter " & Experiment(4).CurrentFilter.ToString & _
                                           " and place in back on the Carousel now.   When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           TakeFilterToOven(4, True, Experiment(4).CurrentFilter)
                        ElseIf Experiment(4).CurrentTask = Tasks.WeighSamplesTakeToCarosel Then
                           MessageBox.Show("For Experiment 5, locate filter " & Experiment(4).CurrentFilter.ToString & _
                                           " and place in back on the Carousel now.   When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           WeighFiltersFromCarosel(4, Locations.Carosel, Experiment(4).CurrentFilter)
                        ElseIf Experiment(4).CurrentTask = Tasks.WeighSamplesTakeToOven Then
                           MessageBox.Show("I'm about to open the door for Oven 5.", "Open Oven Door", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(4).Oven.OpenDoor()
                           MessageBox.Show("For Experiment 5, locate filter " & Experiment(4).CurrentFilter.ToString & _
                                           " and place in back on the Carousel now.   When done, click OK.", "Manual Step Recovery", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           WeighFiltersFromCarosel(4, Locations.Oven, Experiment(4).CurrentFilter)
                        Else
                           MessageBox.Show("The Experiment can not be recovered from this error.", "Unrecoverable Experiment", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                           Experiment(4).Status = RunState.None
                        End If
                     End If
                  End If
               End If
            Else
               My.Settings.NormalProgramExit = True
               My.Settings.Save()
            End If
         End If
         Return My.Settings.NormalProgramExit

      Catch ex As Exception
         MessageBox.Show("An exception was caught in function mnuDeleteUser_Click()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
         Return True
      End Try
   End Function


   Sub ReinitializeExperiment(ByVal exp As Int32)
      Dim Pass As Int32

      Try
         Select Case exp
            Case 0
               If Experiment(0).ExperimentType.ToString.Contains("Tare") Then
                  Me.lblDisplay1Experiment.Text = "Experiment: Tare Experiment"
               ElseIf Experiment(0).ExperimentType.ToString.Contains("Weighing") Then
                  Me.lblDisplay1Experiment.Text = "Experiment: Weigh Samples Experiment"
               ElseIf Experiment(0).ExperimentType.ToString.Contains("User") Then
                  Me.lblDisplay1Experiment.Text = "Experiment: User Defined Experiment"
               End If

               Experiment(0).Oven = Oven1
               Experiment(0).MyDGV = dgvExp1
               'Format the DGV
               Me.dgvExp1.RowHeadersWidth = 60
               Me.dgvExp1.Rows.Add(Experiment(0).TotalSamples)
               For Pass = 0 To Experiment(0).TotalSamples - 1
                  Me.dgvExp1.Rows(Pass).HeaderCell.Value = (Pass + 1).ToString
                  Me.dgvExp1.Rows(Pass).Cells(0).Value = Experiment(0).Filter(Pass + 1).Barcode
                  Me.dgvExp1.Rows(Pass).Cells(1).Value = Experiment(0).Filter(Pass + 1).TareWeight
                  Me.dgvExp1.Rows(Pass).Cells(2).Value = Experiment(0).Filter(Pass + 1).Weights(0)
                  Me.dgvExp1.Rows(Pass).Cells(3).Value = Experiment(0).Filter(Pass + 1).Weights(1)
                  Me.dgvExp1.Rows(Pass).Cells(4).Value = Experiment(0).Filter(Pass + 1).Weights(2)
                  Me.dgvExp1.Rows(Pass).Cells(5).Value = Experiment(0).Filter(Pass + 1).Weights(3)
                  Me.dgvExp1.Rows(Pass).Cells(6).Value = Experiment(0).Filter(Pass + 1).Status.ToString
               Next
               DGVSupport.SizeTable(dgvExp1)
               Me.gbxExp1.Enabled = False
               Me.lblExp1Status.Text = "Active"
               Me.lblExp1Status.BackColor = Color.Yellow

               Me.mnuExp1Abort.Enabled = True
               Me.mnuManualOven1.Enabled = True
               Experiment(0).Status = RunState.Active

            Case 1
               If Experiment(1).ExperimentType.ToString.Contains("Tare") Then
                  Me.lblDisplay2Experiment.Text = "Experiment: Tare Experiment"
               ElseIf Experiment(1).ExperimentType.ToString.Contains("Weighing") Then
                  Me.lblDisplay2Experiment.Text = "Experiment: Weigh Samples Experiment"
               ElseIf Experiment(1).ExperimentType.ToString.Contains("User") Then
                  Me.lblDisplay2Experiment.Text = "Experiment: User Defined Experiment"
               End If

               Experiment(1).Oven = Oven2
               Experiment(1).MyDGV = dgvExp2
               'Format the DGV
               Me.dgvExp2.RowHeadersWidth = 60
               Me.dgvExp2.Rows.Add(Experiment(1).TotalSamples)
               For Pass = 0 To Experiment(1).TotalSamples - 1
                  Me.dgvExp2.Rows(Pass).HeaderCell.Value = (Pass + 1).ToString
                  Me.dgvExp2.Rows(Pass).Cells(0).Value = Experiment(1).Filter(Pass + 1).Barcode
                  Me.dgvExp2.Rows(Pass).Cells(1).Value = Experiment(1).Filter(Pass + 1).TareWeight
                  Me.dgvExp2.Rows(Pass).Cells(2).Value = Experiment(1).Filter(Pass + 1).Weights(0)
                  Me.dgvExp2.Rows(Pass).Cells(3).Value = Experiment(1).Filter(Pass + 1).Weights(1)
                  Me.dgvExp2.Rows(Pass).Cells(4).Value = Experiment(1).Filter(Pass + 1).Weights(2)
                  Me.dgvExp2.Rows(Pass).Cells(5).Value = Experiment(1).Filter(Pass + 1).Weights(3)
                  Me.dgvExp2.Rows(Pass).Cells(6).Value = Experiment(1).Filter(Pass + 1).Status.ToString
               Next
               DGVSupport.SizeTable(dgvExp2)
               Me.gbxExp2.Enabled = False
               Me.lblExp2Status.Text = "Active"
               Me.lblExp2Status.BackColor = Color.Yellow

               Me.mnuExp2Abort.Enabled = True
               Me.mnuManualOven2.Enabled = True
               Experiment(1).Status = RunState.Active

            Case 2
               If Experiment(2).ExperimentType.ToString.Contains("Tare") Then
                  Me.lblDisplay3Experiment.Text = "Experiment: Tare Experiment"
               ElseIf Experiment(2).ExperimentType.ToString.Contains("Weighing") Then
                  Me.lblDisplay3Experiment.Text = "Experiment: Weigh Samples Experiment"
               ElseIf Experiment(2).ExperimentType.ToString.Contains("User") Then
                  Me.lblDisplay3Experiment.Text = "Experiment: User Defined Experiment"
               End If

               Experiment(2).Oven = Oven3
               Experiment(2).MyDGV = dgvExp3
               'Format the DGV
               Me.dgvExp3.RowHeadersWidth = 60
               Me.dgvExp3.Rows.Add(Experiment(2).TotalSamples)
               For Pass = 0 To Experiment(2).TotalSamples - 1
                  Me.dgvExp3.Rows(Pass).HeaderCell.Value = (Pass + 1).ToString
                  Me.dgvExp3.Rows(Pass).Cells(0).Value = Experiment(2).Filter(Pass + 1).Barcode
                  Me.dgvExp3.Rows(Pass).Cells(1).Value = Experiment(2).Filter(Pass + 1).TareWeight
                  Me.dgvExp3.Rows(Pass).Cells(2).Value = Experiment(2).Filter(Pass + 1).Weights(0)
                  Me.dgvExp3.Rows(Pass).Cells(3).Value = Experiment(2).Filter(Pass + 1).Weights(1)
                  Me.dgvExp3.Rows(Pass).Cells(4).Value = Experiment(2).Filter(Pass + 1).Weights(2)
                  Me.dgvExp3.Rows(Pass).Cells(5).Value = Experiment(2).Filter(Pass + 1).Weights(3)
                  Me.dgvExp3.Rows(Pass).Cells(6).Value = Experiment(2).Filter(Pass + 1).Status.ToString
               Next
               DGVSupport.SizeTable(dgvExp3)
               Me.gbxExp3.Enabled = False
               Me.lblExp3Status.Text = "Active"
               Me.lblExp3Status.BackColor = Color.Yellow

               Me.mnuExp3Abort.Enabled = True
               Me.mnuManualOven3.Enabled = True
               Experiment(2).Status = RunState.Active

            Case 3
               If Experiment(3).ExperimentType.ToString.Contains("Tare") Then
                  Me.lblDisplay4Experiment.Text = "Experiment: Tare Experiment"
               ElseIf Experiment(3).ExperimentType.ToString.Contains("Weighing") Then
                  Me.lblDisplay4Experiment.Text = "Experiment: Weigh Samples Experiment"
               ElseIf Experiment(3).ExperimentType.ToString.Contains("User") Then
                  Me.lblDisplay4Experiment.Text = "Experiment: User Defined Experiment"
               End If

               Experiment(3).Oven = Oven4
               Experiment(3).MyDGV = dgvExp4
               'Format the DGV
               Me.dgvExp4.RowHeadersWidth = 60
               Me.dgvExp4.Rows.Add(Experiment(3).TotalSamples)
               For Pass = 0 To Experiment(3).TotalSamples - 1
                  Me.dgvExp4.Rows(Pass).HeaderCell.Value = (Pass + 1).ToString
                  Me.dgvExp4.Rows(Pass).Cells(0).Value = Experiment(3).Filter(Pass + 1).Barcode
                  Me.dgvExp4.Rows(Pass).Cells(1).Value = Experiment(3).Filter(Pass + 1).TareWeight
                  Me.dgvExp4.Rows(Pass).Cells(2).Value = Experiment(3).Filter(Pass + 1).Weights(0)
                  Me.dgvExp4.Rows(Pass).Cells(3).Value = Experiment(3).Filter(Pass + 1).Weights(1)
                  Me.dgvExp4.Rows(Pass).Cells(4).Value = Experiment(3).Filter(Pass + 1).Weights(2)
                  Me.dgvExp4.Rows(Pass).Cells(5).Value = Experiment(3).Filter(Pass + 1).Weights(3)
                  Me.dgvExp4.Rows(Pass).Cells(6).Value = Experiment(3).Filter(Pass + 1).Status.ToString
               Next
               DGVSupport.SizeTable(dgvExp4)
               Me.gbxExp4.Enabled = False
               Me.lblExp4Status.Text = "Active"
               Me.lblExp4Status.BackColor = Color.Yellow

               Me.mnuExp4Abort.Enabled = True
               Me.mnuManualOven4.Enabled = True
               Experiment(3).Status = RunState.Active

            Case 4
               If Experiment(4).ExperimentType.ToString.Contains("Tare") Then
                  Me.lblDisplay5Experiment.Text = "Experiment: Tare Experiment"
               ElseIf Experiment(4).ExperimentType.ToString.Contains("Weighing") Then
                  Me.lblDisplay5Experiment.Text = "Experiment: Weigh Samples Experiment"
               ElseIf Experiment(4).ExperimentType.ToString.Contains("User") Then
                  Me.lblDisplay5Experiment.Text = "Experiment: User Defined Experiment"
               End If

               Experiment(4).Oven = Oven5
               Experiment(4).MyDGV = dgvExp5
               'Format the DGV
               Me.dgvExp5.RowHeadersWidth = 60
               Me.dgvExp5.Rows.Add(Experiment(4).TotalSamples)
               For Pass = 0 To Experiment(4).TotalSamples - 1
                  Me.dgvExp5.Rows(Pass).HeaderCell.Value = (Pass + 1).ToString
                  Me.dgvExp5.Rows(Pass).Cells(0).Value = Experiment(4).Filter(Pass + 1).Barcode
                  Me.dgvExp5.Rows(Pass).Cells(1).Value = Experiment(4).Filter(Pass + 1).TareWeight
                  Me.dgvExp5.Rows(Pass).Cells(2).Value = Experiment(4).Filter(Pass + 1).Weights(0)
                  Me.dgvExp5.Rows(Pass).Cells(3).Value = Experiment(4).Filter(Pass + 1).Weights(1)
                  Me.dgvExp5.Rows(Pass).Cells(4).Value = Experiment(4).Filter(Pass + 1).Weights(2)
                  Me.dgvExp5.Rows(Pass).Cells(5).Value = Experiment(4).Filter(Pass + 1).Weights(3)
                  Me.dgvExp5.Rows(Pass).Cells(6).Value = Experiment(4).Filter(Pass + 1).Status.ToString
               Next
               DGVSupport.SizeTable(dgvExp5)
               Me.gbxExp5.Enabled = False
               Me.lblExp5Status.Text = "Active"
               Me.lblExp5Status.BackColor = Color.Yellow

               Me.mnuExp5Abort.Enabled = True
               Me.mnuManualOven5.Enabled = True
               Experiment(4).Status = RunState.Active
         End Select

         Me.lblMasterTimer.Visible = True
      Catch ex As Exception
         MessageBox.Show("An exception was caught in function ReinitializeExperiment()." & vbCrLf & "The exception was: " & ex.Message _
                            & vbCrLf & "The inner exception was: " & ex.InnerException.ToString _
                            & vbCrLf & "The stack trace was: " & ex.StackTrace, _
                            "Program Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

#End Region


   Private Sub mnuPause_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuPause.Click
      Me.PauseRequested = True
   End Sub


   Private Sub mnuPosition_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuPosition1.Click, _
   mnuPosition2.Click, mnuPosition3.Click, mnuPosition4.Click, mnuPosition5.Click
      'This Carosel menu should only be active at times when it is safe to move the carosel.

      'This sub positions the carousel at the front of the deck for the user to remove and access
      Dim Position As Int32
      Dim Rack As Int32

      'Bob, this was replaced on 6/2013    Many chanages were makde in here, even undocumented changes.
      'If MasterTimer.Enabled = True Then
      '   MasterTimer.Stop()
      '   TimerIsActive = True
      'End If

      Rack = CType(CType(sender, ToolStripMenuItem).Tag, Int32)   'The tag holds the rack position
      Position = 2000 + ((Rack - 1) * 8000)
      Carosel.Move(Position)
   End Sub

   Private Sub btnClear_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClear.Click
      Me.rtbMessages.Text = ""
   End Sub


   Public Sub ErrorDump(ByVal ex As String, ByVal func As String, Optional ByRef dumpData As String = "None")
      'This writes error information to disk if an exception occures
      Try
         Dim SW As New StreamWriter("C:\J-KEM ErrorLog.txt")
         SW.NewLine = vbCrLf
         SW.WriteLine("An error occured.  The exception was: " & ex & vbCrLf & func & vbCrLf & dumpData)
         SW.Flush()
         SW.Dispose()
         MessageBox.Show("An error log was created for this run.  The file name is 'J-KEM ErrorLog.txt', please noitify Scott Jones and email the file to J-KEM.", "Error Log", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1)
      Catch exx As Exception
      End Try
   End Sub


   Private Sub mnuManualOven_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuManualOven1.Click, mnuManualOven2.Click, mnuManualOven3.Click, mnuManualOven4.Click, mnuManualOven5.Click
      Me.ManualDoorOpenRequest = Me.TabControl1.SelectedIndex
      My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Beep)
      If ManualDoorOpenRequest = 1 Then
         lblManOven1.Visible = True
      ElseIf ManualDoorOpenRequest = 2 Then
         lblManOven2.Visible = True
      ElseIf ManualDoorOpenRequest = 3 Then
         lblManOven3.Visible = True
      ElseIf ManualDoorOpenRequest = 4 Then
         lblManOven4.Visible = True
      ElseIf ManualDoorOpenRequest = 5 Then
         lblManOven5.Visible = True
      End If
      mnuManualOven1.Enabled = False
      mnuManualOven2.Enabled = False
      mnuManualOven3.Enabled = False
      mnuManualOven4.Enabled = False
      mnuManualOven5.Enabled = False
   End Sub


   Sub ServiceManualDoorOpenRequest()
      'The user has requested to manually open an oven.
      OvenDef.GotoSafeOpenDoorPosition(ManualDoorOpenRequest - 1)
      MessageBox.Show("The system is about to open the door for Oven " & ManualDoorOpenRequest.ToString & ".  Click OK to continue.", "Manual Operation", MessageBoxButtons.OK, MessageBoxIcon.Hand)
      lblManOven1.Visible = False
      lblManOven2.Visible = False
      lblManOven3.Visible = False
      lblManOven4.Visible = False
      lblManOven5.Visible = False
      Experiment(ManualDoorOpenRequest - 1).Oven.OpenDoor()
      MessageBox.Show("Click OK to close the oven door and continue with the experiment.", "Oven Door Manually Open", MessageBoxButtons.OK, MessageBoxIcon.Hand)
      MessageBox.Show("This is a safety check.  Click OK again to close the oven door.", "Door Close Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Hand)
      Experiment(ManualDoorOpenRequest - 1).Oven.CloseDoor()
      If Experiment(0).Status = RunState.Active Then
         mnuManualOven1.Enabled = True
      End If
      If Experiment(1).Status = RunState.Active Then
         mnuManualOven2.Enabled = True
      End If
      If Experiment(2).Status = RunState.Active Then
         mnuManualOven3.Enabled = True
      End If
      If Experiment(3).Status = RunState.Active Then
         mnuManualOven4.Enabled = True
      End If
      If Experiment(4).Status = RunState.Active Then
         mnuManualOven5.Enabled = True
      End If
      ManualDoorOpenRequest = 0
   End Sub

   Private Sub mnuTestDevices_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuTestDevices.Click
      Dim Password As String

      Password = InputBox("This is a protected screen.  Enter password.", "Protected Screen")
      If Password.ToUpper = "PASSWORD" Then
         Dim FT As New frmTesting
         FT.ShowDialog()
         FT.Dispose()
      End If
   End Sub

   Private Sub mnuBalanceOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuBalanceOpen.Click
      Balance.OpenDoor()
   End Sub

   Private Sub mnuBalanceClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuBalanceClose.Click
      Balance.CloseDoor()
   End Sub

   Private Sub mnuBalanceUnlock_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuBalanceUnlock.Click
      Balance.UnlockKeypad()
   End Sub


   Private Sub ConfigureEmailServicesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ConfigureEmailServicesToolStripMenuItem.Click
      Dim FS As New frmSMTPService()
      FS.ShowDialog()
      FS.Dispose()
   End Sub

   Private Sub mnuUnlockForms_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuUnlockForms.Click
      Dim Password As String

      Password = InputBox("These controls are setup and configuration controls only and should be used with J-KEM assistance.  To unlock these controls, enter the password.", "Unlock Configuration Controls")
      If Password.ToUpper = "PASSWORD" Then
         mnuTestDevices.Enabled = True
         mnuCarosel.Enabled = True
         mnuEmulator.Enabled = True
      End If
   End Sub


End Class

