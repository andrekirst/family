#!/bin/bash

# Workflow Compliance Test Suite
# Tests the implementation of workflow guidelines and branch naming conventions

set -e

# Disable debug mode if set
set +x

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Test counters
TESTS_RUN=0
TESTS_PASSED=0
TESTS_FAILED=0

# Helper functions
print_header() {
    echo -e "${BLUE}=== $1 ===${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
    ((TESTS_PASSED++))
}

print_failure() {
    echo -e "${RED}❌ $1${NC}"
    ((TESTS_FAILED++))
}

print_warning() {
    echo -e "${YELLOW}⚠️ $1${NC}"
}

run_test() {
    ((TESTS_RUN++))
    echo -e "${BLUE}Test $TESTS_RUN: $1${NC}"
}

# Test branch naming convention validation
test_branch_naming() {
    print_header "Testing Branch Naming Convention Validation"
    
    # Valid branch names
    valid_branches=(
        "feature/issue-35-workflow-branch-creation"
        "bugfix/issue-42-authentication-error"
        "docs/issue-50-api-documentation"
        "hotfix/issue-123-critical-security-fix"
        "feature/issue-1-simple-feature"
        "docs/issue-999-very-long-documentation-name-with-many-words"
    )
    
    # Invalid branch names
    invalid_branches=(
        "feature/workflow-branch-creation"
        "bugfix/issue-authentication-error"
        "feature/issue-35"
        "feature/35-workflow"
        "main"
        "develop"
        "feature/issue-35-"
        "feature/issue-35_underscore"
        "Feature/issue-35-uppercase"
        "feature/issue-35-UPPERCASE"
    )
    
    # Test valid branch names
    for branch in "${valid_branches[@]}"; do
        run_test "Valid branch name: $branch"
        if [[ $branch =~ ^(feature|bugfix|docs|hotfix)/issue-[0-9]+-[a-z0-9-]+$ ]]; then
            print_success "Branch name '$branch' is valid"
        else
            print_failure "Branch name '$branch' should be valid but validation failed"
        fi
    done
    
    # Test invalid branch names
    for branch in "${invalid_branches[@]}"; do
        run_test "Invalid branch name: $branch"
        if [[ $branch =~ ^(feature|bugfix|docs|hotfix)/issue-[0-9]+-[a-z0-9-]+$ ]]; then
            print_failure "Branch name '$branch' should be invalid but validation passed"
        else
            print_success "Branch name '$branch' correctly identified as invalid"
        fi
    done
}

# Test conventional commit message validation
test_conventional_commits() {
    print_header "Testing Conventional Commit Message Validation"
    
    # Valid commit messages
    valid_commits=(
        "feat: add new feature"
        "fix: resolve bug in authentication"
        "docs: update README"
        "style: format code"
        "refactor: improve code structure"
        "test: add unit tests"
        "chore: update dependencies"
        "feat(auth): add OAuth2 integration"
        "fix(api): resolve null reference error"
        "docs(workflow): update guidelines"
        "perf(database): optimize query performance"
        "ci(github): add workflow validation"
        "build(docker): update Dockerfile"
        "revert: revert previous changes"
    )
    
    # Invalid commit messages
    invalid_commits=(
        "Add new feature"
        "fix bug"
        "Update documentation"
        "WIP: work in progress"
        "fix:"
        "feat"
        "random commit message"
        "Feat: add feature"
        "fix(scope: missing closing parenthesis"
        "feat(scope): "
    )
    
    valid_types="feat|fix|docs|style|refactor|test|chore|perf|ci|build|revert"
    
    # Test valid commit messages
    for commit in "${valid_commits[@]}"; do
        run_test "Valid commit: $commit"
        if [[ $commit =~ ^($valid_types)(\(.+\))?:\ .+ ]]; then
            print_success "Commit message '$commit' is valid"
        else
            print_failure "Commit message '$commit' should be valid but validation failed"
        fi
    done
    
    # Test invalid commit messages
    for commit in "${invalid_commits[@]}"; do
        run_test "Invalid commit: $commit"
        if [[ $commit =~ ^($valid_types)(\(.+\))?:\ .+ ]]; then
            print_failure "Commit message '$commit' should be invalid but validation passed"
        else
            print_success "Commit message '$commit' correctly identified as invalid"
        fi
    done
}

