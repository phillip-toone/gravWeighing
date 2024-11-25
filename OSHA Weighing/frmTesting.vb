Public Class frmTesting
   Dim ADoorIsOpen As Boolean



   Private Sub frmTesting_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
      ADoorIsOpen = False
   End Sub

   Private Sub btnTestCaroselDriver_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCaroselDriver.Click
      txbCaroselReply.Text = ""
      DelayMS(300)
      txbCaroselReply.Text = Carosel.MyDriver.SendLiteral(":1ST")
   End Sub

   Private Sub btnTestHomeCarosel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnHomeCarosel.Click
      Carosel.Home()
   End Sub

   Private Sub btnTestBarCodeCommunications_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBarCodeCommunications.Click
      txbBarCodeReply.Text = BarCode.SendLiteral("<K221?>")
   End Sub


   Private Sub btnTestBalanceComms_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTestBalanceComms.Click
      Dim Reply As String = ""

      txbBalanceReply.Text = ""
      DelayMS(300)
      Balance.TestComms(Reply)
      txbBalanceReply.Text = Reply
   End Sub

   Private Sub btnGetWeight_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGetWeight.Click
      txbWeight.Text = Balance.Weight
   End Sub

   Private Sub btnOpenDoor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOpenDoor.Click
      Balance.OpenDoor()
   End Sub

   Private Sub btnCloseDoor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCloseDoor.Click
      Balance.CloseDoor()
   End Sub


#Region "Oven Functions"


   Private Sub rdoOven1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rdoOven1.CheckedChanged
      gbxOvens.Text = "Oven One Controls"
   End Sub

   Private Sub rdoOven2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rdoOven2.CheckedChanged
      gbxOvens.Text = "Oven Two Controls"
   End Sub

   Private Sub rdoOven3_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rdoOven3.CheckedChanged
      gbxOvens.Text = "Oven Three Controls"
   End Sub

   Private Sub rdoOven4_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rdoOven4.CheckedChanged
      gbxOvens.Text = "Oven Four Controls"
   End Sub

   Private Sub rdoOven5_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles rdoOven5.CheckedChanged
      gbxOvens.Text = "Oven Five Controls"
   End Sub

   Private Sub btnUSBCommsOven_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUSBCommsOven.Click
      If rdoOven1.Checked = True Then
         txbUSBOven.Text = Oven1.SendLiteral("?(0)")
      ElseIf rdoOven2.Checked = True Then
         txbUSBOven.Text = Oven2.SendLiteral("?(0)")
      ElseIf rdoOven3.Checked = True Then
         txbUSBOven.Text = Oven3.SendLiteral("?(0)")
      ElseIf rdoOven4.Checked = True Then
         txbUSBOven.Text = Oven4.SendLiteral("?(0)")
      ElseIf rdoOven5.Checked = True Then
         txbUSBOven.Text = Oven5.SendLiteral("?(0)")
      End If
   End Sub

   Private Sub btnReadTempOven_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReadTempOven.Click
      If rdoOven1.Checked = True Then
         txbReadTempOven.Text = Oven1.GetTemperature().ToString
      ElseIf rdoOven2.Checked = True Then
         txbReadTempOven.Text = Oven2.GetTemperature().ToString
      ElseIf rdoOven3.Checked = True Then
         txbReadTempOven.Text = Oven3.GetTemperature().ToString
      ElseIf rdoOven4.Checked = True Then
         txbReadTempOven.Text = Oven4.GetTemperature().ToString
      ElseIf rdoOven5.Checked = True Then
         txbReadTempOven.Text = Oven5.GetTemperature().ToString
      End If
   End Sub

   Private Sub btnReadPressOven_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReadPressOven.Click
      If rdoOven1.Checked = True Then
         txbReadPressOven.Text = Oven1.GetPressure().ToString
      ElseIf rdoOven2.Checked = True Then
         txbReadPressOven.Text = Oven2.GetPressure().ToString
      ElseIf rdoOven3.Checked = True Then
         txbReadPressOven.Text = Oven3.GetPressure().ToString
      ElseIf rdoOven4.Checked = True Then
         txbReadPressOven.Text = Oven4.GetPressure().ToString
      ElseIf rdoOven5.Checked = True Then
         txbReadPressOven.Text = Oven5.GetPressure().ToString
      End If
   End Sub

   Private Sub btnOpenDoorOven_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOpenDoorOven.Click
      OvenDef.GotoSafeOpenDoorPosition(1)
      If rdoOven1.Checked = True Then
         Oven1.OpenDoor()
      ElseIf rdoOven2.Checked = True Then
         Oven2.OpenDoor()
      ElseIf rdoOven3.Checked = True Then
         Oven3.OpenDoor()
      ElseIf rdoOven4.Checked = True Then
         Oven4.OpenDoor()
      ElseIf rdoOven5.Checked = True Then
         Oven5.OpenDoor()
      End If
      ADoorIsOpen = True
   End Sub

   Private Sub btnCloseDoorOven_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCloseDoorOven.Click
      OvenDef.GotoSafeOpenDoorPosition(1)
      If rdoOven1.Checked = True Then
         Oven1.CloseDoor()
      ElseIf rdoOven2.Checked = True Then
         Oven2.CloseDoor()
      ElseIf rdoOven3.Checked = True Then
         Oven3.CloseDoor()
      ElseIf rdoOven4.Checked = True Then
         Oven4.CloseDoor()
      ElseIf rdoOven5.Checked = True Then
         Oven5.CloseDoor()
      End If
      ADoorIsOpen = False
   End Sub


   Private Sub btnVacValveOnOven_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnVacValveOnOven.Click
      If rdoOven1.Checked = True Then
         Oven1.SetPressure(100)
      ElseIf rdoOven2.Checked = True Then
         Oven2.SetPressure(100)
      ElseIf rdoOven3.Checked = True Then
         Oven3.SetPressure(100)
      ElseIf rdoOven4.Checked = True Then
         Oven4.SetPressure(100)
      ElseIf rdoOven5.Checked = True Then
         Oven5.SetPressure(100)
      End If
   End Sub

   Private Sub btnVacValveOffOven_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnVacValveOffOven.Click
      If rdoOven1.Checked = True Then
         Oven1.SetPressure(800)
      ElseIf rdoOven2.Checked = True Then
         Oven2.SetPressure(800)
      ElseIf rdoOven3.Checked = True Then
         Oven3.SetPressure(800)
      ElseIf rdoOven4.Checked = True Then
         Oven4.SetPressure(800)
      ElseIf rdoOven5.Checked = True Then
         Oven5.SetPressure(800)
      End If
   End Sub

   Private Sub btnPressReleaseValveOnOven_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPressReleaseValveOnOven.Click
      If rdoOven1.Checked = True Then
         Oven1.OpenVentValve()
      ElseIf rdoOven2.Checked = True Then
         Oven2.OpenVentValve()
      ElseIf rdoOven3.Checked = True Then
         Oven3.OpenVentValve()
      ElseIf rdoOven4.Checked = True Then
         Oven4.OpenVentValve()
      ElseIf rdoOven5.Checked = True Then
         Oven5.OpenVentValve()
      End If
   End Sub

   Private Sub btnPressRelieveValveOffOven_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPressRelieveValveOffOven.Click
      If rdoOven1.Checked = True Then
         Oven1.CloseVentValve()
      ElseIf rdoOven2.Checked = True Then
         Oven2.CloseVentValve()
      ElseIf rdoOven3.Checked = True Then
         Oven3.CloseVentValve()
      ElseIf rdoOven4.Checked = True Then
         Oven4.CloseVentValve()
      ElseIf rdoOven5.Checked = True Then
         Oven5.CloseVentValve()
      End If
   End Sub

