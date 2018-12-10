[Code]
procedure UnInstallPrevision(CurStep: TSetupStep);
var 
  ResultCode: Integer; 
  Uninstall: String; 
  sPrevID: String;
begin 
  sPrevID := '{E611011D-03C9-49D1-8D79-5EF1053F1D6B}';
  if (CurStep = ssInstall) then begin 
    if RegQueryStringValue(HKLM, 'Software\Microsoft\Windows\CurrentVersion\Uninstall\' + sPrevID + '_is1', 'UninstallString', Uninstall) then begin
      if MsgBox( ExpandConstant('{cm:UninstallPrevision}'), mbInformation, MB_YESNO) = IDYES then
        begin
          Exec(RemoveQuotes(Uninstall), ' /SILENT', '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode); 
        end
      else
        begin
          WizardForm.Close;
        end;
    end; 
  end; 
end;