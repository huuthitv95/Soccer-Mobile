param(
    [string]$DocsRoot = (Join-Path $PSScriptRoot '..\docs'),
    [switch]$CheckExternalLinks
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath $DocsRoot).Path
$errors = [System.Collections.Generic.List[string]]::new()
$files = @(Get-ChildItem -LiteralPath $root -Recurse -File -Filter '*.md')
$externalLinks = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

function Add-Error([string]$Path, [string]$Message) {
    $relative = [System.IO.Path]::GetRelativePath($root, $Path).Replace('\', '/')
    $errors.Add("${relative}: $Message")
}

function Get-Anchors([string]$Content) {
    $anchors = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($match in [regex]::Matches($Content, '<a\s+id=["'']([^"'']+)["'']\s*></a>', 'IgnoreCase')) {
        [void]$anchors.Add($match.Groups[1].Value)
    }

    $seen = @{}
    foreach ($line in ($Content -split "`r?`n")) {
        if ($line -notmatch '^#{1,6}\s+(.+?)\s*$') { continue }
        $slug = $Matches[1].ToLowerInvariant()
        $slug = [regex]::Replace($slug, '<[^>]+>', '')
        $slug = [regex]::Replace($slug, '[^\p{L}\p{Nd}\s_-]', '')
        $slug = [regex]::Replace($slug.Trim(), '\s', '-')
        if ($seen.ContainsKey($slug)) {
            $seen[$slug]++
            $slug = "$slug-$($seen[$slug])"
        } else {
            $seen[$slug] = 0
        }
        [void]$anchors.Add($slug)
    }
    return $anchors
}

foreach ($file in $files) {
    $content = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
    $h1Count = @([regex]::Matches($content, '(?m)^#\s+')).Count
    if ($h1Count -ne 1) { Add-Error $file.FullName "expected exactly one H1, found $h1Count" }

    if ($file.Name -ne 'index.md' -and $file.Name -notmatch '^[a-z0-9]+(?:-[a-z0-9]+)*\.md$') {
        Add-Error $file.FullName 'filename must use lowercase kebab-case'
    }
    if ($content -match '\[\[[^\]]+\]\]') { Add-Error $file.FullName 'legacy wikilink detected' }
    if ((@([regex]::Matches($content, '(?m)^```')).Count % 2) -ne 0) { Add-Error $file.FullName 'unbalanced fenced code block' }

    $explicit = @([regex]::Matches($content, '<a\s+id=["'']([^"'']+)["'']\s*></a>', 'IgnoreCase') | ForEach-Object { $_.Groups[1].Value })
    $duplicates = @($explicit | Group-Object | Where-Object Count -gt 1)
    foreach ($duplicate in $duplicates) { Add-Error $file.FullName "duplicate explicit anchor '$($duplicate.Name)'" }

    foreach ($match in [regex]::Matches($content, '(?<!\!)\[[^\]]+\]\(([^)]+)\)')) {
        $raw = $match.Groups[1].Value.Trim().Trim('<', '>')
        if ($raw -match '^https?://') {
            [void]$externalLinks.Add($raw)
            continue
        }
        if ($raw -match '^(mailto:|app://|codex://|vscode://|cursor://)') { continue }
        $parts = $raw -split '#', 2
        $targetPath = [uri]::UnescapeDataString($parts[0])
        $fragment = if ($parts.Count -eq 2) { [uri]::UnescapeDataString($parts[1]) } else { '' }
        $resolved = if ([string]::IsNullOrWhiteSpace($targetPath)) {
            $file.FullName
        } else {
            Join-Path $file.DirectoryName $targetPath
        }
        if (-not (Test-Path -LiteralPath $resolved)) {
            Add-Error $file.FullName "missing link target '$raw'"
            continue
        }
        if ($fragment -and ([System.IO.Path]::GetExtension($resolved) -eq '.md')) {
            $targetContent = Get-Content -LiteralPath $resolved -Raw -Encoding UTF8
            $anchors = Get-Anchors $targetContent
            if (-not $anchors.Contains($fragment)) { Add-Error $file.FullName "missing fragment '#$fragment' in '$targetPath'" }
        }
    }
}

if ($CheckExternalLinks) {
    foreach ($url in $externalLinks) {
        $status = & curl.exe --location --silent --output NUL --write-out '%{http_code}' --max-time 15 --user-agent 'Mozilla/5.0 Soccer-Mobile-Pro-Docs-Validator' -- $url
        if ($LASTEXITCODE -ne 0 -or $status -eq '000' -or [int]$status -ge 500) {
            $errors.Add("external link unavailable ($status): $url")
        }
    }
    Write-Host "External links checked: $($externalLinks.Count)."
}

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Host "Documentation validation passed: $($files.Count) Markdown files."
