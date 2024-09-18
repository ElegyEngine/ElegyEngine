// SPDX-FileCopyrightText: 2022-present Elegy Engine contributors
// SPDX-License-Identifier: MIT

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Elegy.ECS.Generator
{
	public abstract class SimpleGenerator : IIncrementalGenerator
	{
		public void Initialize( IncrementalGeneratorInitializationContext context )
		{
			var pipeline = context.SyntaxProvider.CreateSyntaxProvider(
				predicate: ( node, token ) => node is CompilationUnitSyntax,
				transform: ( syntaxContext, token ) => new SimpleCompilationData( (CompilationUnitSyntax)syntaxContext.Node )
			);

			context.RegisterSourceOutput(
				source: pipeline.Collect(),
				action: ( production, data ) =>
				{
					SimpleCompilationData mergedData = new( data[0] );
					for ( int i = 1; i < data.Length; i++ )
					{
						mergedData.Merge( data[i] );
					}

					Execute( production, mergedData );
				}
			);
		}

		protected abstract void Execute( SourceProductionContext production, SimpleCompilationData data );
	}
}
