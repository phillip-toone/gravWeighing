Imports System.IO
Imports System.Environment


Module FileIO

   ''' <summary>
   ''' If the application data folder does not exist it creates it and copies all files in the application startup path with the extension of '.kem' to the data folder.
   ''' </summary>
   ''' <remarks></remarks>
   Public Sub CreateDataFileFolders(ByRef path As String)
      'If this is the first time the project has been started, then the path will not exist.
      'This section copies any file with the extension of .kem from teh appliaction startup folder to the new data folder.   In this way
      'the project can copy all read/write files to the application startup path on installation, and then copy them to a special folder 
      'Where the program can read and write to them.
      If My.Computer.FileSystem.DirectoryExists(path) = False Then
         My.Computer.FileSystem.CreateDirectory(path)
         Dim DI As New DirectoryInfo(Application.StartupPath)
         Dim FileList() As FileInfo
         Dim ThisFile As FileInfo
         FileList = DI.GetFiles
         For Each ThisFile In FileList
            If ThisFile.Extension = ".kem" Then
               ThisFile.CopyTo(path & "\" & ThisFile.Name)
            End If
         Next
         DI = Nothing
      End If
   End Sub


    ''' <summary>
    ''' Writes a string to a text file with the extension .txt
    ''' </summary>
    ''' <param name="filepath">The fully qualified file path.</param>
    ''' <param name="text">The text string to write.</param>
    ''' <param name="overWrite">Overwrite the file with the new string if the file exists.</param>
    ''' <param name="Append">Append the new string to the existing file.</param>
    ''' <returns>Returns True of False depending on successful completion of the task</returns>
    ''' <remarks></remarks>
   Public Function WriteTextFile(ByVal filepath As String, ByRef text As String, Optional ByVal overWrite As Boolean = True, Optional ByVal Append As Boolean = False) As Boolean
      If overWrite = False Then
         Dim FI As New FileInfo(filepath)

         If FI.Exists = True Then
            If Append = False Then
               If MessageBox.Show("File " & filepath & " already exists.   Do you want to write over it?", "Delete Existing File", MessageBoxButtons.YesNo, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button2) = DialogResult.No Then
                  FI = Nothing
                  Exit Function
               End If
            End If
         End If
         FI = Nothing
      End If

      Try
         Dim SW As New StreamWriter(filepath, Append)
         SW.Write(text)
         SW.Flush()
         SW.Close()
         SW.Dispose()
         Return True
      Catch ex As Exception
         MessageBox.Show("An Error occured while attemping to write to file: " & filepath & ".   Data was not saved.", "File Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
         Return False
      End Try
   End Function



    ''' <summary>
    ''' Reads and returns the content of a file on disk
    ''' </summary>
    ''' <param name="path">The fully qualified path name of the file</param>
    ''' <returns>The text in the file.  If the file does not exist, it returns nothing</returns>
    ''' <remarks></remarks>
   Public Function ReadTextFile(Optional ByVal path As String = "") As String
      Dim MyText As String = ""

      If path = "" Then
         Dim OD As New OpenFileDialog

         OD.Filter = "Text files (*.txt)|*.txt"
         OD.Title = "Open Existing Text File"
         OD.ValidateNames = True
         OD.ShowDialog()
         path = OD.FileName
         OD.Dispose()
      End If

      If path <> "" Then
         Dim FI As New FileInfo(path)

         If FI.Exists = True Then
            Dim SR As New StreamReader(path)

            FI = Nothing
            MyText = SR.ReadToEnd
            SR.Close()
            SR.Dispose()
         End If
      End If


      Return MyText
   End Function


    ''' <summary>
    ''' Either returns the fully qualified file name of an existing or a new file
    ''' </summary>
    ''' <param name="mustExist">If the file MustExist, it only returns the name of an existing file.  If not, then it only returns the name of a new file.</param>
    ''' <returns>Fully qualified file path.</returns>
    ''' <remarks></remarks>
   Function GetFileName(ByVal mustExist As Boolean, Optional ByVal extension As String = "*.txt", Optional ByVal generateName As Boolean = False) As String
      Dim Path As String = ""

      If mustExist Then
         'use an Open file dialog
         Dim OD As New OpenFileDialog

         OD.Title = "Open Existing Text File"
         OD.Filter = "Text files (" & extension & ")|" & extension
         'OD.Filter = "Text files (*.txt)|.txt"
         OD.ValidateNames = True
         OD.ShowDialog()
         Path = OD.FileName
         OD.Dispose()
      ElseIf generateName = True Then
         Path = GetFolderPath(SpecialFolder.Desktop) & "\DataFile " & My.Settings.DefaultFileExtension.ToString & extension
         My.Settings.DefaultFileExtension += 1
         My.Settings.Save()
      Else
         'use a Save file dialog
         Dim SD As New SaveFileDialog

         SD.Title = "Create a New File"
         SD.Filter = "Text files (" & extension & ")|" & extension
         SD.ValidateNames = True
         SD.ShowDialog()
         Path = SD.FileName
         SD.Dispose()
      End If

      Return Path
   End Function


#Region "MethodData Functions"
   'Use these functions to store and manipulate Method data infromation in text files.  Files must be created,
   'saved, deleted using these functions to be safe so that you can assure yourself that the file format is correct.
   'A Data file structure would look like this:
   'DATANAME:First Method             "First Method" is the qualifier.
   'Line of data
   'Line of data
   'Line of data
   'Line of data
   'Line of data
   'DATANAME:Second Method 
   'Line of data
   'Line of data
   'Line of data
   'Line of data
   'Line of data

   ''' <summary>
   ''' This function returns the names for the input file that contain the qualifier passed to the function
   ''' </summary>
   ''' <param name="filepath">The full file path to the data file</param>
   ''' <returns></returns>
   ''' <remarks></remarks>
   Public Function GetDataTitles(ByVal filepath As String) As Collection
      Dim FI As New FileInfo(filepath)
      Dim Names As New Collection

      Try
         If FI.Exists Then
            Dim FileText() As String
            Dim SR As New StreamReader(filepath)

            FileText = SR.ReadToEnd.Split(Chr(10)) 'Split on LF to get the lines in the file
            SR.Close()
            SR.Dispose()

            For Pass As Int32 = 0 To FileText.Length - 1
               If FileText(Pass).Contains("DATANAME:") Then
                  Names.Add(FileText(Pass).Substring(9, FileText.Length - 10))
               End If
            Next
         End If
      Catch ex As Exception
         Names.Clear()
      End Try
      FI = Nothing

      Return Names
   End Function


   ''' <summary>
   ''' This function returns the entire block of data from one qualifer to the next qualifer.
   ''' It also returns the datablock name following the qualifer.
   ''' </summary>
   ''' <param name="dataName">Method data qualifer text</param>
   ''' <param name="filepath">Full file path to the file</param>
   ''' <returns>Associated data, includeing the method name, in a collection</returns>
   ''' <remarks></remarks>
   Public Function GetDataBlock(ByVal dataName As String, ByVal filepath As String) As Collection
      Dim Filetext() As String
      Dim Names As New Collection

      Try
         Dim SR As New StreamReader(filepath)

         Filetext = SR.ReadToEnd.Split(Chr(10))
         SR.Close()
         SR.Dispose()
         For Pass As Int32 = 0 To Filetext.Length - 1
            If Filetext(Pass) = "DATANAME:" & dataName & vbCr Then
               Names.Add(Filetext(Pass).Substring(9, Filetext(Pass).Length - 10))
               Pass += 1
               While Not Filetext(Pass).Contains("DATANAME:") And Pass <= Filetext.Length - 2
                  Names.Add(Filetext(Pass).Substring(0, Filetext(Pass).Length - 1)) 'Remove trailing Cr
                  Pass += 1
               End While
               Exit For
            End If
         Next
      Catch ex As Exception
         Names.Clear()
      End Try

      Return Names
   End Function


   ''' <summary>
   ''' Deletes the block of data specifed from the data file
   ''' </summary>
   ''' <param name="dataName">Title fo the block to delete</param>
   ''' <param name="filepath">Full file path</param>
   ''' <remarks></remarks>
   Public Sub DeleteDataBlock(ByVal dataName As String, ByVal filepath As String)
      Try
         Dim FI As New FileInfo(filepath)
         If FI.Exists Then
            Dim SR As New StreamReader(filepath)
            Dim Filetext() As String
            Dim Pass As Int32
            Dim Element As Int32

            Filetext = SR.ReadToEnd.Split(Chr(10))
            SR.Close()
            SR.Dispose()
            FI.Delete()
            FI = Nothing

            Dim SW As New StreamWriter(filepath)

            For Pass = 0 To Filetext.Length - 2
               If Filetext(Pass) = "DATANAME:" & dataName & vbCr Then
                  'you found the rack, dont write this data
                  Element = 1
                  Do
                     If Filetext(Pass + Element).Contains("DATANAME:") Or (Pass + Element) >= Filetext.Length - 2 Then
                        Exit Do
                     Else
                        Element += 1
                     End If
                  Loop
                  'Now write the remainder of the file
                  If (Pass + Element) >= Filetext.Length - 2 Then
                     Pass = Filetext.Length - 2 'If this is the end of the file, get out
                  Else
                     Pass = Pass + Element - 1
                  End If
               Else
                  SW.Write(Filetext(Pass) & vbLf)
               End If
            Next
            SW.Close()
            SW.Dispose()
         End If

      Catch ex As Exception
         MessageBox.Show("Error writting data to filename: " & filepath, "File Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
      End Try
   End Sub

   ''' <summary>
   ''' Writes the data passed to data block file.   This sub appends CrLf to the end of each line
   ''' </summary>
   ''' <param name="data">Data as a string array</param>
   ''' <param name="filepath">Full file path</param>
   ''' <remarks></remarks>
   Public Sub WriteDataBlock(ByRef data() As String, ByVal filepath As String)
      Dim SW As New StreamWriter(filepath, True)

      SW.Write("DATANAME:" & data(0) & vbCrLf)
      For Pass As Int32 = 1 To data.Length - 1
         SW.Write(data(Pass) & vbCrLf)
      Next
      SW.Close()
      SW.Dispose()
   End Sub

#End Region



End Module
