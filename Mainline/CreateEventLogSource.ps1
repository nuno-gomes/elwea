param($installPath, $toolsPath, $package, $project) 

function Get-EventLogSourceData() {
	[void] [System.Reflection.Assembly]::LoadWithPartialName("System.Drawing") 
	[void] [System.Reflection.Assembly]::LoadWithPartialName("System.Windows.Forms") 

	$objForm = New-Object System.Windows.Forms.Form 
	$objForm.Text = "Data Entry Form"
	$objForm.Size = New-Object System.Drawing.Size(300,200) 
	$objForm.StartPosition = "CenterScreen"

	$objForm.KeyPreview = $True
	$objForm.Add_KeyDown({if ($_.KeyCode -eq "Enter") 
		{$x=$objTextBox.Text;$objForm.Close()}})
	$objForm.Add_KeyDown({if ($_.KeyCode -eq "Escape") 
		{$objForm.Close()}})

	$OKButton = New-Object System.Windows.Forms.Button
	$OKButton.Location = New-Object System.Drawing.Size(75,120)
	$OKButton.Size = New-Object System.Drawing.Size(75,23)
	$OKButton.Text = "OK"
	$OKButton.Add_Click({$x=$objSourceNameTextBox.Text;$y=$objLogNameTextBox.Text;$objForm.Close()})
	$objForm.Controls.Add($OKButton)

	$CancelButton = New-Object System.Windows.Forms.Button
	$CancelButton.Location = New-Object System.Drawing.Size(150,120)
	$CancelButton.Size = New-Object System.Drawing.Size(75,23)
	$CancelButton.Text = "Cancel"
	$CancelButton.Add_Click({$x="";$y="";$objForm.Close()})
	$objForm.Controls.Add($CancelButton)

	$objSourceNameLabel = New-Object System.Windows.Forms.Label
	$objSourceNameLabel.Location = New-Object System.Drawing.Size(10,20) 
	$objSourceNameLabel.Size = New-Object System.Drawing.Size(280,20) 
	$objSourceNameLabel.Text = "Event Source Name:"
	$objForm.Controls.Add($objSourceNameLabel) 

	$objSourceNameTextBox = New-Object System.Windows.Forms.TextBox 
	$objSourceNameTextBox.Location = New-Object System.Drawing.Size(10,40) 
	$objSourceNameTextBox.Size = New-Object System.Drawing.Size(260,20) 
	$objForm.Controls.Add($objSourceNameTextBox) 

	$objLogNameLabel = New-Object System.Windows.Forms.Label
	$objLogNameLabel.Location = New-Object System.Drawing.Size(10,60) 
	$objLogNameLabel.Size = New-Object System.Drawing.Size(280,20) 
	$objLogNameLabel.Text = "Event Log Name:"
	$objForm.Controls.Add($objLogNameLabel) 

	$objLogNameTextBox = New-Object System.Windows.Forms.TextBox 
	$objLogNameTextBox.Location = New-Object System.Drawing.Size(10,80) 
	$objLogNameTextBox.Size = New-Object System.Drawing.Size(260,20) 
	$objForm.Controls.Add($objLogNameTextBox) 

	$objForm.Topmost = $True

	$objForm.Add_Shown({$objForm.Activate()})

	[void] $objForm.ShowDialog()
	
	[hashtable]$Return = @{} 
	$Return.Success = $x -ne "" 
	$Return.EventSourceName = $x 
	if($y -ne "") {
		$Return.EventLogName = $y
	}
	else {
		$Return.EventLogName = "Application"
	}
	
	Return $Return 
}

$formData = Get-EventLogSourceData

if ($formData.Success) {
	$creationData = new-object System.Diagnostics.EventSourceCreationData($formData.EventSourceName, $formData.EventLogName)
	
	$creationData.CategoryCount = 5
	$creationData.CategoryResourceFile = "c:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_rc.dll"
	$creationData.MessageResourceFile = "c:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_rc.dll"
	# $creationData.ParameterResourceFile = ""

	if (![System.Diagnostics.EventLog]::SourceExists($creationData.Source))
	{      
		[System.Diagnostics.EventLog]::CreateEventSource($creationData)  
	} 

	if ([System.Diagnostics.EventLog]::Exists($creationData.LogName))
	{
		$eventlogs = [System.Diagnostics.EventLog]::GetEventLogs()
		foreach ($evtlog in $eventlogs)
		{
			if ($evtlog.Log -eq $creationData.LogName)
			{
				$evtlog.ModifyOverflowPolicy([System.Diagnostics.OverflowAction]::OverwriteAsNeeded, $evtlog.MinimumRetentionDays);
				$evtlog.MaximumKilobytes = 20096;
			}
		}
	}
}