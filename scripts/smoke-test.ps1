param (
    [string]$url,
    [string]$username = "",
    [string]$password = "",
    [int]$durationBetweenRequestsSeconds = 5,
    [int]$maximumDurationMinutes = 2,
    [int]$minimumSequentialSuccessCountRequired = 5
)

if (-not $url) {
    Write-Host "Smoke test URL not provided. Exiting."
    exit 1
}

Write-Host "Starting smoke test with URL: $url"

$base64AuthInfo = if ($username -and $password) {
    [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $username, $password)))
} else {
    ""
}

$maximumDurationSeconds = $maximumDurationMinutes * 60
$attemptNumber = 0
$sequentialSuccessCount = 0
$overallTimer = [Diagnostics.Stopwatch]::StartNew()

do {
    $attemptNumber++
    Write-Host "Attempt $attemptNumber @ $(Get-Date)"
    Write-Host "- Fetching $url"

    $requestTimer = [Diagnostics.Stopwatch]::StartNew()
    try {
        $headers = if ($base64AuthInfo) {
            @{ Authorization = "Basic $base64AuthInfo" }
        } else {
            @{}
        }

        $response = Invoke-WebRequest -Uri $url -Headers $headers
        $requestTimer.Stop()

        if ($response.StatusCode -eq 200) {
            Write-Host "- Success: Status code 200"
            $sequentialSuccessCount++
        } else {
            Write-Host "- Failure: Status code $($response.StatusCode)"
            $sequentialSuccessCount = 0
        }
    } catch {
        Write-Host "- Error: $($_.Exception.Message)"
        $sequentialSuccessCount = 0
    }

    if ($sequentialSuccessCount -ge $minimumSequentialSuccessCountRequired) {
        Write-Host "Smoke test passed after $attemptNumber attempts."
        break
    }

    if ($overallTimer.Elapsed.TotalSeconds -gt $maximumDurationSeconds) {
        Write-Host "Timeout reached. Exiting."
        exit 1
    }

    Start-Sleep -Seconds $durationBetweenRequestsSeconds
} while ($true)

$overallTimer.Stop()
Write-Host "Finished in $($overallTimer.Elapsed.TotalSeconds) seconds."
