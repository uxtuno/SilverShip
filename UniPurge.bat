@echo off

rem Unity用中間ファイル削除バッチコマンド
rem 最終更新日 2016 / 02 / 09

rem メモ : 1バイトの大文字と小文字は区別されない
rem メモ : 2バイトの大文字と小文字は区別される

rem このバッチが存在するフォルダをカレントに
pushd %0\..

rem 不要ファイル削除
del /a- *.suo
del *.csproj
del *.unityproj
del *.userprefs
del *.sln
del *.exe

rem 不要フォルダ削除
rmdir /s /q Temp
rmdir /s /q obj
rmdir /s /q Library
rmdir /s /q .vs

rem ～.sln.ideフォルダを削除したかったが、
rem フォルダ名にワイルドカードが使用できなかった為、
rem やむなく削除しなかった

rem 作成者 久保 諒治

exit