# Test PR title validation
test_pr_titles() {
    print_header "Testing PR Title Validation"
    
    # Valid PR titles
    valid_titles=(
        "Fixes #35: Implement workflow guidelines"
        "Closes #42: Fix authentication bug"
        "feat: add new feature (#123)"
        "Update documentation (resolves #456)"
        "Hotfix for critical issue #789"
        "Feature implementation closes #35"
    )
    
    # Invalid PR titles
    invalid_titles=(
        "Implement workflow guidelines"
        "Fix authentication bug"
        "Update documentation"
        "Random PR title"
        "Fixes: missing issue number"
        "Closes #: invalid issue number"
    )
    
    # Test valid PR titles
    for title in "${valid_titles[@]}"; do
        run_test "Valid PR title: $title"
        if [[ $title =~ \#[0-9]+ ]] || [[ $title =~ [Ff]ixes[[:space:]]+\#[0-9]+ ]] || [[ $title =~ [Cc]loses[[:space:]]+\#[0-9]+ ]]; then
            print_success "PR title '$title' contains issue reference"
        else
            print_failure "PR title '$title' should contain issue reference but validation failed"
        fi
    done
    
    # Test invalid PR titles
    for title in "${invalid_titles[@]}"; do
        run_test "Invalid PR title: $title"
        if [[ $title =~ \#[0-9]+ ]] || [[ $title =~ [Ff]ixes[[:space:]]+\#[0-9]+ ]] || [[ $title =~ [Cc]loses[[:space:]]+\#[0-9]+ ]]; then
            print_failure "PR title '$title' should be invalid but validation passed"
        else
            print_success "PR title '$title' correctly identified as missing issue reference"
        fi
    done
}

# Test current repository compliance
test_current_repo_compliance() {
    print_header "Testing Current Repository Compliance"
    
    # Check if we're in a git repository
    run_test "Check if in git repository"
    if git rev-parse --git-dir > /dev/null 2>&1; then
        print_success "Currently in a git repository"
    else
        print_failure "Not in a git repository"
        return
    fi
    
    # Check current branch name
    run_test "Check current branch name compliance"
    current_branch=$(git branch --show-current)
    if [[ $current_branch =~ ^(feature|bugfix|docs|hotfix)/issue-[0-9]+-[a-z0-9-]+$ ]] || [[ $current_branch == "main" ]]; then
        print_success "Current branch '$current_branch' follows naming convention"
    else
        print_warning "Current branch '$current_branch' does not follow naming convention"
    fi
    
    # Check for CLAUDE.md existence and workflow section
    run_test "Check CLAUDE.md contains workflow guidelines"
    if [[ -f "CLAUDE.md" ]]; then
        if grep -q "## Entwicklungs-Workflow" CLAUDE.md; then
            print_success "CLAUDE.md contains workflow section"
        else
            print_failure "CLAUDE.md missing workflow section"
        fi
    else
        print_failure "CLAUDE.md not found"
    fi
    
    # Check for workflow guidelines documentation
    run_test "Check workflow guidelines documentation exists"
    if [[ -f "docs/workflow-guidelines.md" ]]; then
        print_success "Workflow guidelines documentation exists"
    else
        print_failure "Workflow guidelines documentation not found"
    fi
    
    # Check for GitHub workflow validation
    run_test "Check GitHub workflow validation exists"
    if [[ -f ".github/workflows/workflow-validation.yml" ]]; then
        print_success "GitHub workflow validation exists"
    else
        print_failure "GitHub workflow validation not found"
    fi
}

# Main test execution
main() {
    print_header "Workflow Compliance Test Suite"
    echo "Testing implementation of workflow guidelines and branch naming conventions"
    echo
    
    test_branch_naming
    echo
    
    test_conventional_commits
    echo
    
    test_pr_titles
    echo
    
    test_current_repo_compliance
    echo
    
    # Print summary
    print_header "Test Summary"
    echo "Tests run: $TESTS_RUN"
    echo -e "Tests passed: ${GREEN}$TESTS_PASSED${NC}"
    echo -e "Tests failed: ${RED}$TESTS_FAILED${NC}"
    
    if [[ $TESTS_FAILED -eq 0 ]]; then
        echo -e "${GREEN}🎉 All tests passed! Workflow compliance is excellent.${NC}"
        exit 0
    else
        echo -e "${RED}💥 Some tests failed. Please review workflow compliance.${NC}"
        exit 1
    fi
}

# Run tests
main "$@"