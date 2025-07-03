#!/bin/bash

# Simple Workflow Compliance Test
echo "=== Workflow Compliance Test ==="

# Test branch naming
echo "Testing branch naming conventions..."

# Valid branch names
valid_branches=("feature/issue-35-workflow-branch-creation" "bugfix/issue-42-fix" "docs/issue-50-doc")
invalid_branches=("feature/workflow" "bugfix/issue-fix" "random-branch")

echo "✅ Valid branch names:"
for branch in "${valid_branches[@]}"; do
    if [[ $branch =~ ^(feature|bugfix|docs|hotfix)/issue-[0-9]+-[a-z0-9-]+$ ]]; then
        echo "  ✓ $branch"
    else
        echo "  ✗ $branch (should be valid)"
    fi
done

echo "❌ Invalid branch names:"
for branch in "${invalid_branches[@]}"; do
    if [[ $branch =~ ^(feature|bugfix|docs|hotfix)/issue-[0-9]+-[a-z0-9-]+$ ]]; then
        echo "  ✗ $branch (should be invalid)"
    else
        echo "  ✓ $branch (correctly rejected)"
    fi
done

# Test commit messages
echo ""
echo "Testing conventional commit format..."

valid_commits=("feat: add feature" "fix: resolve bug" "docs: update readme")
invalid_commits=("Add feature" "fix bug" "update docs")

echo "✅ Valid commits:"
for commit in "${valid_commits[@]}"; do
    if [[ $commit =~ ^(feat|fix|docs|style|refactor|test|chore):\ .+ ]]; then
        echo "  ✓ $commit"
    else
        echo "  ✗ $commit (should be valid)"
    fi
done

echo "❌ Invalid commits:"
for commit in "${invalid_commits[@]}"; do
    if [[ $commit =~ ^(feat|fix|docs|style|refactor|test|chore):\ .+ ]]; then
        echo "  ✗ $commit (should be invalid)"
    else
        echo "  ✓ $commit (correctly rejected)"
    fi
done

# Check documentation exists
echo ""
echo "Checking workflow documentation..."

if [[ -f "CLAUDE.md" ]]; then
    echo "✓ CLAUDE.md exists"
else
    echo "✗ CLAUDE.md missing"
fi

if [[ -f "docs/workflow-guidelines.md" ]]; then
    echo "✓ workflow-guidelines.md exists"
else
    echo "✗ workflow-guidelines.md missing"
fi

if [[ -f ".github/workflows/workflow-validation.yml" ]]; then
    echo "✓ GitHub workflow validation exists"
else
    echo "✗ GitHub workflow validation missing"
fi

echo ""
echo "=== Test Complete ==="