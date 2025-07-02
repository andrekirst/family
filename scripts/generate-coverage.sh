#!/bin/bash

# Script to generate local code coverage reports
# Usage: ./scripts/generate-coverage.sh

set -e

echo "🧪 Starting code coverage collection..."

# Clean previous results
rm -rf coverage/
mkdir -p coverage/

# Run tests with coverage collection
echo "🔍 Running tests with coverage collection..."
dotnet test Family.sln \
  --configuration Release \
  --collect:"XPlat Code Coverage" \
  --settings codecoverage.runsettings \
  --results-directory ./coverage \
  --verbosity normal

# Install ReportGenerator if not present
if ! command -v reportgenerator &> /dev/null; then
    echo "📦 Installing ReportGenerator..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
fi

# Generate HTML report
echo "📊 Generating coverage report..."
reportgenerator \
  -reports:"coverage/**/coverage.cobertura.xml" \
  -targetdir:"coverage/report" \
  -reporttypes:Html_Dark,Badges,MarkdownSummaryGithub,TextSummary \
  -assemblyfilters:"-*.Tests"

# Display coverage summary
echo ""
echo "📈 Coverage Summary:"
echo "==================="
cat coverage/report/Summary.txt

echo ""
echo "✅ Coverage report generated successfully!"
echo "📂 Open coverage/report/index.html to view the detailed report"

# Check if coverage meets threshold
COVERAGE=$(grep -oP 'Line coverage: \K[0-9.]+' coverage/report/Summary.txt)
THRESHOLD=40

echo ""
if (( $(echo "$COVERAGE >= $THRESHOLD" | bc -l) )); then
    echo "✅ Coverage threshold met: ${COVERAGE}% >= ${THRESHOLD}%"
else
    echo "❌ Coverage below threshold: ${COVERAGE}% < ${THRESHOLD}%"
    exit 1
fi