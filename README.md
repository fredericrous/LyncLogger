# Lync Logger


logs Microsoft Lync conversations.


## Description

Start the program to begin to log your Lync conversations.
This program adds an icon in the system tray bar.
Right-click brings a menu. You can access the history folder from there or close the program


## Notes

Compatible lync 2010
Should be compatible lync 2013 (dll Microsoft.Lync.Model.dll & Microsoft.Office.Uc.dll may have to be replaced)

To start the program automaticaly at startup you can put a shorcut in the folder "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup"

Files are located to "%appdata%/Lync logs" folder
Source is in "src" folder
The icon that appears in the system tray can be changed. Just replace the file "icon.ico"

## TODO

log audio conversations