#End Region

   Private Sub btnTestOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTestSensors.Click
      Dim OpenSensor As Boolean
      Dim ClosedSensor As Boolean

      lblOpenSensor.Text = "Door OPEN sensor :"
      lblCloseSensor.Text = "Door CLOSED sensor :"
      DelayMS(500)

      If rdoOven1.Checked = True Then
         OpenSensor = Oven1.TestOpenSensor
         ClosedSensor = Oven1.TestCloseSensor
      ElseIf rdoOven2.Checked = True Then
         OpenSensor = Oven2.TestOpenSensor
         ClosedSensor = Oven2.TestCloseSensor
      ElseIf rdoOven3.Checked = True Then
         OpenSensor = Oven3.TestOpenSensor
         ClosedSensor = Oven3.TestCloseSensor
      ElseIf rdoOven4.Checked = True Then
         OpenSensor = Oven4.TestOpenSensor
         ClosedSensor = Oven4.TestCloseSensor
      ElseIf rdoOven5.Checked = True Then
         OpenSensor = Oven5.TestOpenSensor
         ClosedSensor = Oven5.TestCloseSensor
      End If

      If OpenSensor = True Then
         lblOpenSensor.Text = "Door OPEN sensor :   ON"
      Else
         lblOpenSensor.Text = "Door OPEN sensor :   OFF"
      End If
      If ClosedSensor = True Then
         lblCloseSensor.Text = "Door CLOSED sensor :   ON"
      Else
         lblCloseSensor.Text = "Door CLOSED sensor :   OFF"
      End If
   End Sub
End Class