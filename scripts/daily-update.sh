#!/bin/bash
set -e

echo "=== AplikasiBanking8583 Daily Update Routine ==="
echo "Date: $(date)"
echo ""

echo "[1/4] Checking git status..."
git status --short

echo ""
echo "[2/4] Building project..."
dotnet build 2>&1
if [ $? -ne 0 ]; then
    echo "ERROR: Build failed"
    exit 1
fi
echo "Build succeeded."

echo ""
echo "[3/4] Staging all changes..."
git add -A

CHANGES=$(git diff --cached --stat)
if [ -z "$CHANGES" ]; then
    echo "No changes to commit. Everything up to date."
    exit 0
fi

echo "$CHANGES"
echo ""
echo "[4/4] Committing and pushing..."
git commit -m "daily update"
git push origin main
echo "Push completed."

echo ""
echo "=== Daily update complete ==="