#!/usr/bin/env python3
"""
Test Runner for MCP Server Collection
Discovers and runs all unit tests with coverage reporting.
"""

import unittest
import sys
import os
from pathlib import Path

# Add the parent directory to the Python path
project_root = Path(__file__).parent.parent
sys.path.insert(0, str(project_root))

def discover_and_run_tests():
    """Discover and run all tests"""
    # Get the tests directory
    tests_dir = Path(__file__).parent
    
    # Create test suite
    loader = unittest.TestLoader()
    suite = loader.discover(tests_dir, pattern='test_*.py')
    
    # Run tests with detailed output
    runner = unittest.TextTestRunner(
        verbosity=2,
        stream=sys.stdout,
        descriptions=True,
        failfast=False
    )
    
    print("🧪 Running MCP Server Test Suite")
    print("=" * 50)
    
    result = runner.run(suite)
    
    # Print summary
    print("\n" + "=" * 50)
    print("📊 Test Summary:")
    print(f"   Tests run: {result.testsRun}")
    print(f"   Failures: {len(result.failures)}")
    print(f"   Errors: {len(result.errors)}")
    print(f"   Skipped: {len(result.skipped)}")
    
    if result.failures:
        print("\n❌ Failures:")
        for test, traceback in result.failures:
            print(f"   - {test}")
    
    if result.errors:
        print("\n💥 Errors:")
        for test, traceback in result.errors:
            print(f"   - {test}")
    
    if result.skipped:
        print("\n⏭️  Skipped:")
        for test, reason in result.skipped:
            print(f"   - {test}: {reason}")
    
    # Return success/failure
    success = len(result.failures) == 0 and len(result.errors) == 0
    
    if success:
        print("\n✅ All tests passed!")
    else:
        print("\n❌ Some tests failed!")
    
    return success

def run_specific_test(test_module):
    """Run a specific test module"""
    try:
        # Import the test module
        module = __import__(f'tests.{test_module}', fromlist=[test_module])
        
        # Create test suite from module
        loader = unittest.TestLoader()
        suite = loader.loadTestsFromModule(module)
        
        # Run tests
        runner = unittest.TextTestRunner(verbosity=2)
        result = runner.run(suite)
        
        return len(result.failures) == 0 and len(result.errors) == 0
    
    except ImportError as e:
        print(f"❌ Could not import test module '{test_module}': {e}")
        return False

def check_dependencies():
    """Check if optional dependencies are available"""
    print("🔍 Checking dependencies...")
    
    dependencies = {
        'flask': 'Flask (for HTTP server tests)',
        'requests': 'Requests (for discovery tests)',
        'psutil': 'PSUtil (for system monitoring tests)'
    }
    
    available = {}
    for dep, description in dependencies.items():
        try:
            __import__(dep)
            available[dep] = True
            print(f"   ✅ {description}")
        except ImportError:
            available[dep] = False
            print(f"   ❌ {description} - Install with: pip install {dep}")
    
    return available

def main():
    """Main test runner"""
    import argparse
    
    parser = argparse.ArgumentParser(description='MCP Server Test Runner')
    parser.add_argument('--test', '-t', help='Run specific test module (e.g., test_simple_mcp_server)')
    parser.add_argument('--check-deps', '-c', action='store_true', help='Check dependencies only')
    parser.add_argument('--coverage', action='store_true', help='Run with coverage (requires coverage.py)')
    
    args = parser.parse_args()
    
    if args.check_deps:
        check_dependencies()
        return
    
    # Check dependencies
    deps = check_dependencies()
    print()
    
    if args.coverage:
        try:
            import coverage
            cov = coverage.Coverage()
            cov.start()
            print("📊 Running with coverage tracking...")
        except ImportError:
            print("❌ Coverage not available. Install with: pip install coverage")
            args.coverage = False
    
    # Run tests
    if args.test:
        print(f"🧪 Running specific test: {args.test}")
        success = run_specific_test(args.test)
    else:
        success = discover_and_run_tests()
    
    if args.coverage:
        cov.stop()
        cov.save()
        print("\n📊 Coverage Report:")
        cov.report()
    
    # Exit with appropriate code
    sys.exit(0 if success else 1)

if __name__ == '__main__':
    main()
