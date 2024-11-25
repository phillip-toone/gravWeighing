Imports System.IO


Public Class frmSerialCommunications
   Dim MyPort As New System.IO.Ports.SerialPort
   Dim Command(7) As String
   Dim ButtonInEdit As Button


   Private Sub frmSerialCommunications_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
      UpdatePortList()

      dgvPorts.Rows.Add(8)
      dgvPorts.RowHeadersWidth = 20
      dgvPorts.Rows(0).Cells(0).Value = "Oven 1"
      dgvPorts.Rows(1).Cells(0).Value = "Oven 2"
      dgvPorts.Rows(2).Cells(0).Value = "Oven 3"
      dgvPorts.Rows(3).Cells(0).Value = "Oven 4"
      dgvPorts.Rows(4).Cells(0).Value = "Oven 5"
      dgvPorts.Rows(5).Cells(0).Value = "Carosel"
      dgvPorts.Rows(6).Cells(0).Value = "Barcode"
      dgvPorts.Rows(7).Cells(0).Value = "Balance"
      dgvPorts.Rows(0).Cells(1).Value = Oven1.PortName
      dgvPorts.Rows(1).Cells(1).Value = Oven2.PortName
      dgvPorts.Rows(2).Cells(1).Value = Oven3.PortName
      dgvPorts.Rows(3).Cells(1).Value = Oven4.PortName
      dgvPorts.Rows(4).Cells(1).Value = Oven5.PortName
      dgvPorts.Rows(5).Cells(1).Value = Carosel.PortName
      dgvPorts.Rows(6).Cells(1).Value = BarCode.PortName
      dgvPorts.Rows(7).Cells(1).Value = Balance.PortName
   End Sub

   Private Sub mnuUpdatePorts_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuUpdatePorts.Click
      UpdatePortList()
   End Sub

   Sub UpdatePortList()
      Dim Ports() As String

      Ports = IO.Ports.SerialPort.GetPortNames()
      Me.ListBox1.Items.Clear()
      For pass As Int32 = 0 To Ports.Length - 1
         Me.ListBox1.Items.Add(Ports(pass))
      Next

      'Close all the existing serial ports
      For Each TempPort As IO.Ports.SerialPort In PortCollection
         TempPort.Close()
      Next
   End Sub


   Private Sub ListBox1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ListBox1.SelectedIndexChanged
      MyPort.Close()
      MyPort.PortName = Me.ListBox1.SelectedItem.ToString
      If MyPort.IsOpen = True Then
         If MessageBox.Show("This port is already open, do you want to close it?", "Port Open", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then
            MyPort.Close()
            lblClosingPort.Visible = True
            DelayMS(2500)
            lblClosingPort.Visible = False
         Else
            Exit Sub
         End If
      End If

      MyPort.BaudRate = CInt(TextBox1.Text)
      If rdoXmitCr.Checked = True Then
         MyPort.NewLine = vbCr
      ElseIf rdoXmitCrLf.Checked = True Then
         MyPort.NewLine = vbCrLf
      Else
         MyPort.NewLine = txbXmitEOL.Text
      End If
      If rdoDatabits7.Checked = True Then
         MyPort.DataBits = 7
      Else
         MyPort.DataBits = 8
      End If
      If rdoHandshakeNone.Checked = True Then
         MyPort.Handshake = Ports.Handshake.None
      ElseIf rdoHandshakeRTS.Checked = True Then
         MyPort.Handshake = Ports.Handshake.RequestToSend
      ElseIf rdoHandshakeXonXoff.Checked = True Then
         MyPort.Handshake = Ports.Handshake.XOnXOff
      End If
      MyPort.ReadTimeout = 1000
      Try
         MyPort.Open()
      Catch ex As Exception
         MessageBox.Show("Port did not open", "port", MessageBoxButtons.OK, MessageBoxIcon.Hand)
      End Try
   End Sub


   Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSendCommand.Click
      SendCommand()
   End Sub

   Sub SendCommand()
      Dim Reply As String

      txbReply.Text = ""
      MyPort.DiscardInBuffer()
      MyPort.DiscardOutBuffer()
      MyPort.WriteLine(txbCommand.Text)
      Try
         Reply = MyPort.ReadLine
         txbReply.Text = Reply
      Catch ex As Exception
         If MyPort.BytesToRead > 0 Then
            txbReply.Text = MyPort.ReadExisting & "  Non-standard EOL"
         Else
            txbReply.Text = "None"
         End If
      End Try
   End Sub



   Public Sub DelayMS(ByVal Delay As Int32)
      '***********************************************************************
      Dim EndTime As DateTime

      EndTime = Now.AddMilliseconds(Convert.ToDouble(Delay))
      While DateTime.Compare(Now, EndTime) < 0.0
         Application.DoEvents()
      End While
   End Sub

   Private Sub btnAddChar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddChar.Click
      txbCommand.Text = txbCommand.Text & Chr(CInt(txbAddChar.Text))
   End Sub



#Region "Predefined Commands"

   Private Sub btnTestOven_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTestOven.Click
      Dim Reply As String

      MyPort.DiscardInBuffer()
      MyPort.DiscardOutBuffer()
      txbCommand.Text = "?(0)"
      MyPort.WriteLine("?(0)")
      Try
         Reply = MyPort.ReadLine
      Catch ex As Exception
         If MyPort.BytesToRead > 0 Then
            DelayMS(100)
            Reply = "Non-Terminated: " & MyPort.ReadExisting
         Else
            Reply = "NONE"
         End If
      End Try

      txbReply.Text = Reply
   End Sub


   Private Sub btnTestCarosel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTestCarosel.Click
      Dim Reply As String


      MyPort.BaudRate = 9600
      MyPort.ReadTimeout = 1000
      MyPort.NewLine = vbCr
      MyPort.DiscardInBuffer()
      MyPort.DiscardOutBuffer()
      txbCommand.Text = ":1ST"
      MyPort.WriteLine(":1ST")
      Try
         Reply = MyPort.ReadLine
      Catch ex As Exception
         If MyPort.BytesToRead > 0 Then
            DelayMS(100)
            Reply = "Non-Terminated: " & MyPort.ReadExisting
         Else
            Reply = "NONE"
         End If
      End Try

      txbReply.Text = Reply
   End Sub

   Private Sub btnTestBarcode_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTestBarcode.Click
      Dim Reply As String

      MyPort.BaudRate = 115200
      MyPort.ReadTimeout = 2500
      MyPort.NewLine = vbCr
      MyPort.DiscardInBuffer()
      MyPort.DiscardOutBuffer()
      txbCommand.Text = "<K221?>"
      MyPort.WriteLine("<K221?>")

      Try
         Reply = MyPort.ReadLine
      Catch ex As Exception
         If MyPort.BytesToRead > 0 Then
            DelayMS(100)
            Reply = "Non-Terminated: " & MyPort.ReadExisting
         Else
            Reply = "NONE"
         End If
      End Try

      txbReply.Text = Reply
   End Sub


   Private Sub btnTestBalance_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTestBalance.Click
      Dim Reply As String

      MyPort.ReadTimeout = 1000
      MyPort.DiscardInBuffer()
      MyPort.DiscardOutBuffer()
      txbCommand.Text = "I2"
      MyPort.WriteLine("I2") 'Query serial number

      Try
         Reply = MyPort.ReadLine
      Catch ex As Exception
         If MyPort.BytesToRead > 0 Then
            DelayMS(100)
            Reply = "Non-Terminated: " & MyPort.ReadExisting
         Else
            Reply = "NONE"
         End If
      End Try

      txbReply.Text = Reply
   End Sub


#End Region



#Region "User Defined Buttons"

   Private Sub btnCommand1_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles btnCommand1.MouseUp
      If e.Button = Windows.Forms.MouseButtons.Right Then
         gbxCommandDef.Location = New Point(160, 216)
         gbxCommandDef.Size = New Size(548, 96)
         gbxCommandDef.Visible = True
         txbCommandBuilder.Text = ""
         btnDefine.Text = "Define Command 1"
         ButtonInEdit = btnCommand1
      Else
         txbCommand.Text = Command(0)
         Application.DoEvents()
         SendCommand()
      End If
   End Sub

   Private Sub btnCommand2_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles btnCommand2.MouseUp
      If e.Button = Windows.Forms.MouseButtons.Right Then
         gbxCommandDef.Location = New Point(160, 216)
         gbxCommandDef.Size = New Size(548, 96)
         gbxCommandDef.Visible = True
         txbCommandBuilder.Text = ""
         btnDefine.Text = "Define Command 2"
         ButtonInEdit = btnCommand2
      Else
         txbCommand.Text = Command(1)
         Application.DoEvents()
         SendCommand()
      End If
   End Sub

   Private Sub btnCommand3_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles btnCommand3.MouseUp
      If e.Button = Windows.Forms.MouseButtons.Right Then
         gbxCommandDef.Location = New Point(160, 216)
         gbxCommandDef.Size = New Size(548, 96)
         gbxCommandDef.Visible = True
         txbCommandBuilder.Text = ""
         btnDefine.Text = "Define Command 3"
         ButtonInEdit = btnCommand3
      Else
         txbCommand.Text = Command(2)
         Application.DoEvents()
         SendCommand()
      End If
   End Sub

   Private Sub btnCommand4_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles btnCommand4.MouseUp
      If e.Button = Windows.Forms.MouseButtons.Right Then
         gbxCommandDef.Location = New Point(160, 216)
         gbxCommandDef.Size = New Size(548, 96)
         gbxCommandDef.Visible = True
         txbCommandBuilder.Text = ""
         btnDefine.Text = "Define Command 4"
         ButtonInEdit = btnCommand4
      Else
         txbCommand.Text = Command(3)
         Application.DoEvents()
         SendCommand()
      End If
   End Sub

   Private Sub btnCommand5_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles btnCommand5.MouseUp
      If e.Button = Windows.Forms.MouseButtons.Right Then
         gbxCommandDef.Location = New Point(160, 216)
         gbxCommandDef.Size = New Size(548, 96)
         gbxCommandDef.Visible = True
         txbCommandBuilder.Text = ""
         btnDefine.Text = "Define Command 5"
         ButtonInEdit = btnCommand5
      Else
         txbCommand.Text = Command(4)
         Application.DoEvents()
         SendCommand()
      End If
   End Sub

   Private Sub btnCommand6_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles btnCommand6.MouseUp
      If e.Button = Windows.Forms.MouseButtons.Right Then
         gbxCommandDef.Location = New Point(160, 216)
         gbxCommandDef.Size = New Size(548, 96)
         gbxCommandDef.Visible = True
         txbCommandBuilder.Text = ""
         btnDefine.Text = "Define Command 6"
         ButtonInEdit = btnCommand6
      Else
         txbCommand.Text = Command(5)
         Application.DoEvents()
         SendCommand()
      End If
   End Sub

   Private Sub btnCommand7_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles btnCommand7.MouseUp
      If e.Button = Windows.Forms.MouseButtons.Right Then
         gbxCommandDef.Location = New Point(160, 216)
         gbxCommandDef.Size = New Size(548, 96)
         gbxCommandDef.Visible = True
         txbCommandBuilder.Text = ""
         btnDefine.Text = "Define Command 7"
         ButtonInEdit = btnCommand7
      Else
         txbCommand.Text = Command(6)
         Application.DoEvents()
         SendCommand()
      End If
   End Sub

   Private Sub btnCommand8_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles btnCommand8.MouseUp
      If e.Button = Windows.Forms.MouseButtons.Right Then
         gbxCommandDef.Location = New Point(160, 216)
         gbxCommandDef.Size = New Size(548, 96)
         gbxCommandDef.Visible = True
         txbCommandBuilder.Text = ""
         btnDefine.Text = "Define Command 8"
         ButtonInEdit = btnCommand8
      Else
         txbCommand.Text = Command(7)
         Application.DoEvents()
         SendCommand()
      End If
   End Sub

   Private Sub btnDefine_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDefine.Click
      Dim Element As Int32

      Element = CInt(ExtractNumberFromString(btnDefine.Text)) - 1
      If txbCommandBuilder.Text <> "" Then
         Command(Element) = txbCommandBuilder.Text
         ButtonInEdit.Text = Command(Element)
      Else
         Command(Element) = ""
         ButtonInEdit.Text = ""
      End If
      gbxCommandDef.Visible = False
   End Sub

#End Region

   Private Sub btnSavePorts_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSavePorts.Click
      frmMain.SaveSettings()
   End Sub
End Class