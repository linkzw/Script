using Microsoft.CodeAnalysis.Diagnostics;
using RoslynPad.Roslyn;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Script.Utils
{
	// TODO: workaround for GetSolutionAnalyzerReferences bug (should be added once per Solution)
	internal class CustomRoslynHost : RoslynHost
	{
		private bool _addedAnalyzers;

		public CustomRoslynHost(IEnumerable<Assembly>? additionalAssemblies = null, RoslynHostReferences? references = null, ImmutableHashSet<string>? disabledDiagnostics = null) : base(additionalAssemblies, references, disabledDiagnostics)
		{
		}

		protected override IEnumerable<AnalyzerReference> GetSolutionAnalyzerReferences()
		{
			if (!_addedAnalyzers)
			{
				_addedAnalyzers = true;
				return base.GetSolutionAnalyzerReferences();
			}

			return Enumerable.Empty<AnalyzerReference>();
		}
	}
}
