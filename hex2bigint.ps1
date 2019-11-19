	param(
    [Parameter(
        Position=0, 
        Mandatory=$true, 
        ValueFromPipeline=$true)
	]	
	[string]$s)

	if ($s.StartsWith("0x")) {
		$s = $s.Substring(2)
	}
	if ($s.Length % 2 -eq 1) {
		$s = "0" + $s;
	}

	$return = @()
	
	for ($i = 0; $i -lt $s.Length ; $i += 2)
	{
	$return += [Byte]::Parse($s.Substring($i, 2), [System.Globalization.NumberStyles]::HexNumber)
	}
	
	[Array]::Reverse($return)
	$v = [System.Numerics.BigInteger]::new([byte[]]$return);
	$v = $v / 100000000
	$v.ToString()
