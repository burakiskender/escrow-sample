	param(
    [Parameter(
        Position=0, 
        Mandatory=$true, 
        ValueFromPipeline=$true)
	]	
	[string]$s)
	$return = @()
	
	for ($i = 0; $i -lt $s.Length ; $i += 2)
	{
	$return += [Byte]::Parse($s.Substring($i, 2), [System.Globalization.NumberStyles]::HexNumber)
	}
	
	$encoder = new-object System.Text.UTF8Encoding
	$encoder.GetString($return)
