<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSerialCommunications
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
      Me.Label3 = New System.Windows.Forms.Label
      Me.txbAddChar = New System.Windows.Forms.TextBox
      Me.btnAddChar = New System.Windows.Forms.Button
      Me.GroupBox2 = New System.Windows.Forms.GroupBox
      Me.GroupBox4 = New System.Windows.Forms.GroupBox
      Me.rdoHandshakeXonXoff = New System.Windows.Forms.RadioButton
      Me.rdoHandshakeRTS = New System.Windows.Forms.RadioButton
      Me.rdoHandshakeNone = New System.Windows.Forms.RadioButton
      Me.GroupBox3 = New System.Windows.Forms.GroupBox
      Me.rdoDataBits8 = New System.Windows.Forms.RadioButton
      Me.rdoDatabits7 = New System.Windows.Forms.RadioButton
      Me.GroupBox1 = New System.Windows.Forms.GroupBox
      Me.rdoXmitCr = New System.Windows.Forms.RadioButton
      Me.txbXmitEOL = New System.Windows.Forms.TextBox
      Me.rdoXmitCrLf = New System.Windows.Forms.RadioButton
      Me.rdoXmitDec = New System.Windows.Forms.RadioButton
      Me.Label1 = New System.Windows.Forms.Label
      Me.TextBox1 = New System.Windows.Forms.TextBox
      Me.btnCommand8 = New System.Windows.Forms.Button
      Me.btnCommand7 = New System.Windows.Forms.Button
      Me.btnCommand6 = New System.Windows.Forms.Button
      Me.btnCommand5 = New System.Windows.Forms.Button
      Me.btnCommand4 = New System.Windows.Forms.Button
      Me.btnCommand3 = New System.Windows.Forms.Button
      Me.btnCommand2 = New System.Windows.Forms.Button
      Me.btnCommand1 = New System.Windows.Forms.Button
      Me.txbReply = New System.Windows.Forms.TextBox
      Me.Label2 = New System.Windows.Forms.Label
      Me.btnSendCommand = New System.Windows.Forms.Button
      Me.txbCommand = New System.Windows.Forms.TextBox
      Me.ListBox1 = New System.Windows.Forms.ListBox
      Me.GroupBox5 = New System.Windows.Forms.GroupBox
      Me.btnTestBalance = New System.Windows.Forms.Button
      Me.btnTestBarcode = New System.Windows.Forms.Button
      Me.btnTestCarosel = New System.Windows.Forms.Button
      Me.btnTestOven = New System.Windows.Forms.Button
      Me.MenuStrip1 = New System.Windows.Forms.MenuStrip
      Me.DataToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
      Me.mnuUpdatePorts = New System.Windows.Forms.ToolStripMenuItem
      Me.lblClosingPort = New System.Windows.Forms.Label
      Me.gbxCommandDef = New System.Windows.Forms.GroupBox
      Me.Label4 = New System.Windows.Forms.Label
      Me.txbDefChar = New System.Windows.Forms.TextBox
      Me.btnAddDefChar = New System.Windows.Forms.Button
      Me.btnDefine = New System.Windows.Forms.Button
      Me.txbCommandBuilder = New System.Windows.Forms.TextBox
      Me.dgvPorts = New System.Windows.Forms.DataGridView
      Me.Column1 = New System.Windows.Forms.DataGridViewTextBoxColumn
      Me.Column2 = New System.Windows.Forms.DataGridViewTextBoxColumn
      Me.btnSavePorts = New System.Windows.Forms.Button
      Me.GroupBox2.SuspendLayout()
      Me.GroupBox4.SuspendLayout()
      Me.GroupBox3.SuspendLayout()
      Me.GroupBox1.SuspendLayout()
      Me.GroupBox5.SuspendLayout()
      Me.MenuStrip1.SuspendLayout()
      Me.gbxCommandDef.SuspendLayout()
      CType(Me.dgvPorts, System.ComponentModel.ISupportInitialize).BeginInit()
      Me.SuspendLayout()
      '
      'Label3
      '
      Me.Label3.AutoSize = True
      Me.Label3.Location = New System.Drawing.Point(596, 228)
      Me.Label3.Name = "Label3"
      Me.Label3.Size = New System.Drawing.Size(74, 13)
      Me.Label3.TabIndex = 46
      Me.Label3.Text = "Decimal value"
      '
      'txbAddChar
      '
      Me.txbAddChar.Location = New System.Drawing.Point(552, 222)
      Me.txbAddChar.Name = "txbAddChar"
      Me.txbAddChar.Size = New System.Drawing.Size(36, 20)
      Me.txbAddChar.TabIndex = 45
      '
      'btnAddChar
      '
      Me.btnAddChar.Location = New System.Drawing.Point(320, 220)
      Me.btnAddChar.Name = "btnAddChar"
      Me.btnAddChar.Size = New System.Drawing.Size(224, 24)
      Me.btnAddChar.TabIndex = 44
      Me.btnAddChar.Text = "Add this charactor to the output command"
      Me.btnAddChar.UseVisualStyleBackColor = True
      '
      'GroupBox2
      '
      Me.GroupBox2.BackColor = System.Drawing.Color.WhiteSmoke
      Me.GroupBox2.Controls.Add(Me.GroupBox4)
      Me.GroupBox2.Controls.Add(Me.GroupBox3)
      Me.GroupBox2.Controls.Add(Me.GroupBox1)
      Me.GroupBox2.Controls.Add(Me.Label1)
      Me.GroupBox2.Controls.Add(Me.TextBox1)
      Me.GroupBox2.Location = New System.Drawing.Point(160, 72)
      Me.GroupBox2.Name = "GroupBox2"
      Me.GroupBox2.Size = New System.Drawing.Size(588, 124)
      Me.GroupBox2.TabIndex = 43
      Me.GroupBox2.TabStop = False
      Me.GroupBox2.Text = "Communications Parameters"
      '
      'GroupBox4
      '
      Me.GroupBox4.BackColor = System.Drawing.Color.Lavender
      Me.GroupBox4.Controls.Add(Me.rdoHandshakeXonXoff)
      Me.GroupBox4.Controls.Add(Me.rdoHandshakeRTS)
      Me.GroupBox4.Controls.Add(Me.rdoHandshakeNone)
      Me.GroupBox4.Location = New System.Drawing.Point(380, 24)
      Me.GroupBox4.Name = "GroupBox4"
      Me.GroupBox4.Size = New System.Drawing.Size(180, 80)
      Me.GroupBox4.TabIndex = 24
      Me.GroupBox4.TabStop = False
      Me.GroupBox4.Text = "HandShaking"
      '
      'rdoHandshakeXonXoff
      '
      Me.rdoHandshakeXonXoff.AutoSize = True
      Me.rdoHandshakeXonXoff.Location = New System.Drawing.Point(8, 52)
      Me.rdoHandshakeXonXoff.Name = "rdoHandshakeXonXoff"
      Me.rdoHandshakeXonXoff.Size = New System.Drawing.Size(68, 17)
      Me.rdoHandshakeXonXoff.TabIndex = 2
      Me.rdoHandshakeXonXoff.TabStop = True
      Me.rdoHandshakeXonXoff.Text = "Xon/Xoff"
      Me.rdoHandshakeXonXoff.UseVisualStyleBackColor = True
      '
      'rdoHandshakeRTS
      '
      Me.rdoHandshakeRTS.AutoSize = True
      Me.rdoHandshakeRTS.Location = New System.Drawing.Point(76, 28)
      Me.rdoHandshakeRTS.Name = "rdoHandshakeRTS"
      Me.rdoHandshakeRTS.Size = New System.Drawing.Size(47, 17)
      Me.rdoHandshakeRTS.TabIndex = 1
      Me.rdoHandshakeRTS.TabStop = True
      Me.rdoHandshakeRTS.Text = "RTS"
      Me.rdoHandshakeRTS.UseVisualStyleBackColor = True
      '
      'rdoHandshakeNone
      '
      Me.rdoHandshakeNone.AutoSize = True
      Me.rdoHandshakeNone.Checked = True
      Me.rdoHandshakeNone.Location = New System.Drawing.Point(8, 28)
      Me.rdoHandshakeNone.Name = "rdoHandshakeNone"
      Me.rdoHandshakeNone.Size = New System.Drawing.Size(51, 17)
      Me.rdoHandshakeNone.TabIndex = 0
      Me.rdoHandshakeNone.TabStop = True
      Me.rdoHandshakeNone.Text = "None"
      Me.rdoHandshakeNone.UseVisualStyleBackColor = True
      '
      'GroupBox3
      '
      Me.GroupBox3.BackColor = System.Drawing.Color.Lavender
      Me.GroupBox3.Controls.Add(Me.rdoDataBits8)
      Me.GroupBox3.Controls.Add(Me.rdoDatabits7)
      Me.GroupBox3.Location = New System.Drawing.Point(16, 56)
      Me.GroupBox3.Name = "GroupBox3"
      Me.GroupBox3.Size = New System.Drawing.Size(108, 48)
      Me.GroupBox3.TabIndex = 23
      Me.GroupBox3.TabStop = False
      Me.GroupBox3.Text = "Data Bits"
      '
      'rdoDataBits8
      '
      Me.rdoDataBits8.AutoSize = True
      Me.rdoDataBits8.Checked = True
      Me.rdoDataBits8.Location = New System.Drawing.Point(60, 20)
      Me.rdoDataBits8.Name = "rdoDataBits8"
      Me.rdoDataBits8.Size = New System.Drawing.Size(31, 17)
      Me.rdoDataBits8.TabIndex = 1
      Me.rdoDataBits8.TabStop = True
      Me.rdoDataBits8.Text = "8"
      Me.rdoDataBits8.UseVisualStyleBackColor = True
      '
      'rdoDatabits7
      '
      Me.rdoDatabits7.AutoSize = True
      Me.rdoDatabits7.Location = New System.Drawing.Point(12, 20)
      Me.rdoDatabits7.Name = "rdoDatabits7"
      Me.rdoDatabits7.Size = New System.Drawing.Size(31, 17)
      Me.rdoDatabits7.TabIndex = 0
      Me.rdoDatabits7.TabStop = True
      Me.rdoDatabits7.Text = "7"
      Me.rdoDatabits7.UseVisualStyleBackColor = True
      '
      'GroupBox1
      '
      Me.GroupBox1.BackColor = System.Drawing.Color.Lavender
      Me.GroupBox1.Controls.Add(Me.rdoXmitCr)
      Me.GroupBox1.Controls.Add(Me.txbXmitEOL)
      Me.GroupBox1.Controls.Add(Me.rdoXmitCrLf)
      Me.GroupBox1.Controls.Add(Me.rdoXmitDec)
      Me.GroupBox1.Location = New System.Drawing.Point(176, 24)
      Me.GroupBox1.Name = "GroupBox1"
      Me.GroupBox1.Size = New System.Drawing.Size(180, 72)
      Me.GroupBox1.TabIndex = 22
      Me.GroupBox1.TabStop = False
      Me.GroupBox1.Text = "Xmit End of Line Charactor"
      '
      'rdoXmitCr
      '
      Me.rdoXmitCr.AutoSize = True
      Me.rdoXmitCr.Checked = True
      Me.rdoXmitCr.Location = New System.Drawing.Point(12, 24)
      Me.rdoXmitCr.Name = "rdoXmitCr"
      Me.rdoXmitCr.Size = New System.Drawing.Size(35, 17)
      Me.rdoXmitCr.TabIndex = 17
      Me.rdoXmitCr.TabStop = True
      Me.rdoXmitCr.Text = "Cr"
      Me.rdoXmitCr.UseVisualStyleBackColor = True
      '
      'txbXmitEOL
      '
      Me.txbXmitEOL.Location = New System.Drawing.Point(116, 48)
      Me.txbXmitEOL.Name = "txbXmitEOL"
      Me.txbXmitEOL.Size = New System.Drawing.Size(52, 20)
      Me.txbXmitEOL.TabIndex = 21
      '
      'rdoXmitCrLf
      '
      Me.rdoXmitCrLf.AutoSize = True
      Me.rdoXmitCrLf.Location = New System.Drawing.Point(84, 24)
      Me.rdoXmitCrLf.Name = "rdoXmitCrLf"
      Me.rdoXmitCrLf.Size = New System.Drawing.Size(44, 17)
      Me.rdoXmitCrLf.TabIndex = 19
      Me.rdoXmitCrLf.Text = "CrLf"
      Me.rdoXmitCrLf.UseVisualStyleBackColor = True
      '
      'rdoXmitDec
      '
      Me.rdoXmitDec.AutoSize = True
      Me.rdoXmitDec.Location = New System.Drawing.Point(12, 48)
      Me.rdoXmitDec.Name = "rdoXmitDec"
      Me.rdoXmitDec.Size = New System.Drawing.Size(98, 17)
      Me.rdoXmitDec.TabIndex = 20
      Me.rdoXmitDec.Text = "Decimal value :"
      Me.rdoXmitDec.UseVisualStyleBackColor = True
      '
      'Label1
      '
      Me.Label1.AutoSize = True
      Me.Label1.Location = New System.Drawing.Point(16, 28)
      Me.Label1.Name = "Label1"
      Me.Label1.Size = New System.Drawing.Size(58, 13)
      Me.Label1.TabIndex = 2
      Me.Label1.Text = "Baud Rate"
      '
      'TextBox1
      '
      Me.TextBox1.Location = New System.Drawing.Point(84, 24)
      Me.TextBox1.Name = "TextBox1"
      Me.TextBox1.Size = New System.Drawing.Size(60, 20)
      Me.TextBox1.TabIndex = 3
      Me.TextBox1.Text = "9600"
      '
      'btnCommand8
      '
      Me.btnCommand8.Location = New System.Drawing.Point(772, 324)
      Me.btnCommand8.Name = "btnCommand8"
      Me.btnCommand8.Size = New System.Drawing.Size(84, 24)
      Me.btnCommand8.TabIndex = 42
      Me.btnCommand8.Text = "None"
      Me.btnCommand8.UseVisualStyleBackColor = True
      '
      'btnCommand7
      '
      Me.btnCommand7.Location = New System.Drawing.Point(684, 324)
      Me.btnCommand7.Name = "btnCommand7"
      Me.btnCommand7.Size = New System.Drawing.Size(84, 24)
      Me.btnCommand7.TabIndex = 41
      Me.btnCommand7.Text = "None"
      Me.btnCommand7.UseVisualStyleBackColor = True
      '
      'btnCommand6
      '
      Me.btnCommand6.Location = New System.Drawing.Point(596, 324)
      Me.btnCommand6.Name = "btnCommand6"
      Me.btnCommand6.Size = New System.Drawing.Size(84, 24)
      Me.btnCommand6.TabIndex = 40
      Me.btnCommand6.Text = "None"
      Me.btnCommand6.UseVisualStyleBackColor = True
      '
      'btnCommand5
      '
      Me.btnCommand5.Location = New System.Drawing.Point(508, 324)
      Me.btnCommand5.Name = "btnCommand5"
      Me.btnCommand5.Size = New System.Drawing.Size(84, 24)
      Me.btnCommand5.TabIndex = 39
      Me.btnCommand5.Text = "None"
      Me.btnCommand5.UseVisualStyleBackColor = True
      '
      'btnCommand4
      '
      Me.btnCommand4.Location = New System.Drawing.Point(420, 324)
      Me.btnCommand4.Name = "btnCommand4"
      Me.btnCommand4.Size = New System.Drawing.Size(84, 24)
      Me.btnCommand4.TabIndex = 38
      Me.btnCommand4.Text = "None"
      Me.btnCommand4.UseVisualStyleBackColor = True
      '
      'btnCommand3
      '
      Me.btnCommand3.Location = New System.Drawing.Point(332, 324)
      Me.btnCommand3.Name = "btnCommand3"
      Me.btnCommand3.Size = New System.Drawing.Size(84, 24)
      Me.btnCommand3.TabIndex = 37
      Me.btnCommand3.Text = "None"
      Me.btnCommand3.UseVisualStyleBackColor = True
      '
      'btnCommand2
      '
      Me.btnCommand2.Location = New System.Drawing.Point(244, 324)
      Me.btnCommand2.Name = "btnCommand2"
      Me.btnCommand2.Size = New System.Drawing.Size(84, 24)
      Me.btnCommand2.TabIndex = 36
      Me.btnCommand2.Text = "None"
      Me.btnCommand2.UseVisualStyleBackColor = True
      '
      'btnCommand1
      '
      Me.btnCommand1.Location = New System.Drawing.Point(156, 324)
      Me.btnCommand1.Name = "btnCommand1"
      Me.btnCommand1.Size = New System.Drawing.Size(84, 24)
      Me.btnCommand1.TabIndex = 35
      Me.btnCommand1.Text = "None"
      Me.btnCommand1.UseVisualStyleBackColor = True
      '
      'txbReply
      '
      Me.txbReply.Location = New System.Drawing.Point(320, 284)
      Me.txbReply.Name = "txbReply"
      Me.txbReply.Size = New System.Drawing.Size(332, 20)
      Me.txbReply.TabIndex = 33
      '
      'Label2
      '
      Me.Label2.AutoSize = True
      Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label2.Location = New System.Drawing.Point(268, 288)
      Me.Label2.Name = "Label2"
      Me.Label2.Size = New System.Drawing.Size(39, 13)
      Me.Label2.TabIndex = 32
      Me.Label2.Text = "Reply"
      '
      'btnSendCommand
      '
      Me.btnSendCommand.Location = New System.Drawing.Point(164, 248)
      Me.btnSendCommand.Name = "btnSendCommand"
      Me.btnSendCommand.Size = New System.Drawing.Size(148, 24)
      Me.btnSendCommand.TabIndex = 31
      Me.btnSendCommand.Text = "Send Command"
      Me.btnSendCommand.UseVisualStyleBackColor = True
      '
      'txbCommand
      '
      Me.txbCommand.Location = New System.Drawing.Point(320, 252)
      Me.txbCommand.Name = "txbCommand"
      Me.txbCommand.Size = New System.Drawing.Size(332, 20)
      Me.txbCommand.TabIndex = 30
      '
      'ListBox1
      '
      Me.ListBox1.FormattingEnabled = True
      Me.ListBox1.Location = New System.Drawing.Point(8, 32)
      Me.ListBox1.Name = "ListBox1"
      Me.ListBox1.Size = New System.Drawing.Size(136, 316)
      Me.ListBox1.TabIndex = 28
      '
      'GroupBox5
      '
      Me.GroupBox5.BackColor = System.Drawing.Color.Honeydew
      Me.GroupBox5.Controls.Add(Me.btnTestBalance)
      Me.GroupBox5.Controls.Add(Me.btnTestBarcode)
      Me.GroupBox5.Controls.Add(Me.btnTestCarosel)
      Me.GroupBox5.Controls.Add(Me.btnTestOven)
      Me.GroupBox5.Location = New System.Drawing.Point(8, 368)
      Me.GroupBox5.Name = "GroupBox5"
      Me.GroupBox5.Size = New System.Drawing.Size(476, 132)
      Me.GroupBox5.TabIndex = 48
      Me.GroupBox5.TabStop = False
      Me.GroupBox5.Text = "Standard Commands"
      '
      'btnTestBalance
      '
      Me.btnTestBalance.Location = New System.Drawing.Point(176, 28)
      Me.btnTestBalance.Name = "btnTestBalance"
      Me.btnTestBalance.Size = New System.Drawing.Size(136, 24)
      Me.btnTestBalance.TabIndex = 39
      Me.btnTestBalance.Text = "Test Mettler Balance"
      Me.btnTestBalance.UseVisualStyleBackColor = True
      '
      'btnTestBarcode
      '
      Me.btnTestBarcode.Location = New System.Drawing.Point(12, 96)
      Me.btnTestBarcode.Name = "btnTestBarcode"
      Me.btnTestBarcode.Size = New System.Drawing.Size(136, 24)
      Me.btnTestBarcode.TabIndex = 38
      Me.btnTestBarcode.Text = "Test Barcode"
      Me.btnTestBarcode.UseVisualStyleBackColor = True
      '
      'btnTestCarosel
      '
      Me.btnTestCarosel.Location = New System.Drawing.Point(12, 60)
      Me.btnTestCarosel.Name = "btnTestCarosel"
      Me.btnTestCarosel.Size = New System.Drawing.Size(136, 24)
      Me.btnTestCarosel.TabIndex = 37
      Me.btnTestCarosel.Text = "Test Carosel"
      Me.btnTestCarosel.UseVisualStyleBackColor = True
      '
      'btnTestOven
      '
      Me.btnTestOven.Location = New System.Drawing.Point(12, 28)
      Me.btnTestOven.Name = "btnTestOven"
      Me.btnTestOven.Size = New System.Drawing.Size(136, 24)
      Me.btnTestOven.TabIndex = 36
      Me.btnTestOven.Text = "Get Oven Serial Number"
      Me.btnTestOven.UseVisualStyleBackColor = True
      '
      'MenuStrip1
      '
      Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.DataToolStripMenuItem})
      Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
      Me.MenuStrip1.Name = "MenuStrip1"
      Me.MenuStrip1.Size = New System.Drawing.Size(896, 24)
      Me.MenuStrip1.TabIndex = 49
      Me.MenuStrip1.Text = "MenuStrip1"
      '
      'DataToolStripMenuItem
      '
      Me.DataToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuUpdatePorts})
      Me.DataToolStripMenuItem.Name = "DataToolStripMenuItem"
      Me.DataToolStripMenuItem.Size = New System.Drawing.Size(43, 20)
      Me.DataToolStripMenuItem.Text = "Data"
      '
      'mnuUpdatePorts
      '
      Me.mnuUpdatePorts.Name = "mnuUpdatePorts"
      Me.mnuUpdatePorts.Size = New System.Drawing.Size(158, 22)
      Me.mnuUpdatePorts.Text = "Update Port List"
      '
      'lblClosingPort
      '
      Me.lblClosingPort.AutoSize = True
      Me.lblClosingPort.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.lblClosingPort.ForeColor = System.Drawing.Color.Blue
      Me.lblClosingPort.Location = New System.Drawing.Point(152, 36)
      Me.lblClosingPort.Name = "lblClosingPort"
      Me.lblClosingPort.Size = New System.Drawing.Size(106, 20)
      Me.lblClosingPort.TabIndex = 50
      Me.lblClosingPort.Text = "Closing Port"
      Me.lblClosingPort.Visible = False
      '
      'gbxCommandDef
      '
      Me.gbxCommandDef.BackColor = System.Drawing.Color.LightGray
      Me.gbxCommandDef.Controls.Add(Me.Label4)
      Me.gbxCommandDef.Controls.Add(Me.txbDefChar)
      Me.gbxCommandDef.Controls.Add(Me.btnAddDefChar)
      Me.gbxCommandDef.Controls.Add(Me.btnDefine)
      Me.gbxCommandDef.Controls.Add(Me.txbCommandBuilder)
      Me.gbxCommandDef.Location = New System.Drawing.Point(728, 32)
      Me.gbxCommandDef.Name = "gbxCommandDef"
      Me.gbxCommandDef.Size = New System.Drawing.Size(140, 28)
      Me.gbxCommandDef.TabIndex = 51
      Me.gbxCommandDef.TabStop = False
      Me.gbxCommandDef.Text = "Command Definitioin"
      Me.gbxCommandDef.Visible = False
      '
      'Label4
      '
      Me.Label4.AutoSize = True
      Me.Label4.Location = New System.Drawing.Point(448, 36)
      Me.Label4.Name = "Label4"
      Me.Label4.Size = New System.Drawing.Size(74, 13)
      Me.Label4.TabIndex = 51
      Me.Label4.Text = "Decimal value"
      '
      'txbDefChar
      '
      Me.txbDefChar.Location = New System.Drawing.Point(404, 30)
      Me.txbDefChar.Name = "txbDefChar"
      Me.txbDefChar.Size = New System.Drawing.Size(36, 20)
      Me.txbDefChar.TabIndex = 50
      '
      'btnAddDefChar
      '
      Me.btnAddDefChar.Location = New System.Drawing.Point(172, 28)
      Me.btnAddDefChar.Name = "btnAddDefChar"
      Me.btnAddDefChar.Size = New System.Drawing.Size(224, 24)
      Me.btnAddDefChar.TabIndex = 49
      Me.btnAddDefChar.Text = "Add this charactor to the output command"
      Me.btnAddDefChar.UseVisualStyleBackColor = True
      '
      'btnDefine
      '
      Me.btnDefine.BackColor = System.Drawing.Color.LimeGreen
      Me.btnDefine.Location = New System.Drawing.Point(8, 56)
      Me.btnDefine.Name = "btnDefine"
      Me.btnDefine.Size = New System.Drawing.Size(124, 24)
      Me.btnDefine.TabIndex = 48
      Me.btnDefine.Text = "Define Command "
      Me.btnDefine.UseVisualStyleBackColor = False
      '
      'txbCommandBuilder
      '
      Me.txbCommandBuilder.Location = New System.Drawing.Point(144, 60)
      Me.txbCommandBuilder.Name = "txbCommandBuilder"
      Me.txbCommandBuilder.Size = New System.Drawing.Size(388, 20)
      Me.txbCommandBuilder.TabIndex = 47
      '
      'dgvPorts
      '
      Me.dgvPorts.AllowUserToAddRows = False
      Me.dgvPorts.AllowUserToDeleteRows = False
      Me.dgvPorts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
      Me.dgvPorts.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column1, Me.Column2})
      Me.dgvPorts.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter
      Me.dgvPorts.Location = New System.Drawing.Point(496, 368)
      Me.dgvPorts.Name = "dgvPorts"
      Me.dgvPorts.Size = New System.Drawing.Size(300, 168)
      Me.dgvPorts.TabIndex = 52
      '
      'Column1
      '
      Me.Column1.HeaderText = "Object"
      Me.Column1.Name = "Column1"
      Me.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
      Me.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
      '
      'Column2
      '
      Me.Column2.HeaderText = "Port Name"
      Me.Column2.Name = "Column2"
      Me.Column2.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
      Me.Column2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
      '
      'btnSavePorts
      '
      Me.btnSavePorts.BackColor = System.Drawing.Color.Thistle
      Me.btnSavePorts.Location = New System.Drawing.Point(804, 500)
      Me.btnSavePorts.Name = "btnSavePorts"
      Me.btnSavePorts.Size = New System.Drawing.Size(76, 36)
      Me.btnSavePorts.TabIndex = 53
      Me.btnSavePorts.Text = "Save New Port List"
      Me.btnSavePorts.UseVisualStyleBackColor = False
      '
      'frmSerialCommunications
      '
      Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
      Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
      Me.BackColor = System.Drawing.Color.PaleGreen
      Me.ClientSize = New System.Drawing.Size(896, 548)
      Me.Controls.Add(Me.btnSavePorts)
      Me.Controls.Add(Me.dgvPorts)
      Me.Controls.Add(Me.gbxCommandDef)
      Me.Controls.Add(Me.lblClosingPort)
      Me.Controls.Add(Me.GroupBox5)
      Me.Controls.Add(Me.Label3)
      Me.Controls.Add(Me.txbAddChar)
      Me.Controls.Add(Me.btnAddChar)
      Me.Controls.Add(Me.GroupBox2)
      Me.Controls.Add(Me.btnCommand8)
      Me.Controls.Add(Me.btnCommand7)
      Me.Controls.Add(Me.btnCommand6)
      Me.Controls.Add(Me.btnCommand5)
      Me.Controls.Add(Me.btnCommand4)
      Me.Controls.Add(Me.btnCommand3)
      Me.Controls.Add(Me.btnCommand2)
      Me.Controls.Add(Me.btnCommand1)
      Me.Controls.Add(Me.txbReply)
      Me.Controls.Add(Me.Label2)
      Me.Controls.Add(Me.btnSendCommand)
      Me.Controls.Add(Me.txbCommand)
      Me.Controls.Add(Me.ListBox1)
      Me.Controls.Add(Me.MenuStrip1)
      Me.MainMenuStrip = Me.MenuStrip1
      Me.Name = "frmSerialCommunications"
      Me.Text = "Serial Communications"
      Me.GroupBox2.ResumeLayout(False)
      Me.GroupBox2.PerformLayout()
      Me.GroupBox4.ResumeLayout(False)
      Me.GroupBox4.PerformLayout()
      Me.GroupBox3.ResumeLayout(False)
      Me.GroupBox3.PerformLayout()
      Me.GroupBox1.ResumeLayout(False)
      Me.GroupBox1.PerformLayout()
      Me.GroupBox5.ResumeLayout(False)
      Me.MenuStrip1.ResumeLayout(False)
      Me.MenuStrip1.PerformLayout()
      Me.gbxCommandDef.ResumeLayout(False)
      Me.gbxCommandDef.PerformLayout()
      CType(Me.dgvPorts, System.ComponentModel.ISupportInitialize).EndInit()
      Me.ResumeLayout(False)
      Me.PerformLayout()

   End Sub
   Friend WithEvents Label3 As System.Windows.Forms.Label
   Friend WithEvents txbAddChar As System.Windows.Forms.TextBox
   Friend WithEvents btnAddChar As System.Windows.Forms.Button
   Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
   Friend WithEvents GroupBox4 As System.Windows.Forms.GroupBox
   Friend WithEvents rdoHandshakeXonXoff As System.Windows.Forms.RadioButton
   Friend WithEvents rdoHandshakeRTS As System.Windows.Forms.RadioButton
   Friend WithEvents rdoHandshakeNone As System.Windows.Forms.RadioButton
   Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
   Friend WithEvents rdoDataBits8 As System.Windows.Forms.RadioButton
   Friend WithEvents rdoDatabits7 As System.Windows.Forms.RadioButton
   Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
   Friend WithEvents rdoXmitCr As System.Windows.Forms.RadioButton
   Friend WithEvents txbXmitEOL As System.Windows.Forms.TextBox
   Friend WithEvents rdoXmitCrLf As System.Windows.Forms.RadioButton
   Friend WithEvents rdoXmitDec As System.Windows.Forms.RadioButton
   Friend WithEvents Label1 As System.Windows.Forms.Label
   Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
   Friend WithEvents btnCommand8 As System.Windows.Forms.Button
   Friend WithEvents btnCommand7 As System.Windows.Forms.Button
   Friend WithEvents btnCommand6 As System.Windows.Forms.Button
   Friend WithEvents btnCommand5 As System.Windows.Forms.Button
   Friend WithEvents btnCommand4 As System.Windows.Forms.Button
   Friend WithEvents btnCommand3 As System.Windows.Forms.Button
   Friend WithEvents btnCommand2 As System.Windows.Forms.Button
   Friend WithEvents btnCommand1 As System.Windows.Forms.Button
   Friend WithEvents txbReply As System.Windows.Forms.TextBox
   Friend WithEvents Label2 As System.Windows.Forms.Label
   Friend WithEvents btnSendCommand As System.Windows.Forms.Button
   Friend WithEvents txbCommand As System.Windows.Forms.TextBox
   Friend WithEvents ListBox1 As System.Windows.Forms.ListBox
   Friend WithEvents GroupBox5 As System.Windows.Forms.GroupBox
   Friend WithEvents btnTestOven As System.Windows.Forms.Button
   Friend WithEvents btnTestCarosel As System.Windows.Forms.Button
   Friend WithEvents btnTestBalance As System.Windows.Forms.Button
   Friend WithEvents btnTestBarcode As System.Windows.Forms.Button
   Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
   Friend WithEvents DataToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
   Friend WithEvents mnuUpdatePorts As System.Windows.Forms.ToolStripMenuItem
   Friend WithEvents lblClosingPort As System.Windows.Forms.Label
   Friend WithEvents gbxCommandDef As System.Windows.Forms.GroupBox
   Friend WithEvents Label4 As System.Windows.Forms.Label
   Friend WithEvents txbDefChar As System.Windows.Forms.TextBox
   Friend WithEvents btnAddDefChar As System.Windows.Forms.Button
   Friend WithEvents btnDefine As System.Windows.Forms.Button
   Friend WithEvents txbCommandBuilder As System.Windows.Forms.TextBox
   Friend WithEvents dgvPorts As System.Windows.Forms.DataGridView
   Friend WithEvents Column1 As System.Windows.Forms.DataGridViewTextBoxColumn
   Friend WithEvents Column2 As System.Windows.Forms.DataGridViewTextBoxColumn
   Friend WithEvents btnSavePorts As System.Windows.Forms.Button
End Class
