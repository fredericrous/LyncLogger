# Lync Logger


logs Microsoft Lync conversations.


## Description

Start the program to begin to log your Lync conversations.
This program adds an icon in the system tray bar.
Right-click brings a menu. You can access the history folder from there or close the program


## Download

Download the latest version of LyncLogger here:
https://github.com/Zougi/LyncLogger/releases/download/v1.1/LyncLogger.v1.1.zip

## Requirement

This project depends on Microsoft dll:
- Microsoft.Lync.Model.dll
- Microsoft.Office.Uc.dll

Both are available at:
- http://www.microsoft.com/en-us/download/details.aspx?id=18898 (Lync 2010)
- http://www.microsoft.com/en-us/download/details.aspx?id=36824 (Lync 2013)

You should put these dll in the same folder as the executable

The executable also depends on a icon which will appear in the systray bar.
This icon should be in the same folder and called icon.ico


## Notes

Compatible & tested on lync 2010
Should be compatible lync 2013

To start the program automaticaly at startup you can put a shorcut in the folder "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup"

Log files are located to "%appdata%/Lync logs" folder

Sources of the program are located in ./src folder


## TODO

log audio conversations
