@echo off

rem Unity�p���ԃt�@�C���폜�o�b�`�R�}���h
rem �ŏI�X�V�� 2016 / 02 / 09

rem ���� : 1�o�C�g�̑啶���Ə������͋�ʂ���Ȃ�
rem ���� : 2�o�C�g�̑啶���Ə������͋�ʂ����

rem ���̃o�b�`�����݂���t�H���_���J�����g��
pushd %0\..

rem �s�v�t�@�C���폜
del /a- *.suo
del *.csproj
del *.unityproj
del *.userprefs
del *.sln
del *.exe

rem �s�v�t�H���_�폜
rmdir /s /q Temp
rmdir /s /q obj
rmdir /s /q Library
rmdir /s /q .vs

rem �`.sln.ide�t�H���_���폜�������������A
rem �t�H���_���Ƀ��C���h�J�[�h���g�p�ł��Ȃ������ׁA
rem ��ނȂ��폜���Ȃ�����

rem �쐬�� �v�� �Ȏ�

exit