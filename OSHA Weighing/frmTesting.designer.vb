<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmTesting
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
      Me.GroupBox1 = New System.Windows.Forms.GroupBox
      Me.btnHomeCarosel = New System.Windows.Forms.Button
      Me.btnCaroselDriver = New System.Windows.Forms.Button
      Me.Label1 = New System.Windows.Forms.Label
      Me.txbCaroselReply = New System.Windows.Forms.TextBox
      Me.GroupBox2 = New System.Windows.Forms.GroupBox
      Me.btnBarCodeCommunications = New System.Windows.Forms.Button
      Me.Label2 = New System.Windows.Forms.Label
      Me.txbBarCodeReply = New System.Windows.Forms.TextBox
      Me.txbUSBOven = New System.Windows.Forms.TextBox
      Me.Label3 = New System.Windows.Forms.Label
      Me.btnUSBCommsOven = New System.Windows.Forms.Button
      Me.txbReadTempOven = New System.Windows.Forms.TextBox
      Me.btnReadTempOven = New System.Windows.Forms.Button
      Me.txbReadPressOven = New System.Windows.Forms.TextBox
      Me.btnReadPressOven = New System.Windows.Forms.Button
      Me.gbxOvens = New System.Windows.Forms.GroupBox
      Me.GroupBox3 = New System.Windows.Forms.GroupBox
      Me.btnPressRelieveValveOffOven = New System.Windows.Forms.Button
      Me.btnPressReleaseValveOnOven = New System.Windows.Forms.Button
      Me.btnVacValveOffOven = New System.Windows.Forms.Button
      Me.btnVacValveOnOven = New System.Windows.Forms.Button
      Me.btnCloseDoorOven = New System.Windows.Forms.Button
      Me.btnOpenDoorOven = New System.Windows.Forms.Button
      Me.GroupBox7 = New System.Windows.Forms.GroupBox
      Me.GroupBox5 = New System.Windows.Forms.GroupBox
      Me.rdoOven5 = New System.Windows.Forms.RadioButton
      Me.rdoOven4 = New System.Windows.Forms.RadioButton
      Me.rdoOven3 = New System.Windows.Forms.RadioButton
      Me.rdoOven2 = New System.Windows.Forms.RadioButton
      Me.rdoOven1 = New System.Windows.Forms.RadioButton
      Me.GroupBox4 = New System.Windows.Forms.GroupBox
      Me.Label7 = New System.Windows.Forms.Label
      Me.btnCloseDoor = New System.Windows.Forms.Button
      Me.btnOpenDoor = New System.Windows.Forms.Button
      Me.txbWeight = New System.Windows.Forms.TextBox
      Me.btnGetWeight = New System.Windows.Forms.Button
      Me.btnTestBalanceComms = New System.Windows.Forms.Button
      Me.Label6 = New System.Windows.Forms.Label
      Me.txbBalanceReply = New System.Windows.Forms.TextBox
      Me.Label8 = New System.Windows.Forms.Label
      Me.Label9 = New System.Windows.Forms.Label
      Me.Label10 = New System.Windows.Forms.Label
      Me.Label11 = New System.Windows.Forms.Label
      Me.Label12 = New System.Windows.Forms.Label
      Me.btnTestSensors = New System.Windows.Forms.Button
      Me.lblOpenSensor = New System.Windows.Forms.Label
      Me.lblCloseSensor = New System.Windows.Forms.Label
      Me.GroupBox1.SuspendLayout()
      Me.GroupBox2.SuspendLayout()
      Me.gbxOvens.SuspendLayout()
      Me.GroupBox3.SuspendLayout()
      Me.GroupBox7.SuspendLayout()
      Me.GroupBox5.SuspendLayout()
      Me.GroupBox4.SuspendLayout()
      Me.SuspendLayout()
      '
      'GroupBox1
      '
      Me.GroupBox1.BackColor = System.Drawing.Color.Thistle
      Me.GroupBox1.Controls.Add(Me.btnHomeCarosel)
      Me.GroupBox1.Controls.Add(Me.btnCaroselDriver)
      Me.GroupBox1.Controls.Add(Me.Label1)
      Me.GroupBox1.Controls.Add(Me.txbCaroselReply)
      Me.GroupBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
      Me.GroupBox1.Name = "GroupBox1"
      Me.GroupBox1.Size = New System.Drawing.Size(552, 64)
      Me.GroupBox1.TabIndex = 0
      Me.GroupBox1.TabStop = False
      Me.GroupBox1.Text = "Carosel"
      '
      'btnHomeCarosel
      '
      Me.btnHomeCarosel.Location = New System.Drawing.Point(396, 28)
      Me.btnHomeCarosel.Name = "btnHomeCarosel"
      Me.btnHomeCarosel.Size = New System.Drawing.Size(104, 20)
      Me.btnHomeCarosel.TabIndex = 4
      Me.btnHomeCarosel.Text = "Home Carosel"
      Me.btnHomeCarosel.UseVisualStyleBackColor = True
      '
      'btnCaroselDriver
      '
      Me.btnCaroselDriver.Location = New System.Drawing.Point(16, 28)
      Me.btnCaroselDriver.Name = "btnCaroselDriver"
      Me.btnCaroselDriver.Size = New System.Drawing.Size(180, 20)
      Me.btnCaroselDriver.TabIndex = 3
      Me.btnCaroselDriver.Text = "Test Driver Communications"
      Me.btnCaroselDriver.UseVisualStyleBackColor = True
      '
      'Label1
      '
      Me.Label1.AutoSize = True
      Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label1.Location = New System.Drawing.Point(272, 32)
      Me.Label1.Name = "Label1"
      Me.Label1.Size = New System.Drawing.Size(95, 13)
      Me.Label1.TabIndex = 2
      Me.Label1.Text = "Expected reply :  1"
      '
      'txbCaroselReply
      '
      Me.txbCaroselReply.Location = New System.Drawing.Point(204, 28)
      Me.txbCaroselReply.Name = "txbCaroselReply"
      Me.txbCaroselReply.Size = New System.Drawing.Size(56, 20)
      Me.txbCaroselReply.TabIndex = 1
      Me.txbCaroselReply.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
      '
      'GroupBox2
      '
      Me.GroupBox2.BackColor = System.Drawing.Color.Thistle
      Me.GroupBox2.Controls.Add(Me.btnBarCodeCommunications)
      Me.GroupBox2.Controls.Add(Me.Label2)
      Me.GroupBox2.Controls.Add(Me.txbBarCodeReply)
      Me.GroupBox2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.GroupBox2.Location = New System.Drawing.Point(580, 12)
      Me.GroupBox2.Name = "GroupBox2"
      Me.GroupBox2.Size = New System.Drawing.Size(392, 64)
      Me.GroupBox2.TabIndex = 1
      Me.GroupBox2.TabStop = False
      Me.GroupBox2.Text = "Bar Code Reader"
      '
      'btnBarCodeCommunications
      '
      Me.btnBarCodeCommunications.Location = New System.Drawing.Point(16, 32)
      Me.btnBarCodeCommunications.Name = "btnBarCodeCommunications"
      Me.btnBarCodeCommunications.Size = New System.Drawing.Size(140, 20)
      Me.btnBarCodeCommunications.TabIndex = 6
      Me.btnBarCodeCommunications.Text = "Test Communications"
      Me.btnBarCodeCommunications.UseVisualStyleBackColor = True
      '
      'Label2
      '
      Me.Label2.AutoSize = True
      Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label2.Location = New System.Drawing.Point(244, 36)
      Me.Label2.Name = "Label2"
      Me.Label2.Size = New System.Drawing.Size(138, 13)
      Me.Label2.TabIndex = 5
      Me.Label2.Text = "Expected reply :  <K221, 3>"
      '
      'txbBarCodeReply
      '
      Me.txbBarCodeReply.Location = New System.Drawing.Point(160, 32)
      Me.txbBarCodeReply.Name = "txbBarCodeReply"
      Me.txbBarCodeReply.Size = New System.Drawing.Size(80, 20)
      Me.txbBarCodeReply.TabIndex = 4
      '
      'txbUSBOven
      '
      Me.txbUSBOven.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.txbUSBOven.Location = New System.Drawing.Point(244, 28)
      Me.txbUSBOven.Name = "txbUSBOven"
      Me.txbUSBOven.Size = New System.Drawing.Size(184, 20)
      Me.txbUSBOven.TabIndex = 4
      '
      'Label3
      '
      Me.Label3.AutoSize = True
      Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label3.Location = New System.Drawing.Point(240, 52)
      Me.Label3.Name = "Label3"
      Me.Label3.Size = New System.Drawing.Size(179, 13)
      Me.Label3.TabIndex = 5
      Me.Label3.Text = "Expected reply : Board serial number"
      '
      'btnUSBCommsOven
      '
      Me.btnUSBCommsOven.Location = New System.Drawing.Point(16, 28)
      Me.btnUSBCommsOven.Name = "btnUSBCommsOven"
      Me.btnUSBCommsOven.Size = New System.Drawing.Size(216, 20)
      Me.btnUSBCommsOven.TabIndex = 6
      Me.btnUSBCommsOven.Text = "Test USB Board Communicaitons"
      Me.btnUSBCommsOven.UseVisualStyleBackColor = True
      '
      'txbReadTempOven
      '
      Me.txbReadTempOven.Location = New System.Drawing.Point(168, 56)
      Me.txbReadTempOven.Name = "txbReadTempOven"
      Me.txbReadTempOven.Size = New System.Drawing.Size(56, 20)
      Me.txbReadTempOven.TabIndex = 7
      '
      'btnReadTempOven
      '
      Me.btnReadTempOven.Location = New System.Drawing.Point(16, 56)
      Me.btnReadTempOven.Name = "btnReadTempOven"
      Me.btnReadTempOven.Size = New System.Drawing.Size(140, 20)
      Me.btnReadTempOven.TabIndex = 8
      Me.btnReadTempOven.Text = "Read Temperature"
      Me.btnReadTempOven.UseVisualStyleBackColor = True
      '
      'txbReadPressOven
      '
      Me.txbReadPressOven.Location = New System.Drawing.Point(168, 84)
      Me.txbReadPressOven.Name = "txbReadPressOven"
      Me.txbReadPressOven.Size = New System.Drawing.Size(56, 20)
      Me.txbReadPressOven.TabIndex = 9
      '
      'btnReadPressOven
      '
      Me.btnReadPressOven.Location = New System.Drawing.Point(16, 84)
      Me.btnReadPressOven.Name = "btnReadPressOven"
      Me.btnReadPressOven.Size = New System.Drawing.Size(140, 20)
      Me.btnReadPressOven.TabIndex = 10
      Me.btnReadPressOven.Text = "Read Pressure"
      Me.btnReadPressOven.UseVisualStyleBackColor = True
      '
      'gbxOvens
      '
      Me.gbxOvens.BackColor = System.Drawing.Color.MediumPurple
      Me.gbxOvens.Controls.Add(Me.Label12)
      Me.gbxOvens.Controls.Add(Me.Label11)
      Me.gbxOvens.Controls.Add(Me.Label10)
      Me.gbxOvens.Controls.Add(Me.Label9)
      Me.gbxOvens.Controls.Add(Me.Label8)
      Me.gbxOvens.Controls.Add(Me.GroupBox3)
      Me.gbxOvens.Controls.Add(Me.btnPressRelieveValveOffOven)
      Me.gbxOvens.Controls.Add(Me.btnPressReleaseValveOnOven)
      Me.gbxOvens.Controls.Add(Me.btnVacValveOffOven)
      Me.gbxOvens.Controls.Add(Me.btnVacValveOnOven)
      Me.gbxOvens.Controls.Add(Me.btnCloseDoorOven)
      Me.gbxOvens.Controls.Add(Me.btnOpenDoorOven)
      Me.gbxOvens.Controls.Add(Me.btnReadPressOven)
      Me.gbxOvens.Controls.Add(Me.txbReadPressOven)
      Me.gbxOvens.Controls.Add(Me.btnReadTempOven)
      Me.gbxOvens.Controls.Add(Me.txbReadTempOven)
      Me.gbxOvens.Controls.Add(Me.btnUSBCommsOven)
      Me.gbxOvens.Controls.Add(Me.Label3)
      Me.gbxOvens.Controls.Add(Me.txbUSBOven)
      Me.gbxOvens.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.gbxOvens.ForeColor = System.Drawing.Color.Black
      Me.gbxOvens.Location = New System.Drawing.Point(88, 24)
      Me.gbxOvens.Name = "gbxOvens"
      Me.gbxOvens.Size = New System.Drawing.Size(436, 296)
      Me.gbxOvens.TabIndex = 2
      Me.gbxOvens.TabStop = False
      Me.gbxOvens.Text = "Oven One Controls"
      '
      'GroupBox3
      '
      Me.GroupBox3.BackColor = System.Drawing.Color.PaleGreen
      Me.GroupBox3.Controls.Add(Me.lblCloseSensor)
      Me.GroupBox3.Controls.Add(Me.lblOpenSensor)
      Me.GroupBox3.Controls.Add(Me.btnTestSensors)
      Me.GroupBox3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.GroupBox3.Location = New System.Drawing.Point(16, 212)
      Me.GroupBox3.Name = "GroupBox3"
      Me.GroupBox3.Size = New System.Drawing.Size(404, 68)
      Me.GroupBox3.TabIndex = 17
      Me.GroupBox3.TabStop = False
      Me.GroupBox3.Text = "Door Proximity Sensors"
      '
      'btnPressRelieveValveOffOven
      '
      Me.btnPressRelieveValveOffOven.Location = New System.Drawing.Point(204, 168)
      Me.btnPressRelieveValveOffOven.Name = "btnPressRelieveValveOffOven"
      Me.btnPressRelieveValveOffOven.Size = New System.Drawing.Size(176, 36)
      Me.btnPressRelieveValveOffOven.TabIndex = 16
      Me.btnPressRelieveValveOffOven.Text = "Deenergize Pressure Release Valve"
      Me.btnPressRelieveValveOffOven.UseVisualStyleBackColor = True
      '
      'btnPressReleaseValveOnOven
      '
      Me.btnPressReleaseValveOnOven.Location = New System.Drawing.Point(16, 168)
      Me.btnPressReleaseValveOnOven.Name = "btnPressReleaseValveOnOven"
      Me.btnPressReleaseValveOnOven.Size = New System.Drawing.Size(176, 36)
      Me.btnPressReleaseValveOnOven.TabIndex = 15
      Me.btnPressReleaseValveOnOven.Text = "Energize Pressure Release Valve"
      Me.btnPressReleaseValveOnOven.UseVisualStyleBackColor = True
      '
      'btnVacValveOffOven
      '
      Me.btnVacValveOffOven.Location = New System.Drawing.Point(204, 140)
      Me.btnVacValveOffOven.Name = "btnVacValveOffOven"
      Me.btnVacValveOffOven.Size = New System.Drawing.Size(176, 20)
      Me.btnVacValveOffOven.TabIndex = 14
      Me.btnVacValveOffOven.Text = "Deenergize Vacuum Valve"
      Me.btnVacValveOffOven.UseVisualStyleBackColor = True
      '
      'btnVacValveOnOven
      '
      Me.btnVacValveOnOven.Location = New System.Drawing.Point(16, 140)
      Me.btnVacValveOnOven.Name = "btnVacValveOnOven"
      Me.btnVacValveOnOven.Size = New System.Drawing.Size(176, 20)
      Me.btnVacValveOnOven.TabIndex = 13
      Me.btnVacValveOnOven.Text = "Energize Vacuum Valve"
      Me.btnVacValveOnOven.UseVisualStyleBackColor = True
      '
      'btnCloseDoorOven
      '
      Me.btnCloseDoorOven.Location = New System.Drawing.Point(108, 112)
      Me.btnCloseDoorOven.Name = "btnCloseDoorOven"
      Me.btnCloseDoorOven.Size = New System.Drawing.Size(84, 20)
      Me.btnCloseDoorOven.TabIndex = 12
      Me.btnCloseDoorOven.Text = "Close Door"
      Me.btnCloseDoorOven.UseVisualStyleBackColor = True
      '
      'btnOpenDoorOven
      '
      Me.btnOpenDoorOven.Location = New System.Drawing.Point(16, 112)
      Me.btnOpenDoorOven.Name = "btnOpenDoorOven"
      Me.btnOpenDoorOven.Size = New System.Drawing.Size(84, 20)
      Me.btnOpenDoorOven.TabIndex = 11
      Me.btnOpenDoorOven.Text = "Open Door"
      Me.btnOpenDoorOven.UseVisualStyleBackColor = True
      '
      'GroupBox7
      '
      Me.GroupBox7.BackColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(255, Byte), Integer))
      Me.GroupBox7.Controls.Add(Me.GroupBox5)
      Me.GroupBox7.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.GroupBox7.ForeColor = System.Drawing.Color.Black
      Me.GroupBox7.Location = New System.Drawing.Point(12, 92)
      Me.GroupBox7.Name = "GroupBox7"
      Me.GroupBox7.Size = New System.Drawing.Size(552, 360)
      Me.GroupBox7.TabIndex = 3
      Me.GroupBox7.TabStop = False
      Me.GroupBox7.Text = "Oven Controls"
      '
      'GroupBox5
      '
      Me.GroupBox5.BackColor = System.Drawing.Color.Thistle
      Me.GroupBox5.Controls.Add(Me.rdoOven5)
      Me.GroupBox5.Controls.Add(Me.gbxOvens)
      Me.GroupBox5.Controls.Add(Me.rdoOven4)
      Me.GroupBox5.Controls.Add(Me.rdoOven3)
      Me.GroupBox5.Controls.Add(Me.rdoOven2)
      Me.GroupBox5.Controls.Add(Me.rdoOven1)
      Me.GroupBox5.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.GroupBox5.ForeColor = System.Drawing.Color.Black
      Me.GroupBox5.Location = New System.Drawing.Point(12, 24)
      Me.GroupBox5.Name = "GroupBox5"
      Me.GroupBox5.Size = New System.Drawing.Size(532, 328)
      Me.GroupBox5.TabIndex = 3
      Me.GroupBox5.TabStop = False
      Me.GroupBox5.Text = "Oven Selection"
      '
      'rdoOven5
      '
      Me.rdoOven5.AutoSize = True
      Me.rdoOven5.Location = New System.Drawing.Point(12, 104)
      Me.rdoOven5.Name = "rdoOven5"
      Me.rdoOven5.Size = New System.Drawing.Size(66, 17)
      Me.rdoOven5.TabIndex = 4
      Me.rdoOven5.Text = "Oven 5"
      Me.rdoOven5.UseVisualStyleBackColor = True
      '
      'rdoOven4
      '
      Me.rdoOven4.AutoSize = True
      Me.rdoOven4.Location = New System.Drawing.Point(12, 84)
      Me.rdoOven4.Name = "rdoOven4"
      Me.rdoOven4.Size = New System.Drawing.Size(66, 17)
      Me.rdoOven4.TabIndex = 3
      Me.rdoOven4.Text = "Oven 4"
      Me.rdoOven4.UseVisualStyleBackColor = True
      '
      'rdoOven3
      '
      Me.rdoOven3.AutoSize = True
      Me.rdoOven3.Location = New System.Drawing.Point(12, 64)
      Me.rdoOven3.Name = "rdoOven3"
      Me.rdoOven3.Size = New System.Drawing.Size(66, 17)
      Me.rdoOven3.TabIndex = 2
      Me.rdoOven3.Text = "Oven 3"
      Me.rdoOven3.UseVisualStyleBackColor = True
      '
      'rdoOven2
      '
      Me.rdoOven2.AutoSize = True
      Me.rdoOven2.Location = New System.Drawing.Point(12, 44)
      Me.rdoOven2.Name = "rdoOven2"
      Me.rdoOven2.Size = New System.Drawing.Size(66, 17)
      Me.rdoOven2.TabIndex = 1
      Me.rdoOven2.Text = "Oven 2"
      Me.rdoOven2.UseVisualStyleBackColor = True
      '
      'rdoOven1
      '
      Me.rdoOven1.AutoSize = True
      Me.rdoOven1.Checked = True
      Me.rdoOven1.Location = New System.Drawing.Point(12, 24)
      Me.rdoOven1.Name = "rdoOven1"
      Me.rdoOven1.Size = New System.Drawing.Size(66, 17)
      Me.rdoOven1.TabIndex = 0
      Me.rdoOven1.TabStop = True
      Me.rdoOven1.Text = "Oven 1"
      Me.rdoOven1.UseVisualStyleBackColor = True
      '
      'GroupBox4
      '
      Me.GroupBox4.BackColor = System.Drawing.Color.Thistle
      Me.GroupBox4.Controls.Add(Me.Label7)
      Me.GroupBox4.Controls.Add(Me.btnCloseDoor)
      Me.GroupBox4.Controls.Add(Me.btnOpenDoor)
      Me.GroupBox4.Controls.Add(Me.txbWeight)
      Me.GroupBox4.Controls.Add(Me.btnGetWeight)
      Me.GroupBox4.Controls.Add(Me.btnTestBalanceComms)
      Me.GroupBox4.Controls.Add(Me.Label6)
      Me.GroupBox4.Controls.Add(Me.txbBalanceReply)
      Me.GroupBox4.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.GroupBox4.Location = New System.Drawing.Point(580, 92)
      Me.GroupBox4.Name = "GroupBox4"
      Me.GroupBox4.Size = New System.Drawing.Size(392, 148)
      Me.GroupBox4.TabIndex = 4
      Me.GroupBox4.TabStop = False
      Me.GroupBox4.Text = "Balance Controls"
      '
      'Label7
      '
      Me.Label7.AutoSize = True
      Me.Label7.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label7.Location = New System.Drawing.Point(292, 40)
      Me.Label7.Name = "Label7"
      Me.Label7.Size = New System.Drawing.Size(87, 13)
      Me.Label7.TabIndex = 11
      Me.Label7.Text = "A ""1123481116"""
      '
      'btnCloseDoor
      '
      Me.btnCloseDoor.Location = New System.Drawing.Point(12, 120)
      Me.btnCloseDoor.Name = "btnCloseDoor"
      Me.btnCloseDoor.Size = New System.Drawing.Size(96, 20)
      Me.btnCloseDoor.TabIndex = 10
      Me.btnCloseDoor.Text = "Close Door"
      Me.btnCloseDoor.UseVisualStyleBackColor = True
      '
      'btnOpenDoor
      '
      Me.btnOpenDoor.Location = New System.Drawing.Point(12, 92)
      Me.btnOpenDoor.Name = "btnOpenDoor"
      Me.btnOpenDoor.Size = New System.Drawing.Size(96, 20)
      Me.btnOpenDoor.TabIndex = 9
      Me.btnOpenDoor.Text = "Open Door"
      Me.btnOpenDoor.UseVisualStyleBackColor = True
      '
      'txbWeight
      '
      Me.txbWeight.Location = New System.Drawing.Point(124, 64)
      Me.txbWeight.Name = "txbWeight"
      Me.txbWeight.Size = New System.Drawing.Size(100, 20)
      Me.txbWeight.TabIndex = 8
      '
      'btnGetWeight
      '
      Me.btnGetWeight.Location = New System.Drawing.Point(12, 64)
      Me.btnGetWeight.Name = "btnGetWeight"
      Me.btnGetWeight.Size = New System.Drawing.Size(96, 20)
      Me.btnGetWeight.TabIndex = 7
      Me.btnGetWeight.Text = "Get Weight"
      Me.btnGetWeight.UseVisualStyleBackColor = True
      '
      'btnTestBalanceComms
      '
      Me.btnTestBalanceComms.Location = New System.Drawing.Point(12, 32)
      Me.btnTestBalanceComms.Name = "btnTestBalanceComms"
      Me.btnTestBalanceComms.Size = New System.Drawing.Size(136, 20)
      Me.btnTestBalanceComms.TabIndex = 6
      Me.btnTestBalanceComms.Text = "Test Communications"
      Me.btnTestBalanceComms.UseVisualStyleBackColor = True
      '
      'Label6
      '
      Me.Label6.AutoSize = True
      Me.Label6.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label6.Location = New System.Drawing.Point(300, 24)
      Me.Label6.Name = "Label6"
      Me.Label6.Size = New System.Drawing.Size(86, 13)
      Me.Label6.TabIndex = 5
      Me.Label6.Text = "Expected reply : "
      '
      'txbBalanceReply
      '
      Me.txbBalanceReply.Location = New System.Drawing.Point(152, 32)
      Me.txbBalanceReply.Name = "txbBalanceReply"
      Me.txbBalanceReply.Size = New System.Drawing.Size(136, 20)
      Me.txbBalanceReply.TabIndex = 4
      '
      'Label8
      '
      Me.Label8.AutoSize = True
      Me.Label8.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label8.Location = New System.Drawing.Point(288, 68)
      Me.Label8.Name = "Label8"
      Me.Label8.Size = New System.Drawing.Size(113, 13)
      Me.Label8.TabIndex = 18
      Me.Label8.Text = "Oven 1, Address= 103"
      '
      'Label9
      '
      Me.Label9.AutoSize = True
      Me.Label9.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label9.Location = New System.Drawing.Point(288, 80)
      Me.Label9.Name = "Label9"
      Me.Label9.Size = New System.Drawing.Size(113, 13)
      Me.Label9.TabIndex = 19
      Me.Label9.Text = "Oven 2, Address= 104"
      '
      'Label10
      '
      Me.Label10.AutoSize = True
      Me.Label10.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label10.Location = New System.Drawing.Point(288, 92)
      Me.Label10.Name = "Label10"
      Me.Label10.Size = New System.Drawing.Size(113, 13)
      Me.Label10.TabIndex = 20
      Me.Label10.Text = "Oven 3, Address= 100"
      '
      'Label11
      '
      Me.Label11.AutoSize = True
      Me.Label11.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label11.Location = New System.Drawing.Point(288, 104)
      Me.Label11.Name = "Label11"
      Me.Label11.Size = New System.Drawing.Size(113, 13)
      Me.Label11.TabIndex = 21
      Me.Label11.Text = "Oven 4, Address= 102"
      '
      'Label12
      '
      Me.Label12.AutoSize = True
      Me.Label12.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label12.Location = New System.Drawing.Point(288, 116)
      Me.Label12.Name = "Label12"
      Me.Label12.Size = New System.Drawing.Size(113, 13)
      Me.Label12.TabIndex = 22
      Me.Label12.Text = "Oven 5, Address= 101"
      '
      'btnTestSensors
      '
      Me.btnTestSensors.Location = New System.Drawing.Point(16, 28)
      Me.btnTestSensors.Name = "btnTestSensors"
      Me.btnTestSensors.Size = New System.Drawing.Size(140, 20)
      Me.btnTestSensors.TabIndex = 12
      Me.btnTestSensors.Text = "Test Door Sensors"
      Me.btnTestSensors.UseVisualStyleBackColor = True
      '
      'lblOpenSensor
      '
      Me.lblOpenSensor.AutoSize = True
      Me.lblOpenSensor.Location = New System.Drawing.Point(180, 24)
      Me.lblOpenSensor.Name = "lblOpenSensor"
      Me.lblOpenSensor.Size = New System.Drawing.Size(121, 13)
      Me.lblOpenSensor.TabIndex = 13
      Me.lblOpenSensor.Text = "Door OPEN sensor :"
      '
      'lblCloseSensor
      '
      Me.lblCloseSensor.AutoSize = True
      Me.lblCloseSensor.Location = New System.Drawing.Point(180, 44)
      Me.lblCloseSensor.Name = "lblCloseSensor"
      Me.lblCloseSensor.Size = New System.Drawing.Size(136, 13)
      Me.lblCloseSensor.TabIndex = 14
      Me.lblCloseSensor.Text = "Door CLOSED sensor :"
      '
      'frmTesting
      '
      Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
      Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
      Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
      Me.ClientSize = New System.Drawing.Size(985, 506)
      Me.Controls.Add(Me.GroupBox4)
      Me.Controls.Add(Me.GroupBox7)
      Me.Controls.Add(Me.GroupBox2)
      Me.Controls.Add(Me.GroupBox1)
      Me.Name = "frmTesting"
      Me.Text = "Module Test Form"
      Me.GroupBox1.ResumeLayout(False)
      Me.GroupBox1.PerformLayout()
      Me.GroupBox2.ResumeLayout(False)
      Me.GroupBox2.PerformLayout()
      Me.gbxOvens.ResumeLayout(False)
      Me.gbxOvens.PerformLayout()
      Me.GroupBox3.ResumeLayout(False)
      Me.GroupBox3.PerformLayout()
      Me.GroupBox7.ResumeLayout(False)
      Me.GroupBox5.ResumeLayout(False)
      Me.GroupBox5.PerformLayout()
      Me.GroupBox4.ResumeLayout(False)
      Me.GroupBox4.PerformLayout()
      Me.ResumeLayout(False)

   End Sub
   Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
   Friend WithEvents Label1 As System.Windows.Forms.Label
   Friend WithEvents txbCaroselReply As System.Windows.Forms.TextBox
   Friend WithEvents btnHomeCarosel As System.Windows.Forms.Button
   Friend WithEvents btnCaroselDriver As System.Windows.Forms.Button
   Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
   Friend WithEvents btnBarCodeCommunications As System.Windows.Forms.Button
   Friend WithEvents Label2 As System.Windows.Forms.Label
   Friend WithEvents txbBarCodeReply As System.Windows.Forms.TextBox
   Friend WithEvents txbUSBOven As System.Windows.Forms.TextBox
   Friend WithEvents Label3 As System.Windows.Forms.Label
   Friend WithEvents btnUSBCommsOven As System.Windows.Forms.Button
   Friend WithEvents txbReadTempOven As System.Windows.Forms.TextBox
   Friend WithEvents btnReadTempOven As System.Windows.Forms.Button
   Friend WithEvents txbReadPressOven As System.Windows.Forms.TextBox
   Friend WithEvents btnReadPressOven As System.Windows.Forms.Button
   Friend WithEvents gbxOvens As System.Windows.Forms.GroupBox
   Friend WithEvents btnCloseDoorOven As System.Windows.Forms.Button
   Friend WithEvents btnOpenDoorOven As System.Windows.Forms.Button
   Friend WithEvents btnPressRelieveValveOffOven As System.Windows.Forms.Button
   Friend WithEvents btnPressReleaseValveOnOven As System.Windows.Forms.Button
   Friend WithEvents btnVacValveOffOven As System.Windows.Forms.Button
   Friend WithEvents btnVacValveOnOven As System.Windows.Forms.Button
   Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
   Friend WithEvents GroupBox7 As System.Windows.Forms.GroupBox
   Friend WithEvents GroupBox5 As System.Windows.Forms.GroupBox
   Friend WithEvents rdoOven1 As System.Windows.Forms.RadioButton
   Friend WithEvents rdoOven5 As System.Windows.Forms.RadioButton
   Friend WithEvents rdoOven4 As System.Windows.Forms.RadioButton
   Friend WithEvents rdoOven3 As System.Windows.Forms.RadioButton
   Friend WithEvents rdoOven2 As System.Windows.Forms.RadioButton
   Friend WithEvents GroupBox4 As System.Windows.Forms.GroupBox
   Friend WithEvents btnGetWeight As System.Windows.Forms.Button
   Friend WithEvents btnTestBalanceComms As System.Windows.Forms.Button
   Friend WithEvents Label6 As System.Windows.Forms.Label
   Friend WithEvents txbBalanceReply As System.Windows.Forms.TextBox
   Friend WithEvents btnCloseDoor As System.Windows.Forms.Button
   Friend WithEvents btnOpenDoor As System.Windows.Forms.Button
   Friend WithEvents txbWeight As System.Windows.Forms.TextBox
   Friend WithEvents Label7 As System.Windows.Forms.Label
   Friend WithEvents Label10 As System.Windows.Forms.Label
   Friend WithEvents Label9 As System.Windows.Forms.Label
   Friend WithEvents Label8 As System.Windows.Forms.Label
   Friend WithEvents Label12 As System.Windows.Forms.Label
   Friend WithEvents Label11 As System.Windows.Forms.Label
   Friend WithEvents btnTestSensors As System.Windows.Forms.Button
   Friend WithEvents lblCloseSensor As System.Windows.Forms.Label
   Friend WithEvents lblOpenSensor As System.Windows.Forms.Label
End Class
