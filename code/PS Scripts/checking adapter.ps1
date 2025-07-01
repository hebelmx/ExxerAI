# Name of the network adapter to filter
$adapterName = "Tailscale"

# Run ipconfig and filter the output for the specified adapter
$output = ipconfig /all | Select-String -Pattern $adapterName -Context 0,20

# Check if the adapter was found in the output
if ($output) {
    # Display the filtered output
    $output.Context.PreContext + $output.Line + $output.Context.PostContext | ForEach-Object { $_.Trim() }
} else {
    Write-Host "Network adapter '$adapterName' not found." -ForegroundColor Red
}


#netsh interface ip add address Tailscale 192.168.25.25 255.255.0.0