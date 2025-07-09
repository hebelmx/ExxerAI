#!/usr/bin/env python3
"""
XML Documentation Coverage Analyzer for ExxerAI Project
Analyzes C# files for XML documentation compliance per CLAUDE.md requirements
"""

import os
import re
import sys
from typing import List, Dict, Tuple, Set
from dataclasses import dataclass
from pathlib import Path

@dataclass
class ClassInfo:
    name: str
    is_public: bool
    has_xml_doc: bool
    file_path: str
    line_number: int

@dataclass
class MethodInfo:
    name: str
    is_public: bool
    has_xml_doc: bool
    class_name: str
    file_path: str
    line_number: int

@dataclass
class PropertyInfo:
    name: str
    is_public: bool
    has_xml_doc: bool
    class_name: str
    file_path: str
    line_number: int

@dataclass
class FileAnalysis:
    file_path: str
    classes: List[ClassInfo]
    methods: List[MethodInfo]
    properties: List[PropertyInfo]
    total_public_apis: int
    documented_public_apis: int

class XmlDocAnalyzer:
    def __init__(self, root_path: str):
        self.root_path = Path(root_path)
        self.file_analyses: List[FileAnalysis] = []
        
    def analyze_project(self) -> Dict:
        """Analyze all C# files in the project for XML documentation."""
        cs_files = self._find_cs_files()
        
        print(f"Found {len(cs_files)} C# files to analyze...")
        
        for file_path in cs_files:
            try:
                analysis = self._analyze_file(file_path)
                if analysis:
                    self.file_analyses.append(analysis)
            except Exception as e:
                print(f"Error analyzing {file_path}: {e}")
        
        return self._generate_report()
    
    def _find_cs_files(self) -> List[Path]:
        """Find all C# files excluding test, bin, obj directories."""
        cs_files = []
        exclusions = {
            'bin', 'obj', 'migrations', 'Migrations', 'TestResults', 
            'tests', 'test', 'Agents/ModelContextProtcolDocumentation'
        }
        
        for file_path in self.root_path.rglob("*.cs"):
            # Skip excluded directories
            if any(excl in str(file_path) for excl in exclusions):
                continue
            # Skip GlobalUsings.cs files
            if file_path.name == "GlobalUsings.cs":
                continue
            cs_files.append(file_path)
        
        return cs_files
    
    def _analyze_file(self, file_path: Path) -> FileAnalysis:
        """Analyze a single C# file for public APIs and their documentation."""
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
        except Exception:
            return None
        
        lines = content.split('\n')
        
        classes = self._extract_classes(lines, str(file_path))
        methods = self._extract_methods(lines, str(file_path))
        properties = self._extract_properties(lines, str(file_path))
        
        # Calculate total public APIs and documented ones
        public_classes = [c for c in classes if c.is_public]
        public_methods = [m for m in methods if m.is_public]
        public_properties = [p for p in properties if p.is_public]
        
        total_public = len(public_classes) + len(public_methods) + len(public_properties)
        documented_public = (
            len([c for c in public_classes if c.has_xml_doc]) +
            len([m for m in public_methods if m.has_xml_doc]) +
            len([p for p in public_properties if p.has_xml_doc])
        )
        
        return FileAnalysis(
            file_path=str(file_path),
            classes=classes,
            methods=methods,
            properties=properties,
            total_public_apis=total_public,
            documented_public_apis=documented_public
        )
    
    def _extract_classes(self, lines: List[str], file_path: str) -> List[ClassInfo]:
        """Extract class information from file lines."""
        classes = []
        
        for i, line in enumerate(lines):
            # Look for class declarations
            class_match = re.search(r'(public\s+)?(?:abstract\s+|static\s+)?class\s+(\w+)', line.strip())
            if class_match:
                is_public = class_match.group(1) is not None
                class_name = class_match.group(2)
                
                # Check if previous non-empty line has XML doc
                has_xml_doc = self._has_xml_doc_before(lines, i)
                
                classes.append(ClassInfo(
                    name=class_name,
                    is_public=is_public,
                    has_xml_doc=has_xml_doc,
                    file_path=file_path,
                    line_number=i + 1
                ))
        
        return classes
    
    def _extract_methods(self, lines: List[str], file_path: str) -> List[MethodInfo]:
        """Extract method information from file lines."""
        methods = []
        current_class = "Unknown"
        
        for i, line in enumerate(lines):
            # Track current class
            class_match = re.search(r'(?:public\s+)?(?:abstract\s+|static\s+)?class\s+(\w+)', line.strip())
            if class_match:
                current_class = class_match.group(1)
                continue
            
            # Look for method declarations (excluding properties)
            method_match = re.search(
                r'(public\s+|private\s+|protected\s+|internal\s+)?'
                r'(?:static\s+|virtual\s+|override\s+|abstract\s+|async\s+)*'
                r'(?:\w+\s+)?'  # return type
                r'(\w+)\s*\(',  # method name and opening parenthesis
                line.strip()
            )
            
            if method_match and not re.search(r'\s+(get|set)\s*[{;]', line):
                visibility = method_match.group(1)
                method_name = method_match.group(2)
                
                # Skip constructors that match class name
                if method_name == current_class:
                    continue
                    
                is_public = visibility is None or 'public' in visibility
                
                # Check if previous non-empty line has XML doc
                has_xml_doc = self._has_xml_doc_before(lines, i)
                
                methods.append(MethodInfo(
                    name=method_name,
                    is_public=is_public,
                    has_xml_doc=has_xml_doc,
                    class_name=current_class,
                    file_path=file_path,
                    line_number=i + 1
                ))
        
        return methods
    
    def _extract_properties(self, lines: List[str], file_path: str) -> List[PropertyInfo]:
        """Extract property information from file lines."""
        properties = []
        current_class = "Unknown"
        
        for i, line in enumerate(lines):
            # Track current class
            class_match = re.search(r'(?:public\s+)?(?:abstract\s+|static\s+)?class\s+(\w+)', line.strip())
            if class_match:
                current_class = class_match.group(1)
                continue
            
            # Look for property declarations
            prop_match = re.search(
                r'(public\s+|private\s+|protected\s+|internal\s+)?'
                r'(?:static\s+|virtual\s+|override\s+|abstract\s+)*'
                r'\w+\s+'  # return type
                r'(\w+)\s*{'  # property name and opening brace
                r'|'
                r'(public\s+|private\s+|protected\s+|internal\s+)?'
                r'(?:static\s+|virtual\s+|override\s+|abstract\s+)*'
                r'\w+\s+'  # return type
                r'(\w+)\s*=>', # property name with expression body
                line.strip()
            )
            
            if prop_match:
                visibility = prop_match.group(1) or prop_match.group(3)
                prop_name = prop_match.group(2) or prop_match.group(4)
                
                is_public = visibility is None or 'public' in visibility
                
                # Check if previous non-empty line has XML doc
                has_xml_doc = self._has_xml_doc_before(lines, i)
                
                properties.append(PropertyInfo(
                    name=prop_name,
                    is_public=is_public,
                    has_xml_doc=has_xml_doc,
                    class_name=current_class,
                    file_path=file_path,
                    line_number=i + 1
                ))
        
        return properties
    
    def _has_xml_doc_before(self, lines: List[str], line_index: int) -> bool:
        """Check if there's XML documentation before the given line."""
        # Look backwards for XML documentation
        for i in range(line_index - 1, -1, -1):
            line = lines[i].strip()
            if not line:  # Skip empty lines
                continue
            if line.startswith('///'):
                return True
            else:
                break  # Non-empty, non-XML-doc line found
        return False
    
    def _generate_report(self) -> Dict:
        """Generate comprehensive coverage report."""
        total_files = len(self.file_analyses)
        total_public_apis = sum(fa.total_public_apis for fa in self.file_analyses)
        total_documented = sum(fa.documented_public_apis for fa in self.file_analyses)
        
        coverage_percentage = (total_documented / total_public_apis * 100) if total_public_apis > 0 else 100
        
        # Find files with missing documentation
        files_missing_docs = []
        for fa in self.file_analyses:
            if fa.total_public_apis > 0 and fa.documented_public_apis < fa.total_public_apis:
                missing_count = fa.total_public_apis - fa.documented_public_apis
                files_missing_docs.append({
                    'file': fa.file_path.replace(str(self.root_path), '').replace('\\', '/'),
                    'missing': missing_count,
                    'total': fa.total_public_apis,
                    'coverage': (fa.documented_public_apis / fa.total_public_apis * 100) if fa.total_public_apis > 0 else 100
                })
        
        # Sort by most missing documentation
        files_missing_docs.sort(key=lambda x: x['missing'], reverse=True)
        
        # Find specific undocumented APIs
        undocumented_apis = []
        for fa in self.file_analyses:
            file_relative = fa.file_path.replace(str(self.root_path), '').replace('\\', '/')
            
            # Add undocumented public classes
            for cls in fa.classes:
                if cls.is_public and not cls.has_xml_doc:
                    undocumented_apis.append({
                        'type': 'class',
                        'name': cls.name,
                        'file': file_relative,
                        'line': cls.line_number
                    })
            
            # Add undocumented public methods
            for method in fa.methods:
                if method.is_public and not method.has_xml_doc:
                    undocumented_apis.append({
                        'type': 'method',
                        'name': f"{method.class_name}.{method.name}",
                        'file': file_relative,
                        'line': method.line_number
                    })
            
            # Add undocumented public properties
            for prop in fa.properties:
                if prop.is_public and not prop.has_xml_doc:
                    undocumented_apis.append({
                        'type': 'property',
                        'name': f"{prop.class_name}.{prop.name}",
                        'file': file_relative,
                        'line': prop.line_number
                    })
        
        return {
            'summary': {
                'total_files_analyzed': total_files,
                'total_public_apis': total_public_apis,
                'documented_public_apis': total_documented,
                'coverage_percentage': round(coverage_percentage, 2)
            },
            'files_missing_docs': files_missing_docs[:20],  # Top 20 files with missing docs
            'undocumented_apis': undocumented_apis[:50],    # Top 50 undocumented APIs
            'critical_files': self._identify_critical_files(),
            'patterns': self._identify_patterns()
        }
    
    def _identify_critical_files(self) -> List[Dict]:
        """Identify critical files that should be prioritized for documentation."""
        critical_patterns = [
            'Domain/Entities',
            'Application/Services',
            'Application/Interfaces',
            'Infrastructure',
            'Api/Controllers'
        ]
        
        critical_files = []
        for fa in self.file_analyses:
            if any(pattern in fa.file_path for pattern in critical_patterns):
                if fa.total_public_apis > 0:
                    coverage = (fa.documented_public_apis / fa.total_public_apis * 100)
                    if coverage < 100:
                        file_relative = fa.file_path.replace(str(self.root_path), '').replace('\\', '/')
                        critical_files.append({
                            'file': file_relative,
                            'missing': fa.total_public_apis - fa.documented_public_apis,
                            'total': fa.total_public_apis,
                            'coverage': round(coverage, 2),
                            'category': next((p for p in critical_patterns if p in fa.file_path), 'Other')
                        })
        
        return sorted(critical_files, key=lambda x: (x['missing'], -x['coverage']), reverse=True)
    
    def _identify_patterns(self) -> Dict:
        """Identify patterns in missing documentation."""
        patterns = {
            'by_layer': {},
            'by_type': {'class': 0, 'method': 0, 'property': 0},
            'most_problematic_files': []
        }
        
        # Analyze by layer
        layer_stats = {}
        for fa in self.file_analyses:
            if 'Core/' in fa.file_path:
                layer = 'Core'
            elif 'Infraestructure/' in fa.file_path:
                layer = 'Infrastructure'
            elif 'Presentation/' in fa.file_path:
                layer = 'Presentation'
            elif 'Orchestration/' in fa.file_path:
                layer = 'Orchestration'
            else:
                layer = 'Other'
            
            if layer not in layer_stats:
                layer_stats[layer] = {'total': 0, 'documented': 0}
            
            layer_stats[layer]['total'] += fa.total_public_apis
            layer_stats[layer]['documented'] += fa.documented_public_apis
        
        for layer, stats in layer_stats.items():
            if stats['total'] > 0:
                coverage = (stats['documented'] / stats['total'] * 100)
                patterns['by_layer'][layer] = {
                    'total': stats['total'],
                    'documented': stats['documented'],
                    'coverage': round(coverage, 2)
                }
        
        # Count by API type
        for fa in self.file_analyses:
            for cls in fa.classes:
                if cls.is_public and not cls.has_xml_doc:
                    patterns['by_type']['class'] += 1
            for method in fa.methods:
                if method.is_public and not method.has_xml_doc:
                    patterns['by_type']['method'] += 1
            for prop in fa.properties:
                if prop.is_public and not prop.has_xml_doc:
                    patterns['by_type']['property'] += 1
        
        return patterns

