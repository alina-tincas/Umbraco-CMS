using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;

[VersionedApiBackOfficeRoute(Constants.UdiEntityType.DocumentBlueprint)]
[ApiExplorerSettings(GroupName = "Document Blueprint")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public abstract class DocumentBlueprintControllerBase : ContentControllerBase
{
    protected IActionResult DocumentBlueprintNotFound()
        => OperationStatusResult(ContentEditingOperationStatus.NotFound, problemDetailsBuilder
            => NotFound(problemDetailsBuilder
                .WithTitle("The document blueprint could not be found")
                .Build()));
}
