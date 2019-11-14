	param(
    [Parameter(
        Position=0, 
        Mandatory=$true, 
        ValueFromPipeline=$true)
	]	
	[string]$s, 
	[switch]$reverse)

	$return = @()
	
	for ($i = 0; $i -lt $s.Length ; $i += 2)
	{
	$return += [Byte]::Parse($s.Substring($i, 2), [System.Globalization.NumberStyles]::HexNumber)
	}
	
	if ($reverse) {
		[Array]::Reverse($return)
	}

	$v = [System.Numerics.BigInteger]::new([byte[]]$return);
	$v.ToString()
