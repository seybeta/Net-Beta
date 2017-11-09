Public Class Loader
    Private Sub Loader_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim oMain As New Seybeta.Main
        MessageBox.Show(oMain.Main)
        Me.Close()
        End
    End Sub
End Class