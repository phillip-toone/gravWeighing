Public Class frmCountDownTimer
    Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

    Public Sub New(ByVal DurationSeconds As Int32, Optional ByVal Title As String = "Proceedure Delay Time")
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        EndTime = DurationSeconds
        Me.lblUserMessage.Text = Title
    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txbTimeWindow As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents btnExitTimer As System.Windows.Forms.Button
    Friend WithEvents lblUserMessage As System.Windows.Forms.Label
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents btnChangeTime As System.Windows.Forms.Button
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Me.Label1 = New System.Windows.Forms.Label
        Me.txbTimeWindow = New System.Windows.Forms.TextBox
        Me.btnChangeTime = New System.Windows.Forms.Button
        Me.Label2 = New System.Windows.Forms.Label
        Me.btnExitTimer = New System.Windows.Forms.Button
        Me.lblUserMessage = New System.Windows.Forms.Label
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(32, 56)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(120, 16)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Delay time remaining :"
        '
        'txbTimeWindow
        '
        Me.txbTimeWindow.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txbTimeWindow.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txbTimeWindow.Location = New System.Drawing.Point(148, 56)
        Me.txbTimeWindow.Name = "txbTimeWindow"
        Me.txbTimeWindow.Size = New System.Drawing.Size(116, 26)
        Me.txbTimeWindow.TabIndex = 1
        Me.txbTimeWindow.Text = "00:00:00"
        Me.txbTimeWindow.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'btnChangeTime
        '
        Me.btnChangeTime.BackColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.btnChangeTime.Location = New System.Drawing.Point(116, 112)
        Me.btnChangeTime.Name = "btnChangeTime"
        Me.btnChangeTime.Size = New System.Drawing.Size(68, 32)
        Me.btnChangeTime.TabIndex = 2
        Me.btnChangeTime.Text = "Edit Delay Time"
        Me.btnChangeTime.UseVisualStyleBackColor = False
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(140, 84)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(140, 16)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Hours : Minutes : Seconds"
        '
        'btnExitTimer
        '
        Me.btnExitTimer.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.btnExitTimer.Location = New System.Drawing.Point(228, 112)
        Me.btnExitTimer.Name = "btnExitTimer"
        Me.btnExitTimer.Size = New System.Drawing.Size(68, 32)
        Me.btnExitTimer.TabIndex = 4
        Me.btnExitTimer.Text = "Exit and Continue"
        Me.btnExitTimer.UseVisualStyleBackColor = False
        '
        'lblUserMessage
        '
        Me.lblUserMessage.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblUserMessage.Location = New System.Drawing.Point(44, 24)
        Me.lblUserMessage.Name = "lblUserMessage"
        Me.lblUserMessage.Size = New System.Drawing.Size(324, 24)
        Me.lblUserMessage.TabIndex = 5
        Me.lblUserMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'frmCountDownTimer
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(400, 166)
        Me.Controls.Add(Me.lblUserMessage)
        Me.Controls.Add(Me.btnExitTimer)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.btnChangeTime)
        Me.Controls.Add(Me.txbTimeWindow)
        Me.Controls.Add(Me.Label1)
        Me.Name = "frmCountDownTimer"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Process Delay Timer"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region


    Private EndTime As Int32
    Dim TimeBeingEdited As Boolean = False


    Private Sub frmCountDownTimer_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Timer1.Interval = 1000
        Timer1.Start()
    End Sub

    Private Sub Timer1_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Dim MyTS As TimeSpan

        EndTime -= 1
        If EndTime <= 0 Then
            Me.Close()
        End If

        MyTS = TimeSpan.FromSeconds(EndTime)
        If MyTS.Days > 0 Then
            Me.txbTimeWindow.Text = MyTS.Days & "." & Format(MyTS.Hours, "00") & ":" & Format(MyTS.Minutes, "00") & ":" & Format(MyTS.Seconds, "00")
        Else
            Me.txbTimeWindow.Text = MyTS.Hours & ":" & Format(MyTS.Minutes, "00") & ":" & Format(MyTS.Seconds, "00")

        End If
    End Sub


    Private Sub btnChangeTime_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnChangeTime.Click
        Static Dim saveTitle As String
        Dim newPoint As Point


        If TimeBeingEdited = False Then
            Timer1.Stop()
            saveTitle = Me.lblUserMessage.Text
            Me.lblUserMessage.Text = "Enter New Time"
            Me.btnExitTimer.Visible = False
            Me.btnChangeTime.Text = "Load Time"
            newPoint.X = 172
            newPoint.Y = 112
            Me.btnChangeTime.Location = newPoint
            TimeBeingEdited = True
        Else    'Clicked button again to load the new time
            If GetTime(Me.txbTimeWindow) > Int32.MinValue Then
                EndTime = GetTime(Me.txbTimeWindow)
                Me.lblUserMessage.Text = saveTitle
                Me.btnChangeTime.Text = "Enter New Time"
                newPoint.X = 116
                newPoint.Y = 112
                Me.btnChangeTime.Location = newPoint
                Me.btnExitTimer.Visible = True
                TimeBeingEdited = False
                Timer1.Start()
            End If
        End If
    End Sub

    Private Sub btnExitTimer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExitTimer.Click
        Timer1.Stop()
        If MessageBox.Show("Exit timer now?", "Exit and Continue with Proceedure", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1) = Windows.Forms.DialogResult.OK Then
            Me.Close()
        End If
    End Sub
End Class
