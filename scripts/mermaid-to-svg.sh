#!/bin/bash

for file in *.mmd; do
  # Convert Mermaid to SVG
  mmdc -i "$file" -o "${file%.mmd}.svg"
  
  # Check diagram type
  if grep -qE "^flowchart TD" "$file"; then
    width=450
  elif grep -qE "^flowchart LR" "$file"; then
    width=900
  else
    width=900  # default to 900 for other types
  fi
  
  # Optimize with SVGO
  #svgo "${file%.mmd}.svg" --multipass --disable=convertShapeToPath --width=$width
  svgo "${file%.mmd}.svg" -o "${file%.mmd}.svg"
done