VERSION 5.00
Object = "{BDC217C8-ED16-11CD-956C-0000C04E4C0A}#1.1#0"; "tabctl32.ocx"
Object = "{F9043C88-F6F2-101A-A3C9-08002B2F49FB}#1.2#0"; "COMDLG32.OCX"
Begin VB.Form frmOptions 
   BorderStyle     =   3  'Fixed Dialog
   Caption         =   "Options"
   ClientHeight    =   4890
   ClientLeft      =   2760
   ClientTop       =   3750
   ClientWidth     =   9930
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   4890
   ScaleWidth      =   9930
   ShowInTaskbar   =   0   'False
   StartUpPosition =   2  'CenterScreen
   Begin MSComDlg.CommonDialog cm 
      Left            =   2520
      Top             =   4560
      _ExtentX        =   847
      _ExtentY        =   847
      _Version        =   393216
   End
   Begin TabDlg.SSTab SSTab1 
      Height          =   4215
      Left            =   120
      TabIndex        =   2
      Top             =   120
      Width           =   9735
      _ExtentX        =   17171
      _ExtentY        =   7435
      _Version        =   393216
      Style           =   1
      Tabs            =   4
      Tab             =   3
      TabsPerRow      =   6
      TabHeight       =   520
      TabCaption(0)   =   "General"
      TabPicture(0)   =   "mpv_FrmOptions.frx":0000
      Tab(0).ControlEnabled=   0   'False
      Tab(0).Control(0)=   "cmdTextEditor"
      Tab(0).Control(1)=   "editor"
      Tab(0).Control(2)=   "Label1"
      Tab(0).ControlCount=   3
      TabCaption(1)   =   "Application"
      TabPicture(1)   =   "mpv_FrmOptions.frx":001C
      Tab(1).ControlEnabled=   0   'False
      Tab(1).Control(0)=   "WebServer"
      Tab(1).Control(1)=   "Label2"
      Tab(1).ControlCount=   2
      TabCaption(2)   =   "SQL Server"
      TabPicture(2)   =   "mpv_FrmOptions.frx":0038
      Tab(2).ControlEnabled=   0   'False
      Tab(2).Control(0)=   "SQLLogin"
      Tab(2).Control(1)=   "SQLPassWord"
      Tab(2).Control(2)=   "SQLDatabase"
      Tab(2).Control(3)=   "SQLServer"
      Tab(2).Control(4)=   "Label7"
      Tab(2).Control(5)=   "Label6"
      Tab(2).Control(6)=   "Label4"
      Tab(2).Control(7)=   "Label3"
      Tab(2).ControlCount=   8
      TabCaption(3)   =   "Data"
      TabPicture(3)   =   "mpv_FrmOptions.frx":0054
      Tab(3).ControlEnabled=   -1  'True
      Tab(3).Control(0)=   "Label8"
      Tab(3).Control(0).Enabled=   0   'False
      Tab(3).Control(1)=   "SchedulerMaxMinute"
      Tab(3).Control(1).Enabled=   0   'False
      Tab(3).ControlCount=   2
      Begin VB.TextBox SchedulerMaxMinute 
         Height          =   335
         Left            =   2520
         TabIndex        =   17
         Top             =   480
         Width           =   6975
      End
      Begin VB.TextBox SQLLogin 
         Height          =   335
         Left            =   -72360
         TabIndex        =   16
         Top             =   1200
         Width           =   6975
      End
      Begin VB.TextBox SQLPassWord 
         Height          =   335
         Left            =   -72360
         TabIndex        =   15
         Top             =   1560
         Width           =   6975
      End
      Begin VB.TextBox SQLDatabase 
         Height          =   335
         Left            =   -72360
         TabIndex        =   10
         Top             =   840
         Width           =   6975
      End
      Begin VB.TextBox SQLServer 
         Height          =   335
         Left            =   -72360
         TabIndex        =   8
         Top             =   480
         Width           =   6975
      End
      Begin VB.TextBox WebServer 
         Height          =   335
         Left            =   -72480
         TabIndex        =   6
         Top             =   480
         Width           =   6975
      End
      Begin VB.CommandButton cmdTextEditor 
         Caption         =   "..."
         Height          =   335
         Left            =   -65760
         TabIndex        =   5
         Top             =   600
         Width           =   335
      End
      Begin VB.TextBox editor 
         Height          =   335
         Left            =   -72840
         TabIndex        =   3
         Top             =   600
         Width           =   6975
      End
      Begin VB.Label Label8 
         Caption         =   "Refreh every (Minutes)"
         Height          =   375
         Left            =   120
         TabIndex        =   18
         Top             =   480
         Width           =   2895
      End
      Begin VB.Label Label7 
         Caption         =   "Password :"
         Height          =   375
         Left            =   -74760
         TabIndex        =   14
         Top             =   1560
         Width           =   2895
      End
      Begin VB.Label Label6 
         Caption         =   "Login :"
         Height          =   375
         Left            =   -74760
         TabIndex        =   13
         Top             =   1200
         Width           =   2895
      End
      Begin VB.Label Label4 
         Caption         =   "Database :"
         Height          =   375
         Left            =   -74760
         TabIndex        =   11
         Top             =   840
         Width           =   2895
      End
      Begin VB.Label Label3 
         Caption         =   "SQL Server :"
         Height          =   375
         Left            =   -74760
         TabIndex        =   9
         Top             =   480
         Width           =   2895
      End
      Begin VB.Label Label2 
         Caption         =   "Web Server :"
         Height          =   375
         Left            =   -74880
         TabIndex        =   7
         Top             =   480
         Width           =   2895
      End
      Begin VB.Label Label1 
         Caption         =   "Text Editor :"
         Height          =   375
         Left            =   -74760
         TabIndex        =   4
         Top             =   600
         Width           =   2895
      End
   End
   Begin VB.CommandButton cmdCancel 
      Cancel          =   -1  'True
      Caption         =   "Cancel"
      Height          =   375
      Left            =   8640
      TabIndex        =   1
      Top             =   4440
      Width           =   1215
   End
   Begin VB.CommandButton cmdOK 
      Caption         =   "OK"
      Default         =   -1  'True
      Height          =   375
      Left            =   7320
      TabIndex        =   0
      Top             =   4440
      Width           =   1215
   End
   Begin VB.Label Label5 
      Caption         =   "Database :"
      Height          =   375
      Left            =   360
      TabIndex        =   12
      Top             =   1320
      Width           =   2895
   End
