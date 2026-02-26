#!/bin/bash
set -e  # Exit immediately if any command fails

echo "🏗️ Build DocFX API documentation"
cd docfx
docfx metadata docfx.json
docfx build docfx.json