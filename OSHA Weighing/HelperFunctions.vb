Module HelperFunctions


   Public Function GetTime(ByRef myTextBox As TextBox) As Int32
      'This function looks at the supplied text box to see if time is entered in the format
      'of xxx:xx:xx.  If the format is incorrect, the function clears the textbox and returns
      'a value of int32.minval
      'Requirements:
      'The input must have 2 ":" or ";"
      'The seconds and minutes groups can have 0-2 digits.
      Dim returnValue As Int32 = 0
      Dim mychar(1) As Char
      Dim mystrings() As String
      Dim seconds As Int32
      Dim minutes As Int32
      Dim hours As Int32

      Try
         'Varify the input format is OK
         mychar(0) = CType(":", Char)
         mychar(1) = CType(";", Char)
         mystrings = myTextBox.Text.Split(mychar)
         If mystrings.GetUpperBound(0) = 2 Then  'Three substrings were created
            If mystrings(2) <> "" Then  'holds the seconds
               If IsInteger(mystrings(2)) = False Or mystrings(2).Length > 2 Then
                  returnValue = Int32.MinValue
               End If
            End If
            If mystrings(1) <> "" Then  'holds the minutes
               If IsInteger(mystrings(1)) = False Or mystrings(1).Length > 2 Then
                  returnValue = Int32.MinValue
               End If
            End If
            If mystrings(0) <> "" Then  'holds the hours
               If IsInteger(mystrings(0)) = False Then
                  returnValue = Int32.MinValue
               End If
            End If
         Else
            returnValue = Int32.MinValue
         End If

         If returnValue = 0 Then
            If mystrings(2) <> "" Then
               seconds = CType(mystrings(2), Int32)
            End If
            If mystrings(1) <> "" Then
               minutes = CType(mystrings(1), Int32)
               seconds = seconds + minutes * 60
            End If
            If mystrings(0) <> "" Then
               hours = CType(mystrings(0), Int32)
               seconds = seconds + hours * 3600
            End If
            returnValue = seconds
         End If

      Catch ex As Exception
         returnValue = Int32.MinValue
      End Try

      If returnValue = Int32.MinValue Then
         myTextBox.Text = "00:00:00"
         MessageBox.Show("Error in time format.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
      End If

      Return returnValue
   End Function


   Public Function FormatTime(ByVal myTime As TimeSpan, ByVal withDays As Boolean, ByVal withHours As Boolean, ByVal withMinutes As Boolean, ByVal withSeconds As Boolean) As String

      'This function returns a string with the specified formatting
      Dim TimeString As String = ""

      If withDays = True Then
         TimeString = myTime.Days.ToString
         If withHours = True Then
            If TimeString = "" Then
               TimeString = Format(myTime.Hours, "0")
            Else
               TimeString = TimeString & ":" & Format(myTime.Hours, "00")
            End If
         End If
      Else
         If withHours = True Then
            If TimeString = "" Then
               TimeString = Format(((myTime.Days * 24) + myTime.Hours), "0")
            Else
               TimeString = TimeString & ":" & Format(myTime.Hours, "00")
            End If
         End If
      End If

      If withMinutes = True Then
         If TimeString = "" Then
            TimeString = Format(myTime.Minutes, "0")
         Else
            TimeString = TimeString & ":" & Format(myTime.Minutes, "00")
         End If
      End If
      If withSeconds = True Then
         If TimeString = "" Then
            TimeString = Format(myTime.Seconds, "0")
         Else
            If myTime.Milliseconds >= 500 Then
               TimeString = TimeString & ":" & Format((myTime.Seconds + 1), "00")
            Else
               TimeString = TimeString & ":" & Format(myTime.Seconds, "00")
            End If
         End If
      End If


      Return TimeString
   End Function

   Public Function IsInteger(ByRef myString As String, Optional ByVal minimum As Int32 = Int32.MinValue, Optional ByVal maximum As Int32 = Int32.MaxValue, Optional ByRef IntNumber As Int32 = 0) As Boolean
      'This function examines the string and returns true if the string contains nothing
      'except digits and is between the min and max values specified
      Dim answer As Boolean = True
      Dim position As Int32

      Try
         If myString <> "" Then
            For position = 0 To myString.Length - 1
               answer = Char.IsDigit(myString.Chars(position))
               If answer = False Then
                  If Not (position = 0 And myString.Chars(0) = "-") Then
                     Return False
                  End If
               End If
            Next
            position = CType(myString, Int32)
            If position < minimum Or position > maximum Then
               answer = False
            End If
         Else
            answer = False
         End If
         If IntNumber <> 0 Then
            IntNumber = position
         End If

      Catch ex As Exception
         Return False
      End Try

      Return answer
   End Function


   Public Function IsDouble(ByRef myString As String, Optional ByVal minimum As Double = Double.MinValue, Optional ByVal maximum As Double = Double.MaxValue, Optional ByRef number As Double = Nothing) As Boolean
      'This function examines the string and returns true if the string contains nothing
      'except digits and is between the min and max for an a Double
      Dim answer As Boolean = True
      Dim decimalFound As Boolean = False
      Dim position As Int32
      Dim value As Double

      Try
         If myString <> "" Then
            For position = 0 To myString.Length - 1
               If Char.IsDigit(myString.Chars(position)) Or _
                  (position = 0 And myString.Chars(0) = "-") Or _
                  (myString.Chars(position) = "." And decimalFound = False) Then
                  If myString.Chars(position) = "." Then
                     decimalFound = True
                  End If
               Else
                  Return False
               End If
            Next
            value = CType(myString, Double)
            If value < minimum Or value > maximum Then
               answer = False
            End If
            If Not number = Nothing Then
               number = value
            End If
         Else
            answer = False
         End If

      Catch ex As Exception
         Return False
      End Try

      Return answer
   End Function


   Public Sub DelayMS(ByVal Delay As Int32)
      '***********************************************************************
      Dim EndTime As DateTime

      EndTime = Now.AddMilliseconds(Convert.ToDouble(Delay))
      While DateTime.Compare(Now, EndTime) < 0.0
         Application.DoEvents()
      End While
   End Sub


   Public Sub DelaySeconds(ByVal Delay As Int32)
      '***********************************************************************
      Dim EndTime As DateTime

      EndTime = Now.AddSeconds(Convert.ToDouble(Delay))
      While DateTime.Compare(Now, EndTime) < 0.0
         Application.DoEvents()
      End While
   End Sub

   Public Sub DelaySeconds(ByVal Delay As TimeSpan)
      '***********************************************************************
      Dim EndTime As DateTime

      EndTime = Now.Add(Delay)
      While DateTime.Compare(Now, EndTime) < 0.0
         Application.DoEvents()
      End While
   End Sub

   Public Function FindMinimum(ByRef NumArray() As Int32, Optional ByVal startElement As Int32 = 0, Optional ByVal endElement As Int32 = Int32.MaxValue) As Int32
      Dim Element As Int32
      Dim Min As Int32 = Int32.MaxValue

      If endElement = Int32.MaxValue Then
         endElement = NumArray.Length - 1
      End If

      For Element = startElement To endElement
         Try
            If NumArray(Element) < Min Then
               Min = NumArray(Element)
            End If
         Catch ex As Exception
         End Try
      Next

      Return Min
   End Function

   Public Function FindMinimum(ByRef NumArray() As Double, Optional ByVal startElement As Int32 = 0, Optional ByVal endElement As Int32 = Int32.MaxValue) As Double
      Dim Element As Int32
      Dim Min As Double = Double.MaxValue

      If endElement = Int32.MaxValue Then
         endElement = NumArray.Length - 1
      End If

      For Element = startElement To endElement
         Try
            If NumArray(Element) < Min Then
               Min = NumArray(Element)
            End If
         Catch ex As Exception
         End Try
      Next

      Return Min
   End Function

   Public Function FindMaximum(ByRef NumArray() As Int32, Optional ByVal startElement As Int32 = 0, Optional ByVal endElement As Int32 = Int32.MaxValue) As Int32
      Dim Element As Int32
      Dim Max As Int32 = Int32.MinValue

      If endElement = Int32.MaxValue Then
         endElement = NumArray.Length - 1
      End If

      For Element = startElement To endElement
         Try
            If NumArray(Element) > Max Then
               Max = NumArray(Element)
            End If
         Catch ex As Exception
         End Try
      Next

      Return Max
   End Function

   Public Function FindMaximum(ByRef NumArray() As Double, Optional ByVal startElement As Int32 = 0, Optional ByVal endElement As Int32 = Int32.MaxValue) As Double
      Dim Element As Int32
      Dim Max As Double = Double.MinValue

      If endElement = Int32.MaxValue Then
         endElement = NumArray.Length - 1
      End If

      For Element = startElement To endElement
         Try
            If NumArray(Element) > Max Then
               Max = NumArray(Element)
            End If
         Catch ex As Exception
         End Try
      Next

      Return Max
   End Function

   ''' <summary>
   ''' Returns a value within the limits of min and max.
   ''' </summary>
   ''' <param name="input">The number to operate on</param>
   ''' <param name="min">The minimum allowed value</param>
   ''' <param name="max">The maximum allowed value</param>
   ''' <returns></returns>
   ''' <remarks></remarks>
   Function EnforceLimits(ByRef input As Int32, ByVal min As Int32, ByVal max As Int32) As Int32
      If input < min Then
         input = min
      ElseIf input > max Then
         input = max
      End If

      Return input
   End Function


   ''' <summary>
   ''' Returns a value within the limits of min and max.
   ''' </summary>
   ''' <param name="input">The number to operate on</param>
   ''' <param name="min">The minimum allowed value</param>
   ''' <param name="max">The maximum allowed value</param>
   ''' <returns></returns>
   ''' <remarks></remarks>
   Function EnforceLimits(ByRef input As Double, ByVal min As Double, ByVal max As Double) As Double
      If input < min Then
         input = min
      ElseIf input > max Then
         input = max
      End If

      Return input
   End Function


   Sub CountDownTimer(ByVal myTime As Int32, ByVal Title As String)
      Dim myfrmCountDownTimer As New frmCountDownTimer(myTime, Title)
      myfrmCountDownTimer.ShowDialog()
      myfrmCountDownTimer = Nothing
   End Sub

   ''' <summary>
   ''' Extracts the first numeric value it finds in a string including decimal point and decimals
   ''' </summary>
   ''' <param name="myString">String to extact the number from</param>
   ''' <returns>The extracted number as a string</returns>
   ''' <remarks></remarks>
   Function ExtractNumberFromString(ByRef myString As String) As String
      Dim MyChars() As Char
      Dim Pass As Int32
      Dim FoundNumbers As Boolean
      Dim FoundDecimal As Boolean
      Dim FoundSign As Boolean
      Dim MyReturn As String = ""


      MyChars = myString.ToCharArray
      For Pass = 0 To MyChars.Length - 1
         If Asc(MyChars(Pass)) >= 48 And Asc(MyChars(Pass)) <= 57 Then   'numbers 
            FoundNumbers = True
            MyReturn += MyChars(Pass)

         ElseIf Asc(MyChars(Pass)) = 46 Then 'Decimal point
            If FoundDecimal = True Then
               Exit For    'A second decimal point is the end of input
            End If
            If FoundNumbers = True Then
               MyReturn += MyChars(Pass)
               FoundDecimal = True
            Else    'Is there a number immediatlely after the decimal?
               If Pass <= MyChars.Length - 2 Then
                  If Asc(MyChars(Pass + 1)) >= 48 And Asc(MyChars(Pass + 1)) <= 57 Then
                     MyReturn += MyChars(Pass)
                     FoundDecimal = True
                  End If
               End If
            End If

         ElseIf Asc(MyChars(Pass)) = 45 Then 'Negative sign
            If FoundSign = True Then
               Exit For    'cant have 2 signs
            End If
            'Two situations are allowed.  1) the next char is a digit 2) the next char is a decimal followed by a digit
            If Pass <= MyChars.Length - 2 Then
               If Asc(MyChars(Pass + 1)) >= 48 And Asc(MyChars(Pass + 1)) <= 57 Then
                  MyReturn += MyChars(Pass)
                  FoundSign = True
               End If
            End If
            If Pass <= MyChars.Length - 3 And Asc(MyChars(Pass + 1)) = 46 Then
               If Asc(MyChars(Pass + 2)) >= 48 And Asc(MyChars(Pass + 2)) <= 57 Then
                  MyReturn += MyChars(Pass)
                  FoundSign = True
               End If
            End If
         Else
            If FoundNumbers = True Then
               Exit For
            End If
         End If
      Next

      Return MyReturn
   End Function


#Region "BIT manupulation functions"

   ''' <summary>
   ''' Clears the 0-based specified bit
   ''' </summary>
   ''' <param name="data">Integer to modify</param>
   ''' <param name="bit">0 to 31</param>
   ''' <remarks></remarks>
   Sub ClearBit(ByRef data As Int32, ByVal bit As Int32)
      Dim BitMask As Int32 = 1

      BitMask = BitMask << bit
      data = data And Not BitMask
   End Sub

   Sub ClearBit(ByRef data As Int16, ByVal bit As Int32)
      Dim BitMask As Int16 = 1

      BitMask = BitMask << bit
      data = data And Not BitMask
   End Sub

   Sub ClearBit(ByRef data As Byte, ByVal bit As Int32)
      Dim BitMask As Byte = 1

      BitMask = BitMask << bit
      data = data And Not BitMask
   End Sub

   ''' <summary>
   ''' Examines the specified bit of an integer number
   ''' </summary>
   ''' <param name="data">Integer to examine</param>
   ''' <param name="bit">0 based bit to examine</param>
   ''' <returns>True if bit is 1, False if 0</returns>
   ''' <remarks></remarks>
   Function ExamineBit(ByVal data As Int32, ByVal bit As Int32) As Boolean
      Dim BitMask As Int32 = 1

      BitMask = BitMask << bit
      If (data And BitMask) > 0 Then
         Return True
      Else
         Return False
      End If
   End Function

   Function ExamineBit(ByVal data As Int16, ByVal bit As Int32) As Boolean
      Dim BitMask As Int16 = 1

      BitMask = BitMask << bit
      If (data And BitMask) > 0 Then
         Return True
      Else
         Return False
      End If
   End Function

   Function ExamineBit(ByVal data As Byte, ByVal bit As Int32) As Boolean
      Dim BitMask As Byte = 1

      BitMask = BitMask << bit
      If (data And BitMask) > 0 Then
         Return True
      Else
         Return False
      End If
   End Function

   ''' <summary>
   ''' Sets the secifed bit to 1 in the integer passed
   ''' </summary>
   ''' <param name="data">Interger number to operate on</param>
   ''' <param name="bit">0 based bit to set</param>
   ''' <remarks></remarks>
   Sub SetBit(ByRef data As Int32, ByVal bit As Int32)
      Dim BitMask As Int32 = 1

      BitMask = BitMask << bit
      data = data Or BitMask
   End Sub

   Sub SetBit(ByRef data As Int16, ByVal bit As Int32)
      Dim BitMask As Int16 = 1

      BitMask = BitMask << bit
      data = data Or BitMask
   End Sub

   Sub SetBit(ByRef data As Byte, ByVal bit As Int32)
      Dim BitMask As Byte = 1

      BitMask = BitMask << bit
      data = data Or BitMask
   End Sub

   ''' <summary>
   ''' Toggles the state of the specified bit in the referenced integer.
   ''' </summary>
   ''' <param name="data">Integer to operate on</param>
   ''' <param name="bit">0 based bit to toggle</param>
   ''' <remarks></remarks>
   Sub ToggleBit(ByRef data As Int32, ByVal bit As Int32)
      Dim BitMask As Int32 = 1

      BitMask = BitMask << bit
      data = data Xor BitMask
   End Sub

   Sub ToggleBit(ByRef data As Int16, ByVal bit As Int32)
      Dim BitMask As Int16 = 1

      BitMask = BitMask << bit
      data = data Xor BitMask
   End Sub

   Sub ToggleBit(ByRef data As Byte, ByVal bit As Int32)
      Dim BitMask As Byte = 1

      BitMask = BitMask << bit
      data = data Xor BitMask
   End Sub

#End Region


#Region "ASCII Code Functions"

   ''' <summary>
   ''' Returns the ACSII code for the keyboard key pressed.  Implemented for text keys, number keys, symbol keys.
   ''' </summary>
   ''' <param name="keyValue">e.keyvalue of the key pressed.</param>
   ''' <param name="shiftSet">State of the ShiftKey.  Does not check for Cap Lock</param>
   ''' <returns>Ascii value of the key.   0 if the key is not defined.</returns>
   ''' <remarks></remarks>
   Public Function GetAcsiiCode(ByVal keyValue As Int32, ByVal shiftSet As Boolean) As Int32
      Dim AsciiValue As Int32 = 0

      If keyValue >= 48 And keyValue <= 57 Then   'These are the 0 to 9 keys
         If shiftSet = False Then
            AsciiValue = keyValue
         Else
            Select Case keyValue
               Case 48  'shift 0
                  AsciiValue = 41
               Case 49  'shift 1
                  AsciiValue = 33
               Case 50  'shift 2
                  AsciiValue = 64
               Case 51  'shift 3
                  AsciiValue = 35
               Case 52  'shift 4
                  AsciiValue = 36
               Case 53  'shift 5
                  AsciiValue = 37
               Case 54  'shift 6
                  AsciiValue = 94
               Case 55  'shift 7
                  AsciiValue = 38
               Case 56  'shift 8
                  AsciiValue = 42
               Case 57  'shift 9
                  AsciiValue = 40
            End Select
         End If

      ElseIf keyValue >= 65 And keyValue <= 90 Then   'Text keys A to Z
         If shiftSet = False Then
            AsciiValue = keyValue + 32
         Else
            AsciiValue = keyValue
         End If

      Else
         Select Case keyValue
            Case 192
               If shiftSet = False Then
                  AsciiValue = 96
               Else
                  AsciiValue = 126
               End If
            Case 189
               If shiftSet = False Then
                  AsciiValue = 45
               Else
                  AsciiValue = 95
               End If
            Case 187
               If shiftSet = False Then
                  AsciiValue = 61
               Else
                  AsciiValue = 43
               End If
            Case 219
               If shiftSet = False Then
                  AsciiValue = 91
               Else
                  AsciiValue = 123
               End If
            Case 221
               If shiftSet = False Then
                  AsciiValue = 93
               Else
                  AsciiValue = 125
               End If
            Case 220
               If shiftSet = False Then
                  AsciiValue = 92
               Else
                  AsciiValue = 124
               End If
            Case 186
               If shiftSet = False Then
                  AsciiValue = 59
               Else
                  AsciiValue = 58
               End If
            Case 227
               If shiftSet = False Then
                  AsciiValue = 39
               Else
                  AsciiValue = 34
               End If
            Case 188
               If shiftSet = False Then
                  AsciiValue = 44
               Else
                  AsciiValue = 60
               End If
            Case 190
               If shiftSet = False Then
                  AsciiValue = 46
               Else
                  AsciiValue = 62
               End If
            Case 191
               If shiftSet = False Then
                  AsciiValue = 47
               Else
                  AsciiValue = 63
               End If
            Case 32  'Space bar
               AsciiValue = 32
         End Select
      End If

      Return AsciiValue
   End Function
#End Region


End Module
