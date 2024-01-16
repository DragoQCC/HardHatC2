using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;
using System;

namespace HardHatCore.ApiModels.Aspects
{
    /// <summary>
    /// Checks that any marked method, delegate, or event is only invoked by HardHatCore
    /// This is used so the code complies with using an interface but also forces plugins to implement the interface themselves instead of using the provided base version for everything
    /// ex. Asset specific encryption methods, build steps, etc.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate | AttributeTargets.Event)]
    public class InvocableInternalOnlyAttribute : Attribute, IAspect<IMember>
    {
        private static DiagnosticDefinition<IDeclaration> _error = new("HHCore001", Severity.Error, "The type {0} can only be invoked by HardHatCore");

        public void BuildAspect(IAspectBuilder<IMember> builder)
        {
            builder.Outbound.ValidateReferences(this.ValidateReference, ReferenceKinds.All);
        }

        private void ValidateReference(in ReferenceValidationContext context)
        {
            if (context.ReferencingType != context.ReferencedDeclaration.GetClosestNamedType())
            {
                context.Diagnostics.Report(_error.WithArguments(context.ReferencedDeclaration));
            }
        }
    }
}
