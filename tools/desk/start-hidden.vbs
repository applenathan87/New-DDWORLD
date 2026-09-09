' Mawang HR - Desk: start the Node server with NO console window.
' Output goes to tools\desk\desk.log (git-ignored). Used by 출근.bat.
Set sh = CreateObject("WScript.Shell")
Set fso = CreateObject("Scripting.FileSystemObject")
deskDir = fso.GetParentFolderName(WScript.ScriptFullName)          ' ...\tools\desk
repoDir = fso.GetParentFolderName(fso.GetParentFolderName(deskDir)) ' repo root
sh.CurrentDirectory = repoDir
' --no-open: 출근.bat opens the browser itself.  --log: the server appends its own log (a cmd ">>" redirect
' would fail while a running server holds desk.log open, and then node would not start at all).
cmd = "node """ & deskDir & "\server.js"" --no-open --log """ & deskDir & "\desk.log"""
sh.Run cmd, 0, False   ' 0 = hidden window, False = don't wait