End
Attribute VB_Name = "frmOptions"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False

Option Explicit

Private m_booOK As Boolean

Private Sub cmdCancel_Click()
    Hide
End Sub

Private Sub cmdOK_Click()
    m_booOK = True
    Hide
End Sub

Private Sub cmdTextEditor_Click()
    Dim objTool     As New cTool
    Dim strEditor   As String
    
    strEditor = objTool.getUserOpenFile(CM, "Text Editor", "Program Files (*.exe)|*.exe", False)
    If (Len(strEditor)) Then
        editor.Text = strEditor
    End If
    
End Sub

'Private Sub Command1_Click()
'    Dim objWinApi   As New cWindows
'    Dim strPath     As String
'
'    strPath = objWinApi.getBrowseDirectory(0, TESTHARNESS_MESSAGE_7006)
'    If (Len(strPath)) Then
'        DatabasePath.Text = strPath
'
'    End If
'End Sub

Private Sub Form_Load()
    m_booOK = False
End Sub

Public Function OpenDialog(objIniFile As cIniFile) As Boolean
    
    
    
    objIniFile.getForm Me
    
    
    
    Me.Show vbModal
    
    If (m_booOK) Then
        objIniFile.setForm Me
        
        OpenDialog = True
    End If
    Unload Me
    Set frmOptions = Nothing
    
End Function



