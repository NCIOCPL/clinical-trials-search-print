# Check if the input file name is provided
if ($args.Count -eq 0) {
    Write-Host "Please provide the input file name as a command line argument."
    exit
}

# Get the input file name from the command line arguments
$inputFile = $args[0]

# Headers
$headers = @("key", "cacheDate", "trialIDs", "searchParams", "content")

# Read the file content
$records = Import-Csv -Path $inputFile -Header $headers

# Loop over each record
foreach ($record in $records) {
	Write-Output ("key: {0}" -f $record.key)
	Write-Output ("cacheDate: {0}" -f $record.cacheDate)
	Write-Output ("trialIDs: {0}" -f $record.trialIDs)
	Write-Output ("searchParams: {0}" -f $record.searchParams)
}
