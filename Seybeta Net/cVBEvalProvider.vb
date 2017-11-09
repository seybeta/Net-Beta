Imports Microsoft.VisualBasic
Imports System
Imports System.Text
Imports System.CodeDom.Compiler
Imports System.Reflection
Imports System.IO

Public Class cVBEvalProvider
    Private m_oCompilerErrors As CompilerErrorCollection

    Public Property CompilerErrors() As CompilerErrorCollection
        Get
            Return m_oCompilerErrors
        End Get
        Set(ByVal Value As CompilerErrorCollection)
            m_oCompilerErrors = Value
        End Set
    End Property

    Public Sub New()
        MyBase.New()
        m_oCompilerErrors = New CompilerErrorCollection
    End Sub

    Public Function Eval(ByVal vbCode As String) As Object
        Dim oCodeProvider As VBCodeProvider = New VBCodeProvider

        Dim oCParams As CompilerParameters = New CompilerParameters
        Dim oCResults As CompilerResults
        Dim oAssy As System.Reflection.Assembly
        Dim oExecInstance As Object = Nothing
        Dim oRetObj As Object = Nothing
        Dim oMethodInfo As MethodInfo
        Dim oType As Type

        Try
            Dim webClient As New Net.WebClient
            Dim streamReader As StreamReader = New StreamReader(webClient.OpenRead(Main.sAssemblyPath))

            For Each sAssembly As String In streamReader.ReadToEnd.Split(vbLf)
                Try
                    oCParams.ReferencedAssemblies.Add(sAssembly.Replace(Chr(10), String.Empty))
                Catch oExeception As Exception
                    Throw New Exception("[Missing Assembly] " + "{ " + sAssembly + " }")
                End Try
            Next

            oCParams.CompilerOptions = "/t:library"
            oCParams.GenerateInMemory = True

            Try
                oCResults = oCodeProvider.CompileAssemblyFromSource(oCParams, vbCode)

                If oCResults.Errors.Count <> 0 Then
                    Me.CompilerErrors = oCResults.Errors
                    Dim sErrors As String = String.Empty
                    Dim iCounter As Integer = 0
                    For Each oError As CompilerError In oCResults.Errors
                        sErrors += "[ERROR (" + iCounter.ToString() + ")]" + "{ " + oError.ToString + " }" + vbNewLine
                        iCounter = iCounter + 1
                    Next
                    Throw New Exception("[Compiler Error] " + sErrors)
                Else
                    oAssy = oCResults.CompiledAssembly
                    oExecInstance = oAssy.CreateInstance("Seybeta.Main")

                    oType = oExecInstance.GetType
                    oMethodInfo = oType.GetMethod("Main")

                    oRetObj = oMethodInfo.Invoke(oExecInstance, Nothing)
                    Return oRetObj
                End If
            Catch ex As Exception
                Debug.WriteLine(ex.Message)
                MessageBox.Show(ex.Message)
                Stop
            End Try

        Catch ex As Exception
            Debug.WriteLine(ex.Message)
            Stop
        End Try

        Return oRetObj
    End Function
End Class
