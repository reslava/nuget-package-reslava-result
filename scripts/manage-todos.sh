#!/bin/bash
# TODO Management Script for REslava.Result Project
# Usage: ./scripts/manage-todos.sh {count|list|cleanup|overdue|audit}

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Project root
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# Define specific source directories to search (exclude bin, obj, GeneratedFiles, .git, etc.)
SOURCE_DIRS=(
    "$PROJECT_ROOT/SourceGenerator"
    "$PROJECT_ROOT/src"
    "$PROJECT_ROOT/samples"
    "$PROJECT_ROOT/SourceGenerator/Tests"
    "$PROJECT_ROOT/docs"
)

# Helper function to run grep on source directories only
grep_source() {
    local pattern="$1"
    local include="$2"
    
    local result=""
    for dir in "${SOURCE_DIRS[@]}"; do
        if [ -d "$dir" ]; then
            local dir_result=$(grep -r "$pattern" --include="$include" "$dir" 2>/dev/null || true)
            result="$result$dir_result"
        fi
    done
    echo "$result"
}

echo -e "${BLUE}=== REslava.Result TODO Management ===${NC}"
echo "Project Root: $PROJECT_ROOT"
echo "Source Directories: ${SOURCE_DIRS[*]}"
echo ""

case "$1" in
    "count")
        echo -e "${GREEN}=== TODO Count by File Type ===${NC}"
        
        # Source Code Files (.cs)
        CS_DEBUG=$(grep_source "// TODO: DEBUG" "*.cs" | wc -l || echo "0")
        CS_PHASE=$(grep_source "// TODO: PHASE" "*.cs" | wc -l || echo "0")
        CS_TEMP=$(grep_source "// TODO: TEMP" "*.cs" | wc -l || echo "0")
        CS_ISSUE=$(grep_source "// TODO: ISSUE" "*.cs" | wc -l || echo "0")
        CS_PERF=$(grep_source "// TODO: PERF" "*.cs" | wc -l || echo "0")
        CS_CONFIG=$(grep_source "// TODO: CONFIGURATION" "*.cs" | wc -l || echo "0")
        CS_INTEGRATION=$(grep_source "// TODO: INTEGRATION" "*.cs" | wc -l || echo "0")
        CS_TOTAL=$((CS_DEBUG + CS_PHASE + CS_TEMP + CS_ISSUE + CS_PERF + CS_CONFIG + CS_INTEGRATION))
        
        # Project Files (.csproj)
        CSPROJ_COUNT=$(grep_source "<!-- TODO:" "*.csproj" | wc -l || echo "0")
        
        # Configuration Files (.config, .json)
        CONFIG_COUNT=$(grep_source "<!-- TODO:" "*.config" | wc -l || echo "0")
        JSON_COUNT=$(grep_source "// TODO:" "*.json" | wc -l || echo "0")
        
        # Build Scripts (.ps1, .sh)
        PS1_COUNT=$(grep_source "# TODO:" "*.ps1" | wc -l || echo "0")
        SH_COUNT=$(grep_source "# TODO:" "*.sh" | wc -l || echo "0")
        
        # Documentation (.md)
        MD_COUNT=$(grep_source "<!-- TODO:" "*.md" | wc -l || echo "0")
        
        echo -e "${BLUE}📁 Source Code (.cs):${NC}"
        echo -e "  DEBUG:       ${YELLOW}$CS_DEBUG${NC}"
        echo -e "  PHASE:       ${YELLOW}$CS_PHASE${NC}"
        echo -e "  TEMP:        ${YELLOW}$CS_TEMP${NC}"
        echo -e "  ISSUE:       ${YELLOW}$CS_ISSUE${NC}"
        echo -e "  PERF:        ${YELLOW}$CS_PERF${NC}"
        echo -e "  CONFIG:      ${YELLOW}$CS_CONFIG${NC}"
        echo -e "  INTEGRATION: ${YELLOW}$CS_INTEGRATION${NC}"
        echo -e "  CS TOTAL:    ${GREEN}$CS_TOTAL${NC}"
        echo ""
        
        echo -e "${BLUE}📦 Project Files (.csproj):${NC} ${YELLOW}$CSPROJ_COUNT${NC}"
        echo -e "${BLUE}⚙️  Config Files (.config/.json):${NC} ${YELLOW}$((CONFIG_COUNT + JSON_COUNT))${NC}"
        echo -e "${BLUE}🔧 Scripts (.ps1/.sh):${NC} ${YELLOW}$((PS1_COUNT + SH_COUNT))${NC}"
        echo -e "${BLUE}📚 Documentation (.md):${NC} ${YELLOW}$MD_COUNT${NC}"
        echo ""
        
        TOTAL=$((CS_TOTAL + CSPROJ_COUNT + CONFIG_COUNT + JSON_COUNT + PS1_COUNT + SH_COUNT + MD_COUNT))
        echo -e "${GREEN}📊 TOTAL TODOS: $TOTAL${NC}"
        
        # Health indicator
        if [ "$TOTAL" -lt 20 ]; then
            echo -e "${GREEN}✅ TODO Health: GREEN (< 20 TODOs)${NC}"
        elif [ "$TOTAL" -lt 50 ]; then
            echo -e "${YELLOW}⚠️  TODO Health: YELLOW (20-50 TODOs)${NC}"
        else
            echo -e "${RED}❌ TODO Health: RED (> 50 TODOs)${NC}"
        fi
        ;;
        
    "list")
        echo -e "${GREEN}=== All TODOs with Context ===${NC}"
        grep_source "// TODO:" "*.cs" || echo "No TODOs found"
        ;;
        
    "debug")
        echo -e "${GREEN}=== DEBUG TODOs (Candidates for Cleanup) ===${NC}"
        grep_source "// TODO: DEBUG" "*.cs" || echo "No DEBUG TODOs found"
        ;;
        
    "phase")
        echo -e "${GREEN}=== PHASE TODOs (Development Phases) ===${NC}"
        grep_source "// TODO: PHASE" "*.cs" || echo "No PHASE TODOs found"
        ;;
        
    "temp")
        echo -e "${GREEN}=== TEMP TODOs (Temporary Workarounds) ===${NC}"
        grep_source "// TODO: TEMP" "*.cs" || echo "No TEMP TODOs found"
        ;;
        
    "overdue")
        echo -e "${GREEN}=== Overdue TODOs (Check DUE dates) ===${NC}"
        grep_source "// TODO:" "*.cs" | grep -E "DUE:.*202[0-9]" || echo "No overdue TODOs found (or dates not in expected format)"
        ;;
        
    "assigned")
        if [ -n "$2" ]; then
            echo -e "${GREEN}=== TODOs assigned to: $2 ===${NC}"
            grep_source "// TODO:" "*.cs" | grep -E "ASSIGNED:.*$2" || echo "No TODOs assigned to $2"
        else
            echo -e "${RED}Error: Please provide an assignee name${NC}"
            echo "Usage: $0 assigned <assignee-name>"
            exit 1
        fi
        ;;
        
    "cleanup")
        echo -e "${GREEN}=== Cleanup Candidates ===${NC}"
        echo "DEBUG TODOs (can be removed after testing):"
        grep_source "// TODO: DEBUG" "*.cs" | grep -E "(CLEANUP|DUE)" || echo "No DEBUG TODOs found"
        
        echo ""
        echo "TEMP TODOs (need permanent solutions):"
        grep_source "// TODO: TEMP" "*.cs" | grep -E "(CLEANUP|DUE)" || echo "No TEMP TODOs found"
        
        echo ""
        echo "PROJECT CONFIGURATION TODOs (need production setup):"
        grep_source "<!-- TODO:" "*.csproj" | grep -E "(CLEANUP|DUE)" || echo "No PROJECT TODOs found"
        
        echo ""
        echo "CONFIGURATION TODOs (need production environment setup):"
        grep_source "<!-- TODO:" "*.config" | grep -E "(CLEANUP|DUE)" || echo "No CONFIG TODOs found"
        ;;
        
    "audit")
        echo -e "${GREEN}=== TODO Audit ===${NC}"
        
        # Check for TODOs without END-TODO in C# files
        echo "Checking for C# TODOs without END-TODO markers..."
        CS_TODO_COUNT=$(grep_source "// TODO:" "*.cs" | wc -l || echo "0")
        CS_ENDTODO_COUNT=$(grep_source "// END-TODO" "*.cs" | wc -l || echo "0")
        
        echo "C# TODO markers found: $CS_TODO_COUNT"
        echo "C# END-TODO markers found: $CS_ENDTODO_COUNT"
        
        if [ "$CS_TODO_COUNT" -ne "$CS_ENDTODO_COUNT" ]; then
            echo -e "${RED}❌ C# Mismatch: Some TODOs may be missing END-TODO markers${NC}"
        else
            echo -e "${GREEN}✅ All C# TODOs have proper END-TODO markers${NC}"
        fi
        
        echo ""
        # Check for TODOs without END-TODO in XML files
        echo "Checking for XML TODOs without END-TODO markers..."
        XML_TODO_COUNT=$(grep_source "<!-- TODO:" "*.csproj" | wc -l || echo "0")
        XML_TODO_COUNT=$((XML_TODO_COUNT + $(grep_source "<!-- TODO:" "*.config" | wc -l || echo "0")))
        XML_TODO_COUNT=$((XML_TODO_COUNT + $(grep_source "<!-- TODO:" "*.md" | wc -l || echo "0")))
        
        XML_ENDTODO_COUNT=$(grep_source "<!-- END-TODO" "*.csproj" | wc -l || echo "0")
        XML_ENDTODO_COUNT=$((XML_ENDTODO_COUNT + $(grep_source "<!-- END-TODO" "*.config" | wc -l || echo "0")))
        XML_ENDTODO_COUNT=$((XML_ENDTODO_COUNT + $(grep_source "<!-- END-TODO" "*.md" | wc -l || echo "0")))
        
        echo "XML TODO markers found: $XML_TODO_COUNT"
        echo "XML END-TODO markers found: $XML_ENDTODO_COUNT"
        
        if [ "$XML_TODO_COUNT" -ne "$XML_ENDTODO_COUNT" ]; then
            echo -e "${RED}❌ XML Mismatch: Some TODOs may be missing END-TODO markers${NC}"
        else
            echo -e "${GREEN}✅ All XML TODOs have proper END-TODO markers${NC}"
        fi
        
        echo ""
        # Check for TODOs without END-TODO in Script files
        echo "Checking for Script TODOs without END-TODO markers..."
        SCRIPT_TODO_COUNT=$(grep_source "# TODO:" "*.ps1" | wc -l || echo "0")
        SCRIPT_TODO_COUNT=$((SCRIPT_TODO_COUNT + $(grep_source "# TODO:" "*.sh" | wc -l || echo "0")))
        
        SCRIPT_ENDTODO_COUNT=$(grep_source "# END-TODO" "*.ps1" | wc -l || echo "0")
        SCRIPT_ENDTODO_COUNT=$((SCRIPT_ENDTODO_COUNT + $(grep_source "# END-TODO" "*.sh" | wc -l || echo "0")))
        
        echo "Script TODO markers found: $SCRIPT_TODO_COUNT"
        echo "Script END-TODO markers found: $SCRIPT_ENDTODO_COUNT"
        
        if [ "$SCRIPT_TODO_COUNT" -ne "$SCRIPT_ENDTODO_COUNT" ]; then
            echo -e "${RED}❌ Script Mismatch: Some TODOs may be missing END-TODO markers${NC}"
        else
            echo -e "${GREEN}✅ All Script TODOs have proper END-TODO markers${NC}"
        fi
        
        echo ""
        # Overall health
        TOTAL_TODOS=$((CS_TODO_COUNT + XML_TODO_COUNT + SCRIPT_TODO_COUNT))
        TOTAL_ENDTODOS=$((CS_ENDTODO_COUNT + XML_ENDTODO_COUNT + SCRIPT_ENDTODO_COUNT))
        
        if [ "$TOTAL_TODOS" -eq "$TOTAL_ENDTODOS" ]; then
            echo -e "${GREEN}✅ Overall TODO Health: EXCELLENT - All TODOs properly closed${NC}"
        else
            echo -e "${YELLOW}⚠️  Overall TODO Health: NEEDS ATTENTION - $((TOTAL_TODOS - TOTAL_ENDTODOS)) unclosed TODOs${NC}"
        fi
        ;;
        
    "help"|"-h"|"--help")
        echo "Usage: $0 {command}"
        echo ""
        echo "Commands:"
        echo "  count     - Show TODO count by category"
        echo "  list      - List all TODOs with context"
        echo "  debug     - Show DEBUG TODOs (cleanup candidates)"
        echo "  phase     - Show PHASE TODOs (development phases)"
        echo "  temp      - Show TEMP TODOs (temporary workarounds)"
        echo "  overdue   - Show overdue TODOs (check DUE dates)"
        echo "  assigned  - Show TODOs by assignment (optional: specify name)"
        echo "  cleanup   - Show cleanup candidates"
        echo "  audit     - Audit TODO compliance (missing fields, etc.)"
        echo "  help      - Show this help message"
        echo ""
        echo "Examples:"
        echo "  $0 count                    # Count all TODOs by category"
        echo "  $0 debug                    # Show all DEBUG TODOs"
        echo "  $0 assigned Development     # Show TODOs assigned to Development Team"
        echo "  $0 audit                    # Check TODO compliance"
        ;;
        
    *)
        echo -e "${RED}Error: Unknown command '$1'${NC}"
        echo ""
        echo "Available commands: count, list, debug, phase, temp, overdue, assigned, cleanup, audit, help"
        echo "Use '$0 help' for detailed usage information"
        exit 1
        ;;
esac

echo ""
echo -e "${BLUE}=== Done ===${NC}"
