$branches = @(
    "/main/Logun",
    "/main/Phet",
    "/main/May",
    "/main/Ar.Dome",
    "/main/MilkyWay",
    "/main/Maya",
    "/main/Asia",
    "/main/Gen",
    "/main/Pluem"
)

foreach ($branch in $branches) {
    Write-Host "Merging main -> $branch ..." -ForegroundColor Cyan
    cm merge br:/main --to="br:$branch" --merge -c="Merged from main"
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Done: $branch" -ForegroundColor Green
    } else {
        Write-Host "Failed or conflict: $branch (check manually)" -ForegroundColor Red
    }
    Write-Host "---"
}

Write-Host "All done!" -ForegroundColor Magenta
