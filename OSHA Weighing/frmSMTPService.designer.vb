<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSMTPService
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
      Me.GroupBox2 = New System.Windows.Forms.GroupBox
      Me.Label2 = New System.Windows.Forms.Label
      Me.txbOver = New System.Windows.Forms.TextBox
      Me.GroupBox1 = New System.Windows.Forms.GroupBox
      Me.Label1 = New System.Windows.Forms.Label
      Me.txbError = New System.Windows.Forms.TextBox
      Me.txbHost = New System.Windows.Forms.TextBox
      Me.Label3 = New System.Windows.Forms.Label
      Me.GroupBox3 = New System.Windows.Forms.GroupBox
      Me.Label6 = New System.Windows.Forms.Label
      Me.txbFrom = New System.Windows.Forms.TextBox
      Me.Label5 = New System.Windows.Forms.Label
      Me.txbPort = New System.Windows.Forms.TextBox
      Me.Label4 = New System.Windows.Forms.Label
      Me.MenuStrip1 = New System.Windows.Forms.MenuStrip
      Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
      Me.SaveToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
      Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator
      Me.ExitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
      Me.Label7 = New System.Windows.Forms.Label
      Me.txbUserName = New System.Windows.Forms.TextBox
      Me.Label8 = New System.Windows.Forms.Label
      Me.txbPassword = New System.Windows.Forms.TextBox
      Me.GroupBox2.SuspendLayout()
      Me.GroupBox1.SuspendLayout()
      Me.GroupBox3.SuspendLayout()
      Me.MenuStrip1.SuspendLayout()
      Me.SuspendLayout()
      '
      'GroupBox2
      '
      Me.GroupBox2.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
      Me.GroupBox2.Controls.Add(Me.Label2)
      Me.GroupBox2.Controls.Add(Me.txbOver)
      Me.GroupBox2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.GroupBox2.ForeColor = System.Drawing.Color.Black
      Me.GroupBox2.Location = New System.Drawing.Point(20, 40)
      Me.GroupBox2.Name = "GroupBox2"
      Me.GroupBox2.Size = New System.Drawing.Size(468, 108)
      Me.GroupBox2.TabIndex = 3
      Me.GroupBox2.TabStop = False
      Me.GroupBox2.Text = "When the Proceedure is Complete, Send Notification To:"
      '
      'Label2
      '
      Me.Label2.AutoSize = True
      Me.Label2.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
      Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label2.Location = New System.Drawing.Point(288, 24)
      Me.Label2.Name = "Label2"
      Me.Label2.Size = New System.Drawing.Size(170, 39)
      Me.Label2.TabIndex = 1
      Me.Label2.Text = "Example:" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "      Mary_smith@yahoo.com" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "      Harvey_the_rabbit@gmail.com"
      '
      'txbOver
      '
      Me.txbOver.Location = New System.Drawing.Point(8, 24)
      Me.txbOver.Multiline = True
      Me.txbOver.Name = "txbOver"
      Me.txbOver.Size = New System.Drawing.Size(272, 72)
      Me.txbOver.TabIndex = 0
      '
      'GroupBox1
      '
      Me.GroupBox1.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(255, Byte), Integer))
      Me.GroupBox1.Controls.Add(Me.Label1)
      Me.GroupBox1.Controls.Add(Me.txbError)
      Me.GroupBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.GroupBox1.ForeColor = System.Drawing.Color.Black
      Me.GroupBox1.Location = New System.Drawing.Point(20, 160)
      Me.GroupBox1.Name = "GroupBox1"
      Me.GroupBox1.Size = New System.Drawing.Size(468, 108)
      Me.GroupBox1.TabIndex = 2
      Me.GroupBox1.TabStop = False
      Me.GroupBox1.Text = "On Error, Send Email Notification To:"
      '
      'Label1
      '
      Me.Label1.AutoSize = True
      Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.Label1.Location = New System.Drawing.Point(288, 24)
      Me.Label1.Name = "Label1"
      Me.Label1.Size = New System.Drawing.Size(170, 39)
      Me.Label1.TabIndex = 1
      Me.Label1.Text = "Example:" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "      Mary_smith@yahoo.com" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "      Harvey_the_rabbit@gmail.com"
      '
      'txbError
      '
      Me.txbError.Location = New System.Drawing.Point(8, 24)
      Me.txbError.Multiline = True
      Me.txbError.Name = "txbError"
      Me.txbError.Size = New System.Drawing.Size(272, 72)
      Me.txbError.TabIndex = 0
      '
      'txbHost
      '
      Me.txbHost.Location = New System.Drawing.Point(148, 24)
      Me.txbHost.Name = "txbHost"
      Me.txbHost.Size = New System.Drawing.Size(308, 20)
      Me.txbHost.TabIndex = 4
      '
      'Label3
      '
      Me.Label3.AutoSize = True
      Me.Label3.Location = New System.Drawing.Point(16, 28)
      Me.Label3.Name = "Label3"
      Me.Label3.Size = New System.Drawing.Size(126, 13)
      Me.Label3.TabIndex = 5
      Me.Label3.Text = "SMTP Server Name :"
      '
      'GroupBox3
      '
      Me.GroupBox3.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(192, Byte), Integer))
      Me.GroupBox3.Controls.Add(Me.Label8)
      Me.GroupBox3.Controls.Add(Me.txbPassword)
      Me.GroupBox3.Controls.Add(Me.Label7)
      Me.GroupBox3.Controls.Add(Me.txbUserName)
      Me.GroupBox3.Controls.Add(Me.Label6)
      Me.GroupBox3.Controls.Add(Me.txbFrom)
      Me.GroupBox3.Controls.Add(Me.Label5)
      Me.GroupBox3.Controls.Add(Me.txbPort)
      Me.GroupBox3.Controls.Add(Me.Label4)
      Me.GroupBox3.Controls.Add(Me.Label3)
      Me.GroupBox3.Controls.Add(Me.txbHost)
      Me.GroupBox3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.GroupBox3.Location = New System.Drawing.Point(20, 280)
      Me.GroupBox3.Name = "GroupBox3"
      Me.GroupBox3.Size = New System.Drawing.Size(468, 152)
      Me.GroupBox3.TabIndex = 6
      Me.GroupBox3.TabStop = False
      Me.GroupBox3.Text = "Server Configuration Parameters"
      '
      'Label6
      '
      Me.Label6.AutoSize = True
      Me.Label6.Location = New System.Drawing.Point(8, 76)
      Me.Label6.Name = "Label6"
      Me.Label6.Size = New System.Drawing.Size(137, 13)
      Me.Label6.TabIndex = 11
      Me.Label6.Text = """From"" Email Address :"
      Me.Label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight
      '
      'txbFrom
      '
      Me.txbFrom.Location = New System.Drawing.Point(148, 72)
      Me.txbFrom.Name = "txbFrom"
      Me.txbFrom.Size = New System.Drawing.Size(308, 20)
      Me.txbFrom.TabIndex = 10
      '
      'Label5
      '
      Me.Label5.AutoSize = True
      Me.Label5.Location = New System.Drawing.Point(104, 52)
      Me.Label5.Name = "Label5"
      Me.Label5.Size = New System.Drawing.Size(38, 13)
      Me.Label5.TabIndex = 9
      Me.Label5.Text = "Port :"
      Me.Label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight
      '
      'txbPort
      '
      Me.txbPort.Location = New System.Drawing.Point(148, 48)
      Me.txbPort.Name = "txbPort"
      Me.txbPort.Size = New System.Drawing.Size(52, 20)
      Me.txbPort.TabIndex = 8
      '
      'Label4
      '
      Me.Label4.AutoSize = True
      Me.Label4.Location = New System.Drawing.Point(52, 56)
      Me.Label4.Name = "Label4"
      Me.Label4.Size = New System.Drawing.Size(0, 13)
      Me.Label4.TabIndex = 7
      Me.Label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight
      '
      'MenuStrip1
      '
      Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem})
      Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
      Me.MenuStrip1.Name = "MenuStrip1"
      Me.MenuStrip1.Size = New System.Drawing.Size(505, 24)
      Me.MenuStrip1.TabIndex = 7
      Me.MenuStrip1.Text = "MenuStrip1"
      '
      'FileToolStripMenuItem
      '
      Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SaveToolStripMenuItem, Me.ToolStripSeparator1, Me.ExitToolStripMenuItem})
      Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
      Me.FileToolStripMenuItem.Size = New System.Drawing.Size(35, 20)
      Me.FileToolStripMenuItem.Text = "File"
      '
      'SaveToolStripMenuItem
      '
      Me.SaveToolStripMenuItem.Name = "SaveToolStripMenuItem"
      Me.SaveToolStripMenuItem.Size = New System.Drawing.Size(109, 22)
      Me.SaveToolStripMenuItem.Text = "Save"
      '
      'ToolStripSeparator1
      '
      Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
      Me.ToolStripSeparator1.Size = New System.Drawing.Size(106, 6)
      '
      'ExitToolStripMenuItem
      '
      Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
      Me.ExitToolStripMenuItem.Size = New System.Drawing.Size(109, 22)
      Me.ExitToolStripMenuItem.Text = "Exit"
      '
      'Label7
      '
      Me.Label7.AutoSize = True
      Me.Label7.ForeColor = System.Drawing.Color.RoyalBlue
      Me.Label7.Location = New System.Drawing.Point(8, 100)
      Me.Label7.Name = "Label7"
      Me.Label7.Size = New System.Drawing.Size(134, 13)
      Me.Label7.TabIndex = 13
      Me.Label7.Text = "User Name (optional) :"
      Me.Label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight
      '
      'txbUserName
      '
      Me.txbUserName.Location = New System.Drawing.Point(148, 96)
      Me.txbUserName.Name = "txbUserName"
      Me.txbUserName.Size = New System.Drawing.Size(216, 20)
      Me.txbUserName.TabIndex = 12
      '
      'Label8
      '
      Me.Label8.AutoSize = True
      Me.Label8.ForeColor = System.Drawing.Color.RoyalBlue
      Me.Label8.Location = New System.Drawing.Point(12, 124)
      Me.Label8.Name = "Label8"
      Me.Label8.Size = New System.Drawing.Size(126, 13)
      Me.Label8.TabIndex = 15
      Me.Label8.Text = "Password (optional) :"
      Me.Label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight
      '
      'txbPassword
      '
      Me.txbPassword.Location = New System.Drawing.Point(148, 120)
      Me.txbPassword.Name = "txbPassword"
      Me.txbPassword.Size = New System.Drawing.Size(216, 20)
      Me.txbPassword.TabIndex = 14
      '
      'frmSMTPService
      '
      Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
      Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
      Me.ClientSize = New System.Drawing.Size(505, 480)
      Me.Controls.Add(Me.GroupBox3)
      Me.Controls.Add(Me.GroupBox2)
      Me.Controls.Add(Me.GroupBox1)
      Me.Controls.Add(Me.MenuStrip1)
      Me.MainMenuStrip = Me.MenuStrip1
      Me.Name = "frmSMTPService"
      Me.Text = "frmSMTPService"
      Me.GroupBox2.ResumeLayout(False)
      Me.GroupBox2.PerformLayout()
      Me.GroupBox1.ResumeLayout(False)
      Me.GroupBox1.PerformLayout()
      Me.GroupBox3.ResumeLayout(False)
      Me.GroupBox3.PerformLayout()
      Me.MenuStrip1.ResumeLayout(False)
      Me.MenuStrip1.PerformLayout()
      Me.ResumeLayout(False)
      Me.PerformLayout()

   End Sub
   Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
   Friend WithEvents Label2 As System.Windows.Forms.Label
   Friend WithEvents txbOver As System.Windows.Forms.TextBox
   Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
   Friend WithEvents Label1 As System.Windows.Forms.Label
   Friend WithEvents txbError As System.Windows.Forms.TextBox
   Friend WithEvents txbHost As System.Windows.Forms.TextBox
   Friend WithEvents Label3 As System.Windows.Forms.Label
   Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
   Friend WithEvents Label4 As System.Windows.Forms.Label
   Friend WithEvents Label5 As System.Windows.Forms.Label
   Friend WithEvents txbPort As System.Windows.Forms.TextBox
   Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
   Friend WithEvents FileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
   Friend WithEvents SaveToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
   Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
   Friend WithEvents ExitToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
   Friend WithEvents Label6 As System.Windows.Forms.Label
   Friend WithEvents txbFrom As System.Windows.Forms.TextBox
   Friend WithEvents Label8 As System.Windows.Forms.Label
   Friend WithEvents txbPassword As System.Windows.Forms.TextBox
   Friend WithEvents Label7 As System.Windows.Forms.Label
   Friend WithEvents txbUserName As System.Windows.Forms.TextBox
End Class
