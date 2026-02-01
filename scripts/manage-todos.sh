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
SOURCE_DIR="$PROJECT_ROOT/SourceGenerator"

echo -e "${BLUE}=== REslava.Result TODO Management ===${NC}"
echo "Project Root: $PROJECT_ROOT"
echo "Source Directory: $SOURCE_DIR"
echo ""

case "$1" in
    "count")
        echo -e "${GREEN}=== TODO Count by Category ===${NC}"
        
        DEBUG_COUNT=$(grep -r "// TODO: DEBUG" --include="*.cs" "$SOURCE_DIR" 2>/dev/null | wc -l || echo "0")
        PHASE_COUNT=$(grep -r "// TODO: PHASE" --include="*.cs" "$SOURCE_DIR" 2>/dev/null | wc -l || echo "0")
        TEMP_COUNT=$(grep -r "// TODO: TEMP" --include="*.cs" "$SOURCE_DIR" 2>/dev/null | wc -l || echo "0")
        ISSUE_COUNT=$(grep -r "// TODO: ISSUE" --include="*.cs" "$SOURCE_DIR" 2>/dev/null | wc -l || echo "0")
        PERF_COUNT=$(grep -r "// TODO: PERF" --include="*.cs" "$SOURCE_DIR" 2>/dev/null | wc -l || echo "0")
        
        echo -e "DEBUG:  ${YELLOW}$DEBUG_COUNT${NC}"
        echo -e "PHASE:  ${YELLOW}$PHASE_COUNT${NC}"
        echo -e "TEMP:   ${YELLOW}$TEMP_COUNT${NC}"
        echo -e "ISSUE:  ${YELLOW}$ISSUE_COUNT${NC}"
        echo -e "PERF:   ${YELLOW}$PERF_COUNT${NC}"
        
        TOTAL=$((DEBUG_COUNT + PHASE_COUNT + TEMP_COUNT + ISSUE_COUNT + PERF_COUNT))
        echo -e "TOTAL:  ${YELLOW}$TOTAL${NC}"
        
        # Health indicator
        if [ "$TOTAL" -lt 10 ]; then
            echo -e "${GREEN}✅ TODO Health: GREEN (< 10 TODOs)${NC}"
        elif [ "$TOTAL" -lt 25 ]; then
            echo -e "${YELLOW}⚠️  TODO Health: YELLOW (10-25 TODOs)${NC}"
        else
            echo -e "${RED}❌ TODO Health: RED (> 25 TODOs)${NC}"
        fi
        ;;
        
    "list")
        echo -e "${GREEN}=== All TODOs with Context ===${NC}"
        grep -r -A 15 "// TODO:" --include="*.cs" "$SOURCE_DIR" 2>/dev/null || echo "No TODOs found"
        ;;
        
    "debug")
        echo -e "${GREEN}=== DEBUG TODOs (Candidates for Cleanup) ===${NC}"
        grep -r -A 15 "// TODO: DEBUG" --include="*.cs" "$SOURCE_DIR" 2>/dev/null || echo "No DEBUG TODOs found"
        ;;
        
    "phase")
        echo -e "${GREEN}=== PHASE TODOs (Development Phases) ===${NC}"
        grep -r -A 15 "// TODO: PHASE" --include="*.cs" "$SOURCE_DIR" 2>/dev/null || echo "No PHASE TODOs found"
        ;;
        
    "temp")
        echo -e "${GREEN}=== TEMP TODOs (Temporary Workarounds) ===${NC}"
        grep -r -A 15 "// TODO: TEMP" --include="*.cs" "$SOURCE_DIR" 2>/dev/null || echo "No TEMP TODOs found"
        ;;
        
    "overdue")
        echo -e "${GREEN}=== Overdue TODOs (Check DUE dates) ===${NC}"
        grep -r -A 15 "// TODO:" --include="*.cs" "$SOURCE_DIR" 2>/dev/null | grep -E "DUE:.*202[0-9]" || echo "No overdue TODOs found (or dates not in expected format)"
        ;;
        
    "assigned")
        if [ -n "$2" ]; then
            echo -e "${GREEN}=== TODOs assigned to: $2 ===${NC}"
            grep -r -A 15 "// TODO:" --include="*.cs" "$SOURCE_DIR" 2>/dev/null | grep -E "ASSIGNED:.*$2" || echo "No TODOs assigned to $2"
        else
            echo -e "${GREEN}=== TODOs by Assignment ===${NC}"
            grep -r -A 15 "// TODO:" --include="*.cs" "$SOURCE_DIR" 2>/dev/null | grep -E "ASSIGNED:" || echo "No assigned TODOs found"
        fi
        ;;
        
    "cleanup")
        echo -e "${GREEN}=== Cleanup Candidates ===${NC}"
        echo "DEBUG TODOs (can be removed after testing):"
        grep -r -A 5 "// TODO: DEBUG" --include="*.cs" "$SOURCE_DIR" 2>/dev/null | grep -E "(CLEANUP|DUE)" || echo "No DEBUG TODOs found"
        
        echo ""
        echo "TEMP TODOs (need permanent solutions):"
        grep -r -A 5 "// TODO: TEMP" --include="*.cs" "$SOURCE_DIR" 2>/dev/null | grep -E "(CLEANUP|DUE)" || echo "No TEMP TODOs found"
        ;;
        
    "audit")
        echo -e "${GREEN}=== TODO Audit ===${NC}"
        
        # Check for TODOs without END-TODO
        echo "Checking for TODOs without END-TODO markers..."
        TODO_COUNT=$(grep -r "// TODO:" --include="*.cs" "$SOURCE_DIR" 2>/dev/null | wc -l || echo "0")
        ENDTODO_COUNT=$(grep -r "// END-TODO" --include="*.cs" "$SOURCE_DIR" 2>/dev/null | wc -l || echo "0")
        
        echo "TODO markers found: $TODO_COUNT"
        echo "END-TODO markers found: $ENDTODO_COUNT"
        
        if [ "$TODO_COUNT" -ne "$ENDTODO_COUNT" ]; then
            echo -e "${RED}❌ Mismatch: Some TODOs may be missing END-TODO markers${NC}"
        else
            echo -e "${GREEN}✅ All TODOs have proper END-TODO markers${NC}"
        fi
        
        echo ""
        echo "Checking for TODOs without required fields..."
        
        # Check for TODOs without PURPOSE
        TODO_WITHOUT_PURPOSE=$(grep -r -A 10 "// TODO:" --include="*.cs" "$SOURCE_DIR" 2>/dev/null | grep -B 10 "// END-TODO" | grep -v "PURPOSE:" | grep "// TODO:" | wc -l || echo "0")
        if [ "$TODO_WITHOUT_PURPOSE" -gt 0 ]; then
            echo -e "${YELLOW}⚠️  $TODO_WITHOUT_PURPOSE TODO(s) missing PURPOSE field${NC}"
        else
            echo -e "${GREEN}✅ All TODOs have PURPOSE field${NC}"
        fi
        
        echo ""
        echo "Checking for TODOs without CONTEXT..."
        TODO_WITHOUT_CONTEXT=$(grep -r -A 10 "// TODO:" --include="*.cs" "$SOURCE_DIR" 2>/dev/null | grep -B 10 "// END-TODO" | grep -v "CONTEXT:" | grep "// TODO:" | wc -l || echo "0")
        if [ "$TODO_WITHOUT_CONTEXT" -gt 0 ]; then
            echo -e "${YELLOW}⚠️  $TODO_WITHOUT_CONTEXT TODO(s) missing CONTEXT field${NC}"
        else
            echo -e "${GREEN}✅ All TODOs have CONTEXT field${NC}"
        fi
        
        echo ""
        echo "Checking for TODOs without CLEANUP..."
        TODO_WITHOUT_CLEANUP=$(grep -r -A 10 "// TODO:" --include="*.cs" "$SOURCE_DIR" 2>/dev/null | grep -B 10 "// END-TODO" | grep -v "CLEANUP:" | grep "// TODO:" | wc -l || echo "0")
        if [ "$TODO_WITHOUT_CLEANUP" -gt 0 ]; then
            echo -e "${YELLOW}⚠️  $TODO_WITHOUT_CLEANUP TODO(s) missing CLEANUP field${NC}"
        else
            echo -e "${GREEN}✅ All TODOs have CLEANUP field${NC}"
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
