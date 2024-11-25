Imports System.Net.Mail


Public Class frmSMTPService

   Private Sub frmSMTPService_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
      Dim List() As String
      Dim Names() As String
      Dim Pass As Int32

      Try
         List = LoadData()
         Names = List(0).Split(Chr(44))  'Split on commas
         Me.txbOver.Text = Names(0)
         For Pass = 1 To Names.Length - 1
            Names(Pass) = Names(Pass).Trim(Chr(32))
            Me.txbOver.Text = Me.txbOver.Text & vbCrLf & Names(Pass)
         Next

         Names = List(1).Split(Chr(44))  'Split on commas
         Me.txbError.Text = Names(0)
         For Pass = 1 To Names.Length - 1
            Names(Pass) = Names(Pass).Trim(Chr(32))
            Me.txbError.Text = Me.txbError.Text & vbCrLf & Names(Pass)
         Next

         Me.txbHost.Text = List(2)
         Me.txbPort.Text = List(3)
         Me.txbFrom.Text = List(4)
         Me.txbUserName.Text = List(5)
         Me.txbPassword.Text = "**********"

      Catch ex As Exception
         Me.txbPort.Text = "25"
      End Try

      AddHandler SaveToolStripMenuItem.Click, AddressOf SaveList
   End Sub


   Private Sub SaveList(ByVal sender As Object, ByVal e As System.EventArgs)
      Dim Text As String = ""
      Dim List(16) As String
      Dim Pass As Int32


      'Write the Process complete emial list
      List = Me.txbOver.Text.Replace(Chr(10), "").Split(Chr(13))
      If List(0).Contains("@") Then
         Text = List(0).Trim(Chr(32))
         List(0) = ""  'Clear this element so you can use it in the next loop
      End If
      For Pass = 1 To List.Length - 1
         If List(Pass).Contains("@") Then
            List(Pass) = List(Pass).Trim(Chr(32))
            Text = Text & ", " & List(Pass)
            List(Pass) = ""
         End If
      Next
      Text = Text & vbCrLf

      'Write the Process Error emial list
      List = Me.txbError.Text.Replace(Chr(10), "").Split(Chr(13))
      If List(0).Contains("@") Then
         Text = Text & List(0).Trim(Chr(32))
      End If
      For Pass = 1 To List.Length - 1
         If List(Pass).Contains("@") Then
            List(Pass) = List(Pass).Trim(Chr(32))
            Text = Text & ", " & List(Pass)
         End If
      Next
      Text = Text & vbCrLf

      Text = Text & Me.txbHost.Text.Trim(Chr(32)) & vbCrLf
      Text = Text & Me.txbPort.Text.Trim(Chr(32)) & vbCrLf
      Text = Text & Me.txbFrom.Text.Trim(Chr(32)) & vbCrLf
      Text = Text & Me.txbUserName.Text.Trim(Chr(32)) & vbCrLf
      Text = Text & Me.txbPassword.Text.Trim(Chr(32))
      FileIO.WriteTextFile(Application.StartupPath & "\Email_list.txt", Text, True)
   End Sub

   Function LoadData() As String()
      'This function reads the data storred in the file Email_list.txt and loads the 
      'array MyData() with this format
      'Mydata(0) = email addresses to send a completion message to
      'Mydata(1) = email addresses to send an error message to
      'Mydata(2) = Host name
      'Mydata(3) = Port
      'Mydata(4) = From email address
      'Mydata(5) = Username
      'Mydata(6) = Password
      Dim MyData() As String

      MyData = FileIO.ReadTextFile(Application.StartupPath & "\Email_list.txt").Replace(Chr(10), "").Split(Chr(13))
      Return MyData
   End Function


   Public Sub SendMail(ByVal message As String, ByVal processComplete As Boolean, ByVal processError As Boolean)
      'This subs maintains two email lists.  One to send to when the process is complete
      'and one to send to if an error occures.  You must specify which list you wnat to 
      'mail to.
      'Mydata(0) = email addresses to send a completion message to
      'Mydata(1) = email addresses to send an error message to
      'Mydata(2) = Host name
      'Mydata(3) = Port
      'Mydata(4) = From email address
      'Mydata(5) = Username
      'Mydata(6) = Password
      Dim MailClient As New SmtpClient()
      Dim MailData() As String


      Try
         MailData = LoadData()
         MailClient.Host = MailData(2)
         MailClient.Port = CType(MailData(3), Int32)
         'bob this is new   MailClient.Credentials = New Net.NetworkCredential(MailData(5), MailData(6))
         If processComplete = False Then 'use error list
            MailClient.Send(MailData(4), MailData(1), "J-KEM Scientific Robot Error Message", message)
         Else    'send process complete list
            MailClient.Send(MailData(4), MailData(0), "J-KEM Scientific Robot Process Complete", message)
         End If
      Catch ex As Exception
      End Try
   End Sub


    Private Sub frmSMTPServices_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Me.txbPort.Text = "" Then
            Me.txbPort.Text = "25"
        End If
        If Me.txbHost.Text = "" Or Me.txbFrom.Text = "" Then
         If MessageBox.Show("The form is not complete.  Exit anyway?", "Form Incomplete", MessageBoxButtons.YesNo) = Windows.Forms.DialogResult.No Then
            e.Cancel = True
         End If
        Else
            SaveList(Me, New System.Windows.Forms.MouseEventArgs(Windows.Forms.MouseButtons.None, 1, 0, 0, 0))
        End If
    End Sub

    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub
End Class