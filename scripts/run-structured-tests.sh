#!/bin/bash

# Script to run tests with structured reporting locally
# Usage: ./scripts/run-structured-tests.sh

set -e

echo "🧪 Starting structured test execution..."

# Clean previous results
rm -rf test-results/
mkdir -p test-results/{unit,integration,summary}

echo "🔧 Building solution..."
dotnet build Family.sln --configuration Release

echo "📋 Running Unit Tests..."
dotnet test Family.sln \
  --no-build \
  --configuration Release \
  --verbosity normal \
  --filter "Category=Unit" \
  --logger "trx;LogFileName=unit-tests.trx" \
  --results-directory ./test-results/unit \
  --collect:"XPlat Code Coverage" \
  --settings codecoverage.runsettings

echo "🔗 Running Integration Tests..."
dotnet test Family.sln \
  --no-build \
  --configuration Release \
  --verbosity normal \
  --filter "Category=Integration" \
  --logger "trx;LogFileName=integration-tests.trx" \
  --results-directory ./test-results/integration \
  --collect:"XPlat Code Coverage" \
  --settings codecoverage.runsettings

echo "📊 Parsing test results..."

# Parse Unit Tests
if [ -f "./test-results/unit/unit-tests.trx" ]; then
    echo "Parsing Unit Test Results..."
    python3 << 'EOF'
import xml.etree.ElementTree as ET
import json

def parse_trx(file_path, category):
    try:
        tree = ET.parse(file_path)
        root = tree.getroot()
        
        ns = {'ns': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
        
        counters = root.find('.//ns:Counters', ns)
        if counters is not None:
            total = int(counters.get('total', 0))
            passed = int(counters.get('passed', 0))
            failed = int(counters.get('failed', 0))
            duration = root.find('.//ns:Times', ns)
            duration_text = duration.get('finish') if duration is not None else 'Unknown'
        else:
            total = passed = failed = 0
            duration_text = 'Unknown'
        
        tests = []
        for test_result in root.findall('.//ns:UnitTestResult', ns):
            test_name = test_result.get('testName', 'Unknown')
            outcome = test_result.get('outcome', 'Unknown')
            duration_attr = test_result.get('duration', '00:00:00')
            
            tests.append({
                'name': test_name,
                'outcome': outcome,
                'duration': duration_attr
            })
        
        return {
            'category': category,
            'total': total,
            'passed': passed,
            'failed': failed,
            'duration': duration_text,
            'tests': tests
        }
    except Exception as e:
        print(f"Error parsing {file_path}: {e}")
        return {'category': category, 'total': 0, 'passed': 0, 'failed': 0, 'duration': 'Unknown', 'tests': []}

unit_results = parse_trx('./test-results/unit/unit-tests.trx', 'Unit')
with open('./test-results/summary/unit-results.json', 'w') as f:
    json.dump(unit_results, f, indent=2)
    
print(f"Unit Tests: {unit_results['passed']}/{unit_results['total']} passed")
EOF
fi

# Parse Integration Tests
if [ -f "./test-results/integration/integration-tests.trx" ]; then
    echo "Parsing Integration Test Results..."
    python3 << 'EOF'
import xml.etree.ElementTree as ET
import json

def parse_trx(file_path, category):
    try:
        tree = ET.parse(file_path)
        root = tree.getroot()
        
        ns = {'ns': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
        
        counters = root.find('.//ns:Counters', ns)
        if counters is not None:
            total = int(counters.get('total', 0))
            passed = int(counters.get('passed', 0))
            failed = int(counters.get('failed', 0))
            duration = root.find('.//ns:Times', ns)
            duration_text = duration.get('finish') if duration is not None else 'Unknown'
        else:
            total = passed = failed = 0
            duration_text = 'Unknown'
        
        tests = []
        for test_result in root.findall('.//ns:UnitTestResult', ns):
            test_name = test_result.get('testName', 'Unknown')
            outcome = test_result.get('outcome', 'Unknown')
            duration_attr = test_result.get('duration', '00:00:00')
            
            tests.append({
                'name': test_name,
                'outcome': outcome,
                'duration': duration_attr
            })
        
        return {
            'category': category,
            'total': total,
            'passed': passed,
            'failed': failed,
            'duration': duration_text,
            'tests': tests
        }
    except Exception as e:
        print(f"Error parsing {file_path}: {e}")
        return {'category': category, 'total': 0, 'passed': 0, 'failed': 0, 'duration': 'Unknown', 'tests': []}

integration_results = parse_trx('./test-results/integration/integration-tests.trx', 'Integration')
with open('./test-results/summary/integration-results.json', 'w') as f:
    json.dump(integration_results, f, indent=2)
    
print(f"Integration Tests: {integration_results['passed']}/{integration_results['total']} passed")
EOF
fi

echo ""
echo "📈 Test Summary:"
echo "================"

# Display combined results
if [ -f "./test-results/summary/unit-results.json" ] && [ -f "./test-results/summary/integration-results.json" ]; then
    python3 << 'EOF'
import json

try:
    with open('./test-results/summary/unit-results.json', 'r') as f:
        unit = json.load(f)
    with open('./test-results/summary/integration-results.json', 'r') as f:
        integration = json.load(f)
    
    total_tests = unit['total'] + integration['total']
    total_passed = unit['passed'] + integration['passed'] 
    total_failed = unit['failed'] + integration['failed']
    
    print(f"Total Tests: {total_tests}")
    print(f"✅ Passed: {total_passed}")
    print(f"❌ Failed: {total_failed}")
    print("")
    
    print(f"Unit Tests: {unit['passed']}/{unit['total']} passed")
    for test in unit['tests']:
        icon = '✅' if test['outcome'] == 'Passed' else '❌'
        print(f"  {icon} {test['name']}")
    
    print("")
    print(f"Integration Tests: {integration['passed']}/{integration['total']} passed")
    for test in integration['tests']:
        icon = '✅' if test['outcome'] == 'Passed' else '❌'
        print(f"  {icon} {test['name']}")

except Exception as e:
    print(f"Error displaying results: {e}")
EOF
fi

echo ""
echo "✅ Structured test execution completed!"
echo "📂 Results saved in test-results/ directory"