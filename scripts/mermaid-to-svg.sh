#!/bin/bash
# mermaid-to-svg.sh
# Converts every *.mmd file in the current directory to an optimised SVG.
#
# Width auto-detection:
#   flowchart TD  →  width=450  (architecture / error-propagation — tall, narrow)
#   flowchart LR  →  width=900  (pipeline / error-surface — wide)
#
# SVGO_WIDTH is exported so svgo.config.js picks it up without any duplication.

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIG="$SCRIPT_DIR/../images/svgo.config.js"

for mmd_file in *.mmd; do
  svg_file="${mmd_file%.mmd}.svg"

  # Detect orientation and set width
  if grep -qE "^flowchart TD" "$mmd_file"; then
    export SVGO_WIDTH=450
  else
    export SVGO_WIDTH=900
  fi

  # Convert Mermaid → SVG (--width sets viewBox before SVGO runs)
  mmdc -i "$mmd_file" -o "$svg_file" --width "$SVGO_WIDTH"

  # Optimise with SVGO (reads SVGO_WIDTH from env via svgo.config.js)
  svgo "$svg_file" -o "$svg_file" --config "$CONFIG"

  # Also emit PNG for NuGet README images (raw.githubusercontent.com URLs work; SVG does not)
  png_file="${mmd_file%.mmd}.png"
  mmdc -i "$mmd_file" -o "$png_file" --width "$SVGO_WIDTH" --backgroundColor transparent
done