def main():
    """Main function to run the analysis."""
    if len(sys.argv) > 1:
        root_path = sys.argv[1]
    else:
        root_path = '/mnt/f/Dynamic/ExxerAi/ExxerAI/code/src'
    
    print("ExxerAI XML Documentation Coverage Analysis")
    print("=" * 50)
    print()
    
    analyzer = XmlDocAnalyzer(root_path)
    report = analyzer.analyze_project()
    
    # Print summary
    summary = report['summary']
    print(f"📊 SUMMARY")
    print(f"Files Analyzed: {summary['total_files_analyzed']}")
    print(f"Total Public APIs: {summary['total_public_apis']}")
    print(f"Documented APIs: {summary['documented_public_apis']}")
    print(f"Coverage: {summary['coverage_percentage']}%")
    print()
    
    # Print coverage by layer
    if report['patterns']['by_layer']:
        print(f"📈 COVERAGE BY LAYER")
        for layer, stats in report['patterns']['by_layer'].items():
            print(f"{layer}: {stats['coverage']}% ({stats['documented']}/{stats['total']})")
        print()
    
    # Print missing documentation by type
    by_type = report['patterns']['by_type']
    print(f"🔍 MISSING DOCUMENTATION BY TYPE")
    print(f"Classes: {by_type['class']}")
    print(f"Methods: {by_type['method']}")
    print(f"Properties: {by_type['property']}")
    print()
    
    # Print critical files needing documentation
    if report['critical_files']:
        print(f"🚨 CRITICAL FILES NEEDING DOCUMENTATION (Top 10)")
        for i, file_info in enumerate(report['critical_files'][:10], 1):
            print(f"{i:2d}. {file_info['file']}")
            print(f"    Missing: {file_info['missing']}, Coverage: {file_info['coverage']}%")
        print()
    
    # Print files with most missing documentation
    if report['files_missing_docs']:
        print(f"📝 FILES WITH MOST MISSING DOCUMENTATION (Top 10)")
        for i, file_info in enumerate(report['files_missing_docs'][:10], 1):
            print(f"{i:2d}. {file_info['file']}")
            print(f"    Missing: {file_info['missing']}/{file_info['total']} ({file_info['coverage']:.1f}%)")
        print()
    
    # Print specific undocumented APIs
    if report['undocumented_apis']:
        print(f"🔧 SPECIFIC UNDOCUMENTED APIs (Top 20)")
        for i, api in enumerate(report['undocumented_apis'][:20], 1):
            print(f"{i:2d}. {api['type'].upper()}: {api['name']}")
            print(f"    File: {api['file']}:{api['line']}")
        print()
    
    # Generate recommendations
    print(f"💡 RECOMMENDATIONS")
    coverage = summary['coverage_percentage']
    if coverage < 50:
        print("❌ CRITICAL: Documentation coverage is severely lacking")
        print("   Priority: Start with Domain entities and Application services")
    elif coverage < 80:
        print("⚠️  WARNING: Documentation coverage needs improvement")
        print("   Priority: Focus on public interfaces and controllers")
    elif coverage < 95:
        print("🔶 GOOD: Documentation coverage is decent but could be better")
        print("   Priority: Document remaining public APIs for completeness")
    else:
        print("✅ EXCELLENT: Documentation coverage meets CLAUDE.md requirements")
    
    print(f"\n📋 NEXT STEPS")
    if report['critical_files']:
        print("1. Document critical infrastructure and domain files")
        print("2. Focus on public APIs in Application services")
        print("3. Complete API controller documentation")
        print("4. Add missing property documentation")
    
    print(f"\n🎯 CLAUDE.md COMPLIANCE")
    if coverage >= 95:
        print("✅ COMPLIANT: Meets 'Every public class, method, and property must have proper XML documentation'")
    else:
        missing = summary['total_public_apis'] - summary['documented_public_apis']
        print(f"❌ NON-COMPLIANT: {missing} public APIs missing XML documentation")
        print("   CLAUDE.md requires: 'Every public class, method, and property must have proper XML documentation'")

if __name__ == "__main__":
    